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
        public bool IsRunning { get; private set; }

        private readonly FrmBluetoothChat app;
        private readonly object clientLock = new object();
        private readonly List<BluetoothClient> clients = new List<BluetoothClient>();
        private BluetoothListener listener;
        private CancellationTokenSource cancelToken;
        private Task serverTask;


        public AppServer(FrmBluetoothChat app)
        {
            this.app = app;
        }

        public void Start()
        {
            cancelToken = new CancellationTokenSource();
            listener = new BluetoothListener(Messages.Guid);

            app.AppendConsoleText(DisplayFormat.FormatMessage("Starting server..."));
            try
            {
                listener.Start();
            }
            catch (Exception e)
            {
                app.AppendConsoleText(DisplayFormat.FormatMessage($"Unable to start Listener: {e.Message}"));
                app.AppendConsoleText(DisplayFormat.FormatMessage(Messages.ConsolePrompt));
                return;
            }

            app.AppendConsoleText(DisplayFormat.FormatMessage("Waiting for clients"));
            app.AppendConsoleText(DisplayFormat.FormatMessage("Make sure devices are connected to you via Bluetooth pairing for server connections to work"));
            serverTask = Task.Run(() => HostSessionAsync(cancelToken.Token));
            IsRunning = true;
        }

        public void Stop()
        {
            try
            {
                cancelToken?.Cancel();
                cancelToken?.Dispose();
            }
            catch { }

            try
            {
                listener?.Stop();
                listener?.Dispose();
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
                    client.Dispose();
                }
                catch { }
            }

            IsRunning = false;
        }

        private async Task HostSessionAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                BluetoothClient client;
                try
                {
                    client = await listener.AcceptBluetoothClientAsync();
                }
                catch (Exception e)
                {
                    app.AppendConsoleText(DisplayFormat.FormatMessage($"Client could not be connected: {e.Message}"));
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
                        int index = text.IndexOf(Messages.TerminateMessage);
                        if (index == -1)
                            break;

                        // Get the message before the terminate flag
                        string message = text.Substring(0, index);

                        // Clear what is in the builder (the message itself)
                        stringBuilder.Clear();

                        // Append the flag to the builder
                        stringBuilder.Append(text.Substring(index, Messages.TerminateMessage.Length));

                        if (!string.IsNullOrWhiteSpace(message))
                        {
                            app.AppendConsoleText(DisplayFormat.FormatMessage(message));
                            await SendMessageToClientsAsync(message);
                        }
                        return;
                    }

                    string clientMessage = stringBuilder.ToString();
                    if (!string.IsNullOrWhiteSpace(clientMessage))
                    {
                        stringBuilder.Clear();
                        app.AppendConsoleText(DisplayFormat.FormatMessage(clientMessage));
                        await SendMessageToClientsAsync(clientMessage).ConfigureAwait(false);
                    }
                }
            }
            catch (OperationCanceledException)
            {

            }
            catch (Exception e)
            {
                app.AppendConsoleText(DisplayFormat.FormatMessage($"Client error: {e.Message}"));
            }
            finally
            {
                RemoveClient(client);
            }
        }

        public async Task SendMessageToClientsAsync(string message)
        {
            byte[] response;
            try
            {
                response = Encoding.UTF8.GetBytes(message);
            }
            catch (Exception e)
            {
                app.AppendConsoleText(DisplayFormat.FormatMessage($"Error translating message to bytes: {e.Message}"));
                return;
            }

            BluetoothClient[] clientCopy = GetClients();

            // Do all message sends concurrently, in case a client is slow
            IEnumerable<Task> tasks = clientCopy.Select(async client =>
            {
                try
                {
                    await client.GetStream().WriteAsync(response, 0, response.Length, cancelToken.Token).ConfigureAwait(false);
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
                client?.Close();
                client?.Dispose();
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
