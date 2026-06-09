using BluetoothChat.Constants;
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
        public BluetoothClient Client;
        public CancellationTokenSource CancelToken;

        private readonly FrmBluetoothChat app;
        private readonly string bluetoothPrompt = "Enter Bluetooth address: ";

        public AppClient(FrmBluetoothChat app)
        {
            this.app = app;
        }

        public async Task AttemptConnection()
        {
            app.RtbConsole.Text += DisplayFormat.FormatMessage(app.TxtInput.Text);
            BluetoothAddress address;
            try
            {
                address = BluetoothAddress.Parse(app.TxtInput.Text);
                app.BtnSend.Enabled = false;
            }
            catch (Exception)
            {
                app.RtbConsole.AppendText(DisplayFormat.FormatMessage("Incorrect format for Bluetooth address"));
                app.RtbConsole.AppendText(bluetoothPrompt);
                app.BtnSend.Enabled = true;
                return;
            }

            try
            {
                Client = new BluetoothClient();
                app.RtbConsole.AppendText(DisplayFormat.FormatMessage("Connecting to server"));
                await Task.Run(() => Client.Connect(address, Common.Guid));
            }
            catch (Exception ex)
            {
                app.RtbConsole.AppendText(DisplayFormat.FormatMessage($"Error connecting to server: {ex.Message}"));
                app.RtbConsole.AppendText(DisplayFormat.FormatMessage(bluetoothPrompt));
                app.BtnSend.Enabled = true;
                return;
            }

            app.RtbConsole.Clear();
            app.BtnSend.Enabled = true;

            string message = $"[{app.DisplayName}] has joined the server";
            CancelToken?.Cancel();
            CancelToken = new CancellationTokenSource();

            try
            {
                await SendMessageToServer(Client.GetStream(), message);
            }
            catch (Exception ex)
            {
                app.RtbConsole.AppendText(DisplayFormat.FormatMessage($"Error sending welcome message to server: {ex.Message}"));
                app.RtbConsole.AppendText(DisplayFormat.FormatMessage(bluetoothPrompt));
                return;
            }

            try
            {
                await ReadMessagesFromServer(CancelToken.Token);
            }
            catch (Exception ex)
            {
                app.RtbConsole.AppendText(DisplayFormat.FormatMessage($"Error reading messages from server: {ex.Message}"));
                app.RtbConsole.AppendText(DisplayFormat.FormatMessage(bluetoothPrompt));
                return;
            }
        }

        public async Task SendMessageToServer(NetworkStream stream, string message)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(message);

                await stream.WriteAsync(data, 0, data.Length);
                app.TxtInput.Text = string.Empty;
            }
            catch (Exception e)
            {
                app.BeginInvoke((Action)(() => app.RtbConsole.AppendText(DisplayFormat.FormatMessage($"Unable to broadcast message: {e.Message}"))));
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
                    }

                    string response = Encoding.UTF8.GetString(buffer, 0, data);
                    app.BeginInvoke((Action)(() => app.RtbConsole.AppendText(DisplayFormat.FormatMessage(response))));
                }
                catch (Exception)
                {
                    app.ResetUI();
                }
            }
        }

        public async Task SendLeaveMessage()
        {
            if (Client != null && Client.Connected)
            {
                // Attempt to send a message to the server indicating the client is leaving
                // Disconnect even if the message fails
                string message = $"[{app.DisplayName}] has left the server{Common.TerminateMessage}";
                try
                {
                    await SendMessageToServer(Client.GetStream(), message);
                }
                finally
                {
                    Client.Close();
                }
            }
        }
    }
}
