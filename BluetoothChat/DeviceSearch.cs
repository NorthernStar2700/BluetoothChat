using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using BluetoothChat.Utilities;
using InTheHand.Net.Sockets;

namespace BluetoothChat
{
    public partial class FrmDeviceSearch : Form
    {
        private readonly BluetoothClient client;

        private const string searchText = "Searching...";
        private const string searchCompleteText = "Search complete";
        private const string searchErrorText = "Error finding devices. Try scanning again";

        public FrmDeviceSearch()
        {
            InitializeComponent();
            client = new BluetoothClient();
        }

        private void FrmDeviceSearch_Load(object sender, EventArgs e)
        {
            SearchForDevices();
        }

        private void FrmDeviceSearch_FormClosing(object sender, FormClosingEventArgs e)
        {
            client.Dispose();
        }

        private void BtnRestart_Click(object sender, EventArgs e)
        {
            BtnRestart.Enabled = false;
            LbxDevices.Items.Clear();
            SearchForDevices();
        }

        private void BtnCopy_Click(object sender, EventArgs e)
        {
            if (LbxDevices.Items.Count == 0 || LbxDevices.SelectedIndex < 0)
            {
                return;
            }

            string[] deviceInfo = 
                ((string) LbxDevices.Items[LbxDevices.SelectedIndex]).Split(new string[] { " | " }, StringSplitOptions.None);
            string deviceAddress = deviceInfo[1];
            Clipboard.SetText(deviceAddress);
            MessageBox.Show($"Copied device address {deviceAddress} to clipboard",
                "Copy successful",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private async void SearchForDevices()
        {
            LblSearch.Text = searchText;

            try
            {
                await Task.Run(() =>
                {
                    foreach (BluetoothDeviceInfo device in client.DiscoverDevices().ToList()) {
                        string formattedEntry = $"{device.DeviceName} | {device.DeviceAddress}";
                        BeginInvoke((Action)(() => LbxDevices.Items.Add(formattedEntry)));
                    }
                });
                LblSearch.Text = searchCompleteText;
            }
            catch
            {
                LblSearch.Text = searchErrorText;
            }
            finally
            {
                BtnRestart.Enabled = true;
            }
        }
    }
}
