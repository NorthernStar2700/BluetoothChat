using BluetoothChat.Constants;
using BluetoothChat.Enums;
using BluetoothChat.Models;
using BluetoothChat.Utilities;
using InTheHand.Net;
using InTheHand.Net.Sockets;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BluetoothChat.Functions
{
    public class AppClient
    {
        public BluetoothClient Client { get; private set; }
        public bool IsConnected { get; private set; }

        private readonly FrmBluetoothChat app;
        private readonly string bluetoothPrompt = "Enter Bluetooth address: ";
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
                app.AppendConsoleText(bluetoothPrompt);
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
                IsConnected = false;
                app.AppendConsoleText(DisplayFormat.FormatMessage($"Error connecting to server: {ex.Message}"));
                app.AppendConsoleText(DisplayFormat.FormatMessage(bluetoothPrompt));
                app.SetSendButtonEnabled(true);
                return;
            }
        }

        public async Task StartReadingMessagesAsync()
        {
            app.ClearConsoleText();
            app.SetSendButtonEnabled(true);

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
                app.AppendConsoleText(DisplayFormat.FormatMessage(bluetoothPrompt));
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
                    SenderName = app.DisplayName,
                    Message = $"has joined the server"
                };

                await SendMessageToServer(Client.GetStream(), message);
                app.ClearInputText();
            }
            catch (Exception ex)
            {
                app.AppendConsoleText(DisplayFormat.FormatMessage($"Error sending welcome message to server: {ex.Message}"));
                app.AppendConsoleText(DisplayFormat.FormatMessage(bluetoothPrompt));
                IsConnected = false;
                return;
            }
        }

        public async Task SendMessageToServer(NetworkStream stream, ChatMessage message)
        {
            try
            {
                string json = ChatProtocol.Serialize(message);
                byte[] messageData = Encoding.UTF8.GetBytes(json);
                byte[] lengthData = BitConverter.GetBytes(messageData.Length);

                // Tell the server the length of the message as well as the message itself
                await stream.WriteAsync(lengthData, 0, lengthData.Length);
                await stream.WriteAsync(messageData, 0, messageData.Length);
            }
            catch (Exception e)
            {
                app.AppendConsoleText(DisplayFormat.FormatMessage($"Unable to broadcast message: {e.Message}"));
            }
        }

        private async Task ReadMessagesFromServer(CancellationToken token)
        {
            byte[] buffer = new byte[512];
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (Client == null || !Client.Connected)
                        return;

                    int data = await Client.GetStream().ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);

                    // We stopped receiving data from the server (server shut down)
                    if (data == 0)
                    {
                        Client.Close();
                        app.ResetUI();
                        IsConnected = false;
                    }

                    string response = Encoding.UTF8.GetString(buffer, 0, data);
                    app.AppendConsoleText(DisplayFormat.FormatMessage(response));
                }
                catch (Exception)
                {
                    app.ResetUI();
                    IsConnected = false;
                }
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
                        SenderName = app.DisplayName,
                        Message = $"[{app.DisplayName}] has left the server"
                    };

                    await SendMessageToServer(Client.GetStream(), message);
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
