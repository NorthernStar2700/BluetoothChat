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
            this.BtnReload = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // BtnCopy
            // 
            this.BtnCopy.Location = new System.Drawing.Point(232, 232);
            this.BtnCopy.Name = "BtnCopy";
            this.BtnCopy.Size = new System.Drawing.Size(75, 23);
            this.BtnCopy.TabIndex = 4;
            this.BtnCopy.Text = "Copy";
            this.BtnCopy.UseVisualStyleBackColor = true;
            this.BtnCopy.Click += new System.EventHandler(this.BtnCopy_Click);
            // 
            // BtnDelete
            // 
            this.BtnDelete.Location = new System.Drawing.Point(70, 232);
            this.BtnDelete.Name = "BtnDelete";
            this.BtnDelete.Size = new System.Drawing.Size(75, 23);
            this.BtnDelete.TabIndex = 2;
            this.BtnDelete.Text = "Delete";
            this.BtnDelete.UseVisualStyleBackColor = true;
            this.BtnDelete.Click += new System.EventHandler(this.BtnDelete_Click);
            // 
            // LbxDevices
            // 
            this.LbxDevices.FormattingEnabled = true;
            this.LbxDevices.HorizontalScrollbar = true;
            this.LbxDevices.Location = new System.Drawing.Point(12, 12);
            this.LbxDevices.Name = "LbxDevices";
            this.LbxDevices.Size = new System.Drawing.Size(363, 212);
            this.LbxDevices.TabIndex = 1;
            this.LbxDevices.DoubleClick += new System.EventHandler(this.LbxDevices_DoubleClick);
            // 
            // BtnReload
            // 
            this.BtnReload.Location = new System.Drawing.Point(151, 232);
            this.BtnReload.Name = "BtnReload";
            this.BtnReload.Size = new System.Drawing.Size(75, 23);
            this.BtnReload.TabIndex = 3;
            this.BtnReload.Text = "Reload";
            this.BtnReload.UseVisualStyleBackColor = true;
            this.BtnReload.Click += new System.EventHandler(this.BtnReload_Click);
            // 
            // FrmDeviceHistory
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(389, 264);
            this.Controls.Add(this.BtnReload);
            this.Controls.Add(this.BtnCopy);
            this.Controls.Add(this.BtnDelete);
            this.Controls.Add(this.LbxDevices);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(405, 319);
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
        private System.Windows.Forms.Button BtnReload;
    }
}