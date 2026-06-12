using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using InTheHand.Net.Sockets;

namespace BluetoothChat
{
    public partial class FrmDeviceSearch : Form
    {
        private const string searchText = "Searching...";
        private const string searchCompleteText = "Search complete";
        private const string searchErrorText = "Error finding devices. Try scanning again";

        public FrmDeviceSearch()
        {
            InitializeComponent();
        }

        private void FrmDeviceSearch_Load(object sender, EventArgs e)
        {
            SearchForDevices();
        }

        private void LbxDevices_DoubleClick(object sender, EventArgs e)
        {
            BtnCopy.PerformClick();
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

        private async void SearchForDevices()
        {
            LblSearch.Text = searchText;

            try
            {
                List<BluetoothDeviceInfo> devices = await Task.Run(() =>
                {
                    using (BluetoothClient client = new BluetoothClient())
                    {
                        return client.DiscoverDevices().ToList();
                    }
                });

                if (Disposing)
                {
                    return;
                }

                foreach (BluetoothDeviceInfo device in devices)
                {
                    string formattedEntry = $"{device.DeviceName} | {device.DeviceAddress}";
                    BeginInvoke((Action)(() => LbxDevices.Items.Add(formattedEntry)));
                }

                LblSearch.Text = searchCompleteText;
            }
            catch (Exception)
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
