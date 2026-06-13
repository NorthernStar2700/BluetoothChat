namespace BluetoothChat.UI
{
    partial class FrmDeviceSearch
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
            this.LblSearch = new System.Windows.Forms.Label();
            this.LbxDevices = new System.Windows.Forms.ListBox();
            this.BtnRestart = new System.Windows.Forms.Button();
            this.BtnCopy = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // LblSearch
            // 
            this.LblSearch.AutoSize = true;
            this.LblSearch.Location = new System.Drawing.Point(10, 8);
            this.LblSearch.Name = "LblSearch";
            this.LblSearch.Size = new System.Drawing.Size(64, 13);
            this.LblSearch.TabIndex = 0;
            this.LblSearch.Text = "Searching...";
            // 
            // LbxDevices
            // 
            this.LbxDevices.FormattingEnabled = true;
            this.LbxDevices.HorizontalScrollbar = true;
            this.LbxDevices.Location = new System.Drawing.Point(12, 29);
            this.LbxDevices.Name = "LbxDevices";
            this.LbxDevices.Size = new System.Drawing.Size(363, 212);
            this.LbxDevices.TabIndex = 1;
            this.LbxDevices.DoubleClick += new System.EventHandler(this.LbxDevices_DoubleClick);
            // 
            // BtnRestart
            // 
            this.BtnRestart.Location = new System.Drawing.Point(113, 249);
            this.BtnRestart.Name = "BtnRestart";
            this.BtnRestart.Size = new System.Drawing.Size(75, 23);
            this.BtnRestart.TabIndex = 2;
            this.BtnRestart.Text = "Restart";
            this.BtnRestart.UseVisualStyleBackColor = true;
            this.BtnRestart.Click += new System.EventHandler(this.BtnRestart_Click);
            // 
            // BtnCopy
            // 
            this.BtnCopy.Location = new System.Drawing.Point(194, 249);
            this.BtnCopy.Name = "BtnCopy";
            this.BtnCopy.Size = new System.Drawing.Size(75, 23);
            this.BtnCopy.TabIndex = 3;
            this.BtnCopy.Text = "Copy";
            this.BtnCopy.UseVisualStyleBackColor = true;
            this.BtnCopy.Click += new System.EventHandler(this.BtnCopy_Click);
            // 
            // FrmDeviceSearch
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(389, 280);
            this.Controls.Add(this.BtnCopy);
            this.Controls.Add(this.BtnRestart);
            this.Controls.Add(this.LbxDevices);
            this.Controls.Add(this.LblSearch);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(405, 319);
            this.MinimumSize = new System.Drawing.Size(405, 319);
            this.Name = "FrmDeviceSearch";
            this.Text = "Device Searcher";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FrmDeviceSearch_FormClosed);
            this.Load += new System.EventHandler(this.FrmDeviceSearch_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label LblSearch;
        private System.Windows.Forms.ListBox LbxDevices;
        private System.Windows.Forms.Button BtnRestart;
        private System.Windows.Forms.Button BtnCopy;
    }
}