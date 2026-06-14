using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using BluetoothChat.Constants;
using BluetoothChat.Enums;
using BluetoothChat.Functions;
using BluetoothChat.Models;
using BluetoothChat.Properties;

namespace BluetoothChat.UI
{
    public partial class FrmBluetoothChat : Form
    {
        public AppAccount Account { get; private set; }

        private AppMode appMode;
        private readonly AppClient client;
        private readonly AppServer server;
        private readonly string usernamePlaceholder = "Current Username: ";

        public FrmBluetoothChat()
        {
            InitializeComponent();
            client = new AppClient(this);
            server = new AppServer(this);
            Account = new AppAccount()
            {
                Name = Settings.Default.CurrentUsername
            };
            appMode = AppMode.Inactive;
            RtbConsole.Text = Messages.ConsolePrompt;
            AcceptButton = BtnSend;
        }

        private void FrmBluetoothChat_Load(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Account.Name))
            {
                Account.Name = Environment.MachineName;
                Settings.Default.CurrentUsername = Account.Name;
            }

            CurrentUsernameToolStripMenuItem.Text = usernamePlaceholder + Account.Name;
        }

        private void FrmBluetoothChat_FormClosed(object sender, FormClosedEventArgs e)
        {
            Settings.Default.Save();
        }

        private void SearchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FrmDeviceSearch deviceSearch = new FrmDeviceSearch(Settings.Default.DeviceHistory);
            deviceSearch.Show();
        }

        private void HistoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FrmDeviceHistory deviceHistory = new FrmDeviceHistory(Settings.Default.DeviceHistory);
            deviceHistory.Show();
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

                string newUsername = dialog.NewUsername.Trim();

                if (!string.IsNullOrWhiteSpace(newUsername))
                {
                    if (newUsername.ToUpperInvariant().IndexOf("[HOST]") != -1)
                    {
                        newUsername = newUsername.Replace("[HOST]", string.Empty);
                    }

                    CurrentUsernameToolStripMenuItem.Text = usernamePlaceholder + newUsername;
                    Settings.Default.CurrentUsername = newUsername;
                    Account.Name = newUsername;
                }

                // Send a chat message when users change their usernames
                ChatMessage message = new ChatMessage()
                {
                    MessageType = MessageType.UsernameChange,
                    SenderName = Account.Name,
                    SenderId = Account.AccountId,
                    Message = $"[{oldUsername}] changed their name to [{Account.Name}]"
                };

                if (appMode == AppMode.Client && client.IsConnected)
                {
                    await client.SendMessageToServer(message);
                }
                else if (appMode == AppMode.Host && server.IsRunning)
                {
                    message.IsHost = true;
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
                    // Try to connect, if connected read messages
                    if (!client.IsConnected && appMode == AppMode.Client)
                    {
                        await client.AttemptConnection();
                        if (client.IsConnected)
                        {
                            Account.InitializeAccountId();
                            await client.SendJoinMessage();
                            await client.StartReadingMessagesAsync();
                        }
                    }
                    else
                    {
                        // Create a chat message and pass it to all clients
                        ChatMessage message = new ChatMessage()
                        {
                            MessageType = MessageType.Chat,
                            SenderName = Account.Name,
                            SenderId = Account.AccountId,
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
            RtbConsole.Text = Messages.BluetoothPrompt;
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
                LbxMembers.Items.Clear();
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

        public void AddChatMember(AppAccount account)
        {
            RunActionOnUI(() => LbxMembers.Items.Add(account));
        }

        public void RemoveChatMember(AppAccount account)
        {
            RunActionOnUI(() =>
            {
                int index = FindMember(account);

                if (index != -1)
                {
                    LbxMembers.Items.RemoveAt(index);
                }
            });
        }

        public void UpdateChatMember(AppAccount account)
        {

            RunActionOnUI(() =>
            {
                int index = FindMember(account);

                if (index != -1)
                {
                    LbxMembers.Items[index] = account;
                }
            });
        }

        public string GetInputText()
        {
            return TxtInput.Text;
        }

        public List<AppAccount> GetAppAccounts()
        {
            return LbxMembers.Items.OfType<AppAccount>().ToList();
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

        private int FindMember(AppAccount account)
        {
            int index = -1;
            int current = 0;
            foreach (AppAccount appAccount in LbxMembers.Items)
            {
                if (appAccount.AccountId == account.AccountId)
                {
                    index = current;
                    break;
                }
                current++;
            }

            return index;
        }
    }
}
