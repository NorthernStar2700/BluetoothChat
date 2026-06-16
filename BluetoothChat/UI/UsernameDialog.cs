using System;
using System.Windows.Forms;
using BluetoothChat.Constants;

namespace BluetoothChat.UI
{
    public partial class FrmUsernameDialog : Form
    {
        public string NewUsername { get; private set; }
        private readonly string currentUsername;

        public FrmUsernameDialog(string currentUsername)
        {
            InitializeComponent();
            this.currentUsername = currentUsername;
        }

        private void FrmUsernameDialog_Load(object sender, EventArgs e)
        {
            LblCurrentUsername.Text = $"{UIMessages.UsernameMessage}{currentUsername}";
            AcceptButton = BtnOk;
            CancelButton = BtnCancel;
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            NewUsername = TxtUsername.Text.Trim();
            DialogResult = DialogResult.OK;
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void TxtUsername_TextChanged(object sender, EventArgs e)
        {
            BtnOk.Enabled = !string.IsNullOrWhiteSpace(TxtUsername.Text);
        }
    }
}
