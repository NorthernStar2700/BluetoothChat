using BluetoothChat.Constants;
using BluetoothChat.Enums;
using BluetoothChat.Functions;
using BluetoothChat.Models;
using BluetoothChat.Properties;
using BluetoothChat.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace BluetoothChat.UI
{
    public partial class FrmBluetoothChat : Form
    {
        public AppAccount Account { get; private set; }

        private AppMode appMode;
        private readonly AppClient client;
        private readonly AppServer server;

        public FrmBluetoothChat()
        {
            InitializeComponent();
            client = new AppClient(this);
            server = new AppServer(this);
            Account = new AppAccount()
            {
                Name = Settings.Default.CurrentUsername,
            };
            Account.InitializeAccountId();
            appMode = AppMode.Inactive;
            RtbConsole.Text = UIMessages.ConsolePrompt;
            AcceptButton = BtnSend;
        }

        private void FrmBluetoothChat_Load(object sender, EventArgs e)
        {
            TxtInput.Enabled = false;
            if (string.IsNullOrWhiteSpace(Account.Name))
            {
                Account.Name = Environment.MachineName;
                Settings.Default.CurrentUsername = Account.Name;
            }

            CurrentUsernameToolStripMenuItem.Text = UIMessages.UsernameMessage + Account.Name;
        }

        private void FrmBluetoothChat_FormClosing(object sender, FormClosingEventArgs e)
        {
            Settings.Default.Save();

            try
            {
                if (server.IsRunning)
                {
                    server.Stop();
                }
                else if (client.IsConnected)
                {
                    client.SendLeaveMessage();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void SearchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (FrmDeviceSearch deviceSearch = new FrmDeviceSearch(Settings.Default.DeviceHistory))
            {
                deviceSearch.ShowDialog();
            }
        }

        private void HistoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (FrmDeviceHistory deviceHistory = new FrmDeviceHistory(Settings.Default.DeviceHistory))
            {
                deviceHistory.ShowDialog();
            }
        }

        private async void ChangeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string oldUsername = Account.Name;
            using (FrmUsernameDialog dialog = new FrmUsernameDialog(oldUsername))
            {
                DialogResult result = dialog.ShowDialog();
                if (result != DialogResult.OK)
                {
                    return;
                }

                string newUsername = NameSanitizer.Sanitize(dialog.NewUsername);
                CurrentUsernameToolStripMenuItem.Text = UIMessages.UsernameMessage + newUsername;
                Settings.Default.CurrentUsername = newUsername;

                if (appMode == AppMode.Client && client.IsConnected)
                {
                    // Send a chat message when users change their usernames
                    ChatMessage message = new ChatMessage()
                    {
                        MessageType = MessageType.UsernameChange,
                        SenderName = newUsername,
                        SenderId = Account.AccountId,
                    };

                    await client.SendMessageToServer(message);
                    Account.Name = newUsername;
                }
                else if (appMode == AppMode.Host && server.IsRunning)
                {
                    ChatMessage message = server.HandleServerUsernameChange(newUsername);
                    await server.ProcessChatMessage(message);
                    await server.SendMemberListToClients();
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
                    // Try to connect, if connected read messages
                    if (!client.IsConnected && appMode == AppMode.Client)
                    {
                        await client.AttemptConnection();
                        if (client.IsConnected)
                        {
                            Account.InitializeAccountId();
                            await client.SendHandshakeRequest();
                            await client.StartReadingMessagesAsync();
                        }
                    }
                    else
                    {
                        try
                        {
                            SetSendButtonEnabled(false);

                            // Create a chat message and pass it to all clients
                            ChatMessage message = new ChatMessage()
                            {
                                MessageType = MessageType.Chat,
                                SenderName = Account.Name,
                                SenderId = Account.AccountId,
                                Content = TxtInput.Text.Trim()
                            };

                            if (client.IsConnected && appMode == AppMode.Client)
                            {
                                await client.SendMessageToServer(message);
                            }
                            else if (server.IsRunning && appMode == AppMode.Host)
                            {
                                // Server is the one who sends this message
                                message.MessageType = MessageType.ServerMessage;
                                await server.ProcessChatMessage(message);
                            }
                        }
                        finally
                        {
                            SetSendButtonEnabled(true);
                        }
                    }
                    ClearInputText();
                }
            }
            catch (Exception ex)
            {
                AppendConsoleText(DisplayFormat.FormatConsoleMessage($"[ERROR] Cannot send message: {ex.Message}"));
            }
        }

        private async void BtnExit_Click(object sender, EventArgs e)
        {
            if (appMode != AppMode.Inactive)
            {
                BtnSend.Enabled = false;
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
            TxtInput.Enabled = true;
            RtbConsole.Clear();
            RtbConsole.Text = UIMessages.BluetoothPrompt;
        }

        private void CreateServerPrompt()
        {
            appMode = AppMode.Host;
            TxtInput.Enabled = true;
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
                RtbConsole.Text = UIMessages.ConsolePrompt;
                LbxMembers.Items.Clear();
                TxtInput.Clear();
                BtnSend.Enabled = true;
                ChkConnected.Checked = false;
                TxtInput.Enabled = false;
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

        public void ReplaceChatMembers(List<AppAccount> accounts)
        {
            RunActionOnUI(() =>
            {
                LbxMembers.Items.Clear();
                foreach (AppAccount account in accounts)
                {
                    LbxMembers.Items.Add(account);
                }
            });
        }

        public void AddChatMember(AppAccount account)
        {
            RunActionOnUI(() => LbxMembers.Items.Add(account));
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
                // Ignore
            }
            catch (ObjectDisposedException)
            {
                // Ignore
            }
            catch (InvalidOperationException)
            {
                // Ignore
            }
        }
    }
}
