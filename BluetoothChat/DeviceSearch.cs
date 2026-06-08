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
        private BluetoothClient client;
        private List<BluetoothDeviceInfo> devices;

        private const string searchText = "Searching...";
        private const string searchCompleteText = "Search complete";
        private const string searchErrorText = "Error finding devices. Try scanning again";

        public FrmDeviceSearch()
        {
            InitializeComponent();
            client = new BluetoothClient();
            devices = new List<BluetoothDeviceInfo>();
        }

        private void frmDeviceSearch_Load(object sender, EventArgs e)
        {
            searchForDevices();
        }

        private void btnRestart_Click(object sender, EventArgs e)
        {
            BtnRestart.Enabled = false;
            LbxDevices.Items.Clear();
            devices.Clear();
            searchForDevices();
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            if (LbxDevices.Items.Count == 0 || LbxDevices.SelectedIndex < 0)
            {
                return;
            }

            BluetoothDeviceInfo device = devices[LbxDevices.SelectedIndex];
            string deviceAddress = device.DeviceAddress.ToString();
            Clipboard.SetText(deviceAddress);
            MessageBox.Show($"Copied device address {deviceAddress} to clipboard",
                "Copy successful",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private async void searchForDevices()
        {
            LblSearch.Text = searchText;
            try
            {
                await Task.Run(() => devices = client.DiscoverDevices().ToList());
            }
            catch
            {
                LblSearch.Text = searchErrorText;
            }
            finally
            {
                BtnRestart.Enabled = true;
                LblSearch.Text = searchCompleteText;
                if (devices.Count > 0)
                {
                    foreach (BluetoothDeviceInfo device in devices)
                    {
                        LbxDevices.Items.Add(device.DeviceName + " | " + device.DeviceAddress);
                    }
                }
            }
        }
    }
}
