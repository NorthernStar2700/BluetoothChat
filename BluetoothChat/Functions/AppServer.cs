using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using BluetoothChat.Constants;
using BluetoothChat.Enums;
using BluetoothChat.Models;
using BluetoothChat.UI;
using BluetoothChat.Utilities;
using InTheHand.Net.Sockets;
using Newtonsoft.Json;

namespace BluetoothChat.Functions
{
    public class AppServer
    {
        public bool IsRunning { get; private set; }

        private readonly FrmBluetoothChat app;
        private readonly object clientLock = new object();
        private readonly List<BluetoothClient> clients = new List<BluetoothClient>();
        private readonly List<AppAccount> accounts = new List<AppAccount>();
        private BluetoothListener listener;
        private CancellationTokenSource cancelToken;
        private Task sessionTask;

        public AppServer(FrmBluetoothChat app)
        {
            this.app = app;
        }

        public void Start()
        {

            if (IsRunning)
            {
                app.AppendConsoleText(DisplayFormat.FormatConsoleMessage("Server is already active. You cannot run another server"));
                return;
            }

            cancelToken = new CancellationTokenSource();
            listener = new BluetoothListener(BluetoothConstants.ServiceGuid);

            app.AppendConsoleText(DisplayFormat.FormatConsoleMessage("Starting server..."));
            try
            {
                listener.Start();
                app.Account.InitializeAccountId();
            }
            catch (Exception e)
            {
                app.SetAppMode(AppMode.Inactive);
                app.AppendConsoleText(DisplayFormat.FormatConsoleMessage($"[ERROR] Unable to start server: {e.Message}"));
                app.AppendConsoleText(DisplayFormat.FormatConsoleMessage(UIMessages.ConsolePrompt));
                return;
            }

            app.AppendConsoleText(DisplayFormat.FormatConsoleMessage("Waiting for clients"));
            app.AppendConsoleText(DisplayFormat.FormatConsoleMessage("Make sure devices are connected to you via Bluetooth pairing for server connections to work"));
            app.SetConnectedCheckbox(true);
            IsRunning = true;
            app.AddChatMember(app.Account);
            accounts.Add(app.Account);
            sessionTask = Task.Run(() => HostSession(cancelToken.Token));
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
                catch (Exception) 
                {

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
                    app.AppendConsoleText(DisplayFormat.FormatConsoleMessage($"[ERROR] Client could not be connected: {e.Message}"));
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
                    chat = await AdjustChatMessage(chat);
                    await SendMessageToClientsAsync(chat);
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
                app.AppendConsoleText(DisplayFormat.FormatConsoleMessage($"Client error: {e.Message}"));
            }
            finally
            {
                RemoveClient(client);
            }
        }

        public async Task SendMessageToClientsAsync(ChatMessage chat)
        {
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
                    app.AppendConsoleText($"[ERROR] Client send error: {e.Message}");
                }
            });

            try
            {
                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                app.AppendConsoleText($"[ERROR] Task error: {e.Message}");
            }
        }

        public async Task<ChatMessage> AdjustChatMessage(ChatMessage message)
        {
            // Any modifications or displays before passing message to clients
            switch (message.MessageType)
            {
                case MessageType.Chat:
                    app.AppendConsoleText(DisplayFormat.FormatConsoleMessage($"[{message.SenderName}]: {message.Content}"));
                    break;
                case MessageType.Join:
                    message.Content = $">> [{message.SenderName}] has joined the server";
                    AddAccount(message.SenderName, message.SenderId);

                    await SendMemberListToClientsAsync(accounts, app.Account);
                    app.AppendConsoleText(DisplayFormat.FormatConsoleMessage(message.Content));
                    break;
                case MessageType.Leave:
                    message.Content = $">> [{message.SenderName}] has left the server";
                    RemoveAccount(message.SenderId);

                    await SendMemberListToClientsAsync(accounts, app.Account);
                    app.AppendConsoleText(DisplayFormat.FormatConsoleMessage(message.Content));
                    break;
                case MessageType.UsernameChange:
                    UpdateAccountName(message.SenderName, message.SenderId);

                    await SendMemberListToClientsAsync(accounts, app.Account);
                    app.AppendConsoleText(DisplayFormat.FormatConsoleMessage(message.Content));
                    break;
                case MessageType.ServerMessage:
                    app.AppendConsoleText(DisplayFormat.FormatConsoleMessage($"[HOST] [{message.SenderName}]: {message.Content}"));
                    break;
                case MessageType.MemberList:
                    message.Content = JsonConvert.SerializeObject(accounts);
                    app.RemoveChatMembers();
                    app.AddChatMembers(accounts);
                    break;
            }
            return message;
        }

        public async Task SendMemberListToClientsAsync(List<AppAccount> accounts, AppAccount serverAccount)
        {
            try
            {
                ChatMessage chat = new ChatMessage()
                {
                    MessageType = MessageType.MemberList,
                    SenderName = serverAccount.Name,
                    SenderId = serverAccount.AccountId,
                    Content = JsonConvert.SerializeObject(accounts)
                };

                chat = await AdjustChatMessage(chat);
                await SendMessageToClientsAsync(chat);
            }
            catch (Exception e)
            {
                app.AppendConsoleText($"[ERROR] Unable to send member list to clients: {e.Message}");
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

        private void AddAccount(string name, string accountId)
        {
            AppAccount account = new AppAccount()
            {
                Name = name,
                AccountId = accountId
            };
            accounts.Add(account);
        }

        private void RemoveAccount(string accountId)
        {
            AppAccount leaveAcc = accounts.FirstOrDefault(
                                acc => (acc.AccountId == accountId));
            if (leaveAcc != null)
            {
                accounts.Remove(leaveAcc);
            }
        }
        
        private void UpdateAccountName(string name, string accountId)
        {
            AppAccount foundAccount = accounts.FirstOrDefault(
                                acc => (acc.AccountId == accountId));
            if (foundAccount != null)
            {
                foundAccount.Name = name;
            }
        }
    }
}
