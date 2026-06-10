using BluetoothChat.Constants;
using BluetoothChat.Utilities;
using InTheHand.Net;
using InTheHand.Net.Sockets;
using System;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BluetoothChat.Functions
{
    public class AppClient
    {
        public BluetoothClient Client { get; private set; }
        public CancellationTokenSource CancelToken { get; private set; }
        public bool IsConnected { get; private set; }

        private readonly FrmBluetoothChat app;
        private readonly string bluetoothPrompt = "Enter Bluetooth address: ";

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

            app.ClearConsoleText();
            app.SetSendButtonEnabled(true);

            CancelToken?.Cancel();
            CancelToken?.Dispose();
            CancelToken = new CancellationTokenSource();

            try
            {
                string message = $"[{app.DisplayName}] has joined the server";
                await SendMessageToServer(Client.GetStream(), message);
            }
            catch (Exception ex)
            {
                app.AppendConsoleText(DisplayFormat.FormatMessage($"Error sending welcome message to server: {ex.Message}"));
                app.AppendConsoleText(DisplayFormat.FormatMessage(bluetoothPrompt));
                IsConnected = false;
                return;
            }

            try
            {
                await ReadMessagesFromServer(CancelToken.Token);
            }
            catch (Exception ex)
            {
                app.AppendConsoleText(DisplayFormat.FormatMessage($"Error reading messages from server: {ex.Message}"));
                app.AppendConsoleText(DisplayFormat.FormatMessage(bluetoothPrompt));
                IsConnected = false;
                return;
            }
        }

        public async Task SendMessageToServer(NetworkStream stream, string message)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(message);

                await stream.WriteAsync(data, 0, data.Length);
                app.ClearInputText();
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
            if (Client != null && Client.Connected)
            {
                // Attempt to send a message to the server indicating the client is leaving
                // Disconnect even if the message fails
                string message = $"[{app.DisplayName}] has left the server{Messages.TerminateMessage}";
                try
                {
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
