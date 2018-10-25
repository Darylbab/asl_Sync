﻿using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using asl_SyncLibrary;

namespace MountainCollective
{
    public partial class MountainCollective : Form
    {
        Asgard ASG = new Asgard();
        BUY_ALTA_COM BUY = new BUY_ALTA_COM();
        CommonFunctions CF = new CommonFunctions();
        DataWarehouse DW = new DataWarehouse();
        ASLError myError = new ASLError();

        private string CurrentFunction = string.Empty;

        private string[] TArgs;

        string MCReciprocityConnectionString = "Data Source=184.106.144.107; user= altaadmin; password= 0VM6f58k";
        string DataFolder = string.Empty;

        DataTable Resorts = null;
        int AltaResortID = 1000;

        double ChildPrice = 27.00;
        double AdultPrice = 52.00;


        public event Action<Tuple<string, int, int, int>> ProgressChanged;

        private void OnProgressChanged(Tuple<string, int, int, int> progress) => ProgressChanged?.Invoke(progress);

        public MountainCollective(string[] args = null)
        {
            InitializeComponent();
            TArgs = args;
        }

        private void MountainCollective_Shown(object sender, EventArgs e)
        {
            lblStartTime.Text = $"Start Time: {DateTime.Now.ToString(Mirror.AxessDateTimeFormat)}";
            SetTaskStatus("Initializing");
            Application.DoEvents();
            //args[0] -- "DUIWG"
            //      D = run MTCDownload
            //      U = run MTCUpload
            //      I = run Update_MtnCol_Issued
            //      W = run FixMTCWillcallNames
            //      G = run GetMTCUsage

            if (TArgs == null)
            {
                TArgs = new string[1] { "DUIWG" };
            }
            if (TArgs[0].Contains("D"))
            {
                MTCSPDownload();
            }
            if (TArgs[0].Contains("U"))
            {
                MTCSPUpload();
            }
            if (TArgs[0].Contains("I"))
            {
                Update_MtnCol_Issued();
            }
            if (TArgs[0].Contains("W"))
            {
                FixMTCWillcallNames();
            }
            if (TArgs[0].Contains("G"))
            {
                GetMTCUsage();
            }
            statusStrip1.Items[0].Text = "FINISHED!";
            SetTaskStatus("", DateTime.Now.ToString(Mirror.AxessDateTimeFormat));
            Application.Exit();
        }

        public void GetMTCResorts()
        {
            CurrentFunction = "GetMTCResorts";
            string tSQL = "SELECT * FROM PremiumPass.dbo.resort";
            using (SqlConnection tSQLCon = new SqlConnection(MCReciprocityConnectionString))
            {
                Resorts = CF.LoadTable(tSQLCon, tSQL, "Resorts");
                DataColumn[] keys = new DataColumn[1];
                keys[0] = Resorts.Columns["ResortID"];
                Resorts.PrimaryKey = keys;
                CF.Table2CSV(Resorts, CF.DataFolder + "MCResorts.csv");
            }
        }

        public bool MTCSPDownload() //update ClistPerm from MtnCol Passholder table.
        {
            CurrentFunction = "MTCSPDownload";
            string LFileName = "MCReciprocity";
            statusStrip1.Items[0].Text = "Initializing";
            SetTaskStatus("", "", pbMTCDownload);
            Application.DoEvents();
            //if Resorts list has not been loaded, load it now.
            if (Resorts == null)
            {
                GetMTCResorts();
                if (Resorts == null)
                {
                    return false;
                }
                if (Resorts.Rows.Count == 0)
                {
                    return false;
                }
            }
            //load passholder data from Mountain Collective Server for all but Alta.
            statusStrip1.Items[0].Text = "Loading data from MTC";
            SetTaskStatus("", "", pbMTCDownload);
            Application.DoEvents();
            DataSet tDS = new DataSet();
            using (SqlConnection tSQLCon = new SqlConnection(MCReciprocityConnectionString))
            {
                string tSQL =$"SELECT resortid, firstname, lastname, passnumber, age, city, CAST(customerID AS VarChar(20)) AS custkey FROM premiumpass.dbo.PASSHOLDER Passholder WHERE resortid <> {AltaResortID.ToString()}";
                tDS = CF.LoadDataSet(tSQLCon, tSQL, tDS, LFileName);
            }
            //load CSV files from MtnCol reciprocity partners.
            statusStrip1.Items[0].Text = "Loading data from CSVs";
            SetTaskStatus("", "", pbMTCDownload);
            tDS = CF.CSV2DS($"{CF.DataFolder}NZ.csv", LFileName, tDS, true, true);
            //save MtnCol reciprocity passholder table to CSV file.
            statusStrip1.Items[0].Text = "Saving data to CSV";
            SetTaskStatus("", "", pbMTCDownload);
            CF.Table2CSV(tDS.Tables["MCReciprocity"], $"{CF.DataFolder}{LFileName}.csv");
            //clear okayflag in clistperm for all MtnCol passes.
            statusStrip1.Items[0].Text = "Preparing to process";
            SetTaskStatus("", "", pbMTCDownload);
            CF.ExecuteSQL(DW.dwConn, $"UPDATE {DW.ActiveDatabase}.clistperm SET okayflag=false WHERE passcat='MC'");
            //loop through each pass in reciprocity passholder table.
            pbMTCDownload.Maximum = tDS.Tables[LFileName].Rows.Count;
            foreach (DataRow tRow in tDS.Tables[LFileName].Rows)
            {
                pbMTCDownload.Value = tDS.Tables[LFileName].Rows.IndexOf(tRow) + 1;
                statusStrip1.Items[0].Text = "Processing MTC data";
                SetTaskStatus("", "", pbMTCDownload);
                //clean up last and first name so they don't trash the database.
                string LastName = (tRow["lastname"].ToString().Replace('"', '~').Replace("~", "").Replace("'", " ").ToUpper() + new string(' ',40)).Substring(0,40).Trim();
                string FirstName = (tRow["firstname"].ToString().Trim().Replace('"', '~').Replace("~", "").Replace("'", " ").ToUpper() + new string(' ',30)).Substring(0,30).Trim();
                Regex RgxUrl = new Regex("[^A-Za-z0-9/-/'/#/ /.]");
                //if the passholder name is valid then process, otherwise skip
                if (!string.IsNullOrEmpty(LastName + FirstName)) // || RgxUrl.IsMatch(LastName + FirstName))
                {
                    //get resort information (number and name)
                    Int32 ResortID = tRow.Field<int>("ResortID");
                    string ResortName = Resorts.Rows.Find(ResortID)["ResortName"].ToString().Trim();
                    //clean up passnumber.
                    string PassNo = tRow["passnumber"].ToString().Trim().Replace(" ", "").Replace(@" ", "").Replace(",", "").Replace("'", "").Replace("\"", "").Replace("\r", "");
                    string PassKey = ResortID.ToString() + "-" + PassNo;
                    int? Age = tRow.Field<int?>("age");
                    if (Age == null) Age = 0;
                    double SalePrice = ChildPrice;
                    string PersonType = "CHILD";
                    if (Age > 12 || Age == 0)
                    {
                        SalePrice = AdultPrice;
                        PersonType = "ADULT";
                    }
                    string tSQL = string.Empty;
                    DataRow OldRow = CF.LoadDataRow(DW.dwConn, $"SELECT season, fname, lname, company FROM {DW.ActiveDatabase}.CListPerm WHERE passkey='{PassKey}'");
                    if (OldRow[0].ToString() != string.Empty)
                    {
                        tSQL = $"UPDATE {DW.ActiveDatabase}.clistperm SET season='{CF.SeasonShort}', okayflag=true, fname='{FirstName}', lname='{LastName}', company='{ResortName}', reason='MOUNTAIN COLLECTIVE RECIPROCITY' WHERE passkey='{PassKey}'";
                    }
                    else
                    {
                        tSQL = $"INSERT INTO {DW.ActiveDatabase}.clistperm(dexpires,season,lname,fname,category,reason,passkey,passcat,dent,passtype,tickets,barcode,passnbr,tickdesc,media,appby,dept,okayflag,company,maxuses,price,tickpers,uses,extuses) ";
                        tSQL += $"Values ('{CF.MTCYearEnd.ToString(Mirror.AxessDateFormat)}','{CF.SeasonShort}','{LastName}','{FirstName}','";
                        tSQL += $"{ResortName.ToUpper()} SP','MOUNTAIN COLLECTIVE RECIPROCITY','{PassKey}','MC','{DateTime.Now.ToString(Mirror.AxessDateFormat)}','";
                        tSQL += $"AD',1,'{PassNo}','{PassNo}','ALTA AREA DAY','ALTA ONE USE','Mountain Collective','ADMIN',True,'{ResortName}',";
                        tSQL += $"{CF.MTCMaxUse.ToString()}, {SalePrice.ToString("0#.00")},'MTC {PersonType}',0,0)";
                    }
                    CF.ExecuteSQL(DW.dwConn, tSQL);
                }
                else
                {
                    //passholder name issue, currently do nothing but in the future create an email report for all processing issues.
                }
            }
            //remove any passes from clistperm that are no longer in the passholder table or imported CSV files.
            statusStrip1.Items[0].Text = "Cleaning up";
            SetTaskStatus("", "", pbMTCDownload);
            Application.DoEvents();
            CF.ExecuteSQL(DW.dwConn, $"DELETE FROM {DW.ActiveDatabase}.clistperm WHERE passcat='MC' AND okayflag=false");
            return true;
        }


        public bool MTCSPUpload() //update MtnCol Passholder table using ClistPerm
        {
            CurrentFunction = "MTCSPUpload";
            //remove all Alta passholders from MtnCol passholder table.
            statusStrip1.Items[0].Text = "Preparing database";
            SetTaskStatus("", "", pbMTCUpload);
            CF.ExecuteSQL(new SqlConnection(MCReciprocityConnectionString), $"DELETE FROM premiumpass.dbo.passholder WHERE resortid={AltaResortID.ToString()}", 120);
            //load all SPCards with a WTP64 value, testflag set to false, cardstatus is active and mtnrepflag = "Y".
            statusStrip1.Items[0].Text =  "Loading records from Alta...";
            SetTaskStatus("", "", pbMTCUpload);
            DataSet tDS = CF.LoadDataSet(DW.dwConn, $"SELECT * FROM {DW.ActiveDatabase}.spcards WHERE mtnrepflag='Y' AND cardstatus='A' AND testflag=false AND LENGTH(wtp64)<>0", new DataSet(), "spcards");
            //loop through Alta passholders and insert into MtnCol passholder table
            pbMTCUpload.Maximum = tDS.Tables["spcards"].Rows.Count;
            foreach (DataRow tRow in tDS.Tables["spcards"].Rows)
            {
                pbMTCUpload.Value = tDS.Tables["spcards"].Rows.IndexOf(tRow) + 1;
                statusStrip1.Items[0].Text = "Uploading records to MTC...";
                SetTaskStatus("", "", pbMTCUpload);
                int age = (tRow.Field<DateTime>("dob") == null ? 0 : CF.CalcAge(tRow.Field<DateTime>("dob")));
                string tSQL = "INSERT INTO PremiumPass.dbo.PASSHOLDER(resortid, firstname, lastname, city, state, age, passnumber, customerid) values ";
                tSQL += $"({AltaResortID}, '{CF.EscapeChar(tRow.Field<string>("firstname")).ToUpper().Trim()}','{CF.EscapeChar(tRow.Field<string>("lastname")).ToUpper().Trim()}',";
                tSQL += $"'{tRow.Field<string>("city").Replace(",", "")}','{tRow.Field<string>("state")}',{age.ToString()},'{tRow.Field<string>("wtp64")}','{tRow.Field<string>("perskey")}')";
                if (!CF.ExecuteSQL(new SqlConnection(MCReciprocityConnectionString), tSQL))
                {
                    //cannot insert, currently do nothing but in the future create an email report for all processing issues.
                }
            }
            return true;
        }

        public bool Update_MtnCol_Issued()
        {
            CurrentFunction = "Update_MtnCol_Issued";
            //load salesdata with newest salesdate first
            statusStrip1.Items[0].Text = "Loading Mountain Collective Usage";
            SetTaskStatus("", "", pbMTCUpload);
            string CurDate = DateTime.Now.ToString(Mirror.AxessDateFormat);
            string Tomorrow = DateTime.Now.AddDays(1).ToString(Mirror.AxessDateFormat);
            DataSet tDS = CF.LoadDataSet(DW.dwConn, $"SELECT serialkey, nticktype, szprompt1, NUNICODENR, ttype, saledate, nkassanr, nseriennr, mediaid FROM {DW.ActiveDatabase}.salesdata WHERE dtupdate >= {CurDate} AND dtupdate<{Tomorrow} AND SZPROMPT1 LIKE 'MC%' AND UPPER(tgroup)='MC' AND nkassanr<60 AND nticktype IN (2025, 170, 171, 172)", new DataSet(), "salesday");
            if (tDS.Tables.Contains("salesday"))
            {
                PbMTCUsage.Maximum = tDS.Tables["salesday"].Rows.Count;
                //loop through salesdata rows
                foreach (DataRow tRow in tDS.Tables["salesday"].Rows)
                {

                    statusStrip1.Items[0].Text = "Uploading records to MTC";
                    SetTaskStatus("", "", pbMTCUpload);
                    string mkey = tRow["serialkey"].ToString();
                    //load all mtncol_issued rows for this serialkey
                    string mcclink = tRow["nticktype"].ToString() != "2025" ? "N" : "Y";
                    string szprompt1 = tRow["szprompt1"].ToString().ToUpper();
                    int muses = 1;
                    //if serialkey is in SPCards
                    if (CF.RowExists(DW.dwConn, $"DW.ActiveDatabase.spcards", $"UPPER(tgroup)='MC' AND serialkey='{mkey}'"))
                    {
                        //get uses from SPCards (if nticktype=30036 uses = freedays else 1)
                        if (tRow["nticktype"].ToString() == "2025")
                        {
                            muses = Convert.ToInt16(CF.GetSQLField(DW.dwConn, $"SELECT freedays FROM {DW.ActiveDatabase}.spcards WHERE UPPER(tgroup)='MC' AND serialkey='{mkey}'"));
                        }
                    }
                    bool wasUpdate = false;
                    tDS = CF.LoadDataSet(BUY.Buy_Alta_ComConn, $"SELECT mcpnbr, tied_to_cc, uses, mediaid FROM asbshared.mtncol_issued WHERE serialkey='{mkey} '", tDS, "asbissued");
                    if (tDS.Tables.Contains("asbissued"))
                    {
                        //if mtncol_issued rows exist for this serialkey
                        if (tDS.Tables["asbissued"].Rows.Count != 0)
                        {
                            wasUpdate = true;
                            string mcpnbr = tDS.Tables["asbissued"].Rows[0].Field<string>("mcpnbr");
                            //if mcpnbr, cclink or uses do not match
                            if ((mcpnbr.Length == 0 && szprompt1.Length != 0) || (szprompt1 != mcpnbr) || (tDS.Tables["asbissued"].Rows[0].Field<string>("tied_to_cc") != mcclink) || (tDS.Tables["asbissued"].Rows[0].Field<int>("uses") != muses) || (tDS.Tables["asbissued"].Rows[0].Field<string>("mediaid") != tRow["mediaid"].ToString()))
                            {
                                //set these values in mtncol_issued to match salesdata values
                                CF.ExecuteSQL(BUY.Buy_Alta_ComConn, $"UPDATE asbshared.mtncol_issued SET mcpnbr='{szprompt1}', tied_to_cc='{mcclink}', uses={muses.ToString()}, mediaid='{tRow["mediaid"].ToString()}' WHERE serialkey = '{mkey} '");
                            }
                        }
                    }
                    // if not updated (no rows existed in mtncol_issued then insert
                    if (!wasUpdate)
                    {
                        CF.ExecuteSQL(BUY.Buy_Alta_ComConn, $"INSERT asbshared.mtncol_issued (mcpnbr,sale_type,issued_date, issued_by, nkassanr,nserialnr,nunicodenr,tied_to_cc,uses,created,serialkey,mediaid) VALUES ('{szprompt1}','{tRow["ttype"].ToString()}','{Convert.ToDateTime(tRow["saledate"].ToString()).ToString(Mirror.AxessDateTimeFormat)}','ALTA','{tRow["nkassanr"].ToString()}','{tRow["nseriennr"].ToString()}','{tRow["NUNICODENR"].ToString()}','{mcclink}',{muses.ToString()},NOW(),'{tRow["serialkey"].ToString()}','{tRow["mediaid"].ToString()}')");
                    }
                }
                tDS.Tables.Remove("salesday");
            }
            statusStrip1.Items[0].Text = "Loading Mountain Collective Usage";
            SetTaskStatus("", "", pbMTCUpload);
            //load all mtncol_issued grouping by mcpnbr
            tDS = CF.LoadDataSet(BUY.Buy_Alta_ComConn, "SELECT I.nserialnr, I.mcpnbr, (SELECT MIN(issued_date) FROM asbshared.mtncol_issued WHERE mcpnbr = I.mcpnbr) AS first_issued_date, (SELECT CONCAT_WS('-', nkassanr, MIN(nserialnr), nunicodenr) FROM asbshared.mtncol_issued WHERE mcpnbr = I.mcpnbr AND created = (SELECT  MIN(created) FROM asbshared.mtncol_issued WHERE mcpnbr = I.mcpnbr) LIMIT 1) AS first_Serial, (SELECT  issued_by FROM asbshared.mtncol_issued WHERE mcpnbr = I.mcpnbr AND created = (SELECT  MIN(created) FROM asbshared.mtncol_issued WHERE mcpnbr = I.mcpnbr) LIMIT 1) AS first_issued_by, (SELECT  MAX(issued_date) FROM asbshared.mtncol_issued WHERE mcpnbr = I.mcpnbr) AS last_issued_date, (SELECT  CONCAT_WS('-', nkassanr, MAX(nserialnr), nunicodenr) AS last_Serial FROM asbshared.mtncol_issued WHERE mcpnbr = I.mcpnbr AND created = (SELECT  MAX(created) FROM asbshared.mtncol_issued WHERE mcpnbr = I.mcpnbr) LIMIT 1) AS last_Serial, (SELECT  issued_by FROM asbshared.mtncol_issued WHERE mcpnbr = I.mcpnbr AND created = (SELECT  MAX(created) FROM asbshared.mtncol_issued WHERE mcpnbr = I.mcpnbr) LIMIT 1) AS last_issued_by, (SELECT  COUNT(issued_by) FROM asbshared.mtncol_issued WHERE issued_by = 'SB' AND mcpnbr = I.mcpnbr) AS SBCnt, (SELECT COUNT(issued_by) FROM asbshared.mtncol_issued WHERE issued_by <> 'SB' AND mcpnbr = I.mcpnbr) AS AltaCnt, (SELECT  nkassanr FROM asbshared.mtncol_issued WHERE mcpnbr = I.mcpnbr AND issued_date = (SELECT  MIN(issued_date) FROM asbshared.mtncol_issued WHERE mcpnbr = I.mcpnbr) LIMIT 1) AS nkassanr, MAX(tied_to_cc) AS tied_to_cc, COUNT(*) AS uses FROM asbshared.mtncol_issued AS I WHERE I.mcpnbr LIKE 'MCP%' GROUP BY I.mcpnbr", tDS, "asbissued");
            PbMTCUsage.Maximum = tDS.Tables["asbissued"].Rows.Count;
            if (tDS.Tables.Contains("asbissued"))
            {
                foreach (DataRow tRow in tDS.Tables["asbissued"].Rows)
                {
                    //loop through the rows
                    PbMTCUsage.Value = tDS.Tables["asbissued"].Rows.IndexOf(tRow);
                    statusStrip1.Items[0].Text = "Setting Mountain Collective Usage";
                    SetTaskStatus("", "", pbMTCUpload);
                    string mkassanr = tRow["nkassanr"].ToString();
                    string mserialnr = tRow["nserialnr"].ToString() != string.Empty ? tRow["nserialnr"].ToString() : $"{mkassanr}-{tRow["nserialnr"].ToString()}-{tRow["unicodenr"].ToString()}";
                    //DataRow zRow = CF.LoadDataRow(DW.dwConn, "SELECT ");
                    string mkey = tRow["mcpnbr"].ToString().Trim();
                    string mserialkey = tRow["first_serial"].ToString();
                    string mfirstissuedby = tRow["first_issued_by"].ToString();
                    string mfirstissued_date = Convert.ToDateTime(tRow["first_issued_date"].ToString()).ToString(Mirror.AxessDateFormat);
                    string mlastserialkey = tRow["last_serial"].ToString();
                    string mlasstissuedby = tRow["last_issued_by"].ToString();
                    string mlastissued_date = Convert.ToDateTime(tRow["last_issued_date"].ToString()).ToString(Mirror.AxessDateFormat);
                    int msbcnt = Convert.ToInt16(tRow["sbcnt"].ToString());
                    int maltacnt = Convert.ToInt16(tRow["altacnt"].ToString());
                    long mtotUses = CF.RowCount(DW.dwConn, $"{DW.ActiveDatabase}.skivisits", $"mcpnbr = '{mkey}'");
                    string mcclink = tRow["tied_to_cc"].ToString();
                    if (CF.RowExists(DW.dwConn, $"{DW.ActiveDatabase}.willcall", $"res_no = '{mkey}'"))
                    {
                        CF.ExecuteSQL(DW.dwConn, $"UPDATE {DW.ActiveDatabase}.willcall SET uses={mtotUses.ToString()}, itemstatus='I', issue_date='{mfirstissued_date}', POS='{mkassanr}', first_issue_date='{mfirstissued_date}', issuedby='{mfirstissuedby}', firstserial='{mserialkey}', last_issue_date='{mlastissued_date}', lastissuedby='{mlasstissuedby}', lastserial='{mlastserialkey}', altauses={maltacnt.ToString()}, sbuses={msbcnt.ToString()}, cclink='{mcclink}' WHERE res_no='{mkey} '");
                    }
                }
                tDS.Tables.Remove("asbissued");
            }
            return true;
        }

        public bool GetMTCUsage()
        {
            CurrentFunction = "GetMTCUsage";
            string tQuery = $"SELECT mcpnbr, readdate, serialkey, altaflag, sbflag, '    ' AS startcode, '  location      ' AS startscan, 'Alta Snowbird ' AS resort FROM {DW.ActiveDatabase}.skivisits where mcpnbr LIKE 'MC%' order by mcpnbr, readdate";
            DateTime tReadDate = DateTime.Today;
            string tMMCP = string.Empty;
            string startcode = string.Empty;
            string startscan = string.Empty;
            string ITFile = $"{CF.ITDataFolder}AltaSnowbird_mtcuse.csv";
            string tFileName = $"{CF.DataFolder}AltaSnowbird_mtcuse.csv";
            using (DataSet tDS = CF.LoadDataSet(DW.dwConn, tQuery, new DataSet(), "skivisits"))
            {
                if (tDS.Tables.Contains("skivisits"))
                {
                    CF.String2CSV("mcpnbr,readdate,serialkey,altaflag,sbflag,startcode,startscan,resort", tFileName, false, true);
                    foreach (DataRow tRow in tDS.Tables["skivisits"].Rows)
                    {
                        if (!((tRow.Field<DateTime>("readdate") == tReadDate) && (tRow.Field<string>("mcpnbr") == tMMCP)))
                        {
                            if (tRow.Field<string>("altaflag").ToUpper() == "Y")
                            {
                                if (tRow.Field<string>("sbflag").ToUpper() == "Y")
                                {
                                    startcode = "ASB";
                                    startscan = "ALTA SNOWBIRD";
                                }
                                else
                                {
                                    startcode = "A";
                                    startscan = "ALTA";
                                }
                            }
                            else
                            {
                                startcode = "S";
                                startscan = "SNOWBIRD";
                            }
                            string TempData = $"{tRow.Field<string>("mcpnbr")},{tRow.Field<DateTime>("readdate").ToString()},{tRow.Field<string>("serialkey")},{tRow.Field<string>("altaflag")},{tRow.Field<string>("sbflag")},{startcode},{startscan},Alta Snowbird";
                            CF.String2CSV(TempData, tFileName, true, true);
                            CF.String2CSV(TempData, CF.ITDataFolder + "AltaSnowbird_mtcuse.csv", true, true);
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        public void FixMTCWillcallNames()
        {
            CurrentFunction = "FixMTCWillcallNames";
            statusStrip1.Items[0].Text = "Loading invalid MTC records from SPCards.";
            SetTaskStatus("", "", PbMTCUsage);
            Application.DoEvents();
            string tQuery = $"SELECT serialkey, perskey FROM {DW.ActiveDatabase}.spcards WHERE serialkey IN (SELECT LASTSERIAL FROM {DW.ActiveDatabase}.willcall WHERE NPERSNR is null AND company='MC')";
            DataTable tDT = CF.LoadTable(DW.dwConn, tQuery, "tWillcall");
            PBFixMTCWillcallNames.Maximum = tDT.Rows.Count;
            statusStrip1.Items[0].Text = "Repairing missing NPersNr in willcall.";
            Application.DoEvents();
            foreach (DataRow tRow in tDT.Rows)
            {
                PBFixMTCWillcallNames.Value = tDT.Rows.IndexOf(tRow) + 1;
                Application.DoEvents();
                string tPerskey = tRow["perskey"].ToString();
                string tPOS = tPerskey.Substring(0, tPerskey.IndexOf('-'));
                string tPersNo = tPerskey.Substring(tPerskey.IndexOf('-') + 1);
                CF.ExecuteSQL(DW.dwConn, $"UPDATE {DW.ActiveDatabase}.willcall SET inaxess='Y', npersprojnr=705, npersposnr={tPOS}, npersnr={tPersNo} WHERE lastserial='{tRow["serialkey"].ToString()}'");
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
