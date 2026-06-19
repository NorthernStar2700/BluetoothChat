using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using BluetoothChat.Models;
using BluetoothChat.Properties;
using Newtonsoft.Json;

namespace BluetoothChat.UI
{
    public partial class FrmDeviceHistory : Form
    {
        private readonly string deviceList;

        public FrmDeviceHistory(string deviceList)
        {
            InitializeComponent();
            this.deviceList = deviceList;
        }

        private void FrmDeviceHistory_Load(object sender, EventArgs e)
        {
            BtnCopy.Enabled = false;
            BtnDelete.Enabled = false;
            if (string.IsNullOrWhiteSpace(deviceList))
            {
                return;
            }

            try
            {
                List<Device> devices = (List<Device>)JsonConvert.DeserializeObject(deviceList, typeof(List<Device>));
                if (devices == null || devices.Count == 0)
                {
                    return;
                }

                foreach (Device device in devices)
                {
                    LbxDevices.Items.Add(device);
                }

                BtnCopy.Enabled = true;
                BtnDelete.Enabled = true;
            }
            catch (Exception)
            {

            }
        }

        private void FrmDeviceHistory_FormClosed(object sender, FormClosedEventArgs e)
        {
            Settings.Default.Save();
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (LbxDevices.Items.Count == 0 || LbxDevices.SelectedIndex == -1)
            {
                return;
            }

            LbxDevices.Items.RemoveAt(LbxDevices.SelectedIndex);
            string newList = JsonConvert.SerializeObject(LbxDevices.Items.OfType<Device>().ToList());
            Settings.Default.DeviceHistory = newList;
            Settings.Default.Save();

            if (LbxDevices.Items.Count == 0) 
            {
                BtnCopy.Enabled = false;
                BtnDelete.Enabled = false;
            }
        }

        private void BtnCopy_Click(object sender, EventArgs e)
        {
            if (LbxDevices.Items.Count == 0 || LbxDevices.SelectedIndex == -1)
            {
                return;
            }

            Device device = (Device)LbxDevices.Items[LbxDevices.SelectedIndex];
            Clipboard.SetText(device.Address);
            MessageBox.Show($"Copied device address {device.Address} to clipboard",
                "Copy successful",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void LbxDevices_DoubleClick(object sender, EventArgs e)
        {
            if (BtnCopy.Enabled)
            {
                BtnCopy.PerformClick();
            }
        }
    }
}
