namespace ServerMonitor
{
    partial class ServerMonitor
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
            this.components = new System.ComponentModel.Container();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.cboRefreshRate = new System.Windows.Forms.ComboBox();
            this.button1 = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            this.txtAsgardStatus = new System.Windows.Forms.TextBox();
            this.txtAsgardSince = new System.Windows.Forms.TextBox();
            this.txtAsgardLastCheck = new System.Windows.Forms.TextBox();
            this.txtBuyStatus = new System.Windows.Forms.TextBox();
            this.txtBuySince = new System.Windows.Forms.TextBox();
            this.txtBuyLastCheck = new System.Windows.Forms.TextBox();
            this.txtDataWarehouseStatus = new System.Windows.Forms.TextBox();
            this.txtDataWarehouseSince = new System.Windows.Forms.TextBox();
            this.txtDataWarehouseLastCheck = new System.Windows.Forms.TextBox();
            this.txtHildeStatus = new System.Windows.Forms.TextBox();
            this.txtHildeSince = new System.Windows.Forms.TextBox();
            this.txtHildeLastCheck = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.txtCRMStatus = new System.Windows.Forms.TextBox();
            this.txtCRMSince = new System.Windows.Forms.TextBox();
            this.txtCRMLastCheck = new System.Windows.Forms.TextBox();
            this.txtWTPStatus = new System.Windows.Forms.TextBox();
            this.txtWTPSince = new System.Windows.Forms.TextBox();
            this.txtWTPLastCheck = new System.Windows.Forms.TextBox();
            this.txtOBIStatus = new System.Windows.Forms.TextBox();
            this.txtOBISince = new System.Windows.Forms.TextBox();
            this.txtOBILastCheck = new System.Windows.Forms.TextBox();
            this.txtSIStatus = new System.Windows.Forms.TextBox();
            this.txtSISince = new System.Windows.Forms.TextBox();
            this.txtSILastCheck = new System.Windows.Forms.TextBox();
            this.txtSSLastCheck = new System.Windows.Forms.TextBox();
            this.txtSSSince = new System.Windows.Forms.TextBox();
            this.txtSSStatus = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.txtShopLastCheck = new System.Windows.Forms.TextBox();
            this.txtShopSince = new System.Windows.Forms.TextBox();
            this.txtShopStatus = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 300000;
            this.timer1.Tick += new System.EventHandler(this.Timer1_Tick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 28);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(40, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Asgard";
            this.toolTip1.SetToolTip(this.label1, "asgard.alta.com");
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(11, 54);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(25, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Buy";
            this.toolTip1.SetToolTip(this.label2, "buy.alta.com");
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(11, 80);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(85, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "DataWarehouse";
            this.toolTip1.SetToolTip(this.label3, "web-database.alta.com");
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(11, 106);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(31, 13);
            this.label4.TabIndex = 12;
            this.label4.Text = "Hilde";
            this.toolTip1.SetToolTip(this.label4, "axess.alta.com");
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(11, 177);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(55, 13);
            this.label10.TabIndex = 22;
            this.label10.Text = "DCI4CRM";
            this.toolTip1.SetToolTip(this.label10, "asgard.alta.com");
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(11, 229);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(53, 13);
            this.label9.TabIndex = 29;
            this.label9.Text = "OBI4POS";
            this.toolTip1.SetToolTip(this.label9, "web-database.alta.com");
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(11, 255);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(42, 13);
            this.label8.TabIndex = 33;
            this.label8.Text = "WTPSI";
            this.toolTip1.SetToolTip(this.label8, "axess.alta.com");
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(12, 199);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(56, 13);
            this.label11.TabIndex = 37;
            this.label11.Text = "DCI4WTP";
            this.toolTip1.SetToolTip(this.label11, "buy.alta.com");
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(11, 281);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(53, 13);
            this.label12.TabIndex = 38;
            this.label12.Text = "POSSync";
            this.toolTip1.SetToolTip(this.label12, "axess.alta.com");
            // 
            // cboRefreshRate
            // 
            this.cboRefreshRate.FormattingEnabled = true;
            this.cboRefreshRate.Items.AddRange(new object[] {
            "1 minute",
            "2 minutes",
            "3 minutes",
            "5 minutes",
            "10 minutes",
            "15 minutes"});
            this.cboRefreshRate.Location = new System.Drawing.Point(95, 321);
            this.cboRefreshRate.Name = "cboRefreshRate";
            this.cboRefreshRate.Size = new System.Drawing.Size(148, 21);
            this.cboRefreshRate.TabIndex = 42;
            this.toolTip1.SetToolTip(this.cboRefreshRate, "Minutes after last sync before firing off next sync.");
            this.cboRefreshRate.SelectedIndexChanged += new System.EventHandler(this.CboRefreshRate_SelectedIndexChanged);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(363, 319);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 20;
            this.button1.TabStop = false;
            this.button1.Text = "Refresh";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.Button1_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.toolStripProgressBar1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 346);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(450, 22);
            this.statusStrip1.TabIndex = 21;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.AutoSize = false;
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(328, 17);
            this.toolStripStatusLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolStripProgressBar1
            // 
            this.toolStripProgressBar1.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripProgressBar1.Name = "toolStripProgressBar1";
            this.toolStripProgressBar1.Size = new System.Drawing.Size(100, 16);
            // 
            // txtAsgardStatus
            // 
            this.txtAsgardStatus.Location = new System.Drawing.Point(95, 21);
            this.txtAsgardStatus.Name = "txtAsgardStatus";
            this.txtAsgardStatus.ReadOnly = true;
            this.txtAsgardStatus.Size = new System.Drawing.Size(83, 20);
            this.txtAsgardStatus.TabIndex = 1;
            this.txtAsgardStatus.TabStop = false;
            // 
            // txtAsgardSince
            // 
            this.txtAsgardSince.Location = new System.Drawing.Point(184, 21);
            this.txtAsgardSince.Name = "txtAsgardSince";
            this.txtAsgardSince.ReadOnly = true;
            this.txtAsgardSince.Size = new System.Drawing.Size(124, 20);
            this.txtAsgardSince.TabIndex = 2;
            this.txtAsgardSince.TabStop = false;
            // 
            // txtAsgardLastCheck
            // 
            this.txtAsgardLastCheck.Location = new System.Drawing.Point(314, 21);
            this.txtAsgardLastCheck.Name = "txtAsgardLastCheck";
            this.txtAsgardLastCheck.ReadOnly = true;
            this.txtAsgardLastCheck.Size = new System.Drawing.Size(124, 20);
            this.txtAsgardLastCheck.TabIndex = 3;
            this.txtAsgardLastCheck.TabStop = false;
            // 
            // txtBuyStatus
            // 
            this.txtBuyStatus.Location = new System.Drawing.Point(95, 47);
            this.txtBuyStatus.Name = "txtBuyStatus";
            this.txtBuyStatus.ReadOnly = true;
            this.txtBuyStatus.Size = new System.Drawing.Size(83, 20);
            this.txtBuyStatus.TabIndex = 5;
            this.txtBuyStatus.TabStop = false;
            // 
            // txtBuySince
            // 
            this.txtBuySince.Location = new System.Drawing.Point(184, 47);
            this.txtBuySince.Name = "txtBuySince";
            this.txtBuySince.ReadOnly = true;
            this.txtBuySince.Size = new System.Drawing.Size(124, 20);
            this.txtBuySince.TabIndex = 6;
            this.txtBuySince.TabStop = false;
            // 
            // txtBuyLastCheck
            // 
            this.txtBuyLastCheck.Location = new System.Drawing.Point(314, 47);
            this.txtBuyLastCheck.Name = "txtBuyLastCheck";
            this.txtBuyLastCheck.ReadOnly = true;
            this.txtBuyLastCheck.Size = new System.Drawing.Size(124, 20);
            this.txtBuyLastCheck.TabIndex = 7;
            this.txtBuyLastCheck.TabStop = false;
            // 
            // txtDataWarehouseStatus
            // 
            this.txtDataWarehouseStatus.Location = new System.Drawing.Point(95, 73);
            this.txtDataWarehouseStatus.Name = "txtDataWarehouseStatus";
            this.txtDataWarehouseStatus.ReadOnly = true;
            this.txtDataWarehouseStatus.Size = new System.Drawing.Size(83, 20);
            this.txtDataWarehouseStatus.TabIndex = 9;
            this.txtDataWarehouseStatus.TabStop = false;
            // 
            // txtDataWarehouseSince
            // 
            this.txtDataWarehouseSince.Location = new System.Drawing.Point(184, 73);
            this.txtDataWarehouseSince.Name = "txtDataWarehouseSince";
            this.txtDataWarehouseSince.ReadOnly = true;
            this.txtDataWarehouseSince.Size = new System.Drawing.Size(124, 20);
            this.txtDataWarehouseSince.TabIndex = 10;
            this.txtDataWarehouseSince.TabStop = false;
            // 
            // txtDataWarehouseLastCheck
            // 
            this.txtDataWarehouseLastCheck.Location = new System.Drawing.Point(314, 73);
            this.txtDataWarehouseLastCheck.Name = "txtDataWarehouseLastCheck";
            this.txtDataWarehouseLastCheck.ReadOnly = true;
            this.txtDataWarehouseLastCheck.Size = new System.Drawing.Size(124, 20);
            this.txtDataWarehouseLastCheck.TabIndex = 11;
            this.txtDataWarehouseLastCheck.TabStop = false;
            // 
            // txtHildeStatus
            // 
            this.txtHildeStatus.Location = new System.Drawing.Point(95, 99);
            this.txtHildeStatus.Name = "txtHildeStatus";
            this.txtHildeStatus.ReadOnly = true;
            this.txtHildeStatus.Size = new System.Drawing.Size(83, 20);
            this.txtHildeStatus.TabIndex = 13;
            this.txtHildeStatus.TabStop = false;
            // 
            // txtHildeSince
            // 
            this.txtHildeSince.Location = new System.Drawing.Point(184, 99);
            this.txtHildeSince.Name = "txtHildeSince";
            this.txtHildeSince.ReadOnly = true;
            this.txtHildeSince.Size = new System.Drawing.Size(124, 20);
            this.txtHildeSince.TabIndex = 14;
            this.txtHildeSince.TabStop = false;
            // 
            // txtHildeLastCheck
            // 
            this.txtHildeLastCheck.Location = new System.Drawing.Point(314, 99);
            this.txtHildeLastCheck.Name = "txtHildeLastCheck";
            this.txtHildeLastCheck.ReadOnly = true;
            this.txtHildeLastCheck.Size = new System.Drawing.Size(124, 20);
            this.txtHildeLastCheck.TabIndex = 15;
            this.txtHildeLastCheck.TabStop = false;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(97, 5);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(37, 13);
            this.label5.TabIndex = 16;
            this.label5.Text = "Status";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(187, 6);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(34, 13);
            this.label6.TabIndex = 17;
            this.label6.Text = "Since";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(315, 5);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(61, 13);
            this.label7.TabIndex = 18;
            this.label7.Text = "Last Check";
            // 
            // txtCRMStatus
            // 
            this.txtCRMStatus.Location = new System.Drawing.Point(95, 170);
            this.txtCRMStatus.Name = "txtCRMStatus";
            this.txtCRMStatus.ReadOnly = true;
            this.txtCRMStatus.Size = new System.Drawing.Size(83, 20);
            this.txtCRMStatus.TabIndex = 23;
            this.txtCRMStatus.TabStop = false;
            // 
            // txtCRMSince
            // 
            this.txtCRMSince.Location = new System.Drawing.Point(184, 170);
            this.txtCRMSince.Name = "txtCRMSince";
            this.txtCRMSince.ReadOnly = true;
            this.txtCRMSince.Size = new System.Drawing.Size(124, 20);
            this.txtCRMSince.TabIndex = 24;
            this.txtCRMSince.TabStop = false;
            // 
            // txtCRMLastCheck
            // 
            this.txtCRMLastCheck.Location = new System.Drawing.Point(314, 170);
            this.txtCRMLastCheck.Name = "txtCRMLastCheck";
            this.txtCRMLastCheck.ReadOnly = true;
            this.txtCRMLastCheck.Size = new System.Drawing.Size(124, 20);
            this.txtCRMLastCheck.TabIndex = 25;
            this.txtCRMLastCheck.TabStop = false;
            // 
            // txtWTPStatus
            // 
            this.txtWTPStatus.Location = new System.Drawing.Point(95, 196);
            this.txtWTPStatus.Name = "txtWTPStatus";
            this.txtWTPStatus.ReadOnly = true;
            this.txtWTPStatus.Size = new System.Drawing.Size(83, 20);
            this.txtWTPStatus.TabIndex = 26;
            this.txtWTPStatus.TabStop = false;
            // 
            // txtWTPSince
            // 
            this.txtWTPSince.Location = new System.Drawing.Point(184, 196);
            this.txtWTPSince.Name = "txtWTPSince";
            this.txtWTPSince.ReadOnly = true;
            this.txtWTPSince.Size = new System.Drawing.Size(124, 20);
            this.txtWTPSince.TabIndex = 27;
            this.txtWTPSince.TabStop = false;
            // 
            // txtWTPLastCheck
            // 
            this.txtWTPLastCheck.Location = new System.Drawing.Point(314, 196);
            this.txtWTPLastCheck.Name = "txtWTPLastCheck";
            this.txtWTPLastCheck.ReadOnly = true;
            this.txtWTPLastCheck.Size = new System.Drawing.Size(124, 20);
            this.txtWTPLastCheck.TabIndex = 28;
            this.txtWTPLastCheck.TabStop = false;
            // 
            // txtOBIStatus
            // 
            this.txtOBIStatus.Location = new System.Drawing.Point(95, 222);
            this.txtOBIStatus.Name = "txtOBIStatus";
            this.txtOBIStatus.ReadOnly = true;
            this.txtOBIStatus.Size = new System.Drawing.Size(83, 20);
            this.txtOBIStatus.TabIndex = 30;
            this.txtOBIStatus.TabStop = false;
            // 
            // txtOBISince
            // 
            this.txtOBISince.Location = new System.Drawing.Point(184, 222);
            this.txtOBISince.Name = "txtOBISince";
            this.txtOBISince.ReadOnly = true;
            this.txtOBISince.Size = new System.Drawing.Size(124, 20);
            this.txtOBISince.TabIndex = 31;
            this.txtOBISince.TabStop = false;
            // 
            // txtOBILastCheck
            // 
            this.txtOBILastCheck.Location = new System.Drawing.Point(314, 222);
            this.txtOBILastCheck.Name = "txtOBILastCheck";
            this.txtOBILastCheck.ReadOnly = true;
            this.txtOBILastCheck.Size = new System.Drawing.Size(124, 20);
            this.txtOBILastCheck.TabIndex = 32;
            this.txtOBILastCheck.TabStop = false;
            // 
            // txtSIStatus
            // 
            this.txtSIStatus.Location = new System.Drawing.Point(95, 248);
            this.txtSIStatus.Name = "txtSIStatus";
            this.txtSIStatus.ReadOnly = true;
            this.txtSIStatus.Size = new System.Drawing.Size(83, 20);
            this.txtSIStatus.TabIndex = 34;
            this.txtSIStatus.TabStop = false;
            // 
            // txtSISince
            // 
            this.txtSISince.Location = new System.Drawing.Point(184, 248);
            this.txtSISince.Name = "txtSISince";
            this.txtSISince.ReadOnly = true;
            this.txtSISince.Size = new System.Drawing.Size(124, 20);
            this.txtSISince.TabIndex = 35;
            this.txtSISince.TabStop = false;
            // 
            // txtSILastCheck
            // 
            this.txtSILastCheck.Location = new System.Drawing.Point(314, 248);
            this.txtSILastCheck.Name = "txtSILastCheck";
            this.txtSILastCheck.ReadOnly = true;
            this.txtSILastCheck.Size = new System.Drawing.Size(124, 20);
            this.txtSILastCheck.TabIndex = 36;
            this.txtSILastCheck.TabStop = false;
            // 
            // txtSSLastCheck
            // 
            this.txtSSLastCheck.Location = new System.Drawing.Point(314, 274);
            this.txtSSLastCheck.Name = "txtSSLastCheck";
            this.txtSSLastCheck.ReadOnly = true;
            this.txtSSLastCheck.Size = new System.Drawing.Size(124, 20);
            this.txtSSLastCheck.TabIndex = 41;
            this.txtSSLastCheck.TabStop = false;
            // 
            // txtSSSince
            // 
            this.txtSSSince.Location = new System.Drawing.Point(184, 274);
            this.txtSSSince.Name = "txtSSSince";
            this.txtSSSince.ReadOnly = true;
            this.txtSSSince.Size = new System.Drawing.Size(124, 20);
            this.txtSSSince.TabIndex = 40;
            this.txtSSSince.TabStop = false;
            // 
            // txtSSStatus
            // 
            this.txtSSStatus.Location = new System.Drawing.Point(95, 274);
            this.txtSSStatus.Name = "txtSSStatus";
            this.txtSSStatus.ReadOnly = true;
            this.txtSSStatus.Size = new System.Drawing.Size(83, 20);
            this.txtSSStatus.TabIndex = 39;
            this.txtSSStatus.TabStop = false;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(12, 324);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(70, 13);
            this.label13.TabIndex = 43;
            this.label13.Text = "Refresh Rate";
            // 
            // txtShopLastCheck
            // 
            this.txtShopLastCheck.Location = new System.Drawing.Point(314, 125);
            this.txtShopLastCheck.Name = "txtShopLastCheck";
            this.txtShopLastCheck.ReadOnly = true;
            this.txtShopLastCheck.Size = new System.Drawing.Size(124, 20);
            this.txtShopLastCheck.TabIndex = 47;
            this.txtShopLastCheck.TabStop = false;
            // 
            // txtShopSince
            // 
            this.txtShopSince.Location = new System.Drawing.Point(184, 125);
            this.txtShopSince.Name = "txtShopSince";
            this.txtShopSince.ReadOnly = true;
            this.txtShopSince.Size = new System.Drawing.Size(124, 20);
            this.txtShopSince.TabIndex = 46;
            this.txtShopSince.TabStop = false;
            // 
            // txtShopStatus
            // 
            this.txtShopStatus.Location = new System.Drawing.Point(95, 125);
            this.txtShopStatus.Name = "txtShopStatus";
            this.txtShopStatus.ReadOnly = true;
            this.txtShopStatus.Size = new System.Drawing.Size(83, 20);
            this.txtShopStatus.TabIndex = 45;
            this.txtShopStatus.TabStop = false;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(11, 132);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(32, 13);
            this.label14.TabIndex = 44;
            this.label14.Text = "Shop";
            this.toolTip1.SetToolTip(this.label14, "axess.alta.com");
            // 
            // ServerMonitor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(450, 368);
            this.Controls.Add(this.txtShopLastCheck);
            this.Controls.Add(this.txtShopSince);
            this.Controls.Add(this.txtShopStatus);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.cboRefreshRate);
            this.Controls.Add(this.txtSSLastCheck);
            this.Controls.Add(this.txtSSSince);
            this.Controls.Add(this.txtSSStatus);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.txtSILastCheck);
            this.Controls.Add(this.txtSISince);
            this.Controls.Add(this.txtSIStatus);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.txtOBILastCheck);
            this.Controls.Add(this.txtOBISince);
            this.Controls.Add(this.txtOBIStatus);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.txtWTPLastCheck);
            this.Controls.Add(this.txtWTPSince);
            this.Controls.Add(this.txtWTPStatus);
            this.Controls.Add(this.txtCRMLastCheck);
            this.Controls.Add(this.txtCRMSince);
            this.Controls.Add(this.txtCRMStatus);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtHildeLastCheck);
            this.Controls.Add(this.txtHildeSince);
            this.Controls.Add(this.txtHildeStatus);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtDataWarehouseLastCheck);
            this.Controls.Add(this.txtDataWarehouseSince);
            this.Controls.Add(this.txtDataWarehouseStatus);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtBuyLastCheck);
            this.Controls.Add(this.txtBuySince);
            this.Controls.Add(this.txtBuyStatus);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtAsgardLastCheck);
            this.Controls.Add(this.txtAsgardSince);
            this.Controls.Add(this.txtAsgardStatus);
            this.Controls.Add(this.label1);
            this.MaximizeBox = false;
            this.Name = "ServerMonitor";
            this.Text = "Server Monitor";
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtAsgardStatus;
        private System.Windows.Forms.TextBox txtAsgardSince;
        private System.Windows.Forms.TextBox txtAsgardLastCheck;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtBuyStatus;
        private System.Windows.Forms.TextBox txtBuySince;
        private System.Windows.Forms.TextBox txtBuyLastCheck;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtDataWarehouseStatus;
        private System.Windows.Forms.TextBox txtDataWarehouseSince;
        private System.Windows.Forms.TextBox txtDataWarehouseLastCheck;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtHildeStatus;
        private System.Windows.Forms.TextBox txtHildeSince;
        private System.Windows.Forms.TextBox txtHildeLastCheck;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox txtCRMStatus;
        private System.Windows.Forms.TextBox txtCRMSince;
        private System.Windows.Forms.TextBox txtCRMLastCheck;
        private System.Windows.Forms.TextBox txtWTPStatus;
        private System.Windows.Forms.TextBox txtWTPSince;
        private System.Windows.Forms.TextBox txtWTPLastCheck;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox txtOBIStatus;
        private System.Windows.Forms.TextBox txtOBISince;
        private System.Windows.Forms.TextBox txtOBILastCheck;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtSIStatus;
        private System.Windows.Forms.TextBox txtSISince;
        private System.Windows.Forms.TextBox txtSILastCheck;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox txtSSLastCheck;
        private System.Windows.Forms.TextBox txtSSSince;
        private System.Windows.Forms.TextBox txtSSStatus;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.ComboBox cboRefreshRate;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox txtShopLastCheck;
        private System.Windows.Forms.TextBox txtShopSince;
        private System.Windows.Forms.TextBox txtShopStatus;
        private System.Windows.Forms.Label label14;
    }
}

