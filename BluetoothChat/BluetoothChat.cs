using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using InTheHand.Net;
using InTheHand.Net.Sockets;
using Microsoft.VisualBasic;

namespace BluetoothChat
{
    public partial class FrmBluetoothChat : Form
    {
        private readonly Guid guid = new Guid("3831e749-fe75-4c2e-a5d3-06bc74a68b50");
        private readonly string consolePrompt = string.Format("Connect to or create a server using the \"Server\" tab. " +
    "{0}{0}Look up a devices address using the \"Address Lookup\" tab. {0}{0}Change your display name using the \"Username\" tab.", Environment.NewLine);
        private readonly string bluetoothPrompt = "Enter Bluetooth address: ";
        private readonly string username = "Current Username: ";
        private readonly string terminateMessage = "&re9&";
        private string displayName = Properties.Settings.Default.CurrentUsername;

        private readonly object clientLock = new object();
        private Mode appMode;
        private BluetoothClient client;
        private BluetoothListener listener;
        private CancellationTokenSource cancelToken;
        private List<BluetoothClient> clients = new List<BluetoothClient>();

        public FrmBluetoothChat()
        {
            InitializeComponent();
            client = new BluetoothClient();
            listener = new BluetoothListener(guid);
            RtbConsole.Text = consolePrompt;
            appMode = Mode.Inactive;
            AcceptButton = BtnSend;
        }

        private void FrmBluetoothChat_Load(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName = Environment.MachineName;
                Properties.Settings.Default.CurrentUsername = displayName;
            }

            CurrentUsernameToolStripMenuItem.Text = username + displayName;
        }

        private void FrmBluetoothChat_FormClosed(object sender, FormClosedEventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private void SearchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FrmDeviceSearch deviceSearch = new FrmDeviceSearch();
            deviceSearch.Show();
        }

        private async void ChangeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string oldUsername = displayName;
            FrmUsernameDialog dialog = new FrmUsernameDialog();
            DialogResult result = dialog.ShowDialog();
            if (result != DialogResult.OK)
            {
                return;
            }

            string newUsername = dialog.NewUsername;
            if (!string.IsNullOrWhiteSpace(newUsername))
            {
                CurrentUsernameToolStripMenuItem.Text = username + newUsername;
                Properties.Settings.Default.CurrentUsername = newUsername;
                displayName = newUsername;
            }

            if (appMode == Mode.Connect && client != null && client.Connected)
            {
                string message = $"[{oldUsername}] changed their name to [{displayName}]";
                await SendMessageToServer(client.GetStream(), message);
            }

            if (appMode == Mode.Create && listener != null && listener.Active && cancelToken != null)
            {
                string message = $"Host [{oldUsername}] changed their name to [{displayName}]";
                RtbConsole.AppendText(FormatMessage(message));
                await SendMessageToClientsAsync(message, cancelToken.Token);
            }
        }

        private void CreateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CreateServerPrompt();
            ToggleServerTabs();
        }

        private void ConnectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ConnectToServerPrompt();
            ToggleServerTabs();
        }

        private async void BtnSend_Click(object sender, EventArgs e)
        {
            if (appMode == Mode.Inactive || string.IsNullOrWhiteSpace(TxtInput.Text))
            {
                return;
            }
            else
            {
                if (!listener.Active && appMode == Mode.Create)
                {
                    StartServer();
                }
                else if (!client.Connected && appMode == Mode.Connect)
                {
                    await AttemptConnection();
                }
                else
                {
                    if (listener.Active && appMode == Mode.Create)
                    {
                        string message = $"[HOST] [{displayName}]: {TxtInput.Text}";
                        BeginInvoke((Action)(() => RtbConsole.AppendText(FormatMessage(message))));
                        await SendMessageToClientsAsync(message, cancelToken.Token);
                    }
                    else if (client.Connected && appMode == Mode.Connect)
                    {
                        string message = $"[{displayName}]: {TxtInput.Text}";
                        await SendMessageToServer(client.GetStream(), message);
                    }
                }
                TxtInput.Clear();
            }
        }

        private async void BtnExit_Click(object sender, EventArgs e)
        {
            if (appMode != Mode.Inactive)
            {
                if (listener.Active && appMode == Mode.Create)
                {
                    StopServer();
                }

                if (client.Connected && appMode == Mode.Connect)
                {
                    await SendLeaveMessage();
                    client.Close();
                }

                ResetUI();
            }
            else
            {
                Application.Exit();
            }
        }

        private void ConnectToServerPrompt()
        {
            appMode = Mode.Connect;
            RtbConsole.Clear();
            RtbConsole.Text = bluetoothPrompt;
        }

        private void CreateServerPrompt()
        {
            appMode = Mode.Create;
            RtbConsole.Clear();
            StartServer();
        }

        private void ToggleServerTabs()
        {
            CreateToolStripMenuItem.Enabled = appMode == Mode.Inactive ? true : false;
            ConnectToolStripMenuItem.Enabled = appMode == Mode.Inactive ? true : false;
        }

        private string FormatMessage(string message)
        {
            return $"{message}{Environment.NewLine}{Environment.NewLine}";
        }

        private void StartServer()
        {
            cancelToken = new CancellationTokenSource();
            listener = new BluetoothListener(guid);

            RtbConsole.Text = FormatMessage("Starting server...");
            try
            {
                listener.Start();
            }
            catch (Exception e)
            {
                RtbConsole.Text = FormatMessage($"Unable to start listener: {e.Message}");
                RtbConsole.AppendText(FormatMessage(consolePrompt));
                return;
            }

            RtbConsole.AppendText(FormatMessage("Waiting for clients"));
            RtbConsole.AppendText(FormatMessage("Make sure devices are connected to you via Bluetooth pairing for server connections to work"));
            _ = Task.Run(() => HostSessionAsync(listener, cancelToken.Token));
        }

        private void StopServer()
        {
            try
            {
                cancelToken?.Cancel();
            }
            catch { }

            try
            {
                listener?.Stop();
            }
            catch { }

            BluetoothClient[] clientCopy;
            lock (clientLock)
            {
                clientCopy = clients.ToArray();
                clients.Clear();
            }

            foreach (BluetoothClient client in clientCopy)
            {
                try
                {
                    client.Close();
                }
                catch { }

                try
                {
                    client.Dispose();
                }
                catch { }

                listener = null;

                try
                {
                    cancelToken?.Dispose();
                }
                catch { }

                cancelToken = null;
            }
        }

        private async Task HostSessionAsync(BluetoothListener listener, CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                BluetoothClient client;
                try
                {
                    client = await listener.AcceptBluetoothClientAsync();
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (Exception e)
                {
                    BeginInvoke((Action)(() => RtbConsole.AppendText(FormatMessage($"Server cannot be started: {e.Message}"))));
                    await Task.Delay(250, ct).ConfigureAwait(false);
                    return;
                }

                AddClient(client);
                _ = Task.Run(() => HandleClientAsync(client, ct));
            }
        }

        private async Task HandleClientAsync(BluetoothClient client, CancellationToken ct)
        {
            byte[] buffer = new byte[512];
            NetworkStream clientStream = client.GetStream();
            StringBuilder stringBuilder = new StringBuilder();

            try
            {
                while (!ct.IsCancellationRequested && clientStream != null)
                {
                    // Read data from client
                    int data = await clientStream.ReadAsync(buffer, 0, buffer.Length);
                    if (data == 0)
                        break;

                    stringBuilder.Append(Encoding.UTF8.GetString(buffer, 0, data));

                    // Handling terminate flag
                    while (true)
                    {
                        string text = stringBuilder.ToString();
                        int index = text.IndexOf(terminateMessage);
                        if (index == -1)
                            break;

                        // Get the message before the terminate flag
                        string message = text.Substring(0, index);

                        // Clear what is in the builder (the message itself)
                        stringBuilder.Clear();

                        // Append the flag to the builder
                        stringBuilder.Append(text.Substring(index, terminateMessage.Length));

                        if (!string.IsNullOrWhiteSpace(message))
                        {
                            BeginInvoke((Action)(() => RtbConsole.AppendText(FormatMessage(message))));
                            await SendMessageToClientsAsync(message, ct);
                        }

                        RemoveClient(client);
                        return;
                    }

                    string clientMessage = stringBuilder.ToString();
                    if (!string.IsNullOrWhiteSpace(clientMessage))
                    {
                        stringBuilder.Clear();
                        BeginInvoke((Action)(() => RtbConsole.AppendText(FormatMessage(clientMessage))));
                        await SendMessageToClientsAsync(clientMessage, ct).ConfigureAwait(false);
                    }
                }
            }
            catch (OperationCanceledException)
            {

            }
            catch (Exception e)
            {
                RtbConsole.AppendText(FormatMessage($"Client error: {e.Message}"));
            }
            finally
            {
                RemoveClient(client);
            }
        }

        private async Task SendMessageToClientsAsync(string message, CancellationToken ct)
        {
            byte[] response;
            try
            {
                response = Encoding.UTF8.GetBytes(message);
            }
            catch (Exception e)
            {
                RtbConsole.AppendText(FormatMessage($"Error translating message to bytes: {e.Message}"));
                return;
            }

            BluetoothClient[] clientCopy = GetClients();

            // Do all message sends concurrently, in case a client is slow
            IEnumerable<Task> tasks = clientCopy.Select(async client =>
            {
                try
                {
                    await client.GetStream().WriteAsync(response, 0, response.Length, ct).ConfigureAwait(false);
                }
                catch
                {
                    RemoveClient(client);
                }
            });

            try
            {
                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
            catch { }
        }


        private async Task AttemptConnection()
        {
            RtbConsole.Text += FormatMessage(TxtInput.Text);
            BluetoothAddress address;
            try
            {
                address = BluetoothAddress.Parse(TxtInput.Text);
                BtnSend.Enabled = false;
            }
            catch (Exception)
            {
                RtbConsole.AppendText(FormatMessage("Incorrect format for Bluetooth address"));
                RtbConsole.AppendText(bluetoothPrompt);
                BtnSend.Enabled = true;
                return;
            }

            try
            {
                client = new BluetoothClient();
                RtbConsole.AppendText(FormatMessage("Connecting to server"));
                await Task.Run(() => client.Connect(address, guid));
            }
            catch (Exception ex)
            {
                RtbConsole.AppendText(FormatMessage($"Error connecting to server: {ex.Message}"));
                RtbConsole.AppendText(FormatMessage(bluetoothPrompt));
                BtnSend.Enabled = true;
                return;
            }

            RtbConsole.Clear();
            BtnSend.Enabled = true;

            string message = $"[{displayName}] has joined the server";
            cancelToken?.Cancel();
            cancelToken = new CancellationTokenSource();

            try
            {
                await SendMessageToServer(client.GetStream(), message);
            }
            catch (Exception ex)
            {
                RtbConsole.AppendText(FormatMessage($"Error sending welcome message to server: {ex.Message}"));
                RtbConsole.AppendText(FormatMessage(bluetoothPrompt));
                return;
            }

            try
            {
                await ReadMessagesFromServer(cancelToken.Token);
            }
            catch (Exception ex)
            {
                RtbConsole.AppendText(FormatMessage($"Error reading messages from server: {ex.Message}"));
                RtbConsole.AppendText(FormatMessage(bluetoothPrompt));
                return;
            }
        }

        private async Task SendMessageToServer(NetworkStream stream, string message)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(message);

                await stream.WriteAsync(data, 0, data.Length);
                TxtInput.Text = string.Empty;
            }
            catch (Exception e)
            {
                BeginInvoke((Action)(() => RtbConsole.AppendText(FormatMessage($"Unable to broadcast message: {e.Message}"))));
            }
        }

        private async Task ReadMessagesFromServer(CancellationToken token)
        {
            byte[] buffer = new byte[512];
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (client == null || !client.Connected)
                        return;

                    int data = await client.GetStream().ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);

                    // We stopped receiving data from the server (server shut down)
                    if (data == 0)
                    {
                        client.Close();
                        ResetUI();
                    }

                    string response = Encoding.UTF8.GetString(buffer, 0, data);
                    BeginInvoke((Action)(() => RtbConsole.AppendText(FormatMessage(response))));
                }
                catch (Exception)
                {
                    ResetUI();
                }
            }
        }

        private async Task SendLeaveMessage()
        {
            if (client != null && client.Connected)
            {
                // Attempt to send a message to the server indicating the client is leaving
                // Disconnect even if the message fails
                string message = $"[{displayName}] has left the server{terminateMessage}";
                try
                {
                    await SendMessageToServer(client.GetStream(), message);
                }
                finally
                {
                    client.Close();
                }
            }
        }

        private void AddClient(BluetoothClient client)
        {
            lock (clientLock)
            {
                clients.Add(client);
            }
        }

        private void RemoveClient(BluetoothClient client)
        {
            lock (clientLock)
            {
                clients.Remove(client);
            }

            try
            {
                client.Close();
                client.Dispose();
            }
            catch { }
        }

        private BluetoothClient[] GetClients()
        {
            lock (clientLock)
            {
                return clients.ToArray();
            }
        }

        private void ResetUI()
        {
            appMode = Mode.Inactive;
            BeginInvoke((Action)(() => ToggleServerTabs()));
            BeginInvoke((Action)(() => RtbConsole.Clear()));
            BeginInvoke((Action)(() => RtbConsole.Text = consolePrompt));
        }
    }
}
