namespace TestOPC3
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.CheckGroupActive = new System.Windows.Forms.CheckBox();
            this.TxtAWriteComplete = new System.Windows.Forms.TextBox();
            this.TxtAReadComplete = new System.Windows.Forms.TextBox();
            this.TxtDataChange = new System.Windows.Forms.TextBox();
            this._txtChangeVal_1 = new System.Windows.Forms.TextBox();
            this._txtChangeVal_0 = new System.Windows.Forms.TextBox();
            this._txtReadVal_1 = new System.Windows.Forms.TextBox();
            this.txtWriteVal2 = new System.Windows.Forms.TextBox();
            this.txtItem2 = new System.Windows.Forms.TextBox();
            this.txtServer = new System.Windows.Forms.TextBox();
            this.cmdExit = new System.Windows.Forms.Button();
            this.cmdReadAsync = new System.Windows.Forms.Button();
            this.cmdReadSync = new System.Windows.Forms.Button();
            this.cmdWriteAsync = new System.Windows.Forms.Button();
            this.cmdWriteSync = new System.Windows.Forms.Button();
            this._txtReadVal_0 = new System.Windows.Forms.TextBox();
            this.txtWriteVal1 = new System.Windows.Forms.TextBox();
            this.cmdRemItem = new System.Windows.Forms.Button();
            this.cmdAddItem = new System.Windows.Forms.Button();
            this.cmdRemGroup = new System.Windows.Forms.Button();
            this.cmdAddGroup = new System.Windows.Forms.Button();
            this.cmdDisconnect = new System.Windows.Forms.Button();
            this.txtItem1 = new System.Windows.Forms.TextBox();
            this.txtGroup = new System.Windows.Forms.TextBox();
            this.cmdConnect = new System.Windows.Forms.Button();
            this.Label10 = new System.Windows.Forms.Label();
            this.Label9 = new System.Windows.Forms.Label();
            this.Label8 = new System.Windows.Forms.Label();
            this.Label7 = new System.Windows.Forms.Label();
            this.Label6 = new System.Windows.Forms.Label();
            this.Label5 = new System.Windows.Forms.Label();
            this.Label4 = new System.Windows.Forms.Label();
            this.Label3 = new System.Windows.Forms.Label();
            this.Label2 = new System.Windows.Forms.Label();
            this.Label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // CheckGroupActive
            // 
            this.CheckGroupActive.BackColor = System.Drawing.SystemColors.Control;
            this.CheckGroupActive.Cursor = System.Windows.Forms.Cursors.Default;
            this.CheckGroupActive.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CheckGroupActive.ForeColor = System.Drawing.SystemColors.ControlText;
            this.CheckGroupActive.Location = new System.Drawing.Point(552, 128);
            this.CheckGroupActive.Name = "CheckGroupActive";
            this.CheckGroupActive.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.CheckGroupActive.Size = new System.Drawing.Size(137, 17);
            this.CheckGroupActive.TabIndex = 68;
            this.CheckGroupActive.Text = "Group Active State";
            this.CheckGroupActive.UseVisualStyleBackColor = false;
            this.CheckGroupActive.CheckedChanged += new System.EventHandler(this.CheckGroupActive_CheckedChanged);
            // 
            // TxtAWriteComplete
            // 
            this.TxtAWriteComplete.AcceptsReturn = true;
            this.TxtAWriteComplete.BackColor = System.Drawing.SystemColors.Window;
            this.TxtAWriteComplete.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.TxtAWriteComplete.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TxtAWriteComplete.ForeColor = System.Drawing.SystemColors.WindowText;
            this.TxtAWriteComplete.Location = new System.Drawing.Point(857, 208);
            this.TxtAWriteComplete.MaxLength = 0;
            this.TxtAWriteComplete.Name = "TxtAWriteComplete";
            this.TxtAWriteComplete.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.TxtAWriteComplete.Size = new System.Drawing.Size(49, 20);
            this.TxtAWriteComplete.TabIndex = 66;
            this.TxtAWriteComplete.Text = "0";
            this.TxtAWriteComplete.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // TxtAReadComplete
            // 
            this.TxtAReadComplete.AcceptsReturn = true;
            this.TxtAReadComplete.BackColor = System.Drawing.SystemColors.Window;
            this.TxtAReadComplete.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.TxtAReadComplete.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TxtAReadComplete.ForeColor = System.Drawing.SystemColors.WindowText;
            this.TxtAReadComplete.Location = new System.Drawing.Point(857, 240);
            this.TxtAReadComplete.MaxLength = 0;
            this.TxtAReadComplete.Name = "TxtAReadComplete";
            this.TxtAReadComplete.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.TxtAReadComplete.Size = new System.Drawing.Size(49, 20);
            this.TxtAReadComplete.TabIndex = 65;
            this.TxtAReadComplete.Text = "0";
            this.TxtAReadComplete.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // TxtDataChange
            // 
            this.TxtDataChange.AcceptsReturn = true;
            this.TxtDataChange.BackColor = System.Drawing.SystemColors.Window;
            this.TxtDataChange.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.TxtDataChange.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TxtDataChange.ForeColor = System.Drawing.SystemColors.WindowText;
            this.TxtDataChange.Location = new System.Drawing.Point(857, 272);
            this.TxtDataChange.MaxLength = 0;
            this.TxtDataChange.Name = "TxtDataChange";
            this.TxtDataChange.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.TxtDataChange.Size = new System.Drawing.Size(49, 20);
            this.TxtDataChange.TabIndex = 64;
            this.TxtDataChange.Text = "0";
            this.TxtDataChange.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // _txtChangeVal_1
            // 
            this._txtChangeVal_1.AcceptsReturn = true;
            this._txtChangeVal_1.BackColor = System.Drawing.SystemColors.Window;
            this._txtChangeVal_1.Cursor = System.Windows.Forms.Cursors.IBeam;
            this._txtChangeVal_1.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._txtChangeVal_1.ForeColor = System.Drawing.SystemColors.WindowText;
            this._txtChangeVal_1.Location = new System.Drawing.Point(371, 272);
            this._txtChangeVal_1.MaxLength = 0;
            this._txtChangeVal_1.Name = "_txtChangeVal_1";
            this._txtChangeVal_1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._txtChangeVal_1.Size = new System.Drawing.Size(253, 20);
            this._txtChangeVal_1.TabIndex = 63;
            this._txtChangeVal_1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // _txtChangeVal_0
            // 
            this._txtChangeVal_0.AcceptsReturn = true;
            this._txtChangeVal_0.BackColor = System.Drawing.SystemColors.Window;
            this._txtChangeVal_0.Cursor = System.Windows.Forms.Cursors.IBeam;
            this._txtChangeVal_0.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._txtChangeVal_0.ForeColor = System.Drawing.SystemColors.WindowText;
            this._txtChangeVal_0.Location = new System.Drawing.Point(112, 272);
            this._txtChangeVal_0.MaxLength = 0;
            this._txtChangeVal_0.Name = "_txtChangeVal_0";
            this._txtChangeVal_0.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._txtChangeVal_0.Size = new System.Drawing.Size(253, 20);
            this._txtChangeVal_0.TabIndex = 61;
            this._txtChangeVal_0.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // _txtReadVal_1
            // 
            this._txtReadVal_1.AcceptsReturn = true;
            this._txtReadVal_1.BackColor = System.Drawing.SystemColors.Window;
            this._txtReadVal_1.Cursor = System.Windows.Forms.Cursors.IBeam;
            this._txtReadVal_1.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._txtReadVal_1.ForeColor = System.Drawing.SystemColors.WindowText;
            this._txtReadVal_1.Location = new System.Drawing.Point(371, 240);
            this._txtReadVal_1.MaxLength = 0;
            this._txtReadVal_1.Name = "_txtReadVal_1";
            this._txtReadVal_1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._txtReadVal_1.Size = new System.Drawing.Size(253, 20);
            this._txtReadVal_1.TabIndex = 60;
            this._txtReadVal_1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtWriteVal2
            // 
            this.txtWriteVal2.AcceptsReturn = true;
            this.txtWriteVal2.BackColor = System.Drawing.SystemColors.Window;
            this.txtWriteVal2.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtWriteVal2.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtWriteVal2.ForeColor = System.Drawing.SystemColors.WindowText;
            this.txtWriteVal2.Location = new System.Drawing.Point(371, 208);
            this.txtWriteVal2.MaxLength = 0;
            this.txtWriteVal2.Name = "txtWriteVal2";
            this.txtWriteVal2.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.txtWriteVal2.Size = new System.Drawing.Size(253, 20);
            this.txtWriteVal2.TabIndex = 59;
            this.txtWriteVal2.Text = "0";
            this.txtWriteVal2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtItem2
            // 
            this.txtItem2.AcceptsReturn = true;
            this.txtItem2.BackColor = System.Drawing.SystemColors.Window;
            this.txtItem2.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtItem2.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtItem2.ForeColor = System.Drawing.SystemColors.WindowText;
            this.txtItem2.Location = new System.Drawing.Point(371, 176);
            this.txtItem2.MaxLength = 0;
            this.txtItem2.Name = "txtItem2";
            this.txtItem2.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.txtItem2.Size = new System.Drawing.Size(253, 20);
            this.txtItem2.TabIndex = 57;
            this.txtItem2.Text = "2:192.168.2.1:0201:0201,VW4,WORD,RW";
            // 
            // txtServer
            // 
            this.txtServer.AcceptsReturn = true;
            this.txtServer.BackColor = System.Drawing.SystemColors.Window;
            this.txtServer.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtServer.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtServer.ForeColor = System.Drawing.SystemColors.WindowText;
            this.txtServer.Location = new System.Drawing.Point(112, 80);
            this.txtServer.MaxLength = 0;
            this.txtServer.Name = "txtServer";
            this.txtServer.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.txtServer.Size = new System.Drawing.Size(177, 20);
            this.txtServer.TabIndex = 56;
            this.txtServer.Text = "S7200SMART.OPCServer";
            // 
            // cmdExit
            // 
            this.cmdExit.BackColor = System.Drawing.SystemColors.Control;
            this.cmdExit.Cursor = System.Windows.Forms.Cursors.Default;
            this.cmdExit.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdExit.ForeColor = System.Drawing.SystemColors.ControlText;
            this.cmdExit.Location = new System.Drawing.Point(24, 320);
            this.cmdExit.Name = "cmdExit";
            this.cmdExit.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.cmdExit.Size = new System.Drawing.Size(73, 25);
            this.cmdExit.TabIndex = 55;
            this.cmdExit.Text = "Exit";
            this.cmdExit.UseVisualStyleBackColor = false;
            this.cmdExit.Click += new System.EventHandler(this.cmdExit_Click);
            // 
            // cmdReadAsync
            // 
            this.cmdReadAsync.BackColor = System.Drawing.SystemColors.Control;
            this.cmdReadAsync.Cursor = System.Windows.Forms.Cursors.Default;
            this.cmdReadAsync.Enabled = false;
            this.cmdReadAsync.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdReadAsync.ForeColor = System.Drawing.SystemColors.ControlText;
            this.cmdReadAsync.Location = new System.Drawing.Point(753, 240);
            this.cmdReadAsync.Name = "cmdReadAsync";
            this.cmdReadAsync.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.cmdReadAsync.Size = new System.Drawing.Size(89, 25);
            this.cmdReadAsync.TabIndex = 54;
            this.cmdReadAsync.Text = "Read Asyncron";
            this.cmdReadAsync.UseVisualStyleBackColor = false;
            this.cmdReadAsync.Click += new System.EventHandler(this.cmdReadAsync_Click);
            // 
            // cmdReadSync
            // 
            this.cmdReadSync.BackColor = System.Drawing.SystemColors.Control;
            this.cmdReadSync.Cursor = System.Windows.Forms.Cursors.Default;
            this.cmdReadSync.Enabled = false;
            this.cmdReadSync.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdReadSync.ForeColor = System.Drawing.SystemColors.ControlText;
            this.cmdReadSync.Location = new System.Drawing.Point(657, 240);
            this.cmdReadSync.Name = "cmdReadSync";
            this.cmdReadSync.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.cmdReadSync.Size = new System.Drawing.Size(89, 25);
            this.cmdReadSync.TabIndex = 53;
            this.cmdReadSync.Text = "Read Syncron";
            this.cmdReadSync.UseVisualStyleBackColor = false;
            this.cmdReadSync.Click += new System.EventHandler(this.cmdReadSync_Click);
            // 
            // cmdWriteAsync
            // 
            this.cmdWriteAsync.BackColor = System.Drawing.SystemColors.Control;
            this.cmdWriteAsync.Cursor = System.Windows.Forms.Cursors.Default;
            this.cmdWriteAsync.Enabled = false;
            this.cmdWriteAsync.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdWriteAsync.ForeColor = System.Drawing.SystemColors.ControlText;
            this.cmdWriteAsync.Location = new System.Drawing.Point(753, 208);
            this.cmdWriteAsync.Name = "cmdWriteAsync";
            this.cmdWriteAsync.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.cmdWriteAsync.Size = new System.Drawing.Size(89, 25);
            this.cmdWriteAsync.TabIndex = 52;
            this.cmdWriteAsync.Text = "Write Asyncron";
            this.cmdWriteAsync.UseVisualStyleBackColor = false;
            this.cmdWriteAsync.Click += new System.EventHandler(this.cmdWriteAsync_Click);
            // 
            // cmdWriteSync
            // 
            this.cmdWriteSync.BackColor = System.Drawing.SystemColors.Control;
            this.cmdWriteSync.Cursor = System.Windows.Forms.Cursors.Default;
            this.cmdWriteSync.Enabled = false;
            this.cmdWriteSync.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdWriteSync.ForeColor = System.Drawing.SystemColors.ControlText;
            this.cmdWriteSync.Location = new System.Drawing.Point(657, 208);
            this.cmdWriteSync.Name = "cmdWriteSync";
            this.cmdWriteSync.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.cmdWriteSync.Size = new System.Drawing.Size(89, 25);
            this.cmdWriteSync.TabIndex = 51;
            this.cmdWriteSync.Text = "Write Syncron";
            this.cmdWriteSync.UseVisualStyleBackColor = false;
            this.cmdWriteSync.Click += new System.EventHandler(this.cmdWriteSync_Click);
            // 
            // _txtReadVal_0
            // 
            this._txtReadVal_0.AcceptsReturn = true;
            this._txtReadVal_0.BackColor = System.Drawing.SystemColors.Window;
            this._txtReadVal_0.Cursor = System.Windows.Forms.Cursors.IBeam;
            this._txtReadVal_0.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._txtReadVal_0.ForeColor = System.Drawing.SystemColors.WindowText;
            this._txtReadVal_0.Location = new System.Drawing.Point(112, 240);
            this._txtReadVal_0.MaxLength = 0;
            this._txtReadVal_0.Name = "_txtReadVal_0";
            this._txtReadVal_0.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this._txtReadVal_0.Size = new System.Drawing.Size(253, 20);
            this._txtReadVal_0.TabIndex = 50;
            this._txtReadVal_0.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtWriteVal1
            // 
            this.txtWriteVal1.AcceptsReturn = true;
            this.txtWriteVal1.BackColor = System.Drawing.SystemColors.Window;
            this.txtWriteVal1.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtWriteVal1.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtWriteVal1.ForeColor = System.Drawing.SystemColors.WindowText;
            this.txtWriteVal1.Location = new System.Drawing.Point(112, 208);
            this.txtWriteVal1.MaxLength = 0;
            this.txtWriteVal1.Name = "txtWriteVal1";
            this.txtWriteVal1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.txtWriteVal1.Size = new System.Drawing.Size(253, 20);
            this.txtWriteVal1.TabIndex = 49;
            this.txtWriteVal1.Text = "0";
            this.txtWriteVal1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // cmdRemItem
            // 
            this.cmdRemItem.BackColor = System.Drawing.SystemColors.Control;
            this.cmdRemItem.Cursor = System.Windows.Forms.Cursors.Default;
            this.cmdRemItem.Enabled = false;
            this.cmdRemItem.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdRemItem.ForeColor = System.Drawing.SystemColors.ControlText;
            this.cmdRemItem.Location = new System.Drawing.Point(753, 176);
            this.cmdRemItem.Name = "cmdRemItem";
            this.cmdRemItem.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.cmdRemItem.Size = new System.Drawing.Size(89, 25);
            this.cmdRemItem.TabIndex = 46;
            this.cmdRemItem.Text = "Remove Item";
            this.cmdRemItem.UseVisualStyleBackColor = false;
            this.cmdRemItem.Click += new System.EventHandler(this.cmdRemItem_Click);
            // 
            // cmdAddItem
            // 
            this.cmdAddItem.BackColor = System.Drawing.SystemColors.Control;
            this.cmdAddItem.Cursor = System.Windows.Forms.Cursors.Default;
            this.cmdAddItem.Enabled = false;
            this.cmdAddItem.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdAddItem.ForeColor = System.Drawing.SystemColors.ControlText;
            this.cmdAddItem.Location = new System.Drawing.Point(657, 176);
            this.cmdAddItem.Name = "cmdAddItem";
            this.cmdAddItem.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.cmdAddItem.Size = new System.Drawing.Size(89, 25);
            this.cmdAddItem.TabIndex = 45;
            this.cmdAddItem.Text = "Add Item";
            this.cmdAddItem.UseVisualStyleBackColor = false;
            this.cmdAddItem.Click += new System.EventHandler(this.cmdAddItem_Click);
            // 
            // cmdRemGroup
            // 
            this.cmdRemGroup.BackColor = System.Drawing.SystemColors.Control;
            this.cmdRemGroup.Cursor = System.Windows.Forms.Cursors.Default;
            this.cmdRemGroup.Enabled = false;
            this.cmdRemGroup.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdRemGroup.ForeColor = System.Drawing.SystemColors.ControlText;
            this.cmdRemGroup.Location = new System.Drawing.Point(440, 120);
            this.cmdRemGroup.Name = "cmdRemGroup";
            this.cmdRemGroup.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.cmdRemGroup.Size = new System.Drawing.Size(89, 25);
            this.cmdRemGroup.TabIndex = 44;
            this.cmdRemGroup.Text = "Remove Group";
            this.cmdRemGroup.UseVisualStyleBackColor = false;
            this.cmdRemGroup.Click += new System.EventHandler(this.cmdRemGroup_Click);
            // 
            // cmdAddGroup
            // 
            this.cmdAddGroup.BackColor = System.Drawing.SystemColors.Control;
            this.cmdAddGroup.Cursor = System.Windows.Forms.Cursors.Default;
            this.cmdAddGroup.Enabled = false;
            this.cmdAddGroup.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdAddGroup.ForeColor = System.Drawing.SystemColors.ControlText;
            this.cmdAddGroup.Location = new System.Drawing.Point(344, 120);
            this.cmdAddGroup.Name = "cmdAddGroup";
            this.cmdAddGroup.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.cmdAddGroup.Size = new System.Drawing.Size(89, 25);
            this.cmdAddGroup.TabIndex = 43;
            this.cmdAddGroup.Text = "Add Group";
            this.cmdAddGroup.UseVisualStyleBackColor = false;
            this.cmdAddGroup.Click += new System.EventHandler(this.cmdAddGroup_Click);
            // 
            // cmdDisconnect
            // 
            this.cmdDisconnect.BackColor = System.Drawing.SystemColors.Control;
            this.cmdDisconnect.Cursor = System.Windows.Forms.Cursors.Default;
            this.cmdDisconnect.Enabled = false;
            this.cmdDisconnect.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdDisconnect.ForeColor = System.Drawing.SystemColors.ControlText;
            this.cmdDisconnect.Location = new System.Drawing.Point(440, 80);
            this.cmdDisconnect.Name = "cmdDisconnect";
            this.cmdDisconnect.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.cmdDisconnect.Size = new System.Drawing.Size(89, 25);
            this.cmdDisconnect.TabIndex = 42;
            this.cmdDisconnect.Text = "Disconnect";
            this.cmdDisconnect.UseVisualStyleBackColor = false;
            this.cmdDisconnect.Click += new System.EventHandler(this.cmdDisconnect_Click);
            // 
            // txtItem1
            // 
            this.txtItem1.AcceptsReturn = true;
            this.txtItem1.BackColor = System.Drawing.SystemColors.Window;
            this.txtItem1.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtItem1.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtItem1.ForeColor = System.Drawing.SystemColors.WindowText;
            this.txtItem1.Location = new System.Drawing.Point(112, 176);
            this.txtItem1.MaxLength = 0;
            this.txtItem1.Name = "txtItem1";
            this.txtItem1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.txtItem1.Size = new System.Drawing.Size(253, 20);
            this.txtItem1.TabIndex = 41;
            this.txtItem1.Text = "2:192.168.2.1:0201:0201,MB0,BYTE,RW";
            // 
            // txtGroup
            // 
            this.txtGroup.AcceptsReturn = true;
            this.txtGroup.BackColor = System.Drawing.SystemColors.Window;
            this.txtGroup.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtGroup.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtGroup.ForeColor = System.Drawing.SystemColors.WindowText;
            this.txtGroup.Location = new System.Drawing.Point(112, 120);
            this.txtGroup.MaxLength = 0;
            this.txtGroup.Name = "txtGroup";
            this.txtGroup.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.txtGroup.Size = new System.Drawing.Size(177, 20);
            this.txtGroup.TabIndex = 37;
            this.txtGroup.Text = "Group1";
            // 
            // cmdConnect
            // 
            this.cmdConnect.BackColor = System.Drawing.SystemColors.Control;
            this.cmdConnect.Cursor = System.Windows.Forms.Cursors.Default;
            this.cmdConnect.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdConnect.ForeColor = System.Drawing.SystemColors.ControlText;
            this.cmdConnect.Location = new System.Drawing.Point(344, 80);
            this.cmdConnect.Name = "cmdConnect";
            this.cmdConnect.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.cmdConnect.Size = new System.Drawing.Size(89, 25);
            this.cmdConnect.TabIndex = 36;
            this.cmdConnect.Text = "Connect";
            this.cmdConnect.UseVisualStyleBackColor = false;
            this.cmdConnect.Click += new System.EventHandler(this.cmdConnect_Click);
            // 
            // Label10
            // 
            this.Label10.BackColor = System.Drawing.SystemColors.Control;
            this.Label10.Cursor = System.Windows.Forms.Cursors.Default;
            this.Label10.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label10.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Label10.Location = new System.Drawing.Point(657, 275);
            this.Label10.Name = "Label10";
            this.Label10.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.Label10.Size = new System.Drawing.Size(185, 17);
            this.Label10.TabIndex = 69;
            this.Label10.Text = "Goup Active State must be TRUE";
            this.Label10.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // Label9
            // 
            this.Label9.BackColor = System.Drawing.SystemColors.Control;
            this.Label9.Cursor = System.Windows.Forms.Cursors.Default;
            this.Label9.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label9.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Label9.Location = new System.Drawing.Point(857, 304);
            this.Label9.Name = "Label9";
            this.Label9.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.Label9.Size = new System.Drawing.Size(49, 33);
            this.Label9.TabIndex = 67;
            this.Label9.Text = "Callback Count";
            this.Label9.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // Label8
            // 
            this.Label8.BackColor = System.Drawing.SystemColors.Control;
            this.Label8.Cursor = System.Windows.Forms.Cursors.Default;
            this.Label8.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label8.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Label8.Location = new System.Drawing.Point(16, 272);
            this.Label8.Name = "Label8";
            this.Label8.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.Label8.Size = new System.Drawing.Size(89, 25);
            this.Label8.TabIndex = 62;
            this.Label8.Text = "DataChange:";
            this.Label8.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // Label7
            // 
            this.Label7.BackColor = System.Drawing.SystemColors.Control;
            this.Label7.Cursor = System.Windows.Forms.Cursors.Default;
            this.Label7.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label7.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Label7.Location = new System.Drawing.Point(371, 160);
            this.Label7.Name = "Label7";
            this.Label7.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.Label7.Size = new System.Drawing.Size(81, 17);
            this.Label7.TabIndex = 58;
            this.Label7.Text = "Item 2";
            // 
            // Label6
            // 
            this.Label6.BackColor = System.Drawing.SystemColors.Control;
            this.Label6.Cursor = System.Windows.Forms.Cursors.Default;
            this.Label6.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label6.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Label6.Location = new System.Drawing.Point(24, 240);
            this.Label6.Name = "Label6";
            this.Label6.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.Label6.Size = new System.Drawing.Size(81, 25);
            this.Label6.TabIndex = 48;
            this.Label6.Text = "Read Value:";
            this.Label6.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // Label5
            // 
            this.Label5.BackColor = System.Drawing.SystemColors.Control;
            this.Label5.Cursor = System.Windows.Forms.Cursors.Default;
            this.Label5.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label5.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Label5.Location = new System.Drawing.Point(24, 208);
            this.Label5.Name = "Label5";
            this.Label5.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.Label5.Size = new System.Drawing.Size(81, 17);
            this.Label5.TabIndex = 47;
            this.Label5.Text = "Write Value:";
            this.Label5.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // Label4
            // 
            this.Label4.BackColor = System.Drawing.SystemColors.Control;
            this.Label4.Cursor = System.Windows.Forms.Cursors.Default;
            this.Label4.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label4.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Label4.Location = new System.Drawing.Point(112, 160);
            this.Label4.Name = "Label4";
            this.Label4.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.Label4.Size = new System.Drawing.Size(81, 17);
            this.Label4.TabIndex = 40;
            this.Label4.Text = "Item 1";
            // 
            // Label3
            // 
            this.Label3.BackColor = System.Drawing.SystemColors.Control;
            this.Label3.Cursor = System.Windows.Forms.Cursors.Default;
            this.Label3.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label3.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Label3.Location = new System.Drawing.Point(33, 120);
            this.Label3.Name = "Label3";
            this.Label3.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.Label3.Size = new System.Drawing.Size(81, 17);
            this.Label3.TabIndex = 39;
            this.Label3.Text = "Group Name:";
            this.Label3.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // Label2
            // 
            this.Label2.BackColor = System.Drawing.SystemColors.Control;
            this.Label2.Cursor = System.Windows.Forms.Cursors.Default;
            this.Label2.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label2.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Label2.Location = new System.Drawing.Point(17, 80);
            this.Label2.Name = "Label2";
            this.Label2.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.Label2.Size = new System.Drawing.Size(97, 17);
            this.Label2.TabIndex = 38;
            this.Label2.Text = "Selected Server:";
            this.Label2.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // Label1
            // 
            this.Label1.BackColor = System.Drawing.SystemColors.Control;
            this.Label1.Cursor = System.Windows.Forms.Cursors.Default;
            this.Label1.Font = new System.Drawing.Font("Arial", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label1.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Label1.Location = new System.Drawing.Point(201, 17);
            this.Label1.Name = "Label1";
            this.Label1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.Label1.Size = new System.Drawing.Size(529, 41);
            this.Label1.TabIndex = 35;
            this.Label1.Text = "S7-200 SMART OPC Sample for C#";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(931, 359);
            this.Controls.Add(this.CheckGroupActive);
            this.Controls.Add(this.TxtAWriteComplete);
            this.Controls.Add(this.TxtAReadComplete);
            this.Controls.Add(this.TxtDataChange);
            this.Controls.Add(this._txtChangeVal_1);
            this.Controls.Add(this._txtChangeVal_0);
            this.Controls.Add(this._txtReadVal_1);
            this.Controls.Add(this.txtWriteVal2);
            this.Controls.Add(this.txtItem2);
            this.Controls.Add(this.txtServer);
            this.Controls.Add(this.cmdExit);
            this.Controls.Add(this.cmdReadAsync);
            this.Controls.Add(this.cmdReadSync);
            this.Controls.Add(this.cmdWriteAsync);
            this.Controls.Add(this.cmdWriteSync);
            this.Controls.Add(this._txtReadVal_0);
            this.Controls.Add(this.txtWriteVal1);
            this.Controls.Add(this.cmdRemItem);
            this.Controls.Add(this.cmdAddItem);
            this.Controls.Add(this.cmdRemGroup);
            this.Controls.Add(this.cmdAddGroup);
            this.Controls.Add(this.cmdDisconnect);
            this.Controls.Add(this.txtItem1);
            this.Controls.Add(this.txtGroup);
            this.Controls.Add(this.cmdConnect);
            this.Controls.Add(this.Label10);
            this.Controls.Add(this.Label9);
            this.Controls.Add(this.Label8);
            this.Controls.Add(this.Label7);
            this.Controls.Add(this.Label6);
            this.Controls.Add(this.Label5);
            this.Controls.Add(this.Label4);
            this.Controls.Add(this.Label3);
            this.Controls.Add(this.Label2);
            this.Controls.Add(this.Label1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.CheckBox CheckGroupActive;
        public System.Windows.Forms.TextBox TxtAWriteComplete;
        public System.Windows.Forms.TextBox TxtAReadComplete;
        public System.Windows.Forms.TextBox TxtDataChange;
        public System.Windows.Forms.TextBox _txtChangeVal_1;
        public System.Windows.Forms.TextBox _txtChangeVal_0;
        public System.Windows.Forms.TextBox _txtReadVal_1;
        public System.Windows.Forms.TextBox txtWriteVal2;
        public System.Windows.Forms.TextBox txtItem2;
        public System.Windows.Forms.TextBox txtServer;
        public System.Windows.Forms.Button cmdExit;
        public System.Windows.Forms.Button cmdReadAsync;
        public System.Windows.Forms.Button cmdReadSync;
        public System.Windows.Forms.Button cmdWriteAsync;
        public System.Windows.Forms.Button cmdWriteSync;
        public System.Windows.Forms.TextBox _txtReadVal_0;
        public System.Windows.Forms.TextBox txtWriteVal1;
        public System.Windows.Forms.Button cmdRemItem;
        public System.Windows.Forms.Button cmdAddItem;
        public System.Windows.Forms.Button cmdRemGroup;
        public System.Windows.Forms.Button cmdAddGroup;
        public System.Windows.Forms.Button cmdDisconnect;
        public System.Windows.Forms.TextBox txtItem1;
        public System.Windows.Forms.TextBox txtGroup;
        public System.Windows.Forms.Button cmdConnect;
        public System.Windows.Forms.Label Label10;
        public System.Windows.Forms.Label Label9;
        public System.Windows.Forms.Label Label8;
        public System.Windows.Forms.Label Label7;
        public System.Windows.Forms.Label Label6;
        public System.Windows.Forms.Label Label5;
        public System.Windows.Forms.Label Label4;
        public System.Windows.Forms.Label Label3;
        public System.Windows.Forms.Label Label2;
        public System.Windows.Forms.Label Label1;

    }
}

