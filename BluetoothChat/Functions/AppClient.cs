using BluetoothChat.Constants;
using BluetoothChat.Enums;
using BluetoothChat.Models;
using BluetoothChat.UI;
using BluetoothChat.Utilities;
using InTheHand.Net;
using InTheHand.Net.Sockets;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BluetoothChat.Functions
{
    public class AppClient
    {
        public bool IsConnected { get; private set; }

        private readonly FrmBluetoothChat app;
        private ClientSession session;
        private CancellationTokenSource cancelToken;

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
                session = new ClientSession
                {
                    Account = app.Account,
                    Client = new BluetoothClient()
                };
                app.AppendConsoleText(DisplayFormat.FormatConsoleMessage("Connecting to server."));
                await Task.Run(() =>
                {
                    session.Client.Connect(address, BluetoothConstants.ServiceGuid);
                    session.Stream = session.Client.GetStream();
                });
                IsConnected = true;
            }
            catch (Exception ex)
            {
                session?.Dispose();
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
            if (!IsConnected || session == null || session.Client == null || session.Stream == null)
            {
                return;
            }

            bool lockActivated = false;
            try
            {
                await session.SendLock.WaitAsync();
                lockActivated = true;

                await ChatProtocol.SendAsync(session.Stream, message);
            }
            catch (Exception e)
            {
                app.AppendConsoleText(DisplayFormat.FormatConsoleMessage($"[ERROR] Unable to broadcast message: {e.Message}."));
            }
            finally
            {
                if (lockActivated)
                {
                    session.SendLock.Release();
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

                    ChatMessage response = await ChatProtocol.ReadAsync(session.Stream);
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
                catch (Exception)
                {
                    app.ResetUI();
                    session?.Dispose();
                    session = null;
                    IsConnected = false;
                    return;
                }
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
                session?.Dispose();
                session = null;
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
                    session?.Dispose();
                    session = null;
                    IsConnected = false;
                }
            }
        }
    }
}
