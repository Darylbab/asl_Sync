﻿using System;
using System.ComponentModel;
using System.Data;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using asl_SyncLibrary;

namespace SkiVisits
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
        AltaSync ASy = new AltaSync();

        private const decimal IkonAltaOnly = 70;
        private const decimal IkonAltaBird = 35;
        
        private string CurrentFunction = string.Empty;
        private string[] TArgs = null;
        private DateTime SyncDate = DateTime.Today;
        private DateTime StopTime;

        private DataTable SVTable;  //local copy of DW.SkiVisits
        private DataTable LTTable;  //local copy of AM.tabLeserTrans

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        public SkiVisits(string[] args = null)
        {
            InitializeComponent();

            //SyncList();

            string TempTimeStr = CF.GetSQLField(Asgard.AsgardCon, "SELECT stop_time FROM sync.task WHERE name='GateReads'");

            DataRow FormLocation = CF.GetFormLocation("SkiVisits");
            int StopReason = -1;
            if (CF.RowHasData(FormLocation))
            {
                StartPosition = FormStartPosition.Manual;
                Location = new System.Drawing.Point(FormLocation.Field<int>("left"), FormLocation.Field<int>("top"));
                Height = FormLocation.Field<int>("height");
                Width = FormLocation.Field<int>("width");
                StopReason = FormLocation.Field<int>("stopReason");
            }
            CF.SaveFormData("SkiVisits", Location.Y, Location.X, Height, Width, DateTime.Now, null, StopReason);

            DateTime.TryParse(TempTimeStr, out DateTime TempTime);
            StopTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, TempTime.Hour, TempTime.Minute, TempTime.Second);
            TArgs = args;
        }


        private void Form_Shown(object sender, EventArgs e)
        {

            string SyncDateFilter = SyncDate.ToString(Mirror.AxessDateFormat);
            string SyncDateNext = SyncDate.AddDays(1).ToString(Mirror.AxessDateFormat);
            LblStartTime.Text = $"Start Time: {DateTime.Now.ToString(Mirror.AxessDateTimeFormat)}";
            //SVTable = CF.LoadTable(DW.dwConn, $"SELECT * FROM {DW.ActiveDatabase}.skivisits WHERE readdate = '{SyncDate.ToString(Mirror.AxessDateFormat)}'", "SVTable");
            //LTTable = CF.LoadTable(AM.MirrorConn, $"SELECT * FROM {AM.ActiveDatabase}.tablesertrans WHERE dtverwdat >= '{SyncDateFilter}' AND dtverwdat < '{SyncDateNext}' AND nzutrnr < 92", "LTTable");


           // long[] a = new long[] {   1406016113, 1406016124, 1406388958, 1406385020, 1406385014, 1406450643, 1406434011, 1406419990, 1406384349, 1406384281, 1406520217, 1406520278, 1406520211,1406520215,1406669723,1406669716,1406872255,1406872092,1406872168,1406872039,1406872102,1406795135,1406806285,1406907806,1406907788,1406802708,1406914260,1406914141,1406919578,1406806290,1406806305,1406950002,1406950010,1406806278,1406919994,1406946122,1406946118,1407274972,1407274777,1407274953,1407274763,1407274776};
            //foreach (long b in a)
            //{
            //    SyncSkiVisit(b);
            //}
            //SyncSkiVisit(1414385193);
            SyncDate = DateTime.Today;
            //UpdateSkiVisitsBG();
            RebuildDay(SyncDate);
            Cleanup();
            Application.Exit();
            //timer1.Interval = 1000;
            //timer1.Enabled = true;
        }

        private void Cleanup()
        {
            //fix NSAA Gold
            string TSql = "UPDATE applications.skivisits SET nticktype=9999, nperstype=9999, tickdesc='NSAA GOLD CARD', persdesc='ADULT', tgroup = 'NG' WHERE TGROUP='' AND readdate > '2018-08-04' AND NKASSANR =99";
            CF.ExecuteSQL(DW.dwConn, TSql);
            //fix Snowbird issued and skied only
            TSql = "UPDATE applications.skivisits SET tgroup = 'SO' WHERE ALTAFLAG='N' AND TGROUP='' AND readdate > '2018-08-04' AND NKASSANR BETWEEN 60 AND 91";
            CF.ExecuteSQL(DW.dwConn, TSql);
            //fix Snowbird issued Alta skied.
            TSql = "UPDATE applications.skivisits SET tgroup = 'SB' WHERE ALTAFLAG='Y' AND TGROUP='' AND readdate > '2018-08-04' AND NKASSANR BETWEEN 60 AND 91";
            CF.ExecuteSQL(DW.dwConn, TSql);
            //find missing TGroup info.
            TSql = "UPDATE applications.skivisits AS V INNER JOIN applications.salesdata AS S ON S.SERIALKEY=V.SERIALKEY SET V.TGROUP = S.TGROUP, V.NTICKTYPE = S.NTICKTYPE, V.TICKDESC = S.TICKDESC, V.NPERSTYPE = S.NPERSTYPE, V.PERSDESC = S.PERSDESC WHERE V.altaflag = 'Y' AND V.tgroup = '' AND V.readdate > '2018-08-04'";
            CF.ExecuteSQL(DW.dwConn, TSql);
        }

        /// <summary>
        /// Load all gate reads from Axess using GetUsageList API call
        /// Copy each record to Hilde.Axess_CWC.tablesertrans
        /// </summary>
        //private Int32 UpdateLeserTrans(Int32 LastNLFDLeserTransNr = 0)
        //{
        //    CurrentFunction = "UpdateLeserTrans";
        //    Int32 curLeserTransNr = LastNLFDLeserTransNr;
        //    //check for login, if not already logged in then do so.
        //    if (Axess.Login())
        //    {
        //        //Initialize status text and progress bar.
        //        SSLStatus.Text = "Loading gate reads from Axess.";
        //        Refresh();
        //        PBGateReads.Value = 0;
        //        //SetTaskStatus();
        //        //find current maximum nlfdlesertransnr in tabLeserTrans
        //        if (curLeserTransNr == 0)
        //        {
        //            curLeserTransNr = Convert.ToInt32(CF.GetSQLField(AM.MirrorConn, $"SELECT MAX(nlfdlesertransnr) FROM {AM.ActiveDatabase}.tablesertrans"));
        //        }

        //        //load gate reads from Axess
        //        XmlDocument localXML = Axess.GetUsageList(++curLeserTransNr, 1000, 0, 0);
        //        //finish initializing the progress bar
        //        PBGateReads.Maximum = localXML.GetElementsByTagName("ACTLOGLINE").Item(0).ChildNodes.Count;
        //        //notify if 1000 records returned, it means that there are more waiting so this is just a first pass
        //        SSLStatus.Text = "Saving to TabLeserTrans and SkiVisits.";
        //        Refresh();
        //        if (PBGateReads.Maximum == 1000)
        //        {
        //            SSLStatus.Text += " (Partial)";
        //        }
        //        //SetTaskStatus("","",PBGateReads);
        //        //loop through Axess records
        //        foreach (XmlNode tNode in localXML.GetElementsByTagName("ACTLOGLINE").Item(0).ChildNodes)   //loop through each LogLine and copy it to memory, then to the mirror database
        //        {
        //            bool SkipRec = false;
        //            //save the log number
        //            curLeserTransNr = Convert.ToInt32(tNode.SelectSingleNode("NLOGNR").InnerText);
        //            //increment the progress bar counter
        //            PBGateReads.Value++;
        //            //SetTaskStatus("", "", PBGateReads);
        //            //does this record exist in Hilde (axess_cwc)?
        //            if (!CF.RowExists(AM.MirrorConn, $"{AM.ActiveDatabase}.tablesertrans", $"NLFDLESERTRANSNR={curLeserTransNr.ToString()}"))
        //            {
        //                //doesn't exist, insert if it is ours or we can bill it
        //                string myFunction = tNode.SelectSingleNode("SZKIND").InnerText.ToUpper();   //Log function, I=Insert, U=Update, D=Delete
        //                                                                                            //should always point to tablesertrans, but don't always count on always.
        //                string QualifiedTableName = AM.ActiveDatabase + "." + tNode.SelectSingleNode("SZTABNAME").InnerText.ToLower(); //Qualified table name is database name (dot) table name, needed for SQL query
        //                string tFields = string.Empty;  //temp string to hold field values for SQL query
        //                string tValues = string.Empty;  //temp string to hold data values for SQL query
        //                string myQuery = string.Empty;  //temp string to hold SQL query
        //                XmlNode tXMLNode = tNode.SelectSingleNode("ACTLOGVALUES" + ((myFunction == "D") ? "OLD" : "NEW"));  //load the correct values based on function
        //                string tKassa = string.Empty;
        //                string tSerial = string.Empty;
        //                string tMediaID = string.Empty;
        //                string tUnicodeNr = string.Empty;
        //                foreach (XmlNode lNode in tXMLNode) //loop through all values in the row and add them to the correct temp strings
        //                {
        //                    string fieldName = lNode.SelectSingleNode("SZFIELDNAME").InnerText.ToLower();  //get field name
        //                    if (!(SkipRec || (fieldName == "dtablaufdatum") || (fieldName == "nkundenkartentypnr") || (fieldName == "nperstypnr") || (fieldName == "nrestzeitinmin")))
        //                    {
        //                        string fieldValue = (lNode.SelectSingleNode("SZFIELDVALUE").InnerText.Length != 0) ? "'" + CF.EscapeChar(lNode.SelectSingleNode("SZFIELDVALUE").InnerText) + "'" : "NULL";  //get field value and format for mySQL
        //                        if (fieldName == "nzutrnr")
        //                        {
        //                            SkipRec = (Convert.ToInt32(fieldValue.Replace("'","")) > 91);
        //                            //break;
        //                        }
        //                        if (fieldName == "dtverwdat")
        //                        {
        //                            SSLStatus.Text = CF.TimestampConvert(Convert.ToDouble(fieldValue.Replace("'", "")));
        //                            if (PBGateReads.Maximum == 1000)
        //                            {
        //                                SSLStatus.Text += " (Partial)";
        //                            }
        //                            Refresh();
        //                        }
        //                        if (!SkipRec)
        //                        {
        //                            if ((fieldName.Substring(0, 2) == "dt") && (fieldValue != "NULL"))
        //                                fieldValue = "'" + CF.TimestampConvert(Convert.ToDouble(fieldValue.Replace("'", ""))) + "'";
        //                            if (fieldName.ToLower() == "nlognr")
        //                                curLeserTransNr = Convert.ToInt32(lNode.SelectSingleNode("SZFIELDVALUE").InnerText);
        //                            if (fieldName.ToLower() == "nkassanr")
        //                                tKassa = fieldValue;
        //                            if (fieldName == "nseriennr")
        //                                tSerial = fieldValue;
        //                            if (fieldName == "szzusatzfelder")
        //                            {
        //                                if (fieldValue.Replace("'", "").Length > 16)
        //                                {
        //                                    fieldValue = $"'{fieldValue.Replace("'", "").Substring(0, 16)}'";
        //                                }
        //                                tMediaID = fieldValue;
        //                            }
        //                            if (fieldName == "nunicodenrintern")
        //                                tUnicodeNr = fieldValue;
        //                            switch (myFunction)     //build SQL query values based on function
        //                            {
        //                                case "I":   //for insert
        //                                    tFields += fieldName + ",";
        //                                    tValues += fieldValue + ",";
        //                                    break;
        //                                case "U":   //for update
        //                                            //tValues += fieldName + "=" + fieldValue + ",";
        //                                    break;
        //                                case "D":   //TODO for delete
        //                                    break;
        //                            }
        //                        }
        //                    }
        //                }
        //                if (!SkipRec)
        //                {
        //                    switch (myFunction) //now build SQL statement 
        //                    {
        //                        case "I":   //for insert
        //                            myQuery = $"INSERT INTO {QualifiedTableName} ({tFields.Remove(tFields.Length - 1)}) values ({tValues.Remove(tValues.Length - 1)})";
        //                            break;
        //                        case "U":   //for update
        //                                    //myQuery = "UPDATE " + QualifiedTableName + " SET " + tValues.Remove(tValues.Length - 1);
        //                            break;
        //                        case "D":   //TODO for delete
        //                            break;
        //                    }
        //                    CF.ExecuteSQL(AM.MirrorConn, myQuery);

        //                    //now find the tabkassatransaufbereit record and verify the nkartennr
        //                    //if (CF.RowExists(AM.MirrorConn, $"{AM.ActiveDatabase}.tabkassatransaufbereit", $"nkassanr={tKassa} AND nseriennr={tSerial} AND nunicodenr={tUnicodeNr} AND mediaid is null"))
        //                    //{
        //                    //    long ZaehlerNo = Convert.ToInt64(CF.GetSQLField(AM.MirrorConn, $"SELECT nlfdzaehler FROM {AM.ActiveDatabase}.tabkassatransaufbereit WHERE nkassanr={tKassa.Replace("'", "")} AND nseriennr={tSerial.Replace("'", "")} AND nunicodenr={tUnicodeNr.Replace("'", "")}"));
        //                    //    AM.InitSalesTables(ZaehlerNo, true, false);
        //                    //}
        //                    //and finally update the SkiVisits table in DataWarehouse for this readdate/serialkey
        //                    SyncSkiVisit(curLeserTransNr);
        //                }
        //            }
        //        }
        //    }
        //    SSLStatus.Text = $"Last run: {DateTime.Now.ToString(Mirror.AxessDateTimeFormat)}";
        //    PBGateReads.Value = 0;
        //    //PBGateReads.Maximum = 0;
        //    //SetTaskStatus();
        //    return curLeserTransNr;
        //}


        //private void UpdateSkiVisitsBG()
        //{
        //    CurrentFunction = "UpdateSkiVisits";
        //    SSLStatus.Text = "Loading LeserTrans records for IKON.";
        //    Application.DoEvents();
        //    string tSQL = $"SELECT NVERKPROJNR as projnr, NSERIENNR, NUNICODENRINTERN as nunicodenr, NKASSANR, SZZUSATZFELDER as mediaid, dtverwdat FROM {AM.ActiveDatabase}.tablesertrans";
        //    tSQL += $" WHERE nkassanr = 27 AND dtverwdat >='{SyncDate.ToString(Mirror.AxessDateFormat)} 00:00:00' AND dtverwdat <'{SyncDate.AddDays(1).ToString(Mirror.AxessDateFormat)} 00:00:00' AND nzutrnr < 92 GROUP BY SUBSTR(SZZUSATZFELDER,1,16)";
        //    DataSet tDS = CF.LoadDataSet(AM.MirrorConn, tSQL, new DataSet(), "POEReads");
        //    Int32 tCnt = tDS.Tables["POEReads"].Rows.Count;
        //    PBGateReads.Maximum = tCnt;
        //    PBGateReads.Value = 0;
        //    SSLStatus.Text = $"Updating SkiVisits ({tCnt.ToString()})";
        //    Application.DoEvents();
        //    foreach (DataRow tRow in tDS.Tables["POEReads"].Rows)
        //    {
        //        PBGateReads.Value = tDS.Tables["POEReads"].Rows.IndexOf(tRow) + 1;
        //        System.Diagnostics.Debug.Print(tDS.Tables["POEReads"].Rows.IndexOf(tRow) + 1 + " of " + tCnt);
        //        SSLStatus.Text = $"Updating SkiVisits ({PBGateReads.Value.ToString()} of {tCnt.ToString()})";
        //        Application.DoEvents();
        //        SetSkiVisit(tRow);
        //    }

        //    SSLStatus.Text = "Loading LeserTrans records (No IKON).";
        //    Application.DoEvents();
        //    tSQL = $"SELECT NVERKPROJNR as projnr, NSERIENNR, NUNICODENRINTERN as nunicodenr, NKASSANR, SZZUSATZFELDER as mediaid, dtverwdat FROM {AM.ActiveDatabase}.tablesertrans";
        //    tSQL += $" WHERE nkassanr <> 27 AND dtverwdat >='{SyncDate.ToString(Mirror.AxessDateFormat)} 00:00:00' AND dtverwdat <'{SyncDate.AddDays(1).ToString(Mirror.AxessDateFormat)} 00:00:00' AND nzutrnr < 92 GROUP BY nkassanr, NSERIENNR, NUNICODENRINTERN";
        //    tDS = CF.LoadDataSet(AM.MirrorConn, tSQL, new DataSet(), "POEReads");
        //    tCnt = tDS.Tables["POEReads"].Rows.Count;
        //    PBGateReads.Maximum = tCnt;
        //    PBGateReads.Value = 0;
        //    SSLStatus.Text = $"Updating SkiVisits ({tCnt.ToString()})";
        //    Application.DoEvents();
        //    foreach (DataRow tRow in tDS.Tables["POEReads"].Rows)
        //    {
        //        PBGateReads.Value = tDS.Tables["POEReads"].Rows.IndexOf(tRow) + 1;
        //        System.Diagnostics.Debug.Print(tDS.Tables["POEReads"].Rows.IndexOf(tRow) + 1 + " of " + tCnt);
        //        SSLStatus.Text = $"Updating SkiVisits ({PBGateReads.Value.ToString()} of {tCnt.ToString()})";
        //        Application.DoEvents();
        //        SetSkiVisit(tRow);
        //    }
        //    SSLStatus.Text = string.Empty;
        //}

        ///// <summary>
        ///// Syncronize a single ski visit record based on a single leser trans read
        ///// </summary>
        ///// <param name="aLesserTransNr"></param>
        public void SyncSkiVisit(long aLesserTransNr)
        {
            //load all records with this lesertransnr (should only ever be one)
            string tSQL = $"SELECT NVERKPROJNR as projnr, NSERIENNR, NUNICODENRINTERN as nunicodenr, NKASSANR, SZZUSATZFELDER as mediaid, dtverwdat" +
                $" FROM {AM.ActiveDatabase}.tablesertrans" +
                $" WHERE NLFDLESERTRANSNR ='{aLesserTransNr.ToString()}' AND nzutrnr < 92 LIMIT 1";
            using (DataTable tDT = CF.LoadTable(AM.MirrorConn, tSQL, "POEReads"))
            {
                if (tDT.Rows.Count != 0)
                {
                    SetSkiVisit(tDT.Rows[0]);
                }
            }
        }

        private void SyncList()
        {
            long[] LTNos = new long[] { 1409566623, 1410531913 };
            int I = 0;
            foreach (long LT in LTNos)
            {
                I = I + 1;
                System.Diagnostics.Debug.Print(I.ToString() + " of " + LTNos.Length.ToString());
                SyncSkiVisit(LT);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aLeserTransRow"></param>  single tablesertrans record 
        /// required fields for aRow (NVERKPROJNR as projnr, NSERIENNR, NUNICODENRINTERN as nunicodenr, NKASSANR, SZZUSATZFELDER as mediaid, dtverwdat)
        private void SetSkiVisit(DataRow aLeserTransRow)
        {
            DateTime FilterDate = aLeserTransRow.Field<DateTime>("DTVERWDAT");
            if (FilterDate.Date == new DateTime(2018, 12, 31) && FilterDate.TimeOfDay > new TimeSpan(16, 55, 0))
                return;

            string mmcpnbr = string.Empty;
            int mprojnr = Convert.ToInt32(aLeserTransRow["projnr"].ToString());
            int NKassaNr = Convert.ToInt32(aLeserTransRow["nkassanr"].ToString());
            int mnseriennr = Convert.ToInt32(aLeserTransRow["nseriennr"].ToString());
            string MediaID = aLeserTransRow["mediaid"].ToString();
            if (MediaID.Length > 16)
            {
                MediaID = MediaID.Substring(0, 16);
            }
            if (MediaID != "NULL")
            {
                MediaID = $"'{MediaID}'";
            }
            int? mnunicodenr = aLeserTransRow.Field<int?>("nunicodenr");
            string tRunDate = FilterDate.ToString(Mirror.AxessDateFormat) + " 00:00";
            string tEndDate = FilterDate.AddDays(1).ToString(Mirror.AxessDateFormat) + " 00:00";
            string mdate = (FilterDate.Month < 7 ? (FilterDate.Year - 1).ToString() : FilterDate.Year.ToString()) + "-04-01";
            DateTime TimeSkiAfter3 = new DateTime(FilterDate.Year, FilterDate.Month, FilterDate.Day, 14, 55, 0);    //Usually 14:55
            DateTime TimeAM = new DateTime(FilterDate.Year, FilterDate.Month, FilterDate.Day, 12, 25, 0);           //usually 12:25
            DateTime TimeLate = new DateTime(FilterDate.Year, FilterDate.Month, FilterDate.Day, 14, 25, 0);         //usually 14:25
            bool HasMediaID = !(MediaID == "NULL" || MediaID.Length == 0);
            bool SkiedAlta = false;
            bool SkiedSnowBird = false;

            if (mprojnr == 749)
            {
                mnunicodenr = 1;
            }

            if (NKassaNr != 27)
            {
                //fix unicode if null (and possible)
                if (mnunicodenr == null || mnunicodenr == 0)
                {
                    string sunicodenr = CF.GetSQLField(AM.MirrorConn, $"SELECT MAX(NUNICODENRINTERN) FROM {AM.ActiveDatabase}.tablesertrans WHERE dtverwdat>='{tRunDate}' AND dtverwdat<'{tEndDate}' AND nkassanr={NKassaNr.ToString()} AND nseriennr={mnseriennr.ToString()} AND nunicodenrintern > 0").Trim();
                    if (sunicodenr == string.Empty)
                    { 
                        if (HasMediaID)
                        {
                            sunicodenr = CF.GetSQLField(DW.dwConn, $"SELECT MAX(nunicodenr) FROM applications.salesdata WHERE saledate>='{mdate}' AND mediaid={MediaID} AND nkassanr={NKassaNr.ToString()} AND nseriennr={mnseriennr.ToString()} AND nunicodenr > 0");
                        }
                        if (sunicodenr == string.Empty)
                        {
                            sunicodenr = CF.GetSQLField(BUY.Buy_Alta_ComConn, $"SELECT MAX(nunicodenr) FROM asbshared.mtncol_issued WHERE issued_date >= '{mdate}' AND mediaid={MediaID} AND nkassanr={NKassaNr.ToString()} AND nserialnr={mnseriennr.ToString()} AND nunicodenr > 0");
                        }
                    }
                    if (sunicodenr != string.Empty)
                    {
                        int? tunicodenr = Convert.ToInt32(sunicodenr);
                        if (tunicodenr != mnunicodenr)
                        {
                            mnunicodenr = tunicodenr;
                            CF.ExecuteSQL(AM.MirrorConn, $"UPDATE {AM.ActiveDatabase}.tablesertrans SET nunicodenrintern={mnunicodenr.ToString()} WHERE nkassanr={NKassaNr.ToString()} AND nseriennr={mnseriennr.ToString()} AND nunicodenrintern = 0 AND SZZUSATZFELDER={MediaID}");
                        }
                    }
                }
                else
                {
                    CF.ExecuteSQL(DW.dwConn, $"UPDATE {DW.ActiveDatabase}.skivisits SET nunicodenr={mnunicodenr.ToString()}, serialkey = '{NKassaNr.ToString()}-{mnseriennr.ToString()}-{mnunicodenr.ToString()}' WHERE serialkey = '{NKassaNr.ToString()}-{mnseriennr.ToString()}-' AND mediaid={MediaID}");
                }
            }

            if (HasMediaID)
            {
                string SerialKey = $"{NKassaNr.ToString()}-{mnseriennr.ToString()}-{mnunicodenr.ToString()}";
                string KartenNr = string.Empty;
                string tWhere = (NKassaNr != 27 ? $"serialkey = '{SerialKey}'" : $"mediaid = {MediaID}");
                bool IsNewRec = !(CF.RowExists(DW.dwConn, $"{DW.ActiveDatabase}.skivisits", $"readdate='{tRunDate}' AND {tWhere}"));
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

                string tSQL = $"SELECT npoolnr, nzutrnr, nlesernr, dtverwdat AS ReadDate FROM {AM.ActiveDatabase}.tablesertrans WHERE DTVERWDAT>='{tRunDate}' AND DTVERWDAT<'{tEndDate}' AND ";
                if (NKassaNr == 27)
                {
                    tSQL += $"SZZUSATZFELDER = '{MediaID.Replace("'","")}'";
                }
                else
                {
                    string UnicodeFilter = string.Empty;
                    if (mnunicodenr != null)
                    {
                        UnicodeFilter = $" OR (nunicodenrintern={mnunicodenr.ToString()})";
                    }
                    tSQL += $"nkassanr ={NKassaNr.ToString()} AND nseriennr={mnseriennr.ToString()} AND ((nunicodenrintern is null){UnicodeFilter})";
                }
                tSQL += $" AND nzutrnr<92";

                using (DataTable DT_LeserTrans = CF.LoadTable(AM.MirrorConn, tSQL, "lt"))
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
                                case 24:
                                    AltaDeskTap++;
                                    AltaFlag = "Y";
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
                                case 91:
                                    Snowbird++;
                                    SnowbirdFlag = "Y";
                                    break;
                                case 68:
                                case 69:
                                    SnowbirdDeskTap++;
                                    Snowbird++;
                                    SnowbirdFlag = "Y";
                                    break;
                                case 9:
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

                string SaleDateText = $">= '{mdate}'";
                if (NKassaNr == 27)
                {
                    SaleDateText = $"= '{tRunDate}'";
                }
                DataRow DR_SalesData = CF.LoadDataRow(DW.dwConn, $"SELECT nticktype, tickdesc, nperstype, persdesc, wtp32, tgroup, serialkey, nkassanr, nseriennr, nunicodenr FROM applications.salesdata WHERE saledate {SaleDateText} AND ttype='S' AND {tWhere} AND NOT (mediaid is null) ORDER BY saledate DESC LIMIT 1 ");
                bool YAltaFlag = (AltaFlag == "Y");
                if (SkiSaltLake && YAltaFlag)
                {
                    TickGroup = "SL"; // Ski Salt Lake
                    TickType = 9000;
                    PersType = 9000;
                    TickDesc = "SKI SALT LAKE";
                    PersDesc = "ADULT";
                }
                else                      //            ** get ticket info **
                {
                    if (CF.RowHasData(DR_SalesData))
                    {
                        if (DR_SalesData["nticktype"].ToString() != string.Empty)   //data already in salesdata, use it
                        {
                            TickType = Convert.ToInt32(DR_SalesData["nticktype"].ToString());
                            TickDesc = DR_SalesData["tickdesc"].ToString();
                            PersType = Convert.ToInt32(DR_SalesData["nperstype"].ToString());
                            PersDesc = DR_SalesData["persdesc"].ToString();
                            KartenNr = DR_SalesData["wtp32"].ToString();
                            TickGroup = DR_SalesData["tgroup"].ToString();
                            if ((PersType == 90 || PersType == 91 || PersType == 30060 || PersType == 30061) && YAltaFlag)  // alta employee passes
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
                                    if ((TickType == 170 || TickType == 2025) && YAltaFlag) // mountain collective
                                    {
                                        TickGroup = "MC";
                                    }
                                    else
                                    {

                                        if ((NKassaNr >= 50 && NKassaNr <= 53) || (TickType == 30037 && YAltaFlag)) // Lodge Guests
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
                            //string msqlcmd2 = $"nkassanr={NKassaNr.ToString()} AND nseriennr={mnseriennr.ToString()} AND nunicodenr={(mnunicodenr == null ? "null" : mnunicodenr.ToString())}";
                            //if (CF.RowExists(AM.MirrorConn, $"{AM.ActiveDatabase}.tabkassatransaufbereit", msqlcmd2))
                            //{
                            //    msqlcmd2 = $"UPDATE {AM.ActiveDatabase}.tabkassatransaufbereit SET dtupdate = '{DateTime.Now.ToString(Mirror.AxessDateTimeFormat)}' WHERE {msqlcmd2}";
                            //    CF.ExecuteSQL(AM.MirrorConn, msqlcmd2);
                            //}
                            if (mprojnr == 749 && YAltaFlag)
                            {
                                TickType = 9999;
                                PersType = 9999;
                                TickDesc = "NSAA GOLD CARD";
                                TickGroup = "NG";
                                PersDesc = "ADULT";
                            }
                            else
                            {
                                if (NKassaNr >= 60 && NKassaNr <= 98 && YAltaFlag)    // sold at snowbird, used at alta
                                {
                                    TickGroup = "SB";
                                }
                                else
                                {
                                    if (!YAltaFlag && SnowbirdFlag == "Y") // Snowmbird Only Uses
                                    {
                                        TickGroup = "SO";
                                    }
                                    else
                                        TickGroup = "";
                                }
                            }
                        }
                    }
                }
                //verify MCP information (mcpnbr, issued by)
                DataRow DR_MtnCol_Issued = CF.LoadDataRow(BUY.Buy_Alta_ComConn, $"SELECT mcpnbr, issued_by FROM asbshared.mtncol_issued WHERE nkassanr={NKassaNr.ToString()} AND nserialnr={aLeserTransRow["nseriennr"].ToString()} AND (nunicodenr={(mnunicodenr==null ? "null" : mnunicodenr.ToString())} OR mediaid={MediaID}) LIMIT 1");
                if (CF.RowHasData(DR_MtnCol_Issued))
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
                //Ski Utah Gold and Silver
                if (NKassaNr >= 106 && NKassaNr <= 110) //ski utah POS range
                {
                    TickGroup = "SP";
                    TickType = 60002;
                    PersType = 60001;
                    TickDesc = "Ski Utah Gold/Silver";
                    PersDesc = "Ski Utah Adult";
                }
                //if bad MediaID then try to fix using TabKassaTransAufBereit 
                string msqlcmd = string.Empty;
                if (MediaID.Replace("'", "").Trim() == string.Empty)
                {
                    DataRow DR_TransAufBereit = CF.LoadDataRow(AM.MirrorConn, "SELECT nkartennr, mediaid FROM " + AM.ActiveDatabase + ".tabkassatransaufbereit WHERE nkassanr = " + NKassaNr.ToString() + " AND nseriennr = " + mnseriennr.ToString() + " AND nunicodenr = " + mnunicodenr.ToString());
                    if (DR_TransAufBereit["nkartennr"].ToString() != string.Empty)
                    {
                        MediaID = DR_TransAufBereit["mediaid"].ToString();
                        if (MediaID.Length > 16)
                        {
                            MediaID = MediaID.Substring(0, 16);
                        }
                        MediaID = $"'{MediaID}'";
                        if (DR_TransAufBereit["nkartennr"].ToString().Length <= 8 && KartenNr == string.Empty)
                        {
                            KartenNr = DR_TransAufBereit["nkartennr"].ToString();
                            IsUpdate = true;
                        }
                    }
                }
                if (NKassaNr == 27)
                {
                    string lSerialKey = $"'{NKassaNr.ToString()}-{mnseriennr.ToString()}-{mnunicodenr.ToString()}'";
                    string MyMediaID = MediaID.Replace("'", "").Substring(0, 16);
                    decimal IkonValue = 0;

                    DataRow SV = CF.LoadDataRow(DW.dwConn, $"SELECT SBFlag, AltaFlag FROM {DW.ActiveDatabase}.skivisits WHERE readdate='{tRunDate}' AND mediaid={MediaID}");
                    if (CF.RowHasData(SV))
                    {
                        bool SkiedAltaa = SV["altaflag"].ToString() == "Y" || YAltaFlag;
                        bool skiedSBB = SV["sbflag"].ToString() == "Y" || SnowbirdFlag == "Y";
                        TickGroup = SkiedAltaa ? "IK" : (skiedSBB ? "SO" : "UNK");
                        if (SkiedAltaa)
                        {
                            if (skiedSBB)
                            {
                                IkonValue = IkonAltaBird;
                            }
                            else
                            {
                                IkonValue = IkonAltaOnly;
                            }
                        }
                        if (skiedSBB)
                        {
                            string AltaUse = (SkiedAltaa ? "T" : "F");
                            if (CF.RowExists(BUY.Buy_Alta_ComConn, "asbshared.sb_ikon_use", $"readdate='{tRunDate}' AND mediaid='{MyMediaID}'"))
                            {
                                CF.ExecuteSQL(BUY.Buy_Alta_ComConn, $"UPDATE asbshared.sb_ikon_use SET altause = '{AltaUse}', axserial={mnseriennr}, axunicode={(mnunicodenr is null ? 0 : mnunicodenr)} WHERE readdate='{tRunDate}' AND mediaid='{MyMediaID}'");
                            }
                            else
                            {
                                CF.ExecuteSQL(BUY.Buy_Alta_ComConn, $"INSERT INTO asbshared.sb_ikon_use (readdate, mediaid, axproj, axpos, axserial, axunicode, altause) VALUES ('{tRunDate}', '{MyMediaID}', 705, {NKassaNr.ToString()}, {mnseriennr.ToString()}, {(mnunicodenr is null ? 0 : mnunicodenr).ToString()}, '{AltaUse}')");
                            }
                        }
                    }
                    string UpdateQuery = string.Empty;
                    if (mnseriennr > 3)
                    {
                        UpdateQuery = $"serialkey={lSerialKey}";
                    }
                    else
                    {
                        UpdateQuery = $"mediaid='{MyMediaID}'";
                    }
                    UpdateQuery = $"saledate = '{tRunDate}' AND {UpdateQuery}";
                    if (CF.RowExists(DW.dwConn, $"{DW.ActiveDatabase}.salesdata", UpdateQuery))
                    {
                        CF.ExecuteSQL(DW.dwConn, $"UPDATE {DW.ActiveDatabase}.salesdata SET TARIFF={IkonValue.ToString()}, tgroup='{TickGroup}' WHERE {UpdateQuery}");
                    }

                    if (DR_SalesData["tickdesc"].ToString() != string.Empty)
                    {
                        TickType = DR_SalesData.Field<int>("nticktype");
                        PersType = DR_SalesData.Field<int>("nperstype");
                        TickDesc = DR_SalesData["tickdesc"].ToString();
                        PersDesc = DR_SalesData["persdesc"].ToString();
                        NKassaNr = DR_SalesData.Field<int>("nkassanr");
                        mnseriennr = DR_SalesData.Field<int>("nseriennr");
                        mnunicodenr = DR_SalesData.Field<int>("nunicodenr");
                        SerialKey = DR_SalesData["serialkey"].ToString();
                    }
                }
                string RecID = string.Empty;
                if (!IsNewRec)
                {
                    DataRow DR_SkiVisits = CF.LoadDataRow(DW.dwConn, $"SELECT recid, serialkey, tgroup, nticktype, nperstype, mcpnbr, nkartennr, persdesc, rides FROM applications.skivisits WHERE readdate='{tRunDate}' AND {tWhere} LIMIT 1");
                    RecID = DR_SkiVisits["recid"].ToString();
                    IsUpdate |= (TickGroup != DR_SkiVisits.Field<string>("tgroup"));
                    IsUpdate |= (TickType != DR_SkiVisits.Field<int>("nticktype") || PersType != DR_SkiVisits.Field<int>("nperstype"));
                    IsUpdate |= (mmcpnbr.ToUpper().Trim() != DR_SkiVisits.Field<string>("mcpnbr").ToUpper().Trim());
                    IsUpdate |= (KartenNr != DR_SkiVisits.Field<string>("nkartennr"));
                    IsUpdate |= (PersDesc != DR_SkiVisits.Field<string>("persdesc"));
                    IsUpdate |= (Rides != DR_SkiVisits.Field<int>("rides"));
                    IsUpdate |= (TickGroup != DR_SkiVisits.Field<string>("tgroup").Trim());
                    IsUpdate |= (SerialKey != DR_SkiVisits["serialkey"].ToString());
                }
                if (IsNewRec || IsUpdate)
                {
                    //if (TickDesc.Trim() == string.Empty && NKassaNr == 27)
                    //{
                    //    TickDesc = TickDesc.Trim();
                    //}
                    if (!(TickGroup == "IK" && Rides == 0))
                    {
                        if (IsNewRec)
                        {
                            msqlcmd = $"INSERT {DW.ActiveDatabase}.skivisits (serialkey,readdate,projnr,nkassanr,nseriennr,nunicodenr,rides,tgroup,nticktype,tickdesc,nperstype,persdesc,nkartennr,mediaid,wildcat,collins,colangle,sunny,sugar,supreme,albion,passtoalta,passtosb,snowbird,altadt,sbdt,altaflag,amflag,begflag,lateflag,at3flag,skiat3,sbflag,asbflag,sbonly,issued_by,mcpnbr) values ('" +
                                    $"{SerialKey}','{Convert.ToDateTime(tRunDate).ToString(Mirror.AxessDateFormat)}',{mprojnr.ToString()},{NKassaNr.ToString()},{mnseriennr.ToString()}," +
                                    $"{(mnunicodenr == null ? "0" : mnunicodenr.ToString())},{Rides.ToString()},'{TickGroup}',{TickType.ToString()}," +
                                    $"'{TickDesc}',{PersType.ToString()},'{PersDesc}','{KartenNr}',{MediaID},{WildCat.ToString()},{Collins.ToString()},{CollinsAngle.ToString()},{Sunnyside.ToString()},{Sugarloaf.ToString()},{Supreme.ToString()}," +
                                    $"{Albion.ToString()},{PassToAlta.ToString()},{PassToSnowbird.ToString()},{Snowbird.ToString()},{AltaDeskTap.ToString()},{SnowbirdDeskTap.ToString()},'{AltaFlag}'," +
                                    $"'{AMFlag}','{BeginnerFlag}','{LateFlag}','{At3Flag}','{SkiAt3}','{SnowbirdFlag}','{ASBOnly}','{SnowbirdOnly}','{Issued_By}','{mmcpnbr}')";
                        }
                        else
                        {
                            msqlcmd = $"UPDATE applications.skivisits SET " +
                                $"serialkey='{SerialKey}'," +
                                $"nkassanr={NKassaNr}," +
                                $"NSERIENNR={mnseriennr}," +
                                $"nunicodenr={(mnunicodenr == null ? "0" : mnunicodenr.ToString())}," +
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

        public void IKON_Fix()
        {
            string MyQuery = "SELECT readdate, serialkey, nseriennr, nunicodenr, mediaid, nticktype nperstype FROM applications.skivisits WHERE nkassanr = 27 AND ALTAFLAG = 'Y' AND NTICKTYPE = 0";
            using (DataTable BrokenVisits = CF.LoadTable(DW.dwConn, MyQuery, "BrokenVisits"))
            {
                string TransKey = string.Empty;
                foreach (DataRow BV in BrokenVisits.Rows)
                {
                    string ReadDateFilter = BV.Field<DateTime>("ReadDate").ToString(Mirror.AxessDateFormat);
                    //is there a valid SalesData record with enough info?
                    MyQuery = $"SELECT transkey, serialkey, nseriennr, nunicodenr, nticktype, nperstype FROM applications.salesdata WHERE saledate = '{ReadDateFilter}' AND mediaid='{BV["mediaid"].ToString()}'";
                    DataRow OldRec = CF.LoadDataRow(DW.dwConn, MyQuery);
                    if (CF.RowHasData(OldRec))  //SalesData row matches
                    {
                        if (OldRec.Field<Int32>("nseriennr") < 3 || OldRec.Field<Int32>("nticktype") == 0 || OldRec.Field<Int32>("nperstype") == 0)
                        {   //SalesData record has incorrect data as well, so fix it if possible using tabKassaTransAufBereit
                            TransKey = $"{OldRec["transkey"].ToString().Trim()}";
                        }
                        else
                        {
                            BV.SetField<Int32>("nseriennr", OldRec.Field<Int32>("nseriennr"));
                            BV.SetField<Int32>("nunicodenr", OldRec.Field<Int32>("nunicodenr"));
                            BV.SetField<string>("serialkey", $"27-{OldRec.Field<Int32>("nseriennr")}-{OldRec.Field<Int32>("nunicodenr")}");
                        }
                    }
                    else    //no SalesData row found try tabKassaTransAufBereit
                    {
                        MyQuery = $"SELECT ntransnr FROM axess_cwc.tabkassatransaufbereit WHERE date(dtausgabedat) = '{ReadDateFilter}' AND mediaid='{BV["mediaid"].ToString()}'";
                        DataRow OldRec2 = CF.LoadDataRow(AM.MirrorConn, MyQuery);
                        if (CF.RowHasData(OldRec2))
                        {
                            TransKey = $"27-{OldRec2["ntransnr"].ToString()}";
                        }
                    }
                    //if salesdata needs updated, do it now.
                    if (TransKey != string.Empty)
                    {
                        ASy.SyncSale(TransKey);
                        ASy.SyncPurchase(TransKey);
                    }
                    //Now fix the SkiVisit using TabLeserTrans
                    DataRow LeserRec = CF.LoadDataRow(AM.MirrorConn, $"SELECT MAX(NLFDLESERTRANSNR) AS NLFDLESERTRANSNR FROM axess_cwc.tablesertrans WHERE DATE(dtverwdat) = '{ReadDateFilter}' AND nkassanr = 27 AND nseriennr = {BV["nseriennr"].ToString()} AND NUNICODENRINTERN = {BV["nunicodenr"].ToString()}");
                    if (CF.RowHasData(LeserRec))
                    {
                        SyncSkiVisit(LeserRec.Field<Int32>("NLFDLESERTRANSNR"));
                    }
                }
            }
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            if (DateTime.Now > StopTime)
                Application.Exit();
            IKON_Fix();
            SSLStatus.Text = "Fixing IKON Serial Keys.";
            CF.ExecuteSQL(DW.dwConn, "UPDATE applications.skivisits AS V INNER JOIN applications.salesdata AS D ON D.MEDIAID=V.mediaid AND D.SALEDATE=V.READDATE SET V.SERIALKEY = D.SERIALKEY, V.NSERIENNR=D.NSERIENNR, V.NUNICODENR=D.NUNICODENR WHERE V.SERIALKEY IN ('27-1-1', '27-2-1') AND V.ALTAFLAG = 'Y'");
            Int64 NLFDLeserTransNr = AM.GetLogNumber("SkiVisits");
            string MyQuery = $"SELECT NLFDLESERTRANSNR, NVERKPROJNR as projnr, NSERIENNR, NUNICODENRINTERN as nunicodenr, NKASSANR, SZZUSATZFELDER as mediaid, dtverwdat" +
                $" FROM {AM.ActiveDatabase}.tablesertrans" +
                $" WHERE NLFDLESERTRANSNR > {NLFDLeserTransNr.ToString()} AND nzutrnr < 92";
            using (DataTable LeserTrans = CF.LoadTable(AM.MirrorConn, MyQuery, "LeserTrans"))
            {
                PBSkiVisits.Value = 0;
                PBSkiVisits.Maximum = LeserTrans.Rows.Count;
                foreach (DataRow GateRead in LeserTrans.Rows)
                {
                    PBSkiVisits.Value = LeserTrans.Rows.IndexOf(GateRead) + 1;
                    SetSkiVisit(GateRead);
                    SSLStatus.Text = $"Ski Visits: {GateRead["dtverwdat"].ToString()} -- {PBSkiVisits.Value} of {PBSkiVisits.Maximum}";
                    AM.UpdateLog("SkiVisits", GateRead.Field<Int32>("NLFDLESERTRANSNR"));
                }
            }
            PBSkiVisits.Value = 0;
            SSLStatus.Text = $"Last run {DateTime.Now.ToString()}";
            timer1.Enabled = true;
        }

        private void RebuildDay(DateTime RebuildDate)
        {
            string MyDay = RebuildDate.ToString(Mirror.AxessDateFormat);
            string YDay = RebuildDate.AddDays(-1).ToString(Mirror.AxessDateFormat);
            string NDay = RebuildDate.AddDays(1).ToString(Mirror.AxessDateFormat);
            CF.ExecuteSQL(DW.dwConn, $"DELETE FROM {DW.ActiveDatabase}.skivisits WHERE readdate = '{MyDay}'");
            DataTable Rebuilds = CF.LoadTable(AM.MirrorConn, $"SELECT MAX(nlfdlesertransnr) AS nlfdlesertransnr FROM {AM.ActiveDatabase}.tablesertrans WHERE dtverwdat >='{MyDay}' AND dtverwdat < '{NDay}' AND nzutrnr < 92 GROUP BY nkassanr, nseriennr UNION SELECT nlfdlesertransnr FROM {AM.ActiveDatabase}.tablesertrans WHERE dtverwdat >='{MyDay}' AND dtverwdat < '{NDay}' AND nzutrnr < 92 AND nkassanr = 26 AND nseriennr IN (1,2) GROUP BY SZZUSATZFELDER", "Rebuilds");
            PBSkiVisits.Maximum = Rebuilds.Rows.Count;
            statusStrip1.Items[0].Text = $"Rebuilding {MyDay}";
            foreach (DataRow Rebuild in Rebuilds.Rows)
            {
                System.Diagnostics.Debug.Print($"{Rebuilds.Rows.IndexOf(Rebuild).ToString()} of {Rebuilds.Rows.Count.ToString()}");
                SSLStatus.Text = $"Rebuilding {MyDay} {Rebuilds.Rows.IndexOf(Rebuild).ToString()} of {Rebuilds.Rows.Count.ToString()}";
                statusStrip1.Refresh();
                PBSkiVisits.Value = Rebuilds.Rows.IndexOf(Rebuild) + 1;
                SyncSkiVisit(Rebuild.Field<Int32>("nlfdlesertransnr"));
            }
            SSLStatus.Text = $"Completed {DateTime.Now}";
            //CF.ExecuteSQL(DW.dwConn, $"UPDATE {DW.ActiveDatabase}.salesdata SET tgroup = 'UNK' WHERE saledate >='{MyDay}' AND saledate < '{NDay}' AND nkassanr=27 AND tgroup = 'IK' AND tariff = 0");
        }

        private void SkiVisits_FormClosed(object sender, FormClosedEventArgs e)
        {
            CF.SaveFormData("SkiVisits", Location.Y, Location.X, Height, Width, null, DateTime.Now, (int)e.CloseReason);
        }
    }
}
