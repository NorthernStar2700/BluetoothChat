using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using BluetoothChat.Models;
using BluetoothChat.Properties;
using InTheHand.Net.Sockets;
using Newtonsoft.Json;

namespace BluetoothChat.UI
{
    public partial class FrmDeviceSearch : Form
    {
        private const string searchText = "Searching...";
        private const string searchCompleteText = "Search complete";
        private const string searchErrorText = "Error finding devices. Try scanning again";
        private string deviceList;
        private List<Device> devices;

        public FrmDeviceSearch(string deviceList)
        {
            InitializeComponent();
            this.deviceList = deviceList;
        }

        private void FrmDeviceSearch_Load(object sender, EventArgs e)
        {
            BtnCopy.Enabled = false;
            try
            {
                List<Device> devices = (List<Device>)JsonConvert.DeserializeObject(deviceList, typeof(List<Device>));
                if (devices != null && devices.Count > 0)
                {
                    this.devices = devices;
                }
                else
                {
                    this.devices = new List<Device>();
                }
            }
            catch (Exception)
            {
                devices = new List<Device>();
            }
            _ = SearchForDevicesAsync();
        }

        private void FrmDeviceSearch_FormClosed(object sender, FormClosedEventArgs e)
        {
            Settings.Default.DeviceHistory = JsonConvert.SerializeObject(devices);
            Settings.Default.Save();
        }

        private void BtnRestart_Click(object sender, EventArgs e)
        {
            LbxDevices.Items.Clear();
            _ = SearchForDevicesAsync();
        }

        private void BtnCopy_Click(object sender, EventArgs e)
        {
            if (LbxDevices.Items.Count == 0 || LbxDevices.SelectedIndex < 0)
            {
                return;
            }

            // Format: (Device Name) | (Address)
            Device device = (Device)LbxDevices.Items[LbxDevices.SelectedIndex];
            Clipboard.SetText(device.Address);
            MessageBox.Show($"Copied device address {device.Address} to clipboard",
                "Copy successful",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            // Save to history if entry does not exist
            Device deviceExists = devices.FirstOrDefault(dev => (dev.Name == device.Name) && (dev.Address == device.Address));
            if (deviceExists == null)
            {
                devices.Add(device);
            }
        }

        private void LbxDevices_DoubleClick(object sender, EventArgs e)
        {
            if (BtnCopy.Enabled)
            {
                BtnCopy.PerformClick();
            }
        }

        private async Task SearchForDevicesAsync()
        {
            LblSearch.Text = searchText;
            BtnRestart.Enabled = false;
            BtnCopy.Enabled = false;

            try
            {
                List<BluetoothDeviceInfo> devices = await Task.Run(() =>
                {
                    using (BluetoothClient client = new BluetoothClient())
                    {
                        return client.DiscoverDevices().ToList();
                    }
                });

                if (IsDisposed || Disposing || !IsHandleCreated)
                {
                    return;
                }

                foreach (BluetoothDeviceInfo device in devices)
                {
                    string deviceName = !string.IsNullOrWhiteSpace(device.DeviceName) ? device.DeviceName : "No Name Available";
                    Device deviceObj = new Device()
                    {
                        Name = deviceName,
                        Address = device.DeviceAddress.ToString()
                    };
                    LbxDevices.Items.Add(deviceObj);
                }

                LblSearch.Text = searchCompleteText;

                if (devices.Count > 0)
                {
                    BtnCopy.Enabled = true;
                }
            }
            catch (Exception)
            {
                LblSearch.Text = searchErrorText;
            }
            finally
            {
                if (!IsDisposed && !Disposing && IsHandleCreated)
                {
                    BtnRestart.Enabled = true;
                }
            }
        }
    }
}
