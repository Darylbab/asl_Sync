using System;
using System.Data;
using System.Xml;
using MySql.Data.MySqlClient;

namespace asl_SyncLibrary
{
    public class DataWarehouse
    {
        private Mirror AM = new Mirror();
        private BUY_ALTA_COM BUY = new BUY_ALTA_COM();
        private CommonFunctions CF = new CommonFunctions();
        private ASLError ER = new ASLError();
        private WTPSI WTP = new WTPSI();

        public event Action<Tuple<string, int, int, int>> ProgressChanged;

        public MySqlConnection dwConn = new MySqlConnection(Properties.Settings.Default.MySQLDW);    //DataAccess.GetConnectionString("asl_Sync.Properties.Settings.MySQLDW"));
        
        private long myErrorNumber = 0;
        private string myErrorMsg = "";

        private bool tTestMode = false;

        public bool TestMode
        {
            get
            {
                return tTestMode;
            }
            set
            {
                tTestMode = value;
            }
        }

        private const string prodDatabase = "applications";
        private const string testDatabase = "applications_test";
        private string[] TArgs;

        public DataWarehouse(string[] args = null)
        {
            TArgs = args;
        }

        public string ActiveDatabase
        {
            get
            {
                return tTestMode ? testDatabase : prodDatabase;
            }
        }

        public long ErrorNo
        {
            get { return myErrorNumber; }
        }

        public string ErrorMessage
        {
            get { return myErrorMsg; }
        }

        private void OnProgressChanged(Tuple<string, int, int, int> progress) => ProgressChanged?.Invoke(progress);

        public bool WillcallFromMTC(DataRow aDataRow)
        {
            string tableName = "willcall";
            string qualifiedTablename = ActiveDatabase + "." + tableName;
            string wcQuery = string.Empty;
            long qty = 0;
            long uses = 0;
            decimal resPrice = 0;
            decimal axTotal = 0;
            bool isIssued = CF.RowExists(BUY.Buy_Alta_ComConn, "asbshared.mtncol_issued",$"mcpnbr='{aDataRow["barcode"].ToString()}'");
            string itemStatus = (aDataRow["order status"].ToString().ToUpper() == "PAID") ? (isIssued ? "I" : "U") : "R";
            decimal POS = 0;
            DateTime? issueDate = null;
            string ccLink = null;
            string company = null;
            DateTime bookDate = CF.nullDate;
            string webID = string.Empty;
            string pGroup = string.Empty;
            DateTime dob = Convert.ToDateTime(Properties.Settings.Default.DefaultBirthdate);
            long arCustID = 0;
            string arName = string.Empty;
            string purFName = string.Empty;
            string purLName = string.Empty;
            string purCity = string.Empty;
            string purStreet = string.Empty;
            string purZIP = string.Empty;
            string purState = string.Empty;
            string purEMail = string.Empty;
            string notes = null;
            string errFlag = null;
            string errReason = null;
            long? nKassaNr = null;
            string transKey = null;
            string serialKey = null;
            string journalKey = null;
            long nTickType = 0;
            long nPersType = 0;
            long nPoolNr = 0;
            decimal? cancelJnr = null;
            DateTime? cancelDate = null;
            long altaUses = 0;
            long sbUses = 0;
            long? itemNum = null;
            string orderNbr = string.Empty;
            DateTime? firstIssueDate = null;
            string lastIssuedBy = null;
            string lastSerial = null;
            string ccLinkCheck = null;
            long? nPersNr = null;
            string inAxess = "N";

            string resNo = aDataRow.Field<string>("barcode");
            DateTime arrDate = Convert.ToDateTime(aDataRow.Field<string>("Order Date"));
            string tickCode = "NEW";
            string tickDesc = aDataRow.Field<string>("product name").Substring(2,2);
            Int16 ta = Convert.ToInt16(tickDesc);
            tickDesc = ta.ToString() + "-" + (ta + 1).ToString() + " MOUNTAIN COLLECTIVE";
            string tPersDesc = "ADULT";
            if (aDataRow.Field<string>("Ticket Type").ToUpper() != tPersDesc)
                tPersDesc = "CHILD";
            string persdesc = "MTC " + tPersDesc;
            if (persdesc.Contains("ADULT"))
            {
                resPrice = 256;
                axTotal = 256;
            }
            else
            {
                resPrice = 168;
                axTotal = 168;
            }
            string firstName = CF.EscapeChar(aDataRow.Field<string>("guest first name")).ToUpper().Replace("(", "").Replace(")", "");
            string lastName = CF.EscapeChar(aDataRow.Field<string>("guest last name")).ToUpper().Replace("(", "").Replace(")", "");
            if (aDataRow.Field<string>("custom response") != null)
            {
                qty = aDataRow.Field<string>("custom response").ToLower().Contains("alta")  ? 3 : 2;
            }
            else
                qty = 2;

            uses = 0;
            POS = 0;
            issueDate = null;
            ccLink = null;
            company = "MC";
            bookDate = Convert.ToDateTime(aDataRow.Field<string>("order date"));
            webID = string.Empty;
            pGroup = string.Empty;
            if (aDataRow.Field<string>("guest birth date") == "0000-00-00")
            {
                dob = Convert.ToDateTime("12-31-1969");
            }
            else
            {
                dob = Convert.ToDateTime(aDataRow.Field<string>("guest birth date"));
            }
            arCustID = 11522;
            arName = "MOUNTAIN COLLECTIVE";
            purFName = CF.EscapeChar(aDataRow.Field<string>("purchaser first name"));
            purLName = CF.EscapeChar(aDataRow.Field<string>("purchaser last name"));
            purCity = CF.EscapeChar(aDataRow.Field<string>("billing city"));
            purStreet = CF.EscapeChar(aDataRow.Field<string>("billing address"));
            purZIP = CF.EscapeChar(aDataRow.Field<string>("billing zip"));
            purState = CF.EscapeChar(aDataRow.Field<string>("billing state"));
            purEMail = CF.EscapeChar(aDataRow.Field<string>("purchaser email"));
            if (purFName != null) purFName = purFName.ToUpper();
            if (purLName != null) purLName = purLName.ToUpper();
            if (purCity != null) purCity = purCity.ToUpper();
            if (purStreet != null) purStreet = purStreet.ToUpper();
            if (purState != null) purState = purState.ToUpper();
            if (purEMail != null) purEMail = purEMail.ToUpper();
            notes = null;
            errFlag = null;
            errReason = null;
            nKassaNr = null;
            transKey = null;
            serialKey = null;
            journalKey = null;
            nTickType = 0;
            nPersType = 0;
            nPoolNr = 0;
            altaUses = 0;
            sbUses = 0;
            itemNum = null;
            orderNbr = CF.EscapeChar(aDataRow.Field<string>("ticket id"));
            firstIssueDate = null;
            lastIssuedBy = null;
            lastSerial = null;
            ccLinkCheck = null;
            DataSet tDS = CF.LoadDataSet(AM.MirrorConn, "SELECT * FROM axess_cwc.tabpersonen WHERE SZNAME = '" + lastName + "' AND dtgeburt = '" + dob.ToString(Mirror.AxessDateFormat) + "'", new DataSet(), "tabPersonen");
            if (tDS.Tables["tabPersonen"].Rows.Count == 0)
            {   //no match in Mirror
                //bool rtnVal = wtp.lookForBOCPerson(lastName, firstName, dob);
                //if (wtp.ErrorNo == -6105)        //no matching customer found
                //{
                    inAxess = "N";
                    nPersNr = null;
                    //nKassaNr = 20;
                    //skip this code untilwe get time to catch up.
                    //if (wtp.createUserInBO(lastName, firstName, dob))
                    //{
                    //    wtp.createWTPSICustomer(wtp.CustomerID, 20, wtp.PersNr);
                    //    rtnVal = true;
                    //}
                //}
                //if (rtnVal)
                //{
                //    inAxess = "Y";
                //    nPersNr = wtp.PersNr;
                //    nKassaNr = wtp.PersKassaNr;
                //}
            }
            else
            {
                inAxess = "y";
                nPersNr = tDS.Tables["tabPersonen"].Rows[0].Field<int>("nPersNr");
                nKassaNr = tDS.Tables["tabPersonen"].Rows[0].Field<Int16>("nPersKassaNr");

            }
            if (!CF.RowExists(dwConn, qualifiedTablename, "res_no LIKE '%" + resNo + "'"))
            {   //insert new willcall record
                wcQuery = "INSERT INTO " + qualifiedTablename + "(RES_NO, ARR_DATE, TICKCODE, TICKDESC, PERSDESC, FIRSTNAME, LASTNAME, QTY, USES, RESPRICE, AXTOTAL, ITEMSTATUS, POS, ISSUE_DATE, CCLINK, COMPANY, BOOKDATE, WEBID, PGROUP, DOB, ARCUSTID, ARNAME, PURFNAME, PURLNAME, PURCITY, PURSTREET, PURZIP, PURSTATE, PUREMAIL, NOTES, ERRFLAG, ERRREASON, nkassanr, transkey, serialkey, journalkey, NTICKTYPE, NPERSTYPE, NPOOLnr, CANCELJNR, CANCELDATE, ALTAUSES, SBUSES, ITEMNUM, ORDERNBR, FIRST_ISSUE_DATE, ISSUEDBY, FIRSTSERIAL, LAST_ISSUE_DATE, LASTISSUEDBY, LASTSERIAL, CCLINKCHECK, INAXESS, NPERSNR) VALUES (";
                wcQuery += CF.Quote(resNo.ToUpper()) + ",";
                wcQuery += CF.Quote(arrDate.ToString(Mirror.AxessDateFormat)) + ",";
                wcQuery += CF.Quote(tickCode.ToUpper()) + ",";
                wcQuery += CF.Quote(tickDesc.ToUpper()) + ",";
                wcQuery += CF.Quote(persdesc.ToUpper()) + ",";
                wcQuery += CF.Quote(firstName.ToUpper()) + ",";
                wcQuery += CF.Quote(lastName.ToUpper()) + ",";
                wcQuery += CF.Quote(qty.ToString()) + ",";
                wcQuery += CF.Quote(uses.ToString()) + ",";
                wcQuery += CF.Quote(resPrice.ToString()) + ",";
                wcQuery += CF.Quote(axTotal.ToString()) + ",";
                wcQuery += CF.Quote(itemStatus.ToUpper()) + ",";
                wcQuery += CF.Quote(POS.ToString()) + ",";
                wcQuery += (issueDate == null ? "null" : CF.Quote(issueDate.Value.ToString(Mirror.AxessDateFormat))) + ",";
                wcQuery += (ccLink == null ? @"''" : CF.Quote(ccLink)) + ",";
                wcQuery += (company == null ? @"''" : CF.Quote(company.ToUpper())) + ","; 
                wcQuery += CF.Quote(bookDate.ToString(Mirror.AxessDateFormat)) + ",";
                wcQuery += CF.Quote(webID.ToUpper()) + ",";
                wcQuery += CF.Quote(pGroup.ToUpper()) + ",";
                wcQuery += (dob == null ? @"12/31/1969" : CF.Quote(dob.ToString(Mirror.AxessDateFormat))) + ",";
                wcQuery += CF.Quote(arCustID.ToString()) + ",";
                wcQuery += CF.Quote(arName.ToUpper()) + ",";
                wcQuery += (purFName == null ? @"''" : CF.Quote(purFName.ToUpper())) + ",";
                wcQuery += (purLName == null ? @"''" : CF.Quote(purLName.ToUpper())) + ",";
                wcQuery += (purCity == null ? @"''" : CF.Quote(purCity.ToUpper().Substring(0, (purCity.Length < 26 ? purCity.Length : 25)).Trim())) + ",";
                wcQuery += (purStreet == null ? @"''" : CF.Quote(purStreet.ToUpper().Substring(0, (purStreet.Length < 36 ? purStreet.Length : 35)).Trim())) + ",";
                wcQuery += (purZIP == null ? @"''" : CF.Quote(purZIP.ToUpper())) + ",";
                wcQuery += (purState == null ? @"''" : CF.Quote(purState.ToUpper())) + ",";
                wcQuery += (purEMail == null ? @"''" : CF.Quote(purEMail.ToUpper().Substring(0,(purEMail.Length < 31 ? purEMail.Length : 30)).Trim())) + ",";
                wcQuery += (notes == null ? @"''" : CF.Quote(notes)) + ",";
                wcQuery += (errFlag == null ? @"''" : CF.Quote(errFlag)) + ",";
                wcQuery += (errReason == null ? @"''" : CF.Quote(errReason)) + ",";
                wcQuery += (nKassaNr == null ? "'20'" : CF.Quote(nKassaNr.ToString())) + ",";
                wcQuery += (transKey == null ? @"''" : CF.Quote(transKey)) + ",";
                wcQuery += (serialKey == null ? @"''" : CF.Quote(serialKey)) + ",";
                wcQuery += (journalKey == null ? @"''" : CF.Quote(journalKey)) + ",";
                wcQuery += CF.Quote(nTickType.ToString()) + ",";
                wcQuery += CF.Quote(nPersType.ToString()) + ",";
                wcQuery += CF.Quote(nPoolNr.ToString()) + ",";
                wcQuery += (cancelJnr == null ? @"'0.00'" : CF.Quote(cancelJnr.ToString())) + ",";
                wcQuery += (cancelDate == null ? "null" : CF.Quote(cancelDate.Value.ToString(Mirror.AxessDateFormat))) + ",";
                wcQuery += CF.Quote(altaUses.ToString()) + ",";
                wcQuery += CF.Quote(sbUses.ToString()) + ",";
                wcQuery += (itemNum == null ? @"'0'" : CF.Quote(itemNum.ToString())) + ",";
                wcQuery += CF.Quote(orderNbr) + ",";
                wcQuery += (firstIssueDate == null ? "null" : CF.Quote(firstIssueDate.Value.ToString(Mirror.AxessDateFormat))) + ",";
                wcQuery += "'',";
                wcQuery += "'',";
                wcQuery += "null,";
                wcQuery += (lastIssuedBy == null ? @"''" : CF.Quote(lastIssuedBy)) + ",";
                wcQuery += (lastSerial == null ? @"''" : CF.Quote(lastSerial)) + ",";
                wcQuery += (ccLinkCheck == null ? @"''" : CF.Quote(ccLinkCheck)) + ",";
                wcQuery += CF.Quote(inAxess.ToUpper()) + ",";
                wcQuery += (nPersNr == null ? "null" : CF.Quote(nPersNr.ToString()));
                wcQuery += ")";
                CF.ExecuteSQL(dwConn, wcQuery);  //new record or update from datafeed (liftopia/mountain collective)

            }
            else
            {   //update willcall record
                wcQuery = $"UPDATE {qualifiedTablename} SET itemstatus='{itemStatus}', firstname='{firstName}', lastname='{lastName}', qty={qty}, POS={POS},DOB='{dob.ToString(Mirror.AxessDateFormat)}',purfname='{purFName}',purlname='{purLName}',purcity='{purCity}',purstate='{purState}',purzip='{purZIP}',purstreet='{purStreet}',puremail='{purEMail}',PERSDESC ='{persdesc}', RESPRICE={resPrice.ToString()}, AXTOTAL={axTotal.ToString()} WHERE recid={CF.GetSQLField(dwConn, $"SELECT MIN(recid) FROM {ActiveDatabase}.willcall WHERE res_no LIKE '%{resNo}'")}";
                CF.ExecuteSQL(dwConn,wcQuery); 
            }
            return true;
        }

        public bool UpdateIssuedFromCList()
        {
            //if tickdesc(last 2 chars).tolower() = "cc" then issue all to credit card, else single passes and single serial numbers
            string tickdesc = "test cc";
            if (tickdesc.Substring(tickdesc.Length-2).ToLower() == "cc")
            {
                return true;
            }
            return false;
        }

        //public bool UpdateWillcallFromIssued()
        //{

        //    return false;
        //}
        public void FixWillcall()
        {
            string tQuery = $"SELECT * FROM {ActiveDatabase}.willcall WHERE persdesc LIKE 'MTC%'";
            DataSet tDS = CF.LoadDataSet(dwConn, tQuery, new DataSet(), "willcall");
            foreach (DataRow tRow in tDS.Tables["willcall"].Rows)
            {
                int tAge = CF.CalcAge(tRow.Field<DateTime>("DOB"));
                string tVal = "MTC " + ((tAge <= 12) && (tAge != 0) ? "CHILD" : "ADULT");
                if (tRow["persdesc"].ToString() != tVal)
                {
                    CF.ExecuteSQL(dwConn, "UPDATE " + ActiveDatabase + ".willcall SET persdesc='" + tVal + "' WHERE recid=" + tRow["recid"].ToString());
                    System.Diagnostics.Debug.Print(tDS.Tables["willcall"].Rows.IndexOf(tRow).ToString() + " of " + tDS.Tables["willcall"].Rows.Count.ToString());
                }
            }
        }
        public bool SetControl(string aUpdate, string aUserID = "M")
        {
            return CF.ExecuteSQL(dwConn, "UPDATE applications.asl_control SET " + aUpdate + " WHERE UID='" + aUserID + "'");
        }

        public void UpdateSalesDataUses()
        {
            CF.ExecuteSQL(dwConn, $"UPDATE {ActiveDatabase}.salesdata AS A SET A.USES = (SELECT COUNT(*) FROM {ActiveDatabase}.skivisits AS B WHERE B.SERIALKEY=A.SERIALKEY) WHERE SALEDATE>='{Properties.Settings.Default.SeasonStart.ToString(Mirror.AxessDateFormat)}'"); 
        }

        public void UpdateSPCardStatus()
        {
            string TNow = DateTime.Today.ToString(Mirror.AxessDateFormat);
            string t = $"UPDATE {ActiveDatabase}.spcards AS A INNER JOIN {ActiveDatabase}.salesdata AS B ON A.serialkey = B.SERIALKEY SET A.cardstatus=";
            CF.ExecuteSQL(dwConn, $"{t}'X', utasent=0 WHERE A.cardstatus IN ('A','B') AND B.VALIDTILL < '{TNow}' AND B.NTICKTYPE <> 505");
            CF.ExecuteSQL(dwConn, $"{t}'P', utasent=0 WHERE A.cardstatus = 'A' AND B.VALIDFROM >= '{TNow}'");
            CF.ExecuteSQL(dwConn, $"{t}'A', utasent=0 WHERE A.cardstatus = 'P' AND B.VALIDFROM <= '{TNow}' AND B.VALIDTILL > '{TNow}'");

            //unblocked cards
            //first unblock cards that are not replacements
            string Query = $"SELECT serialkey FROM {ActiveDatabase}.spcards WHERE cardstatus = 'B' AND SERIALKEY NOT IN (SELECT C.serialkey FROM applications.spcards AS C INNER JOIN applications.salesdata AS D ON CONCAT_WS('-', D.RKASSANR, D.RSERIENNR, D.RUNICODENR) = C.serialkey WHERE C.cardstatus = 'B' AND C.expdate > '{DateTime.Today.ToString(Mirror.AxessDateFormat)}') AND expdate > '{DateTime.Today.ToString(Mirror.AxessDateFormat)}'";
            using (DataTable Blocked = CF.LoadTable(dwConn, Query, "Blocked"))
            {
                if (CF.TableHasData(Blocked))
                {
                    foreach (DataRow BRow in Blocked.Rows)
                    {
                        if (CF.RowExists(AM.MirrorConn, $"{AM.ActiveDatabase}.tabkartensperre", $"concat_ws('-',nkassanr,nseriennr, nunicodenr) ='{BRow["serialkey"].ToString()}' AND (baktiv=0 OR dtgiltbis <= '{TNow}')"))
                        {
                            CF.ExecuteSQL(dwConn, $"UPDATE {ActiveDatabase}.spcards SET cardstatus='A', utasent=0 WHERE serialkey = '{BRow["serialkey"].ToString()}'");
                        }
                    }
                }
            }
            //next new blocks
            Query = $"SELECT concat_ws('-',nkassanr,nseriennr, nunicodenr) AS serialkey FROM {AM.ActiveDatabase}.tabkartensperre WHERE nkassanr <=55 AND (baktiv=-1 AND dtgiltbis > '{TNow}')";
            using (DataTable NewBlocks = CF.LoadTable(AM.MirrorConn, Query, "NewBlocks"))
            {
                if (CF.TableHasData(NewBlocks))
                {
                    foreach (DataRow NRow in NewBlocks.Rows)
                    {
                        if (CF.RowExists(dwConn, $"{ActiveDatabase}.spcards", $"serialkey='{NRow["SerialKey"].ToString()}' AND cardstatus <> 'B'"))
                        {
                            CF.ExecuteSQL(dwConn, $"UPDATE {ActiveDatabase}.spcards SET cardstatus='B', utasent=0 WHERE serialkey='{NRow["SerialKey"].ToString()}'");
                        }
                    }
                }
            }
            //replaced cards
            Query = $"SELECT DISTINCT concat_ws('-', RKASSANR, RSERIENNR, RUNICODENR) AS RSerialKey FROM {ActiveDatabase}.salesdata WHERE ntranstype = 8 AND Rseriennr > 3 AND VALIDTILL BETWEEN '2018-11-23' AND '2019-11-22'";
            using (DataTable Replaced = CF.LoadTable(dwConn, Query, "Replaced"))
            {
                if (CF.TableHasData(Replaced))
                {
                    foreach(DataRow ReplaceRow in Replaced.Rows)
                    {
                        System.Diagnostics.Debug.Print($"{Replaced.Rows.IndexOf(ReplaceRow)} of {Replaced.Rows.Count}");
                        if (CF.RowExists(dwConn, $"{ActiveDatabase}.spcards", $"serialkey='{ReplaceRow["RSerialKey"].ToString()}' AND cardstatus <> 'B'"))
                        {
                            CF.ExecuteSQL(dwConn, $"UPDATE {ActiveDatabase}.spcards SET cardstatus = 'B', utasent = 0 WHERE serialkey = '{ReplaceRow["RSerialKey"].ToString()}'");
                        }
                    }
                }
            }
        }

        public bool UpdateSPCardsUses() //updated uses field in SPCards based on Skivisits.  
        {
            string seasonStart = Properties.Settings.Default.SeasonStart.ToString(Mirror.AxessDateFormat);
            string tSQL = $"UPDATE {ActiveDatabase}.spcards AS C SET uses = (SELECT  COUNT(*) FROM {ActiveDatabase}.skivisits AS V WHERE V.SERIALKEY = C.serialkey AND readdate > '{seasonStart}') WHERE dtupdate > '{seasonStart}'";
            return CF.ExecuteSQL(dwConn, tSQL, 240);
        }


        public bool SetAxessLog(XmlNode tNode)
        {
            long curLogNr = Convert.ToInt64(tNode.SelectSingleNode("NLOGNR").InnerText);
            System.Diagnostics.Debug.Print(curLogNr.ToString());
            string myFunction = tNode.SelectSingleNode("SZKIND").InnerText.ToUpper();   //Log function, I=Insert, U=Update, D=Delete
            string curTable = tNode.SelectSingleNode("SZTABNAME").InnerText.ToLower();
            string dtUpdateStamp = CF.TimestampConvert(Convert.ToDouble(tNode.SelectSingleNode("DTTIMESTAMP").InnerText));
            if (!CF.RowExists(CF.DWConn, "axess_log.dci4crmlogline", "nlognr=" + curLogNr))
            {
                if (!CF.ExecuteSQL(CF.DWConn, "INSERT INTO axess_log.dci4crmlogline (nlognr, sztabname, szkind, dttimestamp) VALUES (" + curLogNr.ToString() + ",'" + curTable + "','" + myFunction + "','" + dtUpdateStamp + "')"))
                    return false;
                XmlNode tXMLNode = tNode.SelectSingleNode(myFunction=="D" ? "ACTLOGVALUESOLD" : "ACTLOGVALUESNEW");   //new row values only
                foreach (XmlNode lNode in tXMLNode) //loop through all values in the row and add them to the correct temp strings
                {
                    string fieldName = lNode.SelectSingleNode("SZFIELDNAME").InnerText.ToLower();  //get field name
                    string fieldValue = (lNode.SelectSingleNode("SZFIELDVALUE").InnerText.Length != 0) ? "'" + CF.EscapeChar(lNode.SelectSingleNode("SZFIELDVALUE").InnerText) + "'" : "NULL";  //get field value and format for mySQL
                    if (fieldName.StartsWith("dt") && fieldValue != "NULL") fieldValue = CF.TimestampConvert(Convert.ToDouble(fieldValue.Replace("'", "")));
                    if (!CF.ExecuteSQL(CF.DWConn, "INSERT INTO axess_log.dci4crmlogfield (nlognr, szfieldname, szfieldvalue) VALUES (" + curLogNr.ToString() + ",'" + fieldName.Replace("'", "") + "','" + fieldValue.Replace("'", "") + "')"))
                        return false;
                }
            }
            return true;
        }
        public Serverstatus GetServerStatus()
        {
            Serverstatus tStat = Serverstatus.Unknown;
            if (CF.OpenConn(dwConn) == ConnectionState.Open)
            {
                CF.CloseConn(dwConn);
                tStat = Serverstatus.Active;
            }
            else
            {
                try
                {
                    dwConn.Ping();
                    tStat = Serverstatus.Service_Failed;
                }
                catch { tStat = Serverstatus.Server_Failed; }
            }
            return tStat;
        }
    }
}
