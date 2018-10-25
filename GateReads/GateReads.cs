using System;
using System.ComponentModel;
using System.Data;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using asl_SyncLibrary;

namespace GateReads
{
    public partial class SkiVisits : Form
    {
        Asgard ASG = new Asgard();                  //Functions: SetTaskStatus
        Axess_DCI4CRM Axess = new Axess_DCI4CRM();  //Functions: Login, GetUsageList
        Mirror AM = new Mirror();                   //Data: RW-TabLeserTrans, RO-TabKassaTransAufBereit
        BUY_ALTA_COM BUY = new BUY_ALTA_COM();      //Data: RO-MtnCol_Issued
        CommonFunctions CF = new CommonFunctions(); //Functions: GetSQLField, EscapeChar, ExecuteSQL, RowCount, LoadDataSet, Table2CSV      
        DataWarehouse DW = new DataWarehouse();     //Data: RO-SPCards, RW-CListPerm
        ASLError myError = new ASLError();

        private string CurrentFunction = string.Empty;
        private string[] TArgs = null;
        private DateTime SyncDate = DateTime.Today;
        private DateTime StopTime;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        public SkiVisits(string[] args = null)
        {
            InitializeComponent();
            string TempTimeStr = CF.GetSQLField(Asgard.AsgardCon, "SELECT stop_time FROM sync.task WHERE name='GateReads'");
            DateTime.TryParse(TempTimeStr, out DateTime TempTime);
            StopTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, TempTime.Hour, TempTime.Minute, TempTime.Second);
            TArgs = args;
        }


        private void Form_Shown(object sender, EventArgs e)
        {
            //initialize start time and timer interval (default to 10 seconds or 10,000 ticks)
            LblStartTime.Text = $"Start Time: {DateTime.Now.ToString(Mirror.AxessDateTimeFormat)}";
            timer1.Interval = 10000;
            timer1.Enabled = true;
            //SyncSkiVisit(242406984);
            //SyncDate = new DateTime(2018, 4, 23);
            //UpdateSkiVisitsBG();
        }

        /// <summary>
        /// Load all gate reads from Axess using GetUsageList API call
        /// Copy each record to Hilde.Axess_CWC.tablesertrans
        /// </summary>
        private void UpdateLeserTrans()
        {
            CurrentFunction = "UpdateLeserTrans";
            //check for login, if not already logged in then do so.
            if (Axess.Login())
            {
                //Initialize status text and progress bar.
                SSLStatus.Text = "Loading gate reads from Axess.";
                PBGateReads.Value = 0;
                SetTaskStatus();
                //find current maximum nlfdlesertransnr in tabLeserTrans
                Int32 curLeserTransNr = Convert.ToInt32(CF.GetSQLField(AM.MirrorConn, $"SELECT MAX(nlfdlesertransnr) FROM {AM.ActiveDatabase}.tablesertrans"));
                //load gate reads from Axess
                XmlDocument localXML = Axess.GetUsageList(++curLeserTransNr, 1000, 0, 0);
                //finish initializing the progress bar
                PBGateReads.Maximum = localXML.GetElementsByTagName("ACTLOGLINE").Item(0).ChildNodes.Count;
                //notify if 1000 records returned, it means that there are more waiting so this is just a first pass
                if (PBGateReads.Maximum == 1000)
                {
                    SSLStatus.Text += " (Partial)";
                }
                SSLStatus.Text = "Saving to TabLeserTrans and SkiVisits.";
                SetTaskStatus("","",PBGateReads);
                //loop through Axess records
                foreach (XmlNode tNode in localXML.GetElementsByTagName("ACTLOGLINE").Item(0).ChildNodes)   //loop through each LogLine and copy it to memory, then to the mirror database
                {
                    //save the log number
                    curLeserTransNr = Convert.ToInt32(tNode.SelectSingleNode("NLOGNR").InnerText);
                    //increment the progress bar counter
                    PBGateReads.Value++;
                    SetTaskStatus("", "", PBGateReads);
                    //does this record exist in Hilde (axess_cwc)?
                    if (!CF.RowExists(AM.MirrorConn, $"{AM.ActiveDatabase}.tablesertrans", $"NLFDLESERTRANSNR={curLeserTransNr.ToString()}"))
                    {
                        //doesn't exist, insert if it is ours or we can bill it
                        string myFunction = tNode.SelectSingleNode("SZKIND").InnerText.ToUpper();   //Log function, I=Insert, U=Update, D=Delete
                        //should always point to tablesertrans, but don't always count on always.
                        string QualifiedTableName = AM.ActiveDatabase + "." + tNode.SelectSingleNode("SZTABNAME").InnerText.ToLower(); //Qualified table name is database name (dot) table name, needed for SQL query
                        string tFields = string.Empty;  //temp string to hold field values for SQL query
                        string tValues = string.Empty;  //temp string to hold data values for SQL query
                        string myQuery = string.Empty;  //temp string to hold SQL query
                        XmlNode tXMLNode = tNode.SelectSingleNode("ACTLOGVALUES" + ((myFunction == "D") ? "OLD" : "NEW"));  //load the correct values based on function
                        string tKassa = string.Empty;
                        string tSerial = string.Empty;
                        string tMediaID = string.Empty;
                        string tUnicodeNr = string.Empty;
                        foreach (XmlNode lNode in tXMLNode) //loop through all values in the row and add them to the correct temp strings
                        {
                            string fieldName = lNode.SelectSingleNode("SZFIELDNAME").InnerText.ToLower();  //get field name
                            if (!((fieldName == "dtablaufdatum") || (fieldName == "nkundenkartentypnr") || (fieldName == "nperstypnr") || (fieldName == "nrestzeitinmin")))
                            {
                                string fieldValue = (lNode.SelectSingleNode("SZFIELDVALUE").InnerText.Length != 0) ? "'" + CF.EscapeChar(lNode.SelectSingleNode("SZFIELDVALUE").InnerText) + "'" : "NULL";  //get field value and format for mySQL
                                if ((fieldName.Substring(0, 2) == "dt") && (fieldValue != "NULL"))
                                    fieldValue = "'" + CF.TimestampConvert(Convert.ToDouble(fieldValue.Replace("'", ""))) + "'";
                                if (fieldName.ToLower() == "nlognr")
                                    curLeserTransNr = Convert.ToInt32(lNode.SelectSingleNode("SZFIELDVALUE").InnerText);
                                if (fieldName.ToLower() == "nkassanr")
                                    tKassa = fieldValue;
                                if (fieldName == "nseriennr")
                                    tSerial = fieldValue;
                                if (fieldName == "szusatzfelder")
                                    tMediaID = fieldValue;
                                if (fieldName == "nunicodenrintern")
                                    tUnicodeNr = fieldValue;
                                switch (myFunction)     //build SQL query values based on function
                                {
                                    case "I":   //for insert
                                        tFields += fieldName + ",";
                                        tValues += fieldValue + ",";
                                        break;
                                    case "U":   //for update
                                                //tValues += fieldName + "=" + fieldValue + ",";
                                        break;
                                    case "D":   //TODO for delete
                                        break;
                                }
                            }
                        }
                        switch (myFunction) //now build SQL statement 
                        {
                            case "I":   //for insert
                                myQuery = $"INSERT INTO {QualifiedTableName} ({tFields.Remove(tFields.Length - 1)}) values ({tValues.Remove(tValues.Length - 1)})";
                                break;
                            case "U":   //for update
                                        //myQuery = "UPDATE " + QualifiedTableName + " SET " + tValues.Remove(tValues.Length - 1);
                                break;
                            case "D":   //TODO for delete
                                break;
                        }
                        CF.ExecuteSQL(AM.MirrorConn, myQuery);

                        //now find the tabkassatransaufbereit record and verify the nkartennr
                        if (CF.RowExists(AM.MirrorConn, $"{AM.ActiveDatabase}.tabkassatransaufbereit", $"nkassanr={tKassa} AND nseriennr={tSerial} AND nunicodenr={tUnicodeNr} AND mediaid is null"))
                        {
                            long ZaehlerNo = Convert.ToInt64(CF.GetSQLField(AM.MirrorConn, $"SELECT nlfdzaehler FROM {AM.ActiveDatabase}.tabkassatransaufbereit WHERE nkassanr={tKassa.Replace("'", "")} AND nseriennr={tSerial.Replace("'", "")} AND nunicodenr={tUnicodeNr.Replace("'", "")}"));
                            AM.InitSalesTables(ZaehlerNo, true, false);
                        }
                        //and finally update the SkiVisits table in DataWarehouse for this readdate/serialkey
                    SyncSkiVisit(curLeserTransNr);
                    }
                }
            }
            SSLStatus.Text = $"Last run: {DateTime.Now.ToString(Mirror.AxessDateTimeFormat)}";
            PBGateReads.Value = 0;
            PBGateReads.Maximum = 0;
            SetTaskStatus();
            return;
        }


        private void UpdateSkiVisitsBG()
        {
            CurrentFunction = "UpdateSkiVisits";
            SSLStatus.Text = "Loading LeserTrans records.";
            Application.DoEvents();
            string tSQL = $"SELECT NVERKPROJNR as projnr, NSERIENNR, NUNICODENRINTERN as nunicodenr, NKASSANR, SZZUSATZFELDER as mediaid, dtverwdat FROM {AM.ActiveDatabase}.tablesertrans";
            tSQL += $" WHERE dtverwdat >='{SyncDate.ToString(Mirror.AxessDateFormat)} 00:00:00' AND dtverwdat <'{SyncDate.AddDays(1).ToString(Mirror.AxessDateFormat)} 00:00:00' AND nzutrnr < 91 GROUP BY nkassanr, NSERIENNR, NUNICODENRINTERN";
            DataSet tDS = CF.LoadDataSet(AM.MirrorConn, tSQL, new DataSet(), "POEReads");
            Int32 tCnt = tDS.Tables["POEReads"].Rows.Count;
            PBGateReads.Maximum = tCnt;
            PBGateReads.Value = 0;
            SSLStatus.Text = $"Updating SkiVisits ({tCnt.ToString()})";
            Application.DoEvents();
            foreach (DataRow tRow in tDS.Tables["POEReads"].Rows)
            {
                PBGateReads.Value = tDS.Tables["POEReads"].Rows.IndexOf(tRow) + 1;
                System.Diagnostics.Debug.Print(tDS.Tables["POEReads"].Rows.IndexOf(tRow) + 1 + " of " + tCnt);
                SSLStatus.Text = $"Updating SkiVisits ({PBGateReads.Value.ToString()} of {tCnt.ToString()})";
                Application.DoEvents();
                SetSkiVisit(tRow);
            }
            SSLStatus.Text = string.Empty;
        }

        /// <summary>
        /// Syncronize a single ski visit record based on a single leser trans read
        /// </summary>
        /// <param name="aLesserTransNr"></param>
        public void SyncSkiVisit(long aLesserTransNr)
        {
            //load all records with this lesertransnr (should only ever be one)
            string tSQL = $"SELECT NVERKPROJNR as projnr, NSERIENNR, NUNICODENRINTERN as nunicodenr, NKASSANR, SZZUSATZFELDER as mediaid, dtverwdat" +
                $" FROM {AM.ActiveDatabase}.tablesertrans" +
                $" WHERE NLFDLESERTRANSNR ='{aLesserTransNr.ToString()}' AND nzutrnr < 91 LIMIT 1";
            using (DataTable tDT = CF.LoadTable(AM.MirrorConn, tSQL, "POEReads"))
            {
                if (tDT.Rows.Count != 0)
                {
                    SetSkiVisit(tDT.Rows[0]);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aLeserTransRow"></param>  single tablesertrans record 
        /// required fields for aRow (NVERKPROJNR as projnr, NSERIENNR, NUNICODENRINTERN as nunicodenr, NKASSANR, SZZUSATZFELDER as mediaid, dtverwdat)
        private void SetSkiVisit(DataRow aLeserTransRow)
        {
            string mmcpnbr = string.Empty;
            int mprojnr = Convert.ToInt32(aLeserTransRow["projnr"].ToString());
            int NKassaNr = Convert.ToInt32(aLeserTransRow["nkassanr"].ToString());
            int mnseriennr = Convert.ToInt32(aLeserTransRow["nseriennr"].ToString());
            string MediaID = aLeserTransRow["mediaid"].ToString() == null ? "NULL" : "'" + aLeserTransRow["mediaid"].ToString() + "'";
            int? mnunicodenr = aLeserTransRow.Field<int?>("nunicodenr");
            DateTime FilterDate = aLeserTransRow.Field<DateTime>("DTVERWDAT");
            string tRunDate = FilterDate.ToString(Mirror.AxessDateFormat) + " 00:00";
            string tEndDate = FilterDate.AddDays(1).ToString(Mirror.AxessDateFormat) + " 00:00";
            string mdate = (FilterDate.Month < 7 ? (FilterDate.Year - 1).ToString() : FilterDate.Year.ToString()) + "-06-01";
            DateTime TimeSkiAfter3 = new DateTime(FilterDate.Year, FilterDate.Month, FilterDate.Day, 13, 25, 0);    //Usually 14:55
            DateTime TimeAM = new DateTime(FilterDate.Year, FilterDate.Month, FilterDate.Day, 12, 25, 0);           //usually 12:25
            DateTime TimeLate = new DateTime(FilterDate.Year, FilterDate.Month, FilterDate.Day, 13, 25, 0);         //usually 14:25
            bool HasMediaID = !(MediaID == "NULL" || MediaID.Length == 0);

            if (mprojnr == 749)
            {
                mnunicodenr = 1;
            }

            //fix unicode if null (and possible)
            if (mnunicodenr == null)
            {
                string sunicodenr = CF.GetSQLField(AM.MirrorConn, $"SELECT MAX(NUNICODENRINTERN) FROM {AM.ActiveDatabase}.tablesertrans WHERE nkassanr={NKassaNr.ToString()} AND nseriennr={mnseriennr.ToString()} AND nunicodenrintern > 0 AND dtverwdat>='{tRunDate}' AND dtverwdat<'{tEndDate}'").Trim();
                if (sunicodenr == string.Empty)
                { 
                    if (HasMediaID)
                    {
                        sunicodenr = CF.GetSQLField(DW.dwConn, $"SELECT MAX(nunicodenr) FROM applications.salesdata WHERE mediaid={MediaID} AND nkassanr={NKassaNr.ToString()} AND nseriennr={mnseriennr.ToString()} AND nunicodenr > 0 AND saledate>='{mdate}'");
                    }
                    if (sunicodenr == string.Empty)
                    {
                        sunicodenr = CF.GetSQLField(BUY.Buy_Alta_ComConn, $"SELECT MAX(nunicodenr) FROM asbshared.mtncol_issued WHERE mediaid={MediaID} AND nkassanr={NKassaNr.ToString()} AND nserialnr={mnseriennr.ToString()} AND nunicodenr > 0 AND issued_date >= '{mdate}'");
                    }
                }
                if (sunicodenr != string.Empty)
                {
                    int? tunicodenr = Convert.ToInt32(sunicodenr);
                    if (tunicodenr != mnunicodenr)
                    {
                        mnunicodenr = tunicodenr;
                        CF.ExecuteSQL(AM.MirrorConn, $"UPDATE {AM.ActiveDatabase}.tablesertrans SET nunicodenrintern={mnunicodenr.ToString()} WHERE nkassanr={NKassaNr.ToString()} AND nseriennr={mnseriennr.ToString()} AND nunicodenrintern is null AND SZZUSATZFELDER={MediaID}");
                    }
                }
            }

            if (HasMediaID)
            {
                string SerialKey = $"{NKassaNr.ToString()}-{mnseriennr.ToString()}-{mnunicodenr.ToString()}";
                string KartenNr = string.Empty;
                bool IsNewRec = !(CF.RowExists(DW.dwConn, $"{DW.ActiveDatabase}.skivisits",$"readdate='{tRunDate}' AND serialkey='{SerialKey}'"));
                bool IsUpdate = false;
                int WildCat = 0;
                int Collins = 0;
                int Sunnyside = 0;
                int CollinsAngle = 0;
                int Sugarloaf = 0;
                int Supreme = 0;
                int Snowbird = 0;
                int Albion = 0;
                int PassToAlta = 0;
                int PassToSnowbird = 0;
                int SnowbirdDeskTap = 0;
                int AltaDeskTap = 0;
                bool SkiSaltLake = false;
                string SkiAt3Flag = "N";
                string SkiBefore3 = "N";
                string AMFlag = "N";
                string BeginnerFlag = "Y";
                string AltaFlag = "N";
                string SnowbirdFlag = "N";
                //string AltaSnowbirdFlag = "N";
                string LateFlag = "Y";
                string At3Flag = "Y";
                //string ActFlag = "N";
                string TickDesc = "";
                string PersDesc = "";
                string TickGroup = string.Empty;
                int TickType = 0;
                int PersType = 0;
                string ASBOnly = string.Empty;
                string SnowbirdOnly = string.Empty;
                string SkiAt3 = string.Empty;
                string Issued_By = string.Empty;
                int Rides = 0;

                string UnicodeFilter = string.Empty;
                if (mnunicodenr != null)
                {
                    UnicodeFilter = $" OR (nunicodenrintern={mnunicodenr.ToString()})";
                }
                using (DataTable DT_LeserTrans = CF.LoadTable(AM.MirrorConn, $"SELECT npoolnr, nzutrnr, nlesernr, dtverwdat AS ReadDate FROM {AM.ActiveDatabase}.tablesertrans WHERE nkassanr={NKassaNr.ToString()} AND nseriennr={mnseriennr.ToString()} AND ((nunicodenrintern is null){UnicodeFilter}) AND DTVERWDAT>='{tRunDate}' AND DTVERWDAT<'{tEndDate}' AND nzutrnr<91", "lt"))
                {
                    if (DT_LeserTrans != null)
                    {
                        Rides = DT_LeserTrans.Rows.Count;
                        foreach (DataRow DR_LeserTrans in DT_LeserTrans.Rows)
                        {
                            DateTime RideDate = DR_LeserTrans.Field<DateTime>("ReadDate");
                            int LeserNr = Convert.ToInt32(DR_LeserTrans["nlesernr"].ToString());
                            int ZutrNr = Convert.ToInt32(DR_LeserTrans["nzutrnr"].ToString());
                            if (DR_LeserTrans["npoolnr"].ToString().Trim() == "103")
                            {
                                SkiSaltLake = true;
                            }
                            if (RideDate < TimeSkiAfter3)
                            {
                                SkiBefore3 = "Y";
                            }
                            switch (ZutrNr)
                            {
                                case 1:
                                    Collins++;
                                    AltaFlag = "Y";
                                    break;
                                case 2:
                                    CollinsAngle++;
                                    AltaFlag = "Y";
                                    break;
                                case 3:
                                    Sunnyside++;
                                    AltaFlag = "Y";
                                    if (RideDate >= TimeSkiAfter3)
                                    {
                                        SkiAt3Flag = "Y";
                                    }
                                    break;
                                case 4:
                                    Albion++;
                                    AltaFlag = "Y";
                                    if (RideDate >= TimeSkiAfter3)
                                    {
                                        SkiAt3Flag = "Y";
                                    }
                                    break;
                                case 5:
                                    WildCat++;
                                    AltaFlag = "Y";
                                    break;
                                case 6:
                                    Supreme++;
                                    AltaFlag = "Y";
                                    break;
                                case 7:
                                    Sugarloaf++;
                                    AltaFlag = "Y";
                                    break;
                                case 8:
                                    PassToAlta++;
                                    AltaFlag = "Y";
                                    break;
                                case 21:
                                case 22:
                                    AltaDeskTap++;
                                    break;
                                case 23:
                                    AltaDeskTap++;
                                    AltaFlag = "Y";
                                    AMFlag = "Y";
                                    BeginnerFlag = "N";
                                    LateFlag = "N";
                                    At3Flag = "N";
                                    break;
                                case 60:
                                case 61:
                                case 62:
                                case 63:
                                case 64:
                                case 65:
                                case 66:
                                case 67:
                                case 71:
                                case 72:
                                case 73:
                                case 74:
                                case 75:
                                case 76:
                                case 77:
                                case 78:
                                case 79:
                                case 80:
                                case 81:
                                case 82:
                                case 83:
                                case 84:
                                case 85:
                                case 86:
                                case 87:
                                case 88:
                                case 89:
                                case 90:
                                    Snowbird++;
                                    SnowbirdFlag = "Y";
                                    break;
                                case 68:
                                case 69:
                                    SnowbirdDeskTap++;
                                    Snowbird++;
                                    break;
                                case 70:
                                    PassToSnowbird++;
                                    Snowbird++;
                                    SnowbirdFlag = "Y";
                                    break;
                            }

                            DateTime mrtime = RideDate;

                            if (ZutrNr < 3 || ZutrNr > 4) // skied a non beginner lift
                            {
                                BeginnerFlag = "N";
                            }
                            if (mrtime <= TimeAM) // skied in the AM
                            {
                                AMFlag = "Y";
                            }
                            if (mrtime < TimeLate) // skied before 2:30 pm
                            {
                                LateFlag = "N";
                            }
                            if (mrtime < TimeSkiAfter3) //& skied before 3:00 pm
                            {
                                At3Flag = "N";
                            }
                        }
                    }
                }

                //            * *check ski at three***
                SkiAt3 = (SkiAt3Flag == "Y" && SkiBefore3 == "N") ? "Y" : "N";

                DataRow DR_SalesData = CF.LoadDataRow(DW.dwConn, "SELECT nticktype, tickdesc, nperstype, persdesc, wtp32, tgroup FROM applications.salesdata WHERE ttype='S' AND saledate >= '" + mdate + "' AND serialkey='" + SerialKey + "' AND NOT (mediaid is null) LIMIT 1 ");
                if (SkiSaltLake && (AltaFlag == "Y"))
                {
                    TickGroup = "SL"; // Ski Salt Lake
                    TickType = 9000;
                    PersType = 9000;
                    TickDesc = "SKI SALT LAKE";
                    PersDesc = "ADULT";
                }
                else                      //            ** get ticket info **
                {
                    if (DR_SalesData["nticktype"].ToString() != string.Empty)   //data already in salesdata, use it
                    {
                        TickType = Convert.ToInt32(DR_SalesData["nticktype"].ToString());
                        TickDesc = DR_SalesData["tickdesc"].ToString();
                        PersType = Convert.ToInt32(DR_SalesData["nperstype"].ToString());
                        PersDesc = DR_SalesData["persdesc"].ToString();
                        KartenNr = DR_SalesData["wtp32"].ToString();
                        TickGroup = DR_SalesData["tgroup"].ToString();
                        if ((PersType == 90 || PersType == 91 || PersType == 30060 || PersType == 30061) && (AltaFlag == "Y"))  // alta employee passes
                        {
                            TickGroup = "EM";
                        }
                        else
                        {
                            if (SkiAt3 == "Y" && (Rides == Sunnyside + Albion)) // ski at three visits
                            {
                                TickGroup = "S3";
                            }
                            else
                            {
                                if ((TickType == 170 || TickType == 2025) && (AltaFlag == "Y")) // mountain collective
                                {
                                    TickGroup = "MC";
                                }
                                else
                                {

                                    if ((NKassaNr >= 50 && NKassaNr <= 53) || (TickType == 30037 && AltaFlag == "Y")) // Lodge Guests
                                    {
                                        TickGroup = "CC";
                                    }
                                    else
                                    {
                                        if (Rides == Snowbird || Rides == Snowbird + PassToSnowbird)
                                        {
                                            TickGroup = "SO";
                                        }
                                        else
                                        {
                                            if (TickType == 30032) // Alta upgrades to ASB - not alta visits
                                            {
                                                TickGroup = "SO";
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else    //ticktype not in salesdata so look for it in tabKassaTransAufBereit
                    {
                        string msqlcmd2 = $"nkassanr={NKassaNr.ToString()} AND nseriennr={mnseriennr.ToString()} AND nunicodenr={(mnunicodenr==null ? "null" : mnunicodenr.ToString())}";
                        if (CF.RowExists(AM.MirrorConn, $"{AM.ActiveDatabase}.tabkassatransaufbereit", msqlcmd2))
                        {
                            msqlcmd2 = $"UPDATE {AM.ActiveDatabase}.tabkassatransaufbereit SET dtupdate = '{DateTime.Now.ToString(Mirror.AxessDateTimeFormat)}' WHERE {msqlcmd2}";
                            CF.ExecuteSQL(AM.MirrorConn, msqlcmd2);
                        }
                        if (mprojnr == 749 && AltaFlag == "Y")
                        {
                            TickType = 9999;
                            PersType = 9999;
                            TickDesc = "NSAA GOLD CARD";
                            TickGroup = "NG";
                            PersDesc = "ADULT";
                        }
                        else
                        {
                            if (NKassaNr >= 60 && NKassaNr <= 98 && AltaFlag == "Y")    // sold at snowbird, used at alta
                            {
                                TickGroup = "SB";
                            }
                            else
                            {
                                if (AltaFlag == "N" && SnowbirdFlag == "Y") // Snowmbird Only Uses
                                {
                                    TickGroup = "SO";
                                }
                                else
                                    TickGroup = " ";
                            }
                        }
                    }
                }
                //verify MCP information (mcpnbr, issued by)
                DataRow DR_MtnCol_Issued = CF.LoadDataRow(BUY.Buy_Alta_ComConn, $"SELECT mcpnbr, issued_by FROM asbshared.mtncol_issued WHERE nkassanr={NKassaNr.ToString()} AND nserialnr={aLeserTransRow["nseriennr"].ToString()} AND (nunicodenr={(mnunicodenr==null ? "null" : mnunicodenr.ToString())} OR mediaid={MediaID}) LIMIT 1");
                if (DR_MtnCol_Issued != null)
                {
                    mmcpnbr = DR_MtnCol_Issued["mcpnbr"].ToString().ToUpper().Trim();
                    IsUpdate |= (mmcpnbr.ToUpper().Trim() != DR_MtnCol_Issued["mcpnbr"].ToString().ToUpper().Trim());
                    Issued_By = DR_MtnCol_Issued["issued_by"].ToString();
                }
                //if MCP set TickGroup
                if (mmcpnbr.StartsWith("MCP") && (TickGroup != "MC"))
                {
                    TickGroup = "MC";
                    IsUpdate = true;
                }
                //if bad MediaID then try to fix using TabKassaTransAufBereit 
                string msqlcmd = string.Empty;
                if (MediaID == "''" || MediaID.Trim() == string.Empty)
                {
                    DataRow DR_TransAufBereit = CF.LoadDataRow(AM.MirrorConn, "SELECT nkartennr, mediaid FROM " + AM.ActiveDatabase + ".tabkassatransaufbereit WHERE nkassanr = " + NKassaNr.ToString() + " AND nseriennr = " + mnseriennr.ToString() + " AND nunicodenr = " + mnunicodenr.ToString());
                    if (DR_TransAufBereit["nkartennr"].ToString() != string.Empty)
                    {
                        MediaID = $"'{DR_TransAufBereit["mediaid"].ToString()}'";
                        if (DR_TransAufBereit["nkartennr"].ToString().Length <= 8 && KartenNr == string.Empty)
                        {
                            KartenNr = DR_TransAufBereit["nkartennr"].ToString();
                            IsUpdate = true;
                        }
                    }
                }
                string RecID = string.Empty;
                if (!IsNewRec)
                {
                    DataRow DR_SkiVisits = CF.LoadDataRow(DW.dwConn, $"SELECT recid, tgroup, nticktype, nperstype, mcpnbr, nkartennr, persdesc, rides FROM applications.skivisits WHERE readdate='{tRunDate}' AND serialkey='{SerialKey}' LIMIT 1");
                    RecID = DR_SkiVisits["recid"].ToString();
                    IsUpdate |= (TickGroup != DR_SkiVisits.Field<string>("tgroup"));
                    IsUpdate |= (TickType != DR_SkiVisits.Field<int>("nticktype") || PersType != DR_SkiVisits.Field<int>("nperstype"));
                    IsUpdate |= (mmcpnbr.ToUpper().Trim() != DR_SkiVisits.Field<string>("mcpnbr").ToUpper().Trim());
                    IsUpdate |= (KartenNr != DR_SkiVisits.Field<string>("nkartennr"));
                    IsUpdate |= (PersDesc != DR_SkiVisits.Field<string>("persdesc"));
                    IsUpdate |= (Rides != DR_SkiVisits.Field<int>("rides"));
                    IsUpdate |= (TickGroup != DR_SkiVisits.Field<string>("tgroup").Trim());
                }
                if (IsNewRec || IsUpdate)
                {
                    if (IsNewRec)
                    {
                        msqlcmd = $"INSERT {DW.ActiveDatabase}.skivisits (serialkey,readdate,projnr,nkassanr,nseriennr,nunicodenr,rides,tgroup,nticktype,tickdesc,nperstype,persdesc,nkartennr,mediaid,wildcat,collins,colangle,sunny,sugar,supreme,albion,passtoalta,passtosb,snowbird,altadt,sbdt,altaflag,amflag,begflag,lateflag,at3flag,skiat3,sbflag,asbflag,sbonly,issued_by,mcpnbr) values ('" +
                                $"{SerialKey}','{Convert.ToDateTime(tRunDate).ToString(Mirror.AxessDateFormat)}',{mprojnr.ToString()},{NKassaNr.ToString()},{mnseriennr.ToString()}," +
                                $"{(mnunicodenr==null ? "0" : mnunicodenr.ToString())},{Rides.ToString()},'{TickGroup}',{TickType.ToString()}," +
                                $"'{TickDesc}',{PersType.ToString()},'{PersDesc}','{KartenNr}',{MediaID},{WildCat.ToString()},{Collins.ToString()},{CollinsAngle.ToString()},{Sunnyside.ToString()},{Sugarloaf.ToString()},{Supreme.ToString()}," +
                                $"{Albion.ToString()},{PassToAlta.ToString()},{PassToSnowbird.ToString()},{Snowbird.ToString()},{AltaDeskTap.ToString()},{SnowbirdDeskTap.ToString()},'{AltaFlag}'," +
                                $"'{AMFlag}','{BeginnerFlag}','{LateFlag}','{At3Flag}','{SkiAt3}','{SnowbirdFlag}','{ASBOnly}','{SnowbirdOnly}','{Issued_By}','{mmcpnbr}')";
                    }
                    else
                    {
                        msqlcmd = $"UPDATE applications.skivisits SET " +
                            $"rides={Rides.ToString()}," +
                            $"persdesc='{PersDesc}'," +
                            $"tgroup='{TickGroup}'," +
                            $"nticktype={TickType.ToString()}," +
                            $"tickdesc='{TickDesc}'," +
                            $"nperstype={PersType.ToString()}," +
                            $"persdesc='{PersDesc}'," +
                            $"nkartennr='{KartenNr}'," +
                            $"wildcat={WildCat.ToString()}," +
                            $"collins={Collins.ToString()}," +
                            $"colangle={CollinsAngle.ToString()}," +
                            $"sunny={Sunnyside.ToString()}," +
                            $"sugar={Sugarloaf.ToString()}," +
                            $"supreme={Supreme.ToString()}," +
                            $"albion={Albion.ToString()}," +
                            $"passtoalta={PassToAlta.ToString()}," +
                            $"passtosb={PassToSnowbird.ToString()}," +
                            $"altadt={AltaDeskTap.ToString()}," +
                            $"sbdt={SnowbirdDeskTap.ToString()}," +
                            $"snowbird={Snowbird.ToString()}," +
                            $"altaflag='{AltaFlag}'," +
                            $"amflag='{AMFlag}'," +
                            $"begflag='{BeginnerFlag}'," +
                            $"lateflag='{LateFlag}'," +
                            $"at3flag='{At3Flag}'," +
                            $"skiat3='{SkiAt3}'," +
                            $"sbflag='{SnowbirdFlag}'," +
                            $"asbflag='{ASBOnly}'," +
                            $"sbonly='{SnowbirdOnly}'," +
                            $"issued_by='{Issued_By}'," +
                            $"mediaid={MediaID}," +
                            $"mcpnbr='{mmcpnbr}' WHERE recid={RecID}";
                    }
                    CF.ExecuteSQL(DW.dwConn, msqlcmd);
                }
            }
        }

        private void SetTaskStatus(string Description = "", string Extra = "", ProgressBar PB1 = null, ProgressBar PB2 = null)
        {
            if (PB2 != null)
            {
                PB2.Enabled = PB2.Maximum > 0;
                ASG.SetTaskStatus(Application.ProductName, CurrentFunction, SSLStatus.Text, Description, Extra, PB1?.Value, PB1?.Maximum, PB2?.Value, PB2?.Maximum);
                PB1.Refresh();
                PB2.Refresh();
            }
            else if (PB1 != null)
            {
                PB1.Enabled = PB1.Maximum > 0;
                ASG.SetTaskStatus(Application.ProductName, CurrentFunction, SSLStatus.Text, Description, Extra, PB1?.Value, PB1?.Maximum);
                PB1.Refresh();
            }
            else
            {
                ASG.SetTaskStatus(Application.ProductName, CurrentFunction, SSLStatus.Text, Description, Extra);
            }
            Application.DoEvents();
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            if (DateTime.Now > StopTime)
                Application.Exit();
            do
            {
                UpdateLeserTrans();
            }
            while (PBGateReads.Maximum == 1000);
            //UpdateSkiVisitsBG();
            //PBSkiVisits.Value = 0;
            PBGateReads.Value = 0;
            timer1.Enabled = true;
        }


    }
}
