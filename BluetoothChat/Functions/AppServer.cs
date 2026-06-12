using BluetoothChat.Constants;
using BluetoothChat.Enums;
using BluetoothChat.Models;
using BluetoothChat.Utilities;
using InTheHand.Net.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

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
                app.SetAppMode(AppMode.Inactive);
                app.AppendConsoleText(DisplayFormat.FormatMessage($"Unable to start server: {e.Message}"));
                app.AppendConsoleText(DisplayFormat.FormatMessage(Messages.ConsolePrompt));
                return;
            }

            app.AppendConsoleText(DisplayFormat.FormatMessage("Waiting for clients"));
            app.AppendConsoleText(DisplayFormat.FormatMessage("Make sure devices are connected to you via Bluetooth pairing for server connections to work"));
            IsRunning = true;
            serverTask = Task.Run(() => HostSessionAsync(cancelToken.Token));
        }

        public void Stop()
        {
            IsRunning = false;

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
                catch (ObjectDisposedException)
                {
                    return;
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
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    ChatMessage chat = await ChatProtocol.ReadAsync(client.GetStream());

                    switch (chat.MessageType)
                    {
                        case MessageType.Chat:
                            app.AppendConsoleText(DisplayFormat.FormatMessage($"[{chat.SenderName}]: {chat.Message}"));
                            await SendMessageToClientsAsync(chat);
                            break;
                        case MessageType.Join:
                        case MessageType.Leave:
                        case MessageType.UsernameChange:
                            app.AppendConsoleText(DisplayFormat.FormatMessage(chat.Message));
                            await SendMessageToClientsAsync(chat);
                            break;
                    }
                }
            }
            catch (OperationCanceledException)
            {

            }
            catch (ObjectDisposedException)
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

        public async Task SendMessageToClientsAsync(ChatMessage chat)
        {
            switch (chat.MessageType)
            {
                case MessageType.Chat:
                    app.AppendConsoleText(DisplayFormat.FormatMessage($"[{chat.SenderName}]: {chat.Message}"));
                    break;
                case MessageType.Join:
                case MessageType.Leave:
                case MessageType.UsernameChange:
                    app.AppendConsoleText(DisplayFormat.FormatMessage(chat.Message));
                    break;
            }

            BluetoothClient[] clientCopy = GetClients();

            // Do all message sends concurrently, in case a client is slow
            IEnumerable<Task> tasks = clientCopy.Select(async client =>
            {
                try
                {
                    await ChatProtocol.SendAsync(client.GetStream(), chat);
                }
                catch {}
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
