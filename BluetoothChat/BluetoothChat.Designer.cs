namespace BluetoothChat
{
    partial class FrmBluetoothChat
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.UsernameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ChangeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.AddressLookupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SearchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ServerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.CreateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ConnectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.CurrentUsernameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.RtbConsole = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.TxtInput = new System.Windows.Forms.TextBox();
            this.BtnSend = new System.Windows.Forms.Button();
            this.BtnExit = new System.Windows.Forms.Button();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.UsernameToolStripMenuItem,
            this.AddressLookupToolStripMenuItem,
            this.ServerToolStripMenuItem,
            this.CurrentUsernameToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1067, 28);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // UsernameToolStripMenuItem
            // 
            this.UsernameToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ChangeToolStripMenuItem});
            this.UsernameToolStripMenuItem.Name = "UsernameToolStripMenuItem";
            this.UsernameToolStripMenuItem.Size = new System.Drawing.Size(89, 24);
            this.UsernameToolStripMenuItem.Text = "Username";
            // 
            // ChangeToolStripMenuItem
            // 
            this.ChangeToolStripMenuItem.Name = "ChangeToolStripMenuItem";
            this.ChangeToolStripMenuItem.Size = new System.Drawing.Size(142, 26);
            this.ChangeToolStripMenuItem.Text = "Change";
            this.ChangeToolStripMenuItem.Click += new System.EventHandler(this.ChangeToolStripMenuItem_Click);
            // 
            // AddressLookupToolStripMenuItem
            // 
            this.AddressLookupToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.SearchToolStripMenuItem});
            this.AddressLookupToolStripMenuItem.Name = "AddressLookupToolStripMenuItem";
            this.AddressLookupToolStripMenuItem.Size = new System.Drawing.Size(129, 24);
            this.AddressLookupToolStripMenuItem.Text = "Address Lookup";
            // 
            // SearchToolStripMenuItem
            // 
            this.SearchToolStripMenuItem.Name = "SearchToolStripMenuItem";
            this.SearchToolStripMenuItem.Size = new System.Drawing.Size(136, 26);
            this.SearchToolStripMenuItem.Text = "Search";
            this.SearchToolStripMenuItem.Click += new System.EventHandler(this.SearchToolStripMenuItem_Click);
            // 
            // ServerToolStripMenuItem
            // 
            this.ServerToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.CreateToolStripMenuItem,
            this.ConnectToolStripMenuItem});
            this.ServerToolStripMenuItem.Name = "ServerToolStripMenuItem";
            this.ServerToolStripMenuItem.Size = new System.Drawing.Size(64, 24);
            this.ServerToolStripMenuItem.Text = "Server";
            // 
            // CreateToolStripMenuItem
            // 
            this.CreateToolStripMenuItem.Name = "CreateToolStripMenuItem";
            this.CreateToolStripMenuItem.Size = new System.Drawing.Size(146, 26);
            this.CreateToolStripMenuItem.Text = "Create";
            this.CreateToolStripMenuItem.Click += new System.EventHandler(this.CreateToolStripMenuItem_Click);
            // 
            // ConnectToolStripMenuItem
            // 
            this.ConnectToolStripMenuItem.Name = "ConnectToolStripMenuItem";
            this.ConnectToolStripMenuItem.Size = new System.Drawing.Size(146, 26);
            this.ConnectToolStripMenuItem.Text = "Connect";
            this.ConnectToolStripMenuItem.Click += new System.EventHandler(this.ConnectToolStripMenuItem_Click);
            // 
            // CurrentUsernameToolStripMenuItem
            // 
            this.CurrentUsernameToolStripMenuItem.Name = "CurrentUsernameToolStripMenuItem";
            this.CurrentUsernameToolStripMenuItem.Size = new System.Drawing.Size(144, 24);
            this.CurrentUsernameToolStripMenuItem.Text = "Current Username:";
            // 
            // RtbConsole
            // 
            this.RtbConsole.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.RtbConsole.Location = new System.Drawing.Point(16, 33);
            this.RtbConsole.Margin = new System.Windows.Forms.Padding(4);
            this.RtbConsole.Name = "RtbConsole";
            this.RtbConsole.ReadOnly = true;
            this.RtbConsole.Size = new System.Drawing.Size(1033, 475);
            this.RtbConsole.TabIndex = 1;
            this.RtbConsole.Text = "";
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 522);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 16);
            this.label1.TabIndex = 2;
            this.label1.Text = "Input:";
            // 
            // TxtInput
            // 
            this.TxtInput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TxtInput.Location = new System.Drawing.Point(69, 518);
            this.TxtInput.Margin = new System.Windows.Forms.Padding(4);
            this.TxtInput.MaxLength = 2000;
            this.TxtInput.Name = "TxtInput";
            this.TxtInput.Size = new System.Drawing.Size(764, 22);
            this.TxtInput.TabIndex = 3;
            // 
            // BtnSend
            // 
            this.BtnSend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnSend.Location = new System.Drawing.Point(843, 517);
            this.BtnSend.Margin = new System.Windows.Forms.Padding(4);
            this.BtnSend.Name = "BtnSend";
            this.BtnSend.Size = new System.Drawing.Size(100, 28);
            this.BtnSend.TabIndex = 4;
            this.BtnSend.Text = "Send";
            this.BtnSend.UseVisualStyleBackColor = true;
            this.BtnSend.Click += new System.EventHandler(this.BtnSend_Click);
            // 
            // BtnExit
            // 
            this.BtnExit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnExit.Location = new System.Drawing.Point(951, 517);
            this.BtnExit.Margin = new System.Windows.Forms.Padding(4);
            this.BtnExit.Name = "BtnExit";
            this.BtnExit.Size = new System.Drawing.Size(100, 28);
            this.BtnExit.TabIndex = 5;
            this.BtnExit.Text = "Exit";
            this.BtnExit.UseVisualStyleBackColor = true;
            this.BtnExit.Click += new System.EventHandler(this.BtnExit_Click);
            // 
            // FrmBluetoothChat
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1067, 554);
            this.Controls.Add(this.BtnExit);
            this.Controls.Add(this.BtnSend);
            this.Controls.Add(this.TxtInput);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.RtbConsole);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MinimumSize = new System.Drawing.Size(1082, 591);
            this.Name = "FrmBluetoothChat";
            this.Text = "Bluetooth Chat";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FrmBluetoothChat_FormClosed);
            this.Load += new System.EventHandler(this.FrmBluetoothChat_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem UsernameToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ChangeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem AddressLookupToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem SearchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem CurrentUsernameToolStripMenuItem;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolStripMenuItem ServerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem CreateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ConnectToolStripMenuItem;
        public System.Windows.Forms.RichTextBox RtbConsole;
        public System.Windows.Forms.TextBox TxtInput;
        public System.Windows.Forms.Button BtnSend;
        public System.Windows.Forms.Button BtnExit;
    }
}

