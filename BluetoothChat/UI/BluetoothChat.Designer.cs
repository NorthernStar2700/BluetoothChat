namespace BluetoothChat.UI
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
            this.HistoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ServerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.CreateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ConnectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.CurrentUsernameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.RtbConsole = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.TxtInput = new System.Windows.Forms.TextBox();
            this.BtnSend = new System.Windows.Forms.Button();
            this.BtnExit = new System.Windows.Forms.Button();
            this.ChkConnected = new System.Windows.Forms.CheckBox();
            this.GbxMembers = new System.Windows.Forms.GroupBox();
            this.LbxMembers = new System.Windows.Forms.ListBox();
            this.menuStrip1.SuspendLayout();
            this.GbxMembers.SuspendLayout();
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
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(4, 2, 0, 2);
            this.menuStrip1.Size = new System.Drawing.Size(800, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // UsernameToolStripMenuItem
            // 
            this.UsernameToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ChangeToolStripMenuItem});
            this.UsernameToolStripMenuItem.Name = "UsernameToolStripMenuItem";
            this.UsernameToolStripMenuItem.Size = new System.Drawing.Size(72, 20);
            this.UsernameToolStripMenuItem.Text = "Username";
            // 
            // ChangeToolStripMenuItem
            // 
            this.ChangeToolStripMenuItem.Name = "ChangeToolStripMenuItem";
            this.ChangeToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            this.ChangeToolStripMenuItem.Text = "Change";
            this.ChangeToolStripMenuItem.Click += new System.EventHandler(this.ChangeToolStripMenuItem_Click);
            // 
            // AddressLookupToolStripMenuItem
            // 
            this.AddressLookupToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.SearchToolStripMenuItem,
            this.HistoryToolStripMenuItem});
            this.AddressLookupToolStripMenuItem.Name = "AddressLookupToolStripMenuItem";
            this.AddressLookupToolStripMenuItem.Size = new System.Drawing.Size(104, 20);
            this.AddressLookupToolStripMenuItem.Text = "Address Lookup";
            // 
            // SearchToolStripMenuItem
            // 
            this.SearchToolStripMenuItem.Name = "SearchToolStripMenuItem";
            this.SearchToolStripMenuItem.Size = new System.Drawing.Size(112, 22);
            this.SearchToolStripMenuItem.Text = "Search";
            this.SearchToolStripMenuItem.Click += new System.EventHandler(this.SearchToolStripMenuItem_Click);
            // 
            // HistoryToolStripMenuItem
            // 
            this.HistoryToolStripMenuItem.Name = "HistoryToolStripMenuItem";
            this.HistoryToolStripMenuItem.Size = new System.Drawing.Size(112, 22);
            this.HistoryToolStripMenuItem.Text = "History";
            this.HistoryToolStripMenuItem.Click += new System.EventHandler(this.HistoryToolStripMenuItem_Click);
            // 
            // ServerToolStripMenuItem
            // 
            this.ServerToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.CreateToolStripMenuItem,
            this.ConnectToolStripMenuItem});
            this.ServerToolStripMenuItem.Name = "ServerToolStripMenuItem";
            this.ServerToolStripMenuItem.Size = new System.Drawing.Size(51, 20);
            this.ServerToolStripMenuItem.Text = "Server";
            // 
            // CreateToolStripMenuItem
            // 
            this.CreateToolStripMenuItem.Name = "CreateToolStripMenuItem";
            this.CreateToolStripMenuItem.Size = new System.Drawing.Size(119, 22);
            this.CreateToolStripMenuItem.Text = "Create";
            this.CreateToolStripMenuItem.Click += new System.EventHandler(this.CreateToolStripMenuItem_Click);
            // 
            // ConnectToolStripMenuItem
            // 
            this.ConnectToolStripMenuItem.Name = "ConnectToolStripMenuItem";
            this.ConnectToolStripMenuItem.Size = new System.Drawing.Size(119, 22);
            this.ConnectToolStripMenuItem.Text = "Connect";
            this.ConnectToolStripMenuItem.Click += new System.EventHandler(this.ConnectToolStripMenuItem_Click);
            // 
            // CurrentUsernameToolStripMenuItem
            // 
            this.CurrentUsernameToolStripMenuItem.Name = "CurrentUsernameToolStripMenuItem";
            this.CurrentUsernameToolStripMenuItem.Size = new System.Drawing.Size(118, 20);
            this.CurrentUsernameToolStripMenuItem.Text = "Current Username:";
            // 
            // RtbConsole
            // 
            this.RtbConsole.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.RtbConsole.Location = new System.Drawing.Point(12, 31);
            this.RtbConsole.Name = "RtbConsole";
            this.RtbConsole.ReadOnly = true;
            this.RtbConsole.Size = new System.Drawing.Size(614, 381);
            this.RtbConsole.TabIndex = 2;
            this.RtbConsole.Text = "";
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 422);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(34, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Input:";
            // 
            // TxtInput
            // 
            this.TxtInput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TxtInput.Location = new System.Drawing.Point(52, 419);
            this.TxtInput.MaxLength = 2000;
            this.TxtInput.Name = "TxtInput";
            this.TxtInput.Size = new System.Drawing.Size(574, 20);
            this.TxtInput.TabIndex = 4;
            // 
            // BtnSend
            // 
            this.BtnSend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnSend.Location = new System.Drawing.Point(632, 418);
            this.BtnSend.Name = "BtnSend";
            this.BtnSend.Size = new System.Drawing.Size(75, 23);
            this.BtnSend.TabIndex = 5;
            this.BtnSend.Text = "Send";
            this.BtnSend.UseVisualStyleBackColor = true;
            this.BtnSend.Click += new System.EventHandler(this.BtnSend_Click);
            // 
            // BtnExit
            // 
            this.BtnExit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnExit.Location = new System.Drawing.Point(713, 418);
            this.BtnExit.Name = "BtnExit";
            this.BtnExit.Size = new System.Drawing.Size(75, 23);
            this.BtnExit.TabIndex = 6;
            this.BtnExit.Text = "Exit";
            this.BtnExit.UseVisualStyleBackColor = true;
            this.BtnExit.Click += new System.EventHandler(this.BtnExit_Click);
            // 
            // ChkConnected
            // 
            this.ChkConnected.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ChkConnected.AutoCheck = false;
            this.ChkConnected.AutoSize = true;
            this.ChkConnected.Location = new System.Drawing.Point(675, 34);
            this.ChkConnected.Name = "ChkConnected";
            this.ChkConnected.Size = new System.Drawing.Size(78, 17);
            this.ChkConnected.TabIndex = 1;
            this.ChkConnected.Text = "Connected";
            this.ChkConnected.UseVisualStyleBackColor = true;
            // 
            // GbxMembers
            // 
            this.GbxMembers.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.GbxMembers.Controls.Add(this.LbxMembers);
            this.GbxMembers.Location = new System.Drawing.Point(632, 53);
            this.GbxMembers.Margin = new System.Windows.Forms.Padding(2);
            this.GbxMembers.Name = "GbxMembers";
            this.GbxMembers.Padding = new System.Windows.Forms.Padding(2);
            this.GbxMembers.Size = new System.Drawing.Size(156, 360);
            this.GbxMembers.TabIndex = 7;
            this.GbxMembers.TabStop = false;
            this.GbxMembers.Text = "Members";
            // 
            // LbxMembers
            // 
            this.LbxMembers.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LbxMembers.FormattingEnabled = true;
            this.LbxMembers.HorizontalScrollbar = true;
            this.LbxMembers.Location = new System.Drawing.Point(4, 17);
            this.LbxMembers.Margin = new System.Windows.Forms.Padding(2);
            this.LbxMembers.Name = "LbxMembers";
            this.LbxMembers.Size = new System.Drawing.Size(148, 329);
            this.LbxMembers.TabIndex = 0;
            // 
            // FrmBluetoothChat
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 448);
            this.Controls.Add(this.GbxMembers);
            this.Controls.Add(this.ChkConnected);
            this.Controls.Add(this.BtnExit);
            this.Controls.Add(this.BtnSend);
            this.Controls.Add(this.TxtInput);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.RtbConsole);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.MinimumSize = new System.Drawing.Size(816, 486);
            this.Name = "FrmBluetoothChat";
            this.Text = "Bluetooth Chat";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmBluetoothChat_FormClosing);
            this.Load += new System.EventHandler(this.FrmBluetoothChat_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.GbxMembers.ResumeLayout(false);
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
        private System.Windows.Forms.CheckBox ChkConnected;
        private System.Windows.Forms.ToolStripMenuItem HistoryToolStripMenuItem;
        private System.Windows.Forms.GroupBox GbxMembers;
        private System.Windows.Forms.ListBox LbxMembers;
        private System.Windows.Forms.RichTextBox RtbConsole;
        private System.Windows.Forms.TextBox TxtInput;
        private System.Windows.Forms.Button BtnSend;
        private System.Windows.Forms.Button BtnExit;
    }
}

