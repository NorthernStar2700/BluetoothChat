using System;
using System.Windows.Forms;
using BluetoothChat.Properties;

namespace BluetoothChat.UI
{
    public partial class FrmDeviceHistory : Form
    {
        private string deviceList;

        public FrmDeviceHistory(string deviceList)
        {
            InitializeComponent();
            this.deviceList = deviceList;
        }

        private void FrmDeviceHistory_Load(object sender, EventArgs e)
        {
            LoadDevices(deviceList);
        }

        private void FrmDeviceHistory_FormClosed(object sender, FormClosedEventArgs e)
        {
            Settings.Default.DeviceHistory = deviceList;
            Settings.Default.Save();
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (LbxDevices.Items.Count == 0 || LbxDevices.SelectedIndex == -1)
            {
                return;
            }

            LbxDevices.Items.RemoveAt(LbxDevices.SelectedIndex);
            string newDeviceList = string.Empty;
            foreach (string device in LbxDevices.Items)
            {
                newDeviceList += $"{device},";
            }

            deviceList = newDeviceList;
            Settings.Default.DeviceHistory = newDeviceList;
            Settings.Default.Save();
        }

        private void BtnReload_Click(object sender, EventArgs e)
        {
            LoadDevices(Settings.Default.DeviceHistory);
        }

        private void BtnCopy_Click(object sender, EventArgs e)
        {
            if (LbxDevices.Items.Count == 0 || LbxDevices.SelectedIndex == -1)
            {
                return;
            }

            string[] deviceInfo =
                ((string)LbxDevices.Items[LbxDevices.SelectedIndex]).Split(new string[] { " | " }, StringSplitOptions.None);

            if (deviceInfo.Length != 2)
            {
                return;
            }

            string deviceAddress = deviceInfo[1];
            Clipboard.SetText(deviceAddress);
            MessageBox.Show($"Copied device address {deviceAddress} to clipboard",
                "Copy successful",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);


        }

        private void LbxDevices_DoubleClick(object sender, EventArgs e)
        {
            BtnCopy.PerformClick();
        }

        private void LoadDevices(string deviceList)
        {
            if (LbxDevices.Items.Count > 0)
            {
                LbxDevices.Items.Clear();
            }

            string[] devices = deviceList.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            if (devices.Length == 0)
            {
                return;
            }

            foreach (string device in devices)
            {
                LbxDevices.Items.Add(device);
            }
        }
    }
}
