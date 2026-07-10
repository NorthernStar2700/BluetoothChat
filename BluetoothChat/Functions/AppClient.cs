using BluetoothChat.Constants;
using BluetoothChat.Enums;
using BluetoothChat.Models;
using BluetoothChat.UI;
using BluetoothChat.Utilities;
using InTheHand.Net;
using InTheHand.Net.Sockets;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BluetoothChat.Functions
{
    public class AppClient
    {
        public bool IsConnected { get; private set; }

        private readonly FrmBluetoothChat app;
        private BluetoothClient client;
        private NetworkStream stream;
        private CancellationTokenSource cancelToken;
        private SemaphoreSlim sendLock = new SemaphoreSlim(1, 1);
        private SessionKeys sessionKeys;
        private string serverPublicKey;
        private bool isSecure = false;


        public AppClient(FrmBluetoothChat app)
        {
            this.app = app;
        }

        public async Task AttemptConnection()
        {
            app.SetSendButtonEnabled(false);
            bool addressParsed = BluetoothAddress.TryParse(app.GetInputText(), out BluetoothAddress address);
            if (addressParsed)
            {
                app.AppendConsoleText(DisplayFormat.FormatConsoleMessage(address.ToString()));
            }
            else
            {
                app.AppendConsoleText(DisplayFormat.FormatConsoleMessage(app.GetInputText()));
                app.AppendConsoleText(DisplayFormat.FormatConsoleMessage("Incorrect format for Bluetooth address."));
                app.AppendConsoleText(UIMessages.BluetoothPrompt);
                app.SetSendButtonEnabled(true);
                return;
            }

            try
            {
                client = new BluetoothClient();
                sendLock = new SemaphoreSlim(1, 1);
                app.AppendConsoleText(DisplayFormat.FormatConsoleMessage("Connecting to server."));
                await Task.Run(() =>
                {
                    client.Connect(address, BluetoothConstants.ServiceGuid);
                });
                stream = client.GetStream();
                IsConnected = true;
            }
            catch (Exception ex)
            {
                Dispose();
                app.AppendConsoleText(DisplayFormat.FormatConsoleMessage($"[ERROR] Cannot connect to server: {ex.Message}."));
                app.AppendConsoleText(DisplayFormat.FormatConsoleMessage(UIMessages.BluetoothPrompt));
                app.SetSendButtonEnabled(true);
                app.SetConnectedCheckbox(false);
                return;
            }
        }

        public async Task StartReadingMessagesAsync()
        {
            app.ClearConsoleText();
            app.SetSendButtonEnabled(true);
            app.SetConnectedCheckbox(true);

            cancelToken?.Cancel();
            cancelToken?.Dispose();
            cancelToken = new CancellationTokenSource();

            try
            {
                await ReadMessagesFromServer(cancelToken.Token);
            }
            catch (Exception ex)
            {
                app.AppendConsoleText(DisplayFormat.FormatConsoleMessage($"[ERROR] Cannot read messages from server: {ex.Message}."));
                app.AppendConsoleText(DisplayFormat.FormatConsoleMessage(UIMessages.BluetoothPrompt));
                IsConnected = false;
                return;
            }
        }


        public async Task SendMessageToServer(ChatMessage message)
        {
            if (!IsConnected || client == null || stream == null)
            {
                return;
            }

            bool lockActivated = false;
            try
            {
                await sendLock.WaitAsync();
                lockActivated = true;

                if (!isSecure)
                {
                    await ChatProtocol.SendUnencryptedAsync(stream, message);
                }
                else
                {
                    await ChatProtocol.SendAsync(stream, message, sessionKeys.AesKey);
                }
            }
            catch (Exception e)
            {
                app.AppendConsoleText(DisplayFormat.FormatConsoleMessage($"[ERROR] Unable to broadcast message: {e.Message}."));
            }
            finally
            {
                if (lockActivated)
                {
                    sendLock.Release();
                }
            }
        }

        private async Task ReadMessagesFromServer(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (!IsConnected)
                    {
                        return;
                    }

                    if (!isSecure)
                    {
                        ChatMessage response = await ChatProtocol.ReadUnencryptedAsync(stream);
                        switch (response.MessageType)
                        {
                            case MessageType.ServerPublicKey:
                                serverPublicKey = response.Content;
                                CreateSessionKeys();
                                await SendAesKey();
                                break;
                            case MessageType.HandshakeComplete:
                                isSecure = true;
                                await SendJoinMessage();
                                break;
                        }
                    }
                    else
                    {
                        ChatMessage response = await ChatProtocol.ReadAsync(stream, sessionKeys.AesKey);
                        switch (response.MessageType)
                        {
                            case MessageType.Chat:
                                app.AppendConsoleText(DisplayFormat.FormatConsoleMessage($"[{response.SenderName}]: {response.Content}"));
                                break;
                            case MessageType.Join:
                            case MessageType.Leave:
                            case MessageType.UsernameChange:
                                app.AppendConsoleText(DisplayFormat.FormatConsoleMessage(response.Content));
                                break;
                            case MessageType.ServerMessage:
                                app.AppendConsoleText(DisplayFormat.FormatConsoleMessage($"[HOST] [{response.SenderName}]: {response.Content}"));
                                break;
                            case MessageType.MemberList:
                                try
                                {
                                    List<AppAccount> accounts = ObjectConverter.DeserializeAccountMembers(response.Content);
                                    app.ReplaceChatMembers(accounts);
                                }
                                catch (Exception e)
                                {
                                    app.AppendConsoleText($"[ERROR] Message read error: {e.Message}.");
                                }
                                break;
                        }
                    }
                }
                catch (Exception e)
                {
                    app.ResetUI();
                    app.AppendConsoleText(e.Message);
                    Dispose();
                    IsConnected = false;
                    return;
                }
            }
        }

        public async Task SendHandshakeRequest()
        {
            try
            {
                isSecure = false;
                ChatMessage message = new ChatMessage()
                {
                    MessageType = MessageType.HandshakeRequested,
                    SenderName = app.Account.Name,
                    SenderId = app.Account.AccountId
                };

                await SendMessageToServer(message);
                app.ClearInputText();
            }
            catch (Exception ex)
            {
                app.AppendConsoleText(DisplayFormat.FormatConsoleMessage($"[ERROR] Cannot send handshake request to server: {ex.Message}."));
                app.AppendConsoleText(DisplayFormat.FormatConsoleMessage(UIMessages.BluetoothPrompt));
                Dispose();
                IsConnected = false;
                return;
            }
        }

        public async Task SendJoinMessage()
        {
            try
            {
                ChatMessage message = new ChatMessage()
                {
                    MessageType = MessageType.Join,
                    SenderName = app.Account.Name,
                    SenderId = app.Account.AccountId
                };

                await SendMessageToServer(message);
                app.ClearInputText();
            }
            catch (Exception ex)
            {
                app.AppendConsoleText(DisplayFormat.FormatConsoleMessage($"[ERROR] Cannot send join message to server: {ex.Message}."));
                app.AppendConsoleText(DisplayFormat.FormatConsoleMessage(UIMessages.BluetoothPrompt));
                Dispose();
                IsConnected = false;
                return;
            }
        }

        public async Task SendLeaveMessage()
        {
            if (IsConnected)
            {
                // Attempt to send a message to the server indicating the client is leaving
                // Disconnect even if the message fails
                try
                {
                    ChatMessage message = new ChatMessage()
                    {
                        MessageType = MessageType.Leave,
                        SenderName = app.Account.Name,
                        SenderId = app.Account.AccountId
                    };

                    await SendMessageToServer(message);
                }
                finally
                {
                    Dispose();
                    IsConnected = false;
                }
            }
        }

        private async Task SendAesKey()
        {
            if (sessionKeys == null)
            {
                return;
            }

            // Convery session key object to JSON, then a byte array
            string sessionKeyJson = ObjectConverter.SerializeSessionKeys(sessionKeys);
            byte[] sessionKeyData = Encoding.UTF8.GetBytes(sessionKeyJson);
            byte[] encryptedKeyData;

            // Encrypt the byte array using the servers public key
            using (RSACng rsa = new RSACng())
            {
                rsa.FromXmlString(serverPublicKey);
                encryptedKeyData = rsa.Encrypt(sessionKeyData, RSAEncryptionPadding.OaepSHA256);
            }

            string encryptedSessionKeyData = Convert.ToBase64String(encryptedKeyData);

            // Convert the encrypted byte array to a string to send in a message

            ChatMessage message = new ChatMessage()
            {
                MessageType = MessageType.ClientAesKey,
                SenderName = app.Account.Name,
                SenderId = app.Account.AccountId,
                Content = encryptedSessionKeyData
            };

            await SendMessageToServer(message);
        }

        private void CreateSessionKeys()
        {
            sessionKeys = new SessionKeys()
            {
                AesKey = CryptoKeyGenerator.GenerateAesKey()
            };
        }

        private void Dispose()
        {
            stream?.Dispose();
            client?.Close();
            client?.Dispose();
            sendLock?.Dispose();

            stream = null;
            client = null;
            sendLock = null;
            isSecure = false;
        }
    }
}
