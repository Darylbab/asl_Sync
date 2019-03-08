using System;
using System.Data;
using System.Diagnostics;
using System.Windows.Forms;
using asl_SyncLibrary;

namespace MorningUpdate
{
    public partial class MorningUpdates : Form
    {
        AltaSync ASy = new AltaSync();
        private Asgard ASG = new Asgard();
        private CommonFunctions CF = new CommonFunctions();
        private DataWarehouse DW = new DataWarehouse();

        private string CurrentFunction = "MorningUpdates";
        private string[] TArgs;
        private DateTime StartTime = DateTime.Now;



        public MorningUpdates(string[] args = null)
        {
            InitializeComponent();
            TArgs = args;
            string StartTimeStr = StartTime.ToString(Mirror.AxessDateTimeFormat);
            LblStartTime.Text = $"Start Time: {StartTimeStr}";
        }

        private void MorningUpdates_Shown(object sender, EventArgs e)
        {
            CurrentFunction = "MorningUpdates_Shown";
            SSLStatus.Text = "Initializing";
            SetTaskStatus();
            if (TArgs != null)
            {
                //process any passed parameters/arguments.
            }
            //Fix Willcall where Mountain Collective skiers have no Axess Person Number.
            //SSLStatus.Text = "Fix MTC Willcall Person Data.";
            //now done in MountainCollective class.

            SSLStatus.Text = "Updating Salesdata usage.";
            DW.UpdateSalesDataUses();

            //sync previous day's POS sales and purchases
            SSLStatus.Text = "Sync Previous Day's Sales.";
            ASy.ProgressChanged += AltaSyncProgressChanged;
            ASy.SyncPOS("*", DateTime.Today.AddDays(-1));

            //Set cardstatus in SPCards for expired (X) and presales (P).
            DW.UpdateSPCardStatus();
            //update usage for SPCards
            DW.UpdateSPCardsUses();

            //Sync Liftopia files
            using (Process LiftopiaP = new Process())
            {
                LiftopiaP.StartInfo.FileName = "Liftopia_Sync.EXE";
                LiftopiaP.StartInfo.WorkingDirectory = @"C:\asl_Sync\programs\Liftopia\";
                LiftopiaP.StartInfo.UseShellExecute = true;
                LiftopiaP.StartInfo.RedirectStandardError = false;
                LiftopiaP.StartInfo.RedirectStandardInput = false;
                LiftopiaP.StartInfo.RedirectStandardOutput = false;
                LiftopiaP.Start();
                LiftopiaP.WaitForExit();
            }

            //Sync MTC files
            using (Process MTCP = new Process())
            {
                MTCP.StartInfo.FileName = @"C:\asl_Sync\programs\MountainCollective\MountainCollective.EXE";

                MTCP.StartInfo.Arguments = "DUIWG";        //(D)ownload MTC, (U)pload MTC, MtnCol_(I)ssued Usage, Fix (W)illcall Axess Person data, Update MTC Usa(G)e
                MTCP.StartInfo.UseShellExecute = true;
                MTCP.StartInfo.RedirectStandardError = false;
                MTCP.StartInfo.RedirectStandardInput = false;
                MTCP.StartInfo.RedirectStandardOutput = false;
                MTCP.Start();
                MTCP.WaitForExit();
            }

            //UpdateASBShared();


            //UpdateOther();
            using (Process OSAP = new Process())
            {
                OSAP.StartInfo.FileName = @"C:\asl_Sync\programs\OtherSkiAreas\OtherSkiAreas.EXE";
                OSAP.StartInfo.Arguments = "ALL";
                OSAP.StartInfo.UseShellExecute = true;
                OSAP.StartInfo.RedirectStandardError = false;
                OSAP.StartInfo.RedirectStandardInput = false;
                OSAP.StartInfo.RedirectStandardOutput = false;
                OSAP.Start();
                OSAP.WaitForExit();
            }

                //ExtSync.UpdateWasatchBenefit();
                using (Process WABAP = new Process())
            {
                WABAP.StartInfo.FileName = @"C:\asl_Sync\programs\WasatchBenefits\WasatchBenefits.EXE";
                WABAP.StartInfo.UseShellExecute = true;
                WABAP.StartInfo.RedirectStandardError = false;
                WABAP.StartInfo.RedirectStandardInput = false;
                WABAP.StartInfo.RedirectStandardOutput = false;
                WABAP.Start();
                WABAP.WaitForExit();
            }

            Application.Exit();
        }

        /// <summary>
        /// AltaSyncProgressChanged 
        /// 
        /// updates progress bars from events in AltaSync.
        /// 
        /// </summary>
        /// <param name="aProgress"></param>
        /// Tuple<string, int, int, int>(Status.Text, progressbar.Value, progressbar.Maximum, progressbar to use (switch case)
        private void AltaSyncProgressChanged(Tuple<string, int, int, int> aProgress)
        {
            SSLStatus.Text = aProgress.Item1;
            ProgressBar TProgress = new ProgressBar();
            switch (aProgress.Item4)
            {
                case 1:
                    TProgress = PBFixPersNrInWillcall;
                    break;
                case 2:
                    TProgress = PBPOSSyncForPreviousDay;
                    break;
                case 3:
                    TProgress = PBPOSUpdateStatus;
                    break;
                case 4:
                    TProgress = PBUpdateSalesDataCardStatus;
                    break;
                case 5:
                    TProgress = PBUpdateSalesDataUsage;
                    break;
            }
            TProgress.Maximum = aProgress.Item3;
            TProgress.Value = aProgress.Item2;
            SetTaskStatus("", "", TProgress);
        }


        public void UpdateSalesDataUsage()
        {
            CurrentFunction = "UpdateSalesDataUsage";
            SSLStatus.Text = "Loading Salesdata Rows";
            SetTaskStatus();
            string tSQL = $"SELECT serialkey FROM {DW.ActiveDatabase}.salesdata WHERE saledate >= '{CF.SeasonStart.ToString(Mirror.AxessDateFormat)}' GROUP BY serialkey";
            DataTable tDT = CF.LoadTable(DW.dwConn, tSQL, "SalesData");
            PBUpdateSalesDataUsage.Maximum = tDT.Rows.Count;
            SetTaskStatus("", "", PBUpdateSalesDataUsage);
            foreach (DataRow tDR in tDT.Rows)
            {
                string SerialKey = tDR["serialkey"].ToString();
                PBUpdateSalesDataUsage.Value = tDT.Rows.IndexOf(tDR) + 1;
                SSLStatus.Text = $"Updating usage for {SerialKey}.";
                SetTaskStatus("", "", PBUpdateSalesDataUsage);
                string tUses = CF.RowCount(DW.dwConn, $"{DW.ActiveDatabase}.skivisits", $"serialkey='{SerialKey}'").ToString();
                CF.ExecuteSQL(DW.dwConn, $"UPDATE {DW.ActiveDatabase}.salesdata SET uses = {tUses} WHERE serialkey='{SerialKey}'");
            }
        }



        private void SetTaskStatus(string Description = "", string Extra = "", ProgressBar PB1 = null, ProgressBar PB2 = null)
        {
            Application.DoEvents();
            if (PB2 != null)
            {
                PB2.Enabled = PB2.Maximum > 0;
                ASG.SetTaskStatus(Application.ProductName, CurrentFunction, statusStrip1.Items[0].Text, Description, Extra, PB1?.Value, PB1?.Maximum, PB2?.Value, PB2?.Maximum);
                PB1.Refresh();
                PB2.Refresh();
                return;
            }
            if (PB1 != null)
            {
                PB1.Enabled = PB1.Maximum > 0;
                ASG.SetTaskStatus(Application.ProductName, CurrentFunction, statusStrip1.Items[0].Text, Description, Extra, PB1?.Value, PB1?.Maximum);
                PB1.Refresh();
                return;
            }
            ASG.SetTaskStatus(Application.ProductName, CurrentFunction, statusStrip1.Items[0].Text, Description, Extra);
        }

    }
}
