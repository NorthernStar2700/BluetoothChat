using BluetoothChat.Constants;
using BluetoothChat.Enums;
using BluetoothChat.Models;
using BluetoothChat.UI;
using BluetoothChat.Utilities;
using InTheHand.Net.Sockets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
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
            app.SetConnectedCheckbox(true);
            IsRunning = true;
            serverTask = Task.Run(() => HostSession(cancelToken.Token));
        }

        public void Stop()
        {
            IsRunning = false;
            app.SetConnectedCheckbox(false);

            try
            {
                cancelToken?.Cancel();
                cancelToken?.Dispose();
            }
            catch (Exception e) 
            {
                app.AppendConsoleText($"{e.Message}");
            }

            try
            {
                listener?.Stop();
                listener?.Dispose();
            }
            catch (Exception e) 
            { 
                app.AppendConsoleText($"{e.Message}"); 
            }

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
                catch (Exception e) 
                {
                    app.AppendConsoleText($"{e.Message}");
                }
            }
        }

        private async Task HostSession(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                BluetoothClient client;
                try
                {
                    client = await Task.Run(() => listener.AcceptBluetoothClient());

                    NetworkStream stream = client.GetStream();

                    AddClient(client);
                    _ = Task.Run(() => HandleClientAsync(client, stream, ct));

                }
                catch (IOException e) when (e.InnerException is SocketException socketEx && 
                    socketEx.SocketErrorCode == SocketError.Interrupted)
                {
                    return;
                }
                catch (ObjectDisposedException)
                {
                    return;
                }
                catch (SocketException e) when (e.SocketErrorCode == SocketError.Interrupted)
                {
                    return;
                }
                catch (Exception e)
                {
                    app.AppendConsoleText(DisplayFormat.FormatMessage($"Client could not be connected: {e.Message}"));
                    return;
                }
            }
        }

        private async Task HandleClientAsync(BluetoothClient client, NetworkStream stream, CancellationToken ct)
        {
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    ChatMessage chat = await ChatProtocol.ReadAsync(stream);
                    await SendMessageToClientsAsync(chat);

                    switch (chat.MessageType)
                    {
                        case MessageType.Join:
                            AppAccount joinAcc = new AppAccount()
                            {
                                Name = chat.SenderName,
                                AccountId = chat.SenderId,
                            };
                            app.AddChatMember(joinAcc);
                            break;
                        case MessageType.Leave:
                            AppAccount leaveAcc = new AppAccount()
                            {
                                Name = chat.SenderName,
                                AccountId = chat.SenderId,
                            };
                            app.RemoveChatMember(leaveAcc);
                            break;
                        case MessageType.UsernameChange:
                            AppAccount updateAcc = new AppAccount()
                            {
                                Name = chat.SenderName,
                                AccountId = chat.SenderId,
                            };
                            app.UpdateChatMember(updateAcc);
                            break;
                    }
                }
            }
            catch (OperationCanceledException)
            {

            }
            catch (IOException)
            {

            }
            catch (SocketException)
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
            string message = string.Empty;
            switch (chat.MessageType)
            {
                case MessageType.Chat:
                    string isHost = chat.IsHost ? "[HOST] " : string.Empty;
                    app.AppendConsoleText(DisplayFormat.FormatMessage($"{isHost}[{chat.SenderName}]: {chat.Message}"));
                    break;
                case MessageType.Join:
                    message = $">> [{chat.SenderName}] joined the server";
                    app.AppendConsoleText(DisplayFormat.FormatMessage(message));
                    break;
                case MessageType.Leave:
                    message = $">> [{chat.SenderName}] left the server";
                    app.AppendConsoleText(DisplayFormat.FormatMessage(message));
                    break;
                case MessageType.UsernameChange:
                    app.AppendConsoleText(DisplayFormat.FormatMessage(chat.Message));
                    break;
            }

            // Clients send the indicator, server sends the message to all other clients
            if (!string.IsNullOrWhiteSpace(message))
            {
                chat.Message = message;
            }

            BluetoothClient[] clientCopy = GetClients();

            // Do all message sends concurrently, in case a client is slow
            IEnumerable<Task> tasks = clientCopy.Select(async client =>
            {
                try
                {
                    await ChatProtocol.SendAsync(client.GetStream(), chat);
                }
                catch (Exception e)
                {
                    app.AppendConsoleText($"{e.Message}");
                }
            });

            try
            {
                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                app.AppendConsoleText($"{e.Message}");
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
                client?.Dispose();
            }
            catch (NullReferenceException)
            {

            }
            catch (Exception)
            {

            }
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
