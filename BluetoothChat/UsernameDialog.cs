using System;
using System.Windows.Forms;

namespace BluetoothChat
{
    public partial class FrmUsernameDialog : Form
    {
        public string NewUsername { get; private set; }

        public FrmUsernameDialog()
        {
            InitializeComponent();
        }

        private void FrmUsernameDialog_Load(object sender, EventArgs e)
        {
            LblCurrentUsername.Text += $" {Properties.Settings.Default.CurrentUsername}";
            AcceptButton = BtnOk;
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(TxtUsername.Text))
            {
                NewUsername = TxtUsername.Text;
                DialogResult = DialogResult.OK;
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
