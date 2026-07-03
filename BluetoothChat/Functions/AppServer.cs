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

        public AppServer(FrmBluetoothChat app)
        {
            this.app = app;
        }

        public void Start()
        {

            if (IsRunning)
            {
                app.AppendConsoleText(DisplayFormat.FormatConsoleMessage("[ERROR] Server is already active. You cannot run another server."));
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
                app.AppendConsoleText(DisplayFormat.FormatConsoleMessage($"[ERROR] Unable to start server: {e.Message}."));
                app.AppendConsoleText(DisplayFormat.FormatConsoleMessage(UIMessages.ConsolePrompt));
                return;
            }

            app.AppendConsoleText(DisplayFormat.FormatConsoleMessage("Waiting for clients."));
            app.AppendConsoleText(DisplayFormat.FormatConsoleMessage("Make sure devices are connected to you via Bluetooth pairing for server connections to work."));
            app.SetConnectedCheckbox(true);
            IsRunning = true;
            app.AddChatMember(app.Account);
            Task.Run(() => HostSession(cancelToken.Token));
        }

        public void Stop()
        {
            IsRunning = false;
            app.SetConnectedCheckbox(false);

            try
            {
                cancelToken?.Cancel();
            }
            catch (Exception e) 
            {
                app.AppendConsoleText($"[ERROR] Cannot cancel token: {e.Message}.");
            }

            try
            {
                listener?.Stop();
                listener?.Dispose();
            }
            catch (Exception e) 
            { 
                app.AppendConsoleText($"[ERROR] Problem stopping and disposing server: {e.Message}."); 
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
                    session.Dispose();
                }
                catch (Exception) 
                {
                    // Ignore
                }
            }

            try
            {
                cancelToken?.Dispose();
            }
            catch (Exception)
            {
                // Ignore
            }
        }

        private async Task HostSession(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested && IsRunning)
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
                    app.AppendConsoleText(DisplayFormat.FormatConsoleMessage($"[ERROR] Client could not be connected: {e.Message}."));
                    continue;
                }
            }
        }

        private async Task HandleClientAsync(BluetoothClient client, NetworkStream stream, CancellationToken ct)
        {
            try
            {
                while (!ct.IsCancellationRequested && IsRunning)
                {
                    ChatMessage chat = await ChatProtocol.ReadAsync(stream);
                    bool isInvalidMessage = CheckInvalidChatMessage(chat);

                    if (!isInvalidMessage)
                    {
                        if (chat.MessageType == MessageType.Leave)
                        {
                            // Send message to all session accounts
                            ClientSession session = FindClientSession(client);
                            string accountName = string.Empty;
                            string accountId = string.Empty;

                            if (session != null)
                            {
                                accountName = session.Account.Name;
                                accountId = session.Account.AccountId;
                            }

                            ChatMessage message = new ChatMessage()
                            {
                                MessageType = MessageType.Leave,
                                SenderName = accountName,
                                SenderId = accountId,
                                Content = $">> [{accountName}] has left the server."
                            };

                            await ProcessChatMessage(message);

                            bool listAdjusted = ModifyClientSessionList(client, chat);
                            if (listAdjusted)
                            {
                                await SendMemberListToClients();
                            }
                        }
                        else
                        {
                            bool listAdjusted = ModifyClientSessionList(client, chat);
                            if (listAdjusted)
                            {
                                await SendMemberListToClients();
                            }

                            ClientSession session = FindClientSession(client);
                            await ProcessChatMessage(session, chat);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Ignore
            }
            catch (IOException)
            {
                // Ignore
            }
            catch (SocketException)
            {
                // Ignore
            }
            catch (Exception)
            {
                // Ignore
            }
            finally
            {
                ClientSession session;
                bool sessionRemoved;

                // Here prevent modifications and do the removal directly
                // RemoveClientSession() has a lock in it
                lock (sessionLock)
                {
                    session = sessions.FirstOrDefault(ses => ses.Client == client);
                    sessionRemoved = session != null && sessions.Remove(session);
                }

                if (sessionRemoved)
                {
                    session?.Dispose();
                }
            }
        }

        public async Task SendMessageToClientsAsync(ChatMessage chat)
        {
            // Update the text or member list displayed to the host's UI
            UpdateServerUIFromMessage(chat);

            ClientSession[] sessionCopy = GetClientSessions();

            // Do all message sends concurrently in case a client is slow
            IEnumerable<Task> tasks = sessionCopy.Select(async session =>
            {
                bool failedSend = false;
                bool lockActivated = false;
                try
                {
                    await session.SendLock.WaitAsync();
                    lockActivated = true;

                    await ChatProtocol.SendAsync(session.Stream, chat);
                }
                catch (IOException)
                {
                    failedSend = true;
                }
                catch (ObjectDisposedException)
                {
                    failedSend = true;
                }
                catch (SocketException)
                {
                    failedSend = true;
                }
                finally
                {
                    if (lockActivated)
                    {
                        session.SendLock.Release();
                    }

                    if (failedSend)
                    {
                        RemoveClientSession(session);
                    }
                }
            });

            try
            {
                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                app.AppendConsoleText($"[ERROR] Task error: {e.Message}.");
            }
        }

        public async Task ProcessChatMessage(ChatMessage message)
        {
            await SendMessageToClientsAsync(message);
        }

        public async Task SendMemberListToClients()
        {
            List<AppAccount> accounts = GetAppAccounts();
            ChatMessage chat = new ChatMessage()
            {
                MessageType = MessageType.MemberList,
                SenderName = app.Account.Name,
                SenderId = app.Account.AccountId,
                Content = ObjectConverter.SerializeAccountMembers(accounts)
            };

            // MemberList sends a message to all ClientSessions
            await SendMessageToClientsAsync(chat);
        }

        public ChatMessage HandleServerUsernameChange(string name)
        {
            string oldName = app.Account.Name;
            name = NameSanitizer.Sanitize(name);
            app.Account.Name = name;

            ChatMessage message = new ChatMessage()
            {
                MessageType = MessageType.UsernameChange,
                SenderName = app.Account.Name,
                SenderId = app.Account.AccountId,
                Content = $">> [HOST] [{oldName}] has changed their name to [{name}]."
            };

            return message;
        }

        private async Task ProcessChatMessage(ClientSession session, ChatMessage message)
        {
            // Adjust the content of a message (if the user joins or leaves)
            message = PrepareChatMessage(session, message);

            // Sends the finalized message to the clients
            await SendMessageToClientsAsync(message);
        }

        private bool CheckInvalidChatMessage(ChatMessage message)
        {
            return message.MessageType == MessageType.ServerMessage ||
                message.MessageType == MessageType.MemberList;
        }

        private ChatMessage PrepareChatMessage(ClientSession session, ChatMessage message)
        {
            // Any modifications or displays before passing message to clients
            switch (message.MessageType)
            {
                case MessageType.Join:
                    message.Content = $">> [{session.Account.Name}] has joined the server.";
                    break;
                case MessageType.Leave:
                    message.Content = $">> [{session.Account.Name}] has left the server.";
                    break;
                case MessageType.UsernameChange:
                    message.Content = $">> [{session.Account.Name}] has changed their name to [{message.SenderName}].";
                    break;
            }
            return message;
        }

        private bool ModifyClientSessionList(BluetoothClient client, ChatMessage message)
        {
            lock (sessionLock)
            {
                bool listAdjusted = false;
                ClientSession session = sessions.FirstOrDefault(ses => ses.Client == client);

                // In case a user joins, leaves, or changes their name update the session list appropriately
                switch (message.MessageType)
                {
                    case MessageType.Join:
                        if (session == null)
                        {
                            ClientSession newSession = new ClientSession()
                            {
                                Account = new AppAccount()
                                {
                                    Name = NameSanitizer.Sanitize(message.SenderName),
                                    AccountId = message.SenderId
                                },
                                Client = client,
                                Stream = client.GetStream()
                            };

                            sessions.Add(newSession);
                            listAdjusted = true;
                        }
                        break;
                    case MessageType.Leave:
                        if (session != null)
                        {
                            RemoveClientSession(session);
                            listAdjusted = true;
                        }
                        break;
                    case MessageType.UsernameChange:
                        if (session != null)
                        {
                            session.Account.Name = NameSanitizer.Sanitize(message.SenderName);
                            listAdjusted = true;
                        }
                        break;
                }

                if (session != null)
                {
                    message.SenderName = session.Account.Name;
                    message.SenderId = session.Account.AccountId;
                }

                return listAdjusted;
            }
        }

        private void UpdateServerUIFromMessage(ChatMessage message)
        {
            // Displays a message to the host's UI or updates the member list
            switch (message.MessageType)
            {
                case MessageType.Chat:
                    app.AppendConsoleText(DisplayFormat.FormatConsoleMessage($"[{message.SenderName}]: {message.Content}"));
                    break;
                case MessageType.Join:
                case MessageType.Leave:
                case MessageType.UsernameChange:
                    app.AppendConsoleText(DisplayFormat.FormatConsoleMessage(message.Content));
                    List<AppAccount> accounts = GetAppAccounts();
                    app.ReplaceChatMembers(accounts);
                    break;
                case MessageType.ServerMessage:
                    app.AppendConsoleText(DisplayFormat.FormatConsoleMessage($"[HOST] [{message.SenderName}]: {message.Content}"));
                    break;
            }
        }

        private ClientSession[] GetClientSessions()
        {
            lock (sessionLock)
            {
                return sessions.ToArray();
            }
        }

        private List<AppAccount> GetAppAccounts()
        {
            lock (sessionLock)
            {
                // Host account is added in here
                List<AppAccount> accounts = new List<AppAccount>
                {
                    new AppAccount()
                    {
                        Name = app.Account.Name,
                        AccountId = app.Account.AccountId
                    }
                };

                // Add a new copy of AppAccount objects to send to clients
                // Avoid using the server's references
                foreach (ClientSession session in sessions)
                {
                    accounts.Add(new AppAccount()
                    {
                        Name = session.Account.Name,
                        AccountId = session.Account.AccountId
                    });
                }
                return accounts;
            }
        }

        private ClientSession FindClientSession(BluetoothClient client)
        {
            lock (sessionLock)
            {
                return sessions.FirstOrDefault(ses => ses.Client == client);
            }
        }

        private void RemoveClientSession(ClientSession session)
        {
            bool sessionRemoved;
            lock (sessionLock)
            {
                sessionRemoved = sessions.Remove(session);
            }

            if (sessionRemoved)
            {
                session.Dispose();
            }
        }
    }
}