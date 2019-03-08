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

namespace WindowsFormsApp1
{
    public partial class MasterSync : Form
    {
        private AltaSync AlS = new AltaSync();
        private Axess_DCI4CRM AxessSource = new Axess_DCI4CRM();
        private Mirror AM = new Mirror();
        private CommonFunctions CF = new CommonFunctions();
        private DataWarehouse DW = new DataWarehouse();

        public MasterSync()
        {
            InitializeComponent();
        }

        private void MasterSync_Load(object sender, EventArgs e)
        {
            SyncMirror();
        }

        private void SyncMirror()
        {
            MasterSyncStatusStrip.Items[0].Text = "Logging in to Axess..";
            AxessSource.Login();
            Cursor.Current = Cursors.WaitCursor;
            MasterSyncStatusStrip.Items[0].Text = "Updating base tables...";
            DateTime startPromptTime = Convert.ToDateTime(CF.GetSQLField(AM.MirrorConn, $"SELECT MAX(dtupdate) FROM {AM.ActiveDatabase}.tabpromptdata"));
            AM.UpdateTables(AM.GetLogNumber("AM") + 1, string.Empty);
            SetSalesTables(CF.LoadTable(AM.MirrorConn, $"SELECT concat_ws('-',nkassanr,ntransnr) AS TransKey FROM {AM.ActiveDatabase}.tabpromptdata WHERE dtupdate > '{startPromptTime.ToString(Mirror.AxessDateTimeFormat)}' GROUP BY concat_ws('-',nkassanr,ntransnr)", "Transkeys"));
            MasterSyncStatusStrip.Items[0].Text = "Loading new sales and payments...";
            Application.DoEvents();
            SetSalesTables(AM.InitSalesTables());
            MasterSyncStatusStrip.Items[0].Text = "Updating changes to sales and payments...";
            Application.DoEvents();
            SetSalesTables(AM.UpdateSalesTables());
            tslProgressBar.Visible = true;
            //temporary code
            DataTable tt = CF.LoadTable(DW.dwConn, "SELECT TRANSKEY FROM applications.pmtdata AS A WHERE (SELECT COUNT(*) FROM applications.pmtdata WHERE A.transpkey = transpkey) > 1 GROUP BY TRANSKEY", "pmtfix");
            MasterSyncStatusStrip.Items[0].Text = "Cleaning up duplicates in pmtdata...";
            tslProgressBar.Minimum = 0;
            tslProgressBar.Maximum = tt.Rows.Count;
            foreach (DataRow tRow in tt.Rows)
            {
                tslProgressBar.Value = tt.Rows.IndexOf(tRow);
                AlS.SyncPurchase(tRow["transkey"].ToString());
            }
            tt.Dispose();
            //run every evening at 8:00 PM
            //if (DateTime.Now.Date > Convert.ToDateTime(CF.GetSQLField(AM.MirrorConn, "SELECT lastUpdate FROM axess_cwc.tabLog WHERE TableName='EveningUpdate'")).Date && curHour >= 18)
            //{
            //    AxessSource.Logout();
            //    RunEveningUpdates();
            //    FixUnmatchedReads();
            //    AM.UpdateLog("EveningUpdate", 0);
            //}

            //run every morning at 4:00 AM
            //if (DateTime.Now.Date > Convert.ToDateTime(CF.GetSQLField(AM.MirrorConn, "SELECT lastUpdate FROM axess_cwc.tabLog WHERE TableName='MorningUpdate'")).Date && curHour >= 4)
            //{
            //    AxessSource.Logout();
            //    ExtSync.GetLiftopiaMTCFiles(Convert.ToDateTime("2017-03-01"), Convert.ToDateTime("2018-02-27"), MasterSyncStatusStrip, tslProgressBar);
            //    ExtSync.GetLiftopiaAltaFile(MasterSyncStatusStrip, tslProgressBar);
            //    RunMorningUpdates();
            //    AM.UpdateLog("MorningUpdate", 0);
            //    if (runSP2Kunde)
            //        SR.SPCards2Kunde();
            //}
            ////int i = 0;
            //int tLimit = 10;
            //MasterSyncStatusStrip.Items[0].Text = "Loading data for MediaID fix...";
            //ds = CF.loadDataSet(AM.MirrorConn, "SELECT nlfdzaehler FROM axess_cwc.tabkassatransaufbereit where nkartennr != '' and mediaid is null AND year(DTINSERTTIMESTAMP)<=2014 AND NSERIENNR > 0 ORDER BY dtinserttimestamp DESC", ds, "TAB");
            //foreach (DataRow tRow in ds.Tables["TAB"].Rows)
            //{
            //    AM.InitSalesTables(Convert.ToInt32(tRow["nlfdzaehler"].ToString()), true, true);
            //    System.Diagnostics.Debug.Print(ds.Tables["TAB"].Rows.IndexOf(tRow).ToString() + " -- " + ds.Tables["TAB"].Rows.Count.ToString());
            //    if (++i == tLimit) break;
            //}


        }

        private void SetSalesTables(DataTable dtSalesRecs)
        {
            if (dtSalesRecs != null)
                foreach (DataRow tRow in dtSalesRecs.Rows)
                {
                    AlS.SyncSale(tRow["TransKey"].ToString());
                    AlS.SyncPurchase(tRow["TransKey"].ToString());
                }
        }

    }


}
