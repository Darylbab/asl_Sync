using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using asl_SyncLibrary;

namespace POS_Sync
{
    public partial class POS_Sync : Form
    {

        public event Action<Tuple<string, int, int, int>> ProgressChanged;
        private void OnProgressChanged(Tuple<string, int, int, int> progress) => ProgressChanged?.Invoke(progress);

        private Asgard ASG = new Asgard();
        private CommonFunctions CF = new CommonFunctions();
        private Mirror AM = new Mirror();
        private AltaSync ASy = new AltaSync();
        private DataWarehouse DW = new DataWarehouse();

        private string ArgList = string.Empty;
        private string ArgDateStart = string.Empty;

        private string CurrentFunction = string.Empty;

        public POS_Sync()
        {
            InitializeComponent();
            String[] args = Environment.GetCommandLineArgs();
            if (args != null)
            {
                if (args.Length > 1)
                    ArgList = args[1];
                if (args.Length > 2)
                {
                    ArgDateStart = args[2];
                }
                else
                {
                    ArgDateStart = DateTime.Today.ToString(Mirror.AxessDateFormat);
                }
            }
        }

        private void POS_Sync_Load(object sender, EventArgs e)
        {
            CurrentFunction = "POS_Sync_Load";
        }

        private void POS_Sync_Shown(object sender, EventArgs e)
        {
            ASy.ProgressChanged += AltaSyncProgressChanged;
            LblStartTime.Text = DateTime.Now.ToString(Mirror.AxessDateTimeFormat);
            SSLStatus.Text = "Loading POS data";
            SetTaskStatus();
            POSListView.Items.Clear();
            dtpPOSSync.MaxDate = DateTime.Today;
            using (DataTable POSData = CF.LoadTable(AM.MirrorConn, $"SELECT nkassanr, szcomputername, szname FROM {AM.ActiveDatabase}.tabkassakonf WHERE szcomputername LIKE '%alta%' and nkassatypnr = 1 ORDER BY nkassanr", "POSData"))
            {
                foreach (DataRow POSRow in POSData.Rows)
                {
                    POSListView.Items.Add(new ListViewItem(new[] { POSRow["nkassanr"].ToString(), POSRow["szcomputername"].ToString(), POSRow["szname"].ToString() }));
                }
            }

            if (ArgList.Length != 0)
            {
                if (ArgList == "ALL")
                {
                    ArgList = string.Empty;
                    foreach (ListViewItem tItem in POSListView.Items)
                    {
                        ArgList = CF.AddToList(ArgList, tItem.Text);
                        {
                            tItem.Checked = true;
                        }
                    }
                }
                if (ArgList == "PREPAIDS")
                {
                    ASy.FixPrepaids();
                }
                else
                {
                    string[] POS = ArgList.Split(',');
                    foreach (string tPOS in POS)
                    {
                        foreach (ListViewItem tItem in POSListView.Items)
                        {
                            if (tItem.Text == tPOS)
                            {
                                tItem.Checked = true;
                                break;
                            }
                        }
                    }
                    if (!DateTime.TryParse(ArgDateStart, out DateTime StarttDate))
                    {
                        StarttDate = DateTime.Today;
                    }
                    dtpPOSSync.Value = StarttDate;
                    timer1.Interval = 1000;
                    timer1.Enabled = true;
                }
            }
            SSLStatus.Text = "";
        }

        private void SyncNow()
        {
            foreach (ListViewItem POSItem in POSListView.Items)
            {
                if (POSItem.Checked)
                {
                    SSLStatus.Text = $"Loading data for POS {POSItem.Text}.";
                    Application.DoEvents();
                    PBStatus.Visible = true;
                    ASy.SyncPOS(POSItem.SubItems[0].Text, dtpPOSSync.Value);
                    POSItem.Checked = false;
                    PBStatus.Visible = false;
                }
            }
            SSLStatus.Text = "Updating SalesData Uses.";
            Application.DoEvents();
            DW.UpdateSalesDataUses();
            SSLStatus.Text = "Updating SPCard Uses.";
            Application.DoEvents();
            DW.UpdateSPCardsUses();
            SSLStatus.Text = "";
            Application.DoEvents();
        }

        private void BtnSelectAll_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem tItem in POSListView.Items)
            {
                tItem.Checked = true;
            }
        }

        private void BtnClearAll_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem tItem in POSListView.Items)
            {
                tItem.Checked = false;
            }
        }

        private void CmdPOSSync_Click(object sender, EventArgs e)
        {
            Cursor oldCursor = Cursor;
            Cursor = Cursors.WaitCursor;
            SyncNow();
            SSLStatus.Text = "COMPLETE";
            SetTaskStatus();
            Cursor = oldCursor;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="aProgress"></param>
        /// Tuple<string, int, int, int>(Status.Text, progressbar.Value, progressbar.Maximum, progressbar to use 
        private void AltaSyncProgressChanged(Tuple<string, int, int, int> aProgress)
        {
            switch (aProgress.Item4)
            {
                case 3:
                    PBStatus.Value = 0;
                    PBStatus.Maximum = aProgress.Item3;
                    PBStatus.Value = aProgress.Item2;
                    SSLStatus.Text = aProgress.Item1;
                    SetTaskStatus("", "", PBStatus);
                    break;
                default:
                    SetTaskStatus();
                    break;
            }
        }

            private void SetTaskStatus(string Description = "", string Extra = "", ToolStripProgressBar PB1 = null, ToolStripProgressBar PB2 = null)
        {
            Application.DoEvents();
            if (PB2 != null)
            {
                PB2.Enabled = PB2.Maximum > 0;
                ASG.SetTaskStatus(Application.ProductName, CurrentFunction, SSLStatus.Text, Description, Extra, PB1?.Value, PB1?.Maximum, PB2?.Value, PB2?.Maximum);
                return;
            }
            if (PB1 != null)
            {
                PB1.Enabled = PB1.Maximum > 0;
                ASG.SetTaskStatus(Application.ProductName, CurrentFunction, SSLStatus.Text, Description, Extra, PB1?.Value, PB1?.Maximum);
                return;
            }
            ASG.SetTaskStatus(Application.ProductName, CurrentFunction, SSLStatus.Text, Description, Extra);
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            if (DateTime.Now.Date > dtpPOSSync.Value.Date)
            {
                dtpPOSSync.MaxDate = DateTime.Now.Date;
                dtpPOSSync.Value = DateTime.Now.Date;
            }
            if (DateTime.Now.Hour > 6 && DateTime.Now.Hour < 19)
            {
                DateTime StartTime = DateTime.Now;
                foreach (ListViewItem tItem in POSListView.Items)
                {
                    tItem.Checked = true;
                }
                SyncNow();
                Int32 TotTime = Convert.ToInt32(DateTime.Now.Ticks - StartTime.Ticks)/10000;
                if (TotTime >= 899900)
                {
                    TotTime = 899900;
                }
                timer1.Interval = 900000 - TotTime;
                SSLStatus.Text = $"Next Run at {DateTime.Now.AddMilliseconds(timer1.Interval)}";
            }
            timer1.Enabled = true;
        }
    }
}
