namespace BluetoothChat.UI
{
    partial class FrmDeviceHistory
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.BtnCopy = new System.Windows.Forms.Button();
            this.BtnDelete = new System.Windows.Forms.Button();
            this.LbxDevices = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // BtnCopy
            // 
            this.BtnCopy.Location = new System.Drawing.Point(266, 286);
            this.BtnCopy.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.BtnCopy.Name = "BtnCopy";
            this.BtnCopy.Size = new System.Drawing.Size(100, 28);
            this.BtnCopy.TabIndex = 4;
            this.BtnCopy.Text = "Copy";
            this.BtnCopy.UseVisualStyleBackColor = true;
            this.BtnCopy.Click += new System.EventHandler(this.BtnCopy_Click);
            // 
            // BtnDelete
            // 
            this.BtnDelete.Location = new System.Drawing.Point(158, 286);
            this.BtnDelete.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.BtnDelete.Name = "BtnDelete";
            this.BtnDelete.Size = new System.Drawing.Size(100, 28);
            this.BtnDelete.TabIndex = 2;
            this.BtnDelete.Text = "Delete";
            this.BtnDelete.UseVisualStyleBackColor = true;
            this.BtnDelete.Click += new System.EventHandler(this.BtnDelete_Click);
            // 
            // LbxDevices
            // 
            this.LbxDevices.FormattingEnabled = true;
            this.LbxDevices.HorizontalScrollbar = true;
            this.LbxDevices.ItemHeight = 16;
            this.LbxDevices.Location = new System.Drawing.Point(16, 15);
            this.LbxDevices.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.LbxDevices.Name = "LbxDevices";
            this.LbxDevices.Size = new System.Drawing.Size(483, 260);
            this.LbxDevices.TabIndex = 1;
            this.LbxDevices.DoubleClick += new System.EventHandler(this.LbxDevices_DoubleClick);
            // 
            // FrmDeviceHistory
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(516, 325);
            this.Controls.Add(this.BtnCopy);
            this.Controls.Add(this.BtnDelete);
            this.Controls.Add(this.LbxDevices);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(534, 382);
            this.Name = "FrmDeviceHistory";
            this.Text = "Device History";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FrmDeviceHistory_FormClosed);
            this.Load += new System.EventHandler(this.FrmDeviceHistory_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button BtnCopy;
        private System.Windows.Forms.Button BtnDelete;
        private System.Windows.Forms.ListBox LbxDevices;
    }
}