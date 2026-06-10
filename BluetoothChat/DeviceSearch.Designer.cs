namespace BluetoothChat
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
            this.LblSearch.Location = new System.Drawing.Point(16, 10);
            this.LblSearch.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.LblSearch.Name = "LblSearch";
            this.LblSearch.Size = new System.Drawing.Size(77, 16);
            this.LblSearch.TabIndex = 0;
            this.LblSearch.Text = "Searching...";
            // 
            // LbxDevices
            // 
            this.LbxDevices.FormattingEnabled = true;
            this.LbxDevices.ItemHeight = 16;
            this.LbxDevices.Location = new System.Drawing.Point(19, 36);
            this.LbxDevices.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.LbxDevices.Name = "LbxDevices";
            this.LbxDevices.Size = new System.Drawing.Size(483, 260);
            this.LbxDevices.TabIndex = 1;
            // 
            // BtnRestart
            // 
            this.BtnRestart.Location = new System.Drawing.Point(156, 306);
            this.BtnRestart.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.BtnRestart.Name = "BtnRestart";
            this.BtnRestart.Size = new System.Drawing.Size(100, 28);
            this.BtnRestart.TabIndex = 2;
            this.BtnRestart.Text = "Restart";
            this.BtnRestart.UseVisualStyleBackColor = true;
            this.BtnRestart.Click += new System.EventHandler(this.BtnRestart_Click);
            // 
            // BtnCopy
            // 
            this.BtnCopy.Location = new System.Drawing.Point(264, 306);
            this.BtnCopy.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.BtnCopy.Name = "BtnCopy";
            this.BtnCopy.Size = new System.Drawing.Size(100, 28);
            this.BtnCopy.TabIndex = 3;
            this.BtnCopy.Text = "Copy";
            this.BtnCopy.UseVisualStyleBackColor = true;
            this.BtnCopy.Click += new System.EventHandler(this.BtnCopy_Click);
            // 
            // FrmDeviceSearch
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(517, 336);
            this.Controls.Add(this.BtnCopy);
            this.Controls.Add(this.BtnRestart);
            this.Controls.Add(this.LbxDevices);
            this.Controls.Add(this.LblSearch);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(535, 383);
            this.MinimumSize = new System.Drawing.Size(535, 383);
            this.Name = "FrmDeviceSearch";
            this.Text = "Device Searcher";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmDeviceSearch_FormClosing);
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