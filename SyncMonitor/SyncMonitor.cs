using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using asl_SyncLibrary;




namespace SyncMonitor
{
    public partial class SyncMonitor : Form
    {
        static Asgard ASG = new Asgard();
        Mirror AM = new Mirror();
        DataSet TDataSet = new DataSet();
        DataTable TableTaskStatus = new DataTable();
        MySqlDataAdapter TDataAdapter = new MySqlDataAdapter();

        public SyncMonitor()
        {
            InitializeComponent();
            TableTaskStatus = ASG.GetTaskStatusAll();
            dataGridView1.DataSource = TableTaskStatus;
            SyncTimer.Interval = 1;
            SyncTimer.Enabled = true;
        }

        private void SyncTimer_Tick(object sender, EventArgs e)
        {
            SyncTimer.Enabled = false;
            ssLabel1.Text = "Refreshing Task Sync...";
            if (ASG.GetServerStatus() ==  Serverstatus.Active)
            {
                txtAsgardStatus.Text = "ACTIVE";
                txtAsgardStatus.BackColor = txtAsgardLastRun.BackColor;
                txtAsgardStatus.ForeColor = txtAsgardLastRun.ForeColor;
                TableTaskStatus = ASG.GetServers();
                foreach (DataRow tDR in TableTaskStatus.Rows)
                {
                    switch (tDR["Name"].ToString().ToUpper().Trim())
                    {
                        case "ASGARD":
                            txtAsgardLastRun.Text = tDR.Field<DateTime>("last_status_check").ToString(Mirror.AxessDateTimeFormat);
                            txtAsgardUpSince.Text = tDR.Field<DateTime>("last_status_change").ToString(Mirror.AxessDateTimeFormat);
                            break;
                        case "BUY":
                            txtBuyStatus.Text = tDR["status"].ToString();
                            if (txtBuyStatus.Text == "ACTIVE")
                            {
                                txtBuyStatus.BackColor = txtBuyLastRun.BackColor;
                                txtBuyStatus.ForeColor = txtBuyLastRun.ForeColor;
                            }
                            else
                            {
                                txtBuyStatus.BackColor = Color.Red;
                                txtBuyStatus.ForeColor = Color.White;
                            }
                            txtBuyLastRun.Text = tDR.Field<DateTime>("last_status_check").ToString(Mirror.AxessDateTimeFormat);
                            txtBuyUpSince.Text = tDR.Field<DateTime>("last_status_change").ToString(Mirror.AxessDateTimeFormat);
                            break;
                        case "DATAWAREHOUSE":
                            txtDWStatus.Text = tDR["status"].ToString();
                            if (txtDWStatus.Text == "ACTIVE")
                            {
                                txtDWStatus.BackColor = txtBuyLastRun.BackColor;
                                txtDWStatus.ForeColor = txtBuyLastRun.ForeColor;
                            }
                            else
                            {
                                txtDWStatus.BackColor = Color.Red;
                                txtDWStatus.ForeColor = Color.White;
                            }
                            txtDWLastRun.Text = tDR.Field<DateTime>("last_status_check").ToString(Mirror.AxessDateTimeFormat);
                            txtDWUpSince.Text = tDR.Field<DateTime>("last_status_change").ToString(Mirror.AxessDateTimeFormat);
                            break;
                        case "HILDE":
                            txtHildeStatus.Text = tDR["status"].ToString();
                            if (txtHildeStatus.Text == "ACTIVE")
                            {
                                txtHildeStatus.BackColor = txtBuyLastRun.BackColor;
                                txtHildeStatus.ForeColor = txtBuyLastRun.ForeColor;
                            }
                            else
                            {
                                txtHildeStatus.BackColor = Color.Red;
                                txtHildeStatus.ForeColor = Color.White;
                            }
                            txtHildeLastRun.Text = tDR.Field<DateTime>("last_status_check").ToString(Mirror.AxessDateTimeFormat);
                            txtHildeUpSince.Text = tDR.Field<DateTime>("last_status_change").ToString(Mirror.AxessDateTimeFormat);
                            break;
                    }
                }
            }
            else
            {
                txtAsgardStatus.Text = "FAILED";
                txtAsgardStatus.BackColor = Color.Red;
                txtAsgardStatus.ForeColor = Color.White;
                txtAsgardLastRun.Text = string.Empty;
                txtAsgardUpSince.Text = string.Empty;
                txtBuyStatus.Text = string.Empty;
                txtBuyStatus.BackColor = txtBuyLastRun.BackColor;
                txtBuyStatus.ForeColor = txtBuyLastRun.ForeColor;
                txtBuyLastRun.Text = string.Empty;
                txtBuyUpSince.Text = string.Empty;
                txtDWStatus.Text = string.Empty;
                txtDWStatus.BackColor = txtDWLastRun.BackColor;
                txtDWStatus.ForeColor = txtDWLastRun.ForeColor;
                txtDWLastRun.Text = string.Empty;
                txtDWUpSince.Text = string.Empty;
            }
            ssLabel1.Text = string.Empty;
            SyncTimer.Interval = 2000;
            SyncTimer.Enabled = true;
        }

 
    }
}
