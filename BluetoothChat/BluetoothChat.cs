using System;
using System.IO;
using System.Windows.Forms;
using BluetoothChat.Constants;
using BluetoothChat.Enums;
using BluetoothChat.Functions;
using BluetoothChat.Models;
using BluetoothChat.Utilities;

namespace BluetoothChat
{
    public partial class FrmBluetoothChat : Form
    {
        public string DisplayName { get; private set; }

        private AppMode appMode;
        private readonly AppClient client;
        private readonly AppServer server;
        private readonly string bluetoothPrompt = "Enter Bluetooth address: ";
        private readonly string username = "Current Username: ";

        public FrmBluetoothChat()
        {
            InitializeComponent();
            client = new AppClient(this);
            server = new AppServer(this);
            appMode = AppMode.Inactive;
            RtbConsole.Text = Messages.ConsolePrompt;
            AcceptButton = BtnSend;
        }

        private void FrmBluetoothChat_Load(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(DisplayName))
            {
                DisplayName = Environment.MachineName;
                Properties.Settings.Default.CurrentUsername = DisplayName;
            }

            DisplayName = Properties.Settings.Default.CurrentUsername;
            CurrentUsernameToolStripMenuItem.Text = username + DisplayName;
        }

        private void FrmBluetoothChat_FormClosed(object sender, FormClosedEventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private void SearchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (FrmDeviceSearch deviceSearch = new FrmDeviceSearch())
            {
                DialogResult result = deviceSearch.ShowDialog();
            }
        }

        private async void ChangeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string oldUsername = DisplayName;
            using (FrmUsernameDialog dialog = new FrmUsernameDialog(oldUsername))
            {
                DialogResult result = dialog.ShowDialog();
                if (result != DialogResult.OK)
                {
                    return;
                }

                string newUsername = dialog.NewUsername.Trim();

                if (!string.IsNullOrWhiteSpace(newUsername))
                {
                    if (newUsername.ToUpperInvariant().IndexOf("[HOST]") != -1)
                    {
                        newUsername = newUsername.Replace("[HOST]", string.Empty);
                    }
                    CurrentUsernameToolStripMenuItem.Text = username + newUsername;
                    Properties.Settings.Default.CurrentUsername = newUsername;
                    DisplayName = newUsername;
                }

                ChatMessage message = new ChatMessage()
                {
                    MessageType = MessageType.UsernameChange,
                    SenderName = newUsername,
                    Message = $"[{oldUsername}] changed their name to [{DisplayName}]"
                };

                if (appMode == AppMode.Client && client.IsConnected)
                {
                    await client.SendMessageToServer(message);
                    ClearInputText();
                }
                else if (appMode == AppMode.Host && server.IsRunning)
                {
                    message.IsHost = true;
                    AppendConsoleText(DisplayFormat.FormatMessage(message.Message));
                    await server.SendMessageToClientsAsync(message);
                }
            }
        }

        private void CreateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CreateServerPrompt();
            ToggleServerTabs();
        }

        private void ConnectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ConnectToServerPrompt();
            ToggleServerTabs();
        }

        private async void BtnSend_Click(object sender, EventArgs e)
        {
            try
            {
                if (appMode == AppMode.Inactive || string.IsNullOrWhiteSpace(TxtInput.Text))
                {
                    return;
                }
                else
                {
                    if (!client.IsConnected && appMode == AppMode.Client)
                    {
                        await client.AttemptConnection();
                        if (client.IsConnected)
                        {
                            await client.SendJoinMessage();
                            await client.StartReadingMessagesAsync();
                        }
                    }
                    else
                    {
                        ChatMessage message = new ChatMessage()
                        {
                            MessageType = MessageType.Chat,
                            SenderName = DisplayName,
                            Message = TxtInput.Text.Trim()
                        };

                        if (client.IsConnected && appMode == AppMode.Client)
                        {
                            await client.SendMessageToServer(message);
                        }
                        else if (server.IsRunning && appMode == AppMode.Host)
                        {
                            message.IsHost = true;
                            await server.SendMessageToClientsAsync(message);
                        }
                    }
                    ClearInputText();
                }
            }
            catch { }
        }

        private async void BtnExit_Click(object sender, EventArgs e)
        {
            if (appMode != AppMode.Inactive)
            {
                if (server.IsRunning && appMode == AppMode.Host)
                {
                    server.Stop();
                }

                if (client.IsConnected && appMode == AppMode.Client)
                {
                    await client.SendLeaveMessage();
                }

                ResetUI();
            }
            else
            {
                Application.Exit();
            }
        }

        private void ConnectToServerPrompt()
        {
            appMode = AppMode.Client;
            RtbConsole.Clear();
            RtbConsole.Text = bluetoothPrompt;
        }

        private void CreateServerPrompt()
        {
            appMode = AppMode.Host;
            RtbConsole.Clear();
            server.Start();
        }

        private void ToggleServerTabs()
        {
            CreateToolStripMenuItem.Enabled = appMode == AppMode.Inactive;
            ConnectToolStripMenuItem.Enabled = appMode == AppMode.Inactive;
        }

        public void ResetUI()
        {
            appMode = AppMode.Inactive;
            RunActionOnUI(() =>
            {
                ToggleServerTabs();
                RtbConsole.Clear();
                RtbConsole.Text = Messages.ConsolePrompt;
                BtnSend.Enabled = true;
                ChkConnected.Checked = false;
            });
        }

        public void AppendConsoleText(string text)
        {
            RunActionOnUI(() => RtbConsole.AppendText(text));
        }

        public void ClearConsoleText()
        {
            RunActionOnUI(() => RtbConsole.Clear());
        }

        public void ClearInputText()
        {
            RunActionOnUI(() => TxtInput.Clear());
        }

        public void SetSendButtonEnabled(bool enabled)
        {
            RunActionOnUI(() => BtnSend.Enabled = enabled);
        }

        public void SetConnectedCheckbox(bool enabled)
        {
            RunActionOnUI(() => ChkConnected.Checked = enabled);
        }

        public void SetAppMode(AppMode mode)
        {
            appMode = mode;
        }

        public string GetInputText()
        {
            return TxtInput.Text;
        }

        private void RunActionOnUI(Action action)
        {
            if (IsDisposed || Disposing || !IsHandleCreated)
            {
                return;
            }

            try
            {
                if (InvokeRequired)
                {
                    BeginInvoke(action);
                }
                else
                {
                    action();
                }
            }
            catch (IOException)
            {

            }
            catch (ObjectDisposedException)
            {

            }
        }
    }
}
