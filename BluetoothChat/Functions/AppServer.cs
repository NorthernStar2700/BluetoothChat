using BluetoothChat.Constants;
using BluetoothChat.Utilities;
using InTheHand.Net.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BluetoothChat.Functions
{
    public class AppServer
    {
        public BluetoothListener Listener;
        public CancellationTokenSource CancelToken;

        private readonly FrmBluetoothChat app;
        private List<BluetoothClient> clients;
        private readonly string consolePrompt = string.Format("Connect to or create a server using the \"Server\" tab. " +
"{0}{0}Look up a devices address using the \"Address Lookup\" tab. {0}{0}Change your display name using the \"Username\" tab.", Environment.NewLine);
        private readonly string terminateMessage = "&re9&";
        private readonly object clientLock = new object();

        public AppServer(FrmBluetoothChat app)
        {
            this.app = app;
        }

        public void Start()
        {
            CancelToken = new CancellationTokenSource();
            Listener = new BluetoothListener(Common.Guid);
            clients = new List<BluetoothClient>();

            app.RtbConsole.Text = DisplayFormat.FormatMessage("Starting server...");
            try
            {
                Listener.Start();
            }
            catch (Exception e)
            {
                app.RtbConsole.Text = DisplayFormat.FormatMessage($"Unable to start Listener: {e.Message}");
                app.RtbConsole.AppendText(DisplayFormat.FormatMessage(consolePrompt));
                return;
            }

            app.RtbConsole.AppendText(DisplayFormat.FormatMessage("Waiting for clients"));
            app.RtbConsole.AppendText(DisplayFormat.FormatMessage("Make sure devices are connected to you via Bluetooth pairing for server connections to work"));
            _ = Task.Run(() => HostSessionAsync(Listener, CancelToken.Token));
        }

        public void Stop()
        {
            try
            {
                CancelToken?.Cancel();
            }
            catch { }

            try
            {
                Listener?.Stop();
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

                Listener = null;

                try
                {
                    CancelToken?.Dispose();
                }
                catch { }

                CancelToken = null;
            }
        }

        private async Task HostSessionAsync(BluetoothListener Listener, CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                BluetoothClient client;
                try
                {
                    client = await Listener.AcceptBluetoothClientAsync();
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (Exception e)
                {
                    app.BeginInvoke((Action)(() => app.RtbConsole.AppendText(DisplayFormat.FormatMessage($"Server cannot be started: {e.Message}"))));
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
                            app.BeginInvoke((Action)(() => app.RtbConsole.AppendText(DisplayFormat.FormatMessage(message))));
                            await SendMessageToClientsAsync(message, ct);
                        }

                        RemoveClient(client);
                        return;
                    }

                    string clientMessage = stringBuilder.ToString();
                    if (!string.IsNullOrWhiteSpace(clientMessage))
                    {
                        stringBuilder.Clear();
                        app.BeginInvoke((Action)(() => app.RtbConsole.AppendText(DisplayFormat.FormatMessage(clientMessage))));
                        await SendMessageToClientsAsync(clientMessage, ct).ConfigureAwait(false);
                    }
                }
            }
            catch (OperationCanceledException)
            {

            }
            catch (Exception e)
            {
                app.RtbConsole.AppendText(DisplayFormat.FormatMessage($"Client error: {e.Message}"));
            }
            finally
            {
                RemoveClient(client);
            }
        }

        public async Task SendMessageToClientsAsync(string message, CancellationToken ct)
        {
            byte[] response;
            try
            {
                response = Encoding.UTF8.GetBytes(message);
            }
            catch (Exception e)
            {
                app.RtbConsole.AppendText(DisplayFormat.FormatMessage($"Error translating message to bytes: {e.Message}"));
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
    }
}
