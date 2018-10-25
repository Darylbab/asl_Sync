using System;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Windows.Forms;
using asl_SyncLibrary;

namespace OtherSkiAreas
{
    public partial class OtherSkiAreas : Form
    {
        Asgard ASG = new Asgard();                  //Functions: SetTaskStatus
        BUY_ALTA_COM BUY = new BUY_ALTA_COM();      //Data: RW-asb_shared
        CommonFunctions CF = new CommonFunctions(); //Functions: CSV2DS, EscapeChar, ExecuteSQL, RowCount, LoadDataSet, Table2CSV      Values: ITDataFolder, DataFolder, SeasonShort
        DataWarehouse DW = new DataWarehouse();     //Data: RO-SPCards, RW-CListPerm

        private string CurrentFunction = string.Empty;
        private string[] TArgs = null;

        public OtherSkiAreas(string[] args = null)
        {
            //standard object initialization
            InitializeComponent();
            //local initialization
            pbUpdateOtherSkiAreas.Step = 1;
            pbUpdateASBShared.Step = 1;
            //save a copy to a local variable and then process passed arguments
            TArgs = args;
            if (args == null)    //Manually run processes
            {
                cbUpdateOtherSkiAreas.Checked = false;
                cbUpdateASBShared.Checked = false;
                btnRunSelectedUpdates.Visible = true;
                btnRunSelectedUpdates.Enabled = true;
            }
            else
            {
                switch (args[0].ToUpper())    //args[0] value tells program which update(s) to run automatically
                {
                    case "OSA":     
                        cbUpdateOtherSkiAreas.Checked = true;
                        break;
                    case "ASB":
                        cbUpdateASBShared.Checked = true;
                        break;
                    case "ALL":
                        cbUpdateOtherSkiAreas.Checked = true;
                        cbUpdateASBShared.Checked = true;
                        break;
                }
                //all automated runs hide and disable the button for manual run.
                btnRunSelectedUpdates.Visible = false;
                btnRunSelectedUpdates.Enabled = false;
            }
        }

        public void UpdateOther()
        {
            pbUpdateOtherSkiAreas.Maximum = 6;
            var bgw = new BackgroundWorker();
            bgw.ProgressChanged += UpdateOther_ProgressChanged;
            bgw.DoWork += UpdateOtherBG;
            bgw.WorkerReportsProgress = true;
            bgw.RunWorkerAsync();
        }

        private void UpdateOther_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            pbUpdateOtherSkiAreas.Value = e.ProgressPercentage;
            lblStatus.Text = e.UserState.ToString();
            SetTaskStatus("", "", pbUpdateOtherSkiAreas);
        }

        private void UpdateOtherBG(object sender, DoWorkEventArgs e)
        {
            CurrentFunction = "UpdateOther";
            double AdultPrice = 52.00;
            double ChildPrice = 27.00;

            ((BackgroundWorker)sender).ReportProgress(1, "Loading Alta SPCards");
            string tQuery = $"SELECT firstname, lastname, wtp64, mediaid, city, CAST((DATEDIFF(now(), dob) / 365) AS signed) AS age, 'ALTA' as resortname, 1 as cardstatus FROM {DW.ActiveDatabase}.spcards WHERE cardstatus='A' AND osaflag='Y' AND testflag=false";
            DataSet tDS = CF.LoadDataSet(DW.dwConn, tQuery, new DataSet(), "altapass");
            if (tDS.Tables.Contains("altapass"))
            {
                CF.Table2CSV(tDS.Tables["altapass"], @"\\taskman\c$\acctg\winscp\data\altasp.csv");
                CF.Table2CSV(tDS.Tables["altapass"], CF.ITDataFolder + "altasp.csv");
                tDS.Tables.Remove("altapass");
            }
            ((BackgroundWorker)sender).ReportProgress(2, "Loading Red Lodge");
            if (File.Exists(@"\\taskman\c$\acctg\winscp\data\redlodgesp.csv"))
            {
                tDS = CF.CSV2DS(@"\\taskman\c$\acctg\winscp\data\redlodgesp.csv", "osaload", tDS, false, true);
                File.Copy(@"\\taskman\c$\acctg\winscp\data\redlodgesp.csv", CF.ITDataFolder + "redlodgesp.csv", true);
            }
            ((BackgroundWorker)sender).ReportProgress(3, "Loading Waschusetts");
            if (File.Exists(@"\\taskman\c$\acctg\winscp\data\waschusettsp.csv"))
            {
                tDS = CF.CSV2DS(@"\\taskman\c$\acctg\winscp\data\waschusettsp.csv", "osaload", tDS, true, true);
                File.Copy(@"\\taskman\c$\acctg\winscp\data\waschusettsp.csv", CF.ITDataFolder + "waschusettsp.csv", true);
            }
            ((BackgroundWorker)sender).ReportProgress(4, "Loading Bridger");
            if (File.Exists(@"\\taskman\c$\acctg\winscp\data\bridgersp.csv"))
            {
                tDS = CF.CSV2DS(@"\\taskman\c$\acctg\winscp\data\bridgersp.csv", "osaload", tDS, true, true);
                File.Copy(@"\\taskman\c$\acctg\winscp\data\bridgersp.csv", CF.ITDataFolder + "bridgersp.csv", true);
            } 
            ((BackgroundWorker)sender).ReportProgress(5, "Loading Brundage");
            if (File.Exists(@"\\taskman\c$\acctg\winscp\data\brundagesp.csv"))
            {
                tDS = CF.CSV2DS(@"\\taskman\c$\acctg\winscp\data\brundagesp.csv", "osaload", tDS, true, true);
                File.Copy(@"\\taskman\c$\acctg\winscp\data\brundagesp.csv", CF.ITDataFolder + "brundagesp.csv", true);
            }
            ((BackgroundWorker)sender).ReportProgress(6, "Loading Homewood");
            if (File.Exists(@"\\taskman\c$\acctg\winscp\data\homewoodsp.csv"))
            {
                tDS = CF.CSV2DS(@"\\taskman\c$\acctg\winscp\data\homewoodsp.csv", "osaload", tDS, true, true);
                File.Copy(@"\\taskman\c$\acctg\winscp\data\homewoodsp.csv", CF.ITDataFolder + "homewoodsp.csv", true);
            }
            if (btnRunSelectedUpdates.Enabled)
            {
                string mkey = string.Empty;
                ((BackgroundWorker)sender).ReportProgress(0, "Loading Alta SPCards");
                if (tDS.Tables.Contains("osaload"))
                {   
                    foreach (DataRow tRow in tDS.Tables["osaload"].Rows)
                    {
                        if ((tRow[2].ToString() != mkey) && (tRow[2].ToString() != string.Empty))
                        {
                            string tResortName = CF.EscapeChar(tRow[6].ToString().ToUpper());   //resortname
                            mkey = tRow[2].ToString().ToUpper();    //skip duplicates           //passnumber
                            string tPassKey = mkey;                                             //passnumber
                            string tPassNumber = mkey;                                          //passnumber
                            string tLastName = CF.EscapeChar(tRow[1].ToString().ToUpper());     //lastname
                            string tFirstName = CF.EscapeChar(tRow[0].ToString().ToUpper());    //firstname
                            string tBarCode = tRow[3].ToString().ToUpper();                     //barcode
                            double tPrice = AdultPrice;
                            string tTickPers = "AD OTHER SKI AREA";
                            int tAge = 0;
                            if ((tRow[5].ToString() != "NULL") && (tRow[5].ToString() != string.Empty))
                            {
                                tAge = Convert.ToInt32(tRow[5].ToString());
                            }
                            if ((tAge <= 12) && (tAge != 0))                     //age
                            {
                                tPrice = ChildPrice;
                                tTickPers = "CH OTHER SKI AREA";
                            }
                            if (!CF.RowExists(DW.dwConn, $"{DW.ActiveDatabase}.clistperm", $"passcat='OS' AND passnbr='{tPassNumber}'"))
                            {
                                tQuery = $"INSERT INTO {DW.ActiveDatabase}.clistperm (dexpires,season,lname,fname,category,reason,passkey,passcat,dent,passtype,tickets,barcode,passnbr,tickdesc,media,appby,dept,okayflag,company,maxuses,price,tickpers,uses,extuses) " +
                                $" Values ('{CF.YearEnd.ToString(Mirror.AxessDateFormat)}','{CF.SeasonShort}','{CF.EscapeChar(tLastName.ToUpper())}','{CF.EscapeChar(tFirstName.ToUpper())}','{CF.EscapeChar(tResortName.ToUpper())} SP','OTHER SKI AREA','{tPassKey}','OS','{DateTime.Now.ToString(Mirror.AxessDateTimeFormat)}','" +
                                $"AD',1,'{tBarCode}','{tPassNumber}','ALTA AREA DAY','ALTA ONE USE','OTHER SKI AREA','ADMIN',True,'{CF.EscapeChar(tResortName.ToUpper())}',999,{tPrice.ToString("#.00")},'{tTickPers}',0,0)";
                                CF.ExecuteSQL(DW.dwConn, tQuery);
                            }
                        }
                    }
                    tDS.Tables.Remove("osaload");
                }
                //now create file and transmit
                //tQuery = "SELECT 10 AS resortid, 'ALTA' AS resortname, 2 AS status, 'A' AS cardstatus,  upper(city), upper(lastname), upper(firstname) from " + DW.activeDatabase + ".spcards where TRIM(passcat) = 'WB' AND okayflag = 1 AND NOT (resortname='SNOWBIRD' AND barcode LIKE 'Y%')";
                tQuery = $"SELECT 10 AS resortid, perskey, UPPER(firstname) AS firstname,  UPPER(replace(lastname,',','')) AS lastname, UPPER(replace(city,',','')) AS city, CAST((DATEDIFF(now(), dob) / 365) AS signed) AS age, mediaid, wtp64, 1 AS status FROM {DW.ActiveDatabase}.spcards WHERE cardstatus='A' AND osaflag = 'Y' AND testflag=false";
                tDS = CF.LoadDataSet(DW.dwConn, tQuery, tDS, "spcards");
                if (tDS.Tables.Contains("spcards"))
                {
                    CF.Table2CSV(tDS.Tables["spcards"], $"{CF.DataFolder}altasp.csv", true);
                    CF.Table2CSV(tDS.Tables["spcards"], $"{CF.ITDataFolder}altasp.csv", true);
                }
            }
        }

        public void UpdateASBShared()
        {
            pbUpdateOtherSkiAreas.Maximum = 6;
            var bgw = new BackgroundWorker();
            bgw.ProgressChanged += UpdateASBShared_ProgressChanged;
            bgw.DoWork += UpdateASBSharedBG;
            bgw.WorkerReportsProgress = true;
            bgw.RunWorkerAsync();
        }

        private void UpdateASBShared_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            pbUpdateOtherSkiAreas.Value = e.ProgressPercentage;
            lblStatus.Text = e.UserState.ToString();
            SetTaskStatus("", "", pbUpdateOtherSkiAreas);
        }


        public void UpdateASBSharedBG(object sender, DoWorkEventArgs e)
        {
            CurrentFunction = "UpdateASBShared...";
            DataSet tDS = CF.LoadDataSet(DW.dwConn, $"SELECT * FROM {DW.ActiveDatabase}.spcards WHERE nticktype >= 30000 AND nticktype <=30010 AND NOT (wtp32 is null) AND NOT (lastname = 'TEST' OR firstname = 'TEST')", new DataSet(), "spcards");
            foreach (DataRow tRow in tDS.Tables["spcards"].Rows)
            {
                ((BackgroundWorker)sender).ReportProgress(tDS.Tables["spcards"].Rows.IndexOf(tRow), "Updating asb_passholders");
                string tSQL = string.Empty;
                string mkey = tRow.Field<string>("serialkey");
                if (CF.RowExists(BUY.Buy_Alta_ComConn, "asbshared.asb_passholders", $"issued_by='ALTA' AND serialkey='{mkey}'"))
                {
                    tSQL =  $"UPDATE asbshared.asb_passholders SET pass_status='{tRow.Field<string>("cardstatus")}' WHERE recid={tRow.Field<int>("recid").ToString()}";
                }
                else
                {
                    string mkassanr = mkey.Substring(0, mkey.IndexOf("-"));
                    string mserialnr = mkey.Substring(mkey.IndexOf("-") + 1);
                    string municodenr = mserialnr.Substring(mserialnr.IndexOf("-") + 1);
                    mserialnr = mserialnr.Replace("-" + municodenr, "");
                    int tAge = (tRow.Field<DateTime?>("dob") == null ? 0 : CF.CalcAge(tRow.Field<DateTime>("dob")));
                    tSQL = "INSERT asbshared.asb_passholders (serialkey,nkassanr,nserialnr,nunicodenr,firstname,lastname,city,age,pass_status,issue_date,exp_date,wtp64,issued_by)";
                    tSQL += $" Values ('{mkey}','{mkassanr}','{mserialnr}','{municodenr}','{CF.EscapeChar(tRow.Field<string>("firstname"))}','";
                    tSQL += $"{CF.EscapeChar(tRow.Field<string>("lastname"))}','{tRow.Field<string>("city").Trim()}','{tAge.ToString()}','{tRow.Field<string>("cardstatus")}','";
                    tSQL += $"{tRow.Field<DateTime>("saledate").ToString(Mirror.AxessDateTimeFormat)}','{tRow.Field<DateTime>("expdate").ToString(Mirror.AxessDateFormat)}','{tRow.Field<string>("wtp64")}','ALTA')";
                }
                CF.ExecuteSQL(BUY.Buy_Alta_ComConn, tSQL);
            }
            if (tDS.Tables.Contains("spcards")) tDS.Tables.Remove("spcards");
            return;
        }

        private void OtherSkiAreas_Shown(object sender, EventArgs e)
        {
            LblStartTime.Text = DateTime.Now.ToString(Mirror.AxessDateTimeFormat);
            if (cbUpdateOtherSkiAreas.Checked)
            {
                UpdateOther();
            }
            if (cbUpdateASBShared.Checked)
            {
                UpdateASBShared();
            }
            if (TArgs != null)
            {
                this.Close();
            }
        }

        private void SetTaskStatus(string Description = "", string Extra = "", ProgressBar PB1 = null, ProgressBar PB2 = null)
        {
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
