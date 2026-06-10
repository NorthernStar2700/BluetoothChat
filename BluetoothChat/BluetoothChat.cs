using BluetoothChat.Constants;
using BluetoothChat.Functions;
using BluetoothChat.Utilities;
using System;
using System.Windows.Forms;

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
            FrmDeviceSearch deviceSearch = new FrmDeviceSearch();
            deviceSearch.Show();
        }

        private async void ChangeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string oldUsername = DisplayName;
            using (FrmUsernameDialog dialog = new FrmUsernameDialog(Properties.Settings.Default.CurrentUsername))
            {
                DialogResult result = dialog.ShowDialog();
                if (result != DialogResult.OK)
                {
                    return;
                }

                string newUsername = dialog.NewUsername;

                if (!string.IsNullOrWhiteSpace(newUsername))
                {
                    CurrentUsernameToolStripMenuItem.Text = username + newUsername;
                    Properties.Settings.Default.CurrentUsername = newUsername;
                    DisplayName = newUsername;
                }

                if (appMode == AppMode.Client && client.IsConnected)
                {
                    string message = $"[{oldUsername}] changed their name to [{DisplayName}]";
                    await client.SendMessageToServer(client.Client.GetStream(), message);
                }

                if (appMode == AppMode.Host && server.IsRunning && server.CancelToken != null)
                {
                    string message = $"Host [{oldUsername}] changed their name to [{DisplayName}]";
                    RtbConsole.AppendText(DisplayFormat.FormatMessage(message));
                    await server.SendMessageToClientsAsync(message, server.CancelToken.Token);
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
                    if (!server.IsRunning && appMode == AppMode.Host)
                    {
                        server.Start();
                    }
                    else if (!client.IsConnected && appMode == AppMode.Client)
                    {
                        await client.AttemptConnection();
                    }
                    else
                    {
                        if (server.IsRunning && appMode == AppMode.Host)
                        {
                            string message = $"[HOST] [{DisplayName}]: {TxtInput.Text}";
                            BeginInvoke((Action)(() => RtbConsole.AppendText(DisplayFormat.FormatMessage(message))));
                            await server.SendMessageToClientsAsync(message, server.CancelToken.Token);
                        }
                        else if (client.IsConnected && appMode == AppMode.Client)
                        {
                            string message = $"[{DisplayName}]: {TxtInput.Text}";
                            await client.SendMessageToServer(client.Client.GetStream(), message);
                        }
                    }
                    TxtInput.Clear();
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
                System.Windows.Forms.Application.Exit();
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
            BeginInvoke((Action)(() =>
            {
                ToggleServerTabs();
                RtbConsole.Clear();
                RtbConsole.Text = Messages.ConsolePrompt;
            }));
        }

        public void AppendConsoleText(string text)
        {
            BeginInvoke((Action)(() => RtbConsole.Text += text ));
        }

        public void ClearConsoleText()
        {
            BeginInvoke((Action)(() => RtbConsole.Clear() ));
        }

        public void ClearInputText()
        {
            BeginInvoke((Action)(() => TxtInput.Clear()));
        }

        public void SetSendButtonEnabled(bool enabled)
        {
            BeginInvoke((Action)(() => BtnSend.Enabled = enabled));
        }

        public string GetInputText()
        {
            return TxtInput.Text;
        }
    }
}
