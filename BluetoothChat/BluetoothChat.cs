using System;
using System.Windows.Forms;
using BluetoothChat.Constants;
using BluetoothChat.Functions;
using BluetoothChat.Utilities;

namespace BluetoothChat
{
    public partial class FrmBluetoothChat : Form
    {
        public string DisplayName = Properties.Settings.Default.CurrentUsername;

        private Mode appMode;
        private readonly AppClient client;
        private readonly AppServer server;
        private readonly string consolePrompt = string.Format("Connect to or create a server using the \"Server\" tab. " +
   "{0}{0}Look up a devices address using the \"Address Lookup\" tab. {0}{0}Change your display name using the \"Username\" tab.", Environment.NewLine);
        private readonly string bluetoothPrompt = "Enter Bluetooth address: ";
        private readonly string username = "Current Username: ";

        public FrmBluetoothChat()
        {
            InitializeComponent();
            client = new AppClient(this);
            server = new AppServer(this);
            appMode = Mode.Inactive;
            RtbConsole.Text = consolePrompt;
            AcceptButton = BtnSend;
        }

        private void FrmBluetoothChat_Load(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(DisplayName))
            {
                DisplayName = Environment.MachineName;
                Properties.Settings.Default.CurrentUsername = DisplayName;
            }

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
            FrmUsernameDialog dialog = new FrmUsernameDialog();
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

            if (appMode == Mode.Connect && client != null && client.Client.Connected)
            {
                string message = $"[{oldUsername}] changed their name to [{DisplayName}]";
                await client.SendMessageToServer(client.Client.GetStream(), message);
            }

            if (appMode == Mode.Create && server.Listener != null && server.Listener.Active && server.CancelToken != null)
            {
                string message = $"Host [{oldUsername}] changed their name to [{DisplayName}]";
                RtbConsole.AppendText(DisplayFormat.FormatMessage(message));
                await server.SendMessageToClientsAsync(message, server.CancelToken.Token);
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
            if (appMode == Mode.Inactive || string.IsNullOrWhiteSpace(TxtInput.Text))
            {
                return;
            }
            else
            {
                if ((server.Listener == null || !server.Listener.Active) && appMode == Mode.Create)
                {
                    server.Start();
                }
                else if ((client.Client == null || !client.Client.Connected) && appMode == Mode.Connect)
                {
                    await client.AttemptConnection();
                }
                else
                {
                    if (server.Listener.Active && appMode == Mode.Create)
                    {
                        string message = $"[HOST] [{DisplayName}]: {TxtInput.Text}";
                        BeginInvoke((Action)(() => RtbConsole.AppendText(DisplayFormat.FormatMessage(message))));
                        await server.SendMessageToClientsAsync(message, server.CancelToken.Token);
                    }
                    else if (client.Client.Connected && appMode == Mode.Connect)
                    {
                        string message = $"[{DisplayName}]: {TxtInput.Text}";
                        await client.SendMessageToServer(client.Client.GetStream(), message);
                    }
                }
                TxtInput.Clear();
            }
        }

        private async void BtnExit_Click(object sender, EventArgs e)
        {
            if (appMode != Mode.Inactive)
            {
                if (server.Listener != null && server.Listener.Active && appMode == Mode.Create)
                {
                    server.Listener.Stop();
                }

                if (client.Client != null && client.Client.Connected && appMode == Mode.Connect)
                {
                    await client.SendLeaveMessage();
                    client.Client.Close();
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
            appMode = Mode.Connect;
            RtbConsole.Clear();
            RtbConsole.Text = bluetoothPrompt;
        }

        private void CreateServerPrompt()
        {
            appMode = Mode.Create;
            RtbConsole.Clear();
            server.Start();
        }

        private void ToggleServerTabs()
        {
            CreateToolStripMenuItem.Enabled = appMode == Mode.Inactive;
            ConnectToolStripMenuItem.Enabled = appMode == Mode.Inactive;
        }

        public void ResetUI()
        {
            appMode = Mode.Inactive;
            BeginInvoke((Action)(() => ToggleServerTabs()));
            BeginInvoke((Action)(() => RtbConsole.Clear()));
            BeginInvoke((Action)(() => RtbConsole.Text = consolePrompt));
        }
    }
}
