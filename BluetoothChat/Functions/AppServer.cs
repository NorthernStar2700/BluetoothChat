using BluetoothChat.Constants;
using BluetoothChat.Enums;
using BluetoothChat.Models;
using BluetoothChat.UI;
using BluetoothChat.Utilities;
using InTheHand.Net.Sockets;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Collections.Generic;
using System.IO;
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

        private readonly object sessionLock = new object();
        private readonly List<ClientSession> sessions = new List<ClientSession>();
        private readonly FrmBluetoothChat app;
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

            try
            {
                app.AppendConsoleText(DisplayFormat.FormatConsoleMessage("Starting server..."));
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

            app.AppendConsoleText(DisplayFormat.FormatConsoleMessage("Creating RSA keys..."));
            RsaProtocol.CreateRsaKeys();

            app.AppendConsoleText(DisplayFormat.FormatConsoleMessage("Creating session..."));
            Task.Run(() => HostSession(cancelToken.Token));

            IsRunning = true;
            app.AddChatMember(app.Account);
            app.AppendConsoleText(DisplayFormat.FormatConsoleMessage("Waiting for clients."));
            app.AppendConsoleText(DisplayFormat.FormatConsoleMessage(">> Make sure devices are connected to you via Bluetooth so devices can connect to your server."));
            app.SetConnectedCheckbox(true);
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
                catch (IOException)
                {
                    return;
                }
                catch (ObjectDisposedException)
                {
                    return;
                }
                catch (SocketException e) when (e.SocketErrorCode == SocketError.Interrupted)
                {
                    continue;
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
                    // TODO: Handle messages meant for handshakes (these would not be encrypted)
                    ClientSession session = FindClientSession(client);
                    ChatMessage chat;

                    if (client != null && (session == null || !session.IsSecure))
                    {
                        chat = await ChatProtocol.ReadUnencryptedAsync(stream);

                        bool validMessage = IsClientMessageHandshakeRelated(chat);
                        if (!validMessage)
                        {
                            throw new Exception("Client is not secure");
                        }

                        // Client-server handshake messages should only be processed if the client is not secure
                        if (chat.MessageType == MessageType.HandshakeRequested)
                        {
                            await SendPublicKeyToClient(stream);
                        }
                        else if (chat.MessageType == MessageType.ClientAesKey)
                        {
                            // TODO: Create Session with AES key
                            ModifyClientSessionList(client, chat);
                            await SendHandshakeCompleteToClient(stream);
                        }

                        continue;
                    }
                    else
                    {
                        chat = await ChatProtocol.ReadAsync(stream, session.AesKey);
                    }

                    if (session == null || !session.IsSecure)
                    {
                        return;
                    }

                    bool isValidMessage = IsClientMessageAllowed(chat);
                    if (!isValidMessage)
                    {
                        throw new Exception("Clients are not allowed to send handshake or server-sided messages");
                    }

                    bool clientLeft = false;
                    bool listAdjusted = ModifyClientSessionList(client, chat);

                    string accountName = session.Account.Name;
                    string accountId = session.Account.AccountId;

                    if (chat.MessageType == MessageType.Join)
                    {
                        chat = new ChatMessage()
                        {
                            MessageType = MessageType.Join,
                            SenderName = accountName,
                            SenderId = accountId,
                            Content = $">> [{accountName}] has joined the server."
                        };
                    }
                    else if (chat.MessageType == MessageType.Leave)
                    {
                        chat = new ChatMessage()
                        {
                            MessageType = MessageType.Leave,
                            SenderName = accountName,
                            SenderId = accountId,
                            Content = $">> [{accountName}] has left the server."
                        };

                        clientLeft = true;
                    }

                    await ProcessChatMessage(chat);

                    if (listAdjusted)
                    {
                        await SendMemberListToClients();
                    }

                    // Exit the loop, client is no longer connected
                    if (clientLeft)
                    {
                        break;
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

            // Send message to only secured clients
            ClientSession[] sessionCopy = GetClientSessions().Where(ses => ses.IsSecure).ToArray();

            // Do all message sends concurrently in case a client is slow
            IEnumerable<Task> tasks = sessionCopy.Select(async session =>
            {
                bool failedSend = false;
                bool lockActivated = false;
                try
                {
                    await session.SendLock.WaitAsync();
                    lockActivated = true;

                    await ChatProtocol.SendAsync(session.Stream, chat, session.AesKey);
                }
                catch (Exception)
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

        public async Task SendPublicKeyToClient(NetworkStream stream)
        {
            ChatMessage message = new ChatMessage()
            {
                MessageType = MessageType.ServerPublicKey,
                SenderName = app.Account.Name,
                SenderId = app.Account.AccountId,
                Content = RsaProtocol.GetPublicKeyString()
            };

            await ChatProtocol.SendUnencryptedAsync(stream, message);
        }

        public async Task SendHandshakeCompleteToClient(NetworkStream stream)
        {
            ChatMessage message = new ChatMessage()
            {
                MessageType = MessageType.HandshakeComplete,
                SenderName = app.Account.Name,
                SenderId = app.Account.AccountId
            };

            await ChatProtocol.SendUnencryptedAsync(stream, message);
        }

        private bool IsClientMessageAllowed(ChatMessage message)
        {
            switch (message.MessageType)
            {
                case MessageType.Chat:
                case MessageType.Join:
                case MessageType.Leave:
                case MessageType.UsernameChange:
                    return true;
                default:
                    return false;
            }
        }

        private bool IsClientMessageHandshakeRelated(ChatMessage message)
        {
            switch (message.MessageType)
            {
                case MessageType.HandshakeRequested:
                case MessageType.ClientAesKey:
                    return true;
                default:
                    return false;
            }
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
                    case MessageType.ClientAesKey:
                        if (session == null)
                        {
                            byte[] encryptedKeyData = Convert.FromBase64String(message.Content);
                            byte[] decryptedKeyData;

                            // Decrypt the byte array using the servers private key
                            IAsymmetricBlockCipher cipher = new OaepEncoding(new RsaEngine(), new Sha256Digest(), null);
                            RsaKeyParameters rsaPrivateKey = RsaProtocol.GetPrivateKey();

                            cipher.Init(false, rsaPrivateKey);
                            decryptedKeyData = cipher.ProcessBlock(encryptedKeyData, 0, encryptedKeyData.Length);

                            // Convert the decrypted SessionKeys object to a string
                            string keyData = Encoding.UTF8.GetString(decryptedKeyData);
                            SessionKeys keys = ObjectConverter.DeserializeSessionKeys(keyData);

                            ClientSession newSession = new ClientSession()
                            {
                                Account = new AppAccount()
                                {
                                    Name = NameSanitizer.Sanitize(message.SenderName),
                                    AccountId = message.SenderId
                                },
                                Client = client,
                                Stream = client.GetStream(),
                                AesKey = keys.AesKey,
                                IsSecure = true
                            };

                            sessions.Add(newSession);
                        }
                        break;
                    case MessageType.Join:
                        // Client was already initialized when they sent an AES key to server
                        if (session != null)
                        {
                            session.IsSecure = true;
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
                            // Use the old name + new name here
                            string oldName = session.Account.Name;
                            string newName = NameSanitizer.Sanitize(message.SenderName);
                            message.Content = $">> [{oldName}] has changed their name to [{newName}].";
                            session.Account.Name = newName;
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