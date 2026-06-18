using BluetoothChat.Constants;
using BluetoothChat.Enums;
using BluetoothChat.Models;
using BluetoothChat.UI;
using BluetoothChat.Utilities;
using InTheHand.Net.Sockets;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace BluetoothChat.Functions
{
    public class AppServer
    {
        public bool IsRunning { get; private set; }

        private readonly FrmBluetoothChat app;
        private readonly object sessionLock = new object();
        private readonly List<ClientSession> sessions = new List<ClientSession>();
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

            ClientSession[] sessionCopy;
            lock (sessionLock)
            {
                sessionCopy = sessions.ToArray();
                sessions.Clear();
            }

            foreach (ClientSession session in sessionCopy)
            {
                try
                {
                    session.Client.Dispose();
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
                    chat = await AdjustChatMessage(client, chat);
                    await SendMessageToClientsAsync(chat);
                }
            }
            catch (OperationCanceledException)
            {
                RemoveClientSession(client);
            }
            catch (IOException)
            {
                RemoveClientSession(client);
            }
            catch (SocketException)
            {
                RemoveClientSession(client);
            }
            catch (Exception e)
            {
                app.AppendConsoleText(DisplayFormat.FormatConsoleMessage($"Client error: {e.Message}"));
            }
            finally
            {
                RemoveClientSession(client);
            }
        }

        public async Task SendMessageToClientsAsync(ChatMessage chat)
        {
            List<ClientSession> sessionCopy = GetClientSessions();

            // Do all message sends concurrently, in case a client is slow
            IEnumerable<Task> tasks = sessionCopy.Select(async session =>
            {
                try
                {
                    await session.Lock.WaitAsync();

                    try
                    {
                        await ChatProtocol.SendAsync(session.Stream, chat);
                    }
                    catch (IOException)
                    {
                        RemoveClientSession(session);
                    }
                    catch (ObjectDisposedException)
                    {
                        RemoveClientSession(session);
                    }
                    catch (SocketException)
                    {
                        RemoveClientSession(session);
                    }
                    finally
                    {
                        session.Lock.Release();
                    }
                }
                catch (Exception)
                {
                    RemoveClientSession(session);
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

        public async Task<ChatMessage> AdjustChatMessage(BluetoothClient client, ChatMessage message)
        {
            // Any modifications or displays before passing message to clients
            switch (message.MessageType)
            {
                case MessageType.Chat:
                    app.AppendConsoleText(DisplayFormat.FormatConsoleMessage($"[{message.SenderName}]: {message.Content}"));
                    break;
                case MessageType.Join:
                    message.Content = $">> [{message.SenderName}] has joined the server";
                    AddClientSession(client, message.SenderName, message.SenderId);

                    await SendMemberListToClientsAsync(GetAppAccounts(), app.Account);
                    app.AppendConsoleText(DisplayFormat.FormatConsoleMessage(message.Content));
                    break;
                case MessageType.Leave:
                    message.Content = $">> [{message.SenderName}] has left the server";
                    RemoveClientSession(message.SenderId);

                    await SendMemberListToClientsAsync(GetAppAccounts(), app.Account);
                    app.AppendConsoleText(DisplayFormat.FormatConsoleMessage(message.Content));
                    break;
                case MessageType.UsernameChange:
                    UpdateAccountName(message.SenderName, message.SenderId);

                    await SendMemberListToClientsAsync(GetAppAccounts(), app.Account);
                    app.AppendConsoleText(DisplayFormat.FormatConsoleMessage(message.Content));
                    break;
                case MessageType.ServerMessage:
                    app.AppendConsoleText(DisplayFormat.FormatConsoleMessage($"[HOST] [{message.SenderName}]: {message.Content}"));
                    break;
                case MessageType.MemberList:
                    List<AppAccount> accounts = GetAppAccounts();
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

                // MemberList sends a message to all ClientSessions
                chat = await AdjustChatMessage(null, chat);
                await SendMessageToClientsAsync(chat);
            }
            catch (Exception e)
            {
                app.AppendConsoleText($"[ERROR] Unable to send member list to clients: {e.Message}");
            }
        }

        private List<ClientSession> GetClientSessions()
        {
            lock (sessionLock)
            {
                return sessions;
            }
        }

        private List<AppAccount> GetAppAccounts()
        {
            lock (sessionLock)
            {
                List<AppAccount> accounts = new List<AppAccount>
                {
                    new AppAccount()
                    {
                        Name = app.Account.Name,
                        AccountId = app.Account.AccountId
                    }
                };
                accounts.AddRange(sessions
                    .Where(ses => ses.Account != null)
                    .Select(ses => ses.Account)
                    .ToList());
                return accounts;
            }
        }

        private void AddClientSession(BluetoothClient client, string name, string accountId)
        {
            ClientSession session = new ClientSession()
            {
                Account = new AppAccount()
                {
                    Name = name,
                    AccountId = accountId
                },
                Client = client,
                Stream = client.GetStream()
            };

            lock (sessionLock)
            {
                sessions.Add(session);
            }
        }

        private void RemoveClientSession(ClientSession session)
        {
            lock (sessionLock)
            {
                sessions.Remove(session);
            }
        }

        private void RemoveClientSession(BluetoothClient client)
        {
            ClientSession session = sessions.FirstOrDefault(ses => ses.Client == client);
            if (session == null)
            {
                return;
            }

            lock (sessionLock)
            {
                sessions.Remove(session);
            }
        }

        private void RemoveClientSession(string accountId)
        {
            ClientSession session = sessions.FirstOrDefault(ses => ses.Account.AccountId == accountId);
            if (session == null)
            {
                return;
            }

            lock (sessionLock)
            {
                sessions.Remove(session);
            }
        }
        
        private void UpdateAccountName(string name, string accountId)
        {
            ClientSession foundAccount = sessions.FirstOrDefault(
                                ses => (ses.Account.AccountId == accountId));
            if (foundAccount == null)
            {
                return;
            }

            foundAccount.Account.Name = name;
        }
    }
}