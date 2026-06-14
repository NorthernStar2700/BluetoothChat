using BluetoothChat.Constants;
using BluetoothChat.Enums;
using BluetoothChat.Models;
using BluetoothChat.UI;
using BluetoothChat.Utilities;
using InTheHand.Net;
using InTheHand.Net.Sockets;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BluetoothChat.Functions
{
    public class AppClient
    {
        public BluetoothClient Client { get; private set; }
        public bool IsConnected { get; private set; }

        private readonly FrmBluetoothChat app;
        private CancellationTokenSource cancelToken;

        public AppClient(FrmBluetoothChat app)
        {
            this.app = app;
        }

        public async Task AttemptConnection()
        {
            app.AppendConsoleText(DisplayFormat.FormatMessage(app.GetInputText()));
            BluetoothAddress address;
            try
            {
                address = BluetoothAddress.Parse(app.GetInputText());
                app.SetSendButtonEnabled(false);
            }
            catch (Exception)
            {
                app.AppendConsoleText(DisplayFormat.FormatMessage("Incorrect format for Bluetooth address"));
                app.AppendConsoleText(Messages.BluetoothPrompt);
                app.SetSendButtonEnabled(true);
                return;
            }

            try
            {
                Client = new BluetoothClient();
                app.AppendConsoleText(DisplayFormat.FormatMessage("Connecting to server"));
                await Task.Run(() => Client.Connect(address, Messages.Guid));
                IsConnected = true;
            }
            catch (Exception ex)
            {
                Client.Dispose();
                app.AppendConsoleText(DisplayFormat.FormatMessage($"Error connecting to server: {ex.Message}"));
                app.AppendConsoleText(DisplayFormat.FormatMessage(Messages.BluetoothPrompt));
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
                app.AppendConsoleText(DisplayFormat.FormatMessage($"Error reading messages from server: {ex.Message}"));
                app.AppendConsoleText(DisplayFormat.FormatMessage(Messages.BluetoothPrompt));
                IsConnected = false;
                return;
            }
        }


        public async Task SendMessageToServer(ChatMessage message)
        {
            if (!IsConnected || Client == null)
            {
                return;
            }

            try
            {
                await ChatProtocol.SendAsync(Client.GetStream(), message);
            }
            catch (Exception e)
            {
                app.AppendConsoleText(DisplayFormat.FormatMessage($"Unable to broadcast message: {e.Message}"));
            }
        }

        private async Task ReadMessagesFromServer(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (!IsConnected)
                        return;

                    ChatMessage response = await ChatProtocol.ReadAsync(Client.GetStream());

                    switch (response.MessageType) 
                    {
                        case MessageType.Chat:
                            string isHost = response.IsHost ? "[HOST] " : string.Empty;
                            string message = $"{isHost}[{response.SenderName}]: {response.Message}";
                            app.AppendConsoleText(DisplayFormat.FormatMessage(message));
                            break;
                        case MessageType.Join:
                            AppAccount joinAcc = new AppAccount()
                            {
                                Name = response.SenderName,
                                AccountId = response.SenderId,
                            };
                            app.AddChatMember(joinAcc);
                            app.AppendConsoleText(DisplayFormat.FormatMessage(response.Message));
                            break;
                        case MessageType.Leave:
                            AppAccount leaveAcc = new AppAccount()
                            {
                                Name = response.SenderName,
                                AccountId = response.SenderId,
                            };
                            app.RemoveChatMember(leaveAcc);
                            app.AppendConsoleText(DisplayFormat.FormatMessage(response.Message));
                            break;
                        case MessageType.UsernameChange:
                            AppAccount updateAcc = new AppAccount()
                            {
                                Name = response.SenderName,
                                AccountId = response.SenderId,
                            };
                            app.UpdateChatMember(updateAcc);
                            app.AppendConsoleText(DisplayFormat.FormatMessage(response.Message));
                            break;
                    }
                }
                catch (Exception)
                {
                    app.ResetUI();
                    Client.Close();
                    IsConnected = false;
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
                app.AppendConsoleText(DisplayFormat.FormatMessage($"Error sending welcome message to server: {ex.Message}"));
                app.AppendConsoleText(DisplayFormat.FormatMessage(Messages.BluetoothPrompt));
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
                    Client.Close();
                    IsConnected = false;
                }
            }
        }
    }
}
