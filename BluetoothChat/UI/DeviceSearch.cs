using BluetoothChat.Models;
using BluetoothChat.Properties;
using InTheHand.Net.Sockets;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BluetoothChat.UI
{
    public partial class FrmDeviceSearch : Form
    {
        private const string searchText = "Searching...";
        private const string searchCompleteText = "Search complete";
        private const string searchErrorText = "Error finding devices. Try scanning again";
        private readonly string deviceList;
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
                List<Device> deviceHistory = (List<Device>)JsonConvert.DeserializeObject(deviceList, typeof(List<Device>));
                if (deviceHistory != null && deviceHistory.Count > 0)
                {
                    devices = deviceHistory;
                }
                else
                {
                    devices = new List<Device>();
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

            try
            {
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
            catch (Exception ex)
            {
                MessageBox.Show($"Cannot copy Bluetooth address: {ex.Message}",
                    "Copy error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
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
            if (IsDisposed || Disposing || !IsHandleCreated)
            {
                return;
            }

            RunActionOnUI(() =>
            {
                LblSearch.Text = searchText;
                BtnRestart.Enabled = false;
                BtnCopy.Enabled = false;
            });

            BluetoothClient client = new BluetoothClient();
            try
            {
                List<BluetoothDeviceInfo> foundDevices = await Task.Run(() => client.DiscoverDevices().ToList());

                RunActionOnUI(() =>
                {
                    foreach (BluetoothDeviceInfo device in foundDevices)
                    {
                        string deviceName = !string.IsNullOrWhiteSpace(device.DeviceName) ? device.DeviceName : "No Name Available";
                        Device deviceObj = new Device()
                        {
                            Name = deviceName,
                            Address = device.DeviceAddress.ToString()
                        };
                        LbxDevices.Items.Add(deviceObj);
                    }
                });

                RunActionOnUI(() => LblSearch.Text = searchCompleteText);

                if (LbxDevices.Items.Count > 0)
                {
                    RunActionOnUI(() => BtnCopy.Enabled = true);
                }
            }
            catch (Exception)
            {
                RunActionOnUI(() => LblSearch.Text = searchErrorText);
            }
            finally
            {
                BtnRestart.Enabled = true;
                client.Close();
                client.Dispose();
            }
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
