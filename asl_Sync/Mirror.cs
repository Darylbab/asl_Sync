using System;
using System.Data;
using System.Xml;
using MySql.Data.MySqlClient;

namespace asl_SyncLibrary
{
    public class Mirror
    {
        private CommonFunctions cf = new CommonFunctions();
        private ASLError myError = new ASLError();
        private Axess_DCI4CRM AxessSource = new Axess_DCI4CRM();
        private BUY_ALTA_COM Buy = new BUY_ALTA_COM();

        private const string AltaPOSFilter = "(POSFilter<61)";
        private const string OrderEntryPOSFilter = "((POSFilter>=117) AND-(POSFilter<=121))";
        private const string SkiSaltLakePOSFilter = "((POSFilter>=100) AND (POSFilter<=105))";
        public const string AllAltaPOSFilter = "(" + AltaPOSFilter + " OR " + OrderEntryPOSFilter + "OR" + SkiSaltLakePOSFilter + ")";

        public const string AxessDateTimeFormat = "yyyy-MM-dd HH:mm:ss";
        public const string AxessDateFormat = "yyyy-MM-dd";

        public MySqlConnection MirrorConn = new MySqlConnection(Properties.Settings.Default.Mirror);

        private DataSet DS = new DataSet();

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

        private const string prodDatabase = "axess_cwc";
        private const string testDatabase = "axess_cwc";

        public string ActiveDatabase
        {
            get
            {
                return tTestMode ? testDatabase : prodDatabase;
            }
        }

        public ConnectionState State
        {
            get { return MirrorConn.State; }
        }

        private int tAxessTimeDifference = 8;    //usually the default

        //public Mirror(StatusStrip aStatusStrip = null, ToolStripProgressBar aProgressBar = null)
        //{
        //    if (aStatusStrip != null)
        //    {
        //        tStatus = aStatusStrip;
        //    }
        //    if (aProgressBar != null)
        //    {
        //        tProgress = aProgressBar;
        //    }
        //}

        public int AxessTimeDifference
        {
            get
            {
                tAxessTimeDifference = 8;
                var MountainTime = TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time");
                var CentralEuropeanTime = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");

                var now = DateTimeOffset.UtcNow;
                TimeSpan MSTOffset = MountainTime.GetUtcOffset(now);
                TimeSpan CESOffset = CentralEuropeanTime.GetUtcOffset(now);
                tAxessTimeDifference = Convert.ToInt32(MSTOffset - CESOffset);
                return tAxessTimeDifference;
            }
        }

        public long ErrorNumber
        {
            get { return myErrorNumber; }
        }

        public string ErrorMessage
        {
            get { return myErrorMsg; }
        }

        public Serverstatus GetServerStatus()
        {
            Serverstatus tStat = Serverstatus.Unknown;
            if (cf.OpenConn(MirrorConn) == ConnectionState.Open)
            {
                cf.CloseConn(MirrorConn);
                tStat = Serverstatus.Active;
            }
            else
            {
                try
                {
                    MirrorConn.Ping();
                    tStat = Serverstatus.Service_Failed;
                }
                catch { tStat = Serverstatus.Server_Failed; }
            }
            return tStat;
        }


        public bool MergePromptData(string POSNo, string TransNo, string ZaehlerNo, string PromptNo, string ElementNo)
        {
            //promptdata row for each storepromptdetail row that has a matching transkey in tabaxpmtdata or tabkassatransaufbereit.
            //if TransNo is empty it comes from tabstorepromptdetail
            //if ElementNo is empty it comes from tabstorepromptmaster
            string myQuery = $"INSERT INTO {ActiveDatabase}.tabpromptdata (SELECT 0 AS RECID, M.NPROJNR, M.NKASSANR, M.NTRANSNR, M.NJOURNALNR, M.NPROMPTZAEHLERNR, M.NPROMPTNR, D.NPROMPTELEMENTNR, D.NLFDELEMENTNR, M.NPROMPTTYPNR, D.SZTEXT, 0.00 AS SZNBR, (SELECT DISTINCT DTAUSGABEDAT FROM {ActiveDatabase}.tabkassatransaufbereit AS T WHERE T.NACTKASSANR=M.NKASSANR AND T.NTRANSNR=M.NTRANSNR) AS SALEDATE,'' AS PROMPTKEY,'' AS JOURNALKEY,'' AS TRANSKEY, D.DTUPDATE FROM {ActiveDatabase}.tabstorepromptmaster AS M INNER JOIN {ActiveDatabase}.tabstorepromptdetail AS D ON D.NKASSANR = M.NKASSANR AND D.NPROMPTZAEHLERNR=M.NPROMPTZAEHLERNR AND D.NPROMPTNR=M.NPROMPTNR";
            string delQuery = $"DELETE FROM {ActiveDatabase}.tabpromptdata";
            string myWhere = $" WHERE M.npromptzaehlernr={ZaehlerNo} AND M.nkassanr={POSNo} AND M.npromptnr={PromptNo}";
            if (TransNo != string.Empty)
            {
                myWhere += " AND M.ntransnr=" + TransNo;
            }
            myQuery += myWhere + ")";
            delQuery += myWhere.Replace("M.", "").Replace("D.", "");
            if (!cf.ExecuteSQL(MirrorConn, delQuery)) return false;
            return cf.ExecuteSQL(MirrorConn, myQuery);
        }

        public void MergeCustAddTables(string PersKey = "", bool addresskey = false)
        {
            if (PersKey.Length == 0) return;
            //add where clause to query if one is given, otherwise everything.
            PersKey = "'" + PersKey.Replace(",", "','") + "'";
            if (addresskey)
            {
                DataSet addDS = cf.LoadDataSet(MirrorConn, "(SELECT concat_ws('-',NPERSKASSANR,NPERSNR) AS PERSKEY FROM " + ActiveDatabase + ".tabpersonen WHERE concat_ws('-',NADRKASSANR,NADRNR) IN (" + PersKey + "))", new DataSet(), "AddressKey");
                PersKey = string.Empty;
                foreach (DataRow myRow in addDS.Tables["AddressKey"].Rows)
                {
                    PersKey += ",'" + myRow.Field<string>("PersKey") + "'";
                }
                if (PersKey.Length > 1) PersKey = PersKey.Substring(1);
            }
            if (PersKey.Length == 0) return;
            cf.ExecuteSQL(MirrorConn, "DELETE FROM " + ActiveDatabase + ".tabaxcust  WHERE concat_ws('-',NPERSKASSA,NPERSNR) IN (" + PersKey + ")");
            string myQuery = "SELECT DISTINCT concat_ws('-',NPERSKASSANR,NPERSNR) AS PERSKEY, NADRPROJNR, NPERSKASSANR, NPERSNR, SZNAME AS lastname, SZVORNAME AS firstname, SZGESCHLECHT AS Gender, DTGEBURT AS dob, '' AS ctitle, SZANREDE AS cheight, SZMOBILNR AS mobile, SZEMAIL AS email, NFAMKASSANR, NFAMPERSNR, NFAMSORTNR AS fampersort, BPERSAKTIV, NADRPROJNR, NADRKASSANR, NADRNR FROM " + ActiveDatabase + ".tabpersonen WHERE concat_ws('-', NPERSKASSANR, NPERSNR) IN(" + PersKey + ")";
            DataSet myData = cf.LoadDataSet(MirrorConn, myQuery, new DataSet(), "AXCust");
            foreach (DataRow tRow in myData.Tables["AXCust"].Rows)
            {
                //insert record into tabAXCust
                myData = cf.LoadDataSet(MirrorConn, "SELECT DTUPDATE, SZSTRASSE, SZPLZ, SZORT, SZSTAATKURZNAME, SZTELNR FROM " + ActiveDatabase + ".tabadressen WHERE NADRPROJNR = " + tRow.Field<short>("NADRPROJNR") + " AND NADRKASSANR = " + tRow.Field<short>("NADRKASSANR") + " AND NADRNR = " + tRow.Field<int>("NADRNR") + " ORDER BY DTUPDATE DESC", myData, "ADDR");
                if (myData.Tables["ADDR"].Rows.Count != 0)
                {
                    DataRow t2Row = myData.Tables["ADDR"].Rows[0];
                    myQuery = "INSERT INTO " + ActiveDatabase + ".tabaxcust (NPERSKASSA, NPERSNR, lastname, firstname, gender, dob, ctitle, cheight, mobile, email, FAMKASSA, FAMPERSNR, FAMPERSORT, NADRKASSA, NADRNR, DTUPDATE, activeflag, STREET, ZIP, CITY, STATE, phone, country, addtupdate) VALUES (";
                    myQuery += "'" + tRow.Field<short>("NPERSKASSANR") + "',";
                    myQuery += "'" + tRow.Field<int>("NPERSNR") + "',";
                    myQuery += "'" + cf.EscapeChar(tRow.Field<string>("lastname")) + "',";
                    myQuery += "'" + cf.EscapeChar(tRow.Field<string>("firstname")) + "',";
                    myQuery += "'" + cf.EscapeChar(tRow.Field<string>("gender")) + "',";
                    myQuery += "'" + (tRow.Field<DateTime?>("dob") == null ? "0000-00-00" : tRow.Field<DateTime>("dob").ToString(Mirror.AxessDateFormat)) + "',";
                    myQuery += "'" + cf.EscapeChar(tRow.Field<string>("ctitle")) + "',";
                    myQuery += "'" + cf.EscapeChar(tRow.Field<string>("cheight")) + "',";
                    myQuery += "'" + cf.EscapeChar(tRow.Field<string>("mobile")) + "',";
                    myQuery += "'" + cf.EscapeChar(tRow.Field<string>("email")) + "',";
                    myQuery += "'" + tRow.Field<short>("NFAMKASSANR") + "',";
                    myQuery += "'" + tRow.Field<int>("NFAMPERSNR") + "',";
                    myQuery += "'" + tRow.Field<int>("FAMPERSORT") + "',";
                    myQuery += "'" + tRow.Field<short>("NADRKASSANR") + "',";
                    myQuery += "'" + tRow.Field<int>("NADRNR") + "',";
                    myQuery += "'" + DateTime.Now.ToString(Mirror.AxessDateTimeFormat) + "',";  //(t2Row.Field<DateTime?>("DTUPDATE") == null ? "0/0/0000 00:00:00" : t2Row.Field<DateTime>("DTUPDATE").ToString()) + "',";
                    myQuery += "'" + tRow.Field<short>("BPERSAKTIV") + "',";
                    myQuery += "'" + cf.EscapeChar(t2Row.Field<string>("SZSTRASSE")) + "',";
                    myQuery += "'" + cf.EscapeChar(t2Row.Field<string>("SZPLZ")) + "',";
                    myQuery += "'" + cf.EscapeChar(t2Row.Field<string>("SZORT")) + "',";
                    myQuery += "'" + cf.EscapeChar(t2Row.Field<string>("SZSTAATKURZNAME")) + "',";
                    myQuery += "'" + cf.EscapeChar(t2Row.Field<string>("SZTELNR")) + "',";
                    myQuery += "'USA',";
                    myQuery += "'" + (t2Row.Field<DateTime?>("dtupdate") == null ? "0/0/0000  00:00:00" : DateTime.Now.ToString(Mirror.AxessDateTimeFormat)) + "')"; // t2Row.Field<DateTime>("dtupdate").ToString(Mirror.AxessDateTimeFormat)) + "')";
                    cf.ExecuteSQL(MirrorConn, myQuery.Replace("1/1/0001", "0/0/0000").Replace("12:00:00 AM", "00:00:00"));
                }
            }
        }

        public bool DeleteTable(string tableName)
        {
            return cf.ExecuteSQL(MirrorConn, "DELETE FROM " + ActiveDatabase + "." + tableName);
        }

        public long GetLogNumber(string tableName)
        // returns LogNumber if log row exists for this table
        {
            if (!cf.RowExists(MirrorConn, ActiveDatabase + ".tabLog", "TableName = '" + tableName + "'"))
            {
                return 0;    //no log entry so no log number
            }
            return Convert.ToInt64(cf.GetSQLField(MirrorConn, "SELECT LogNo FROM " + ActiveDatabase + ".tabLog WHERE TableName='" + tableName + "'"));
        }


        public DateTime GetLogTimestamp(string aTableName)
        {
            if (!cf.RowExists(MirrorConn, ActiveDatabase + ".tabLog", "TableName = '" + aTableName + "'"))
            {
                return Properties.Settings.Default.SeasonStart;    //no log entry so no log number
            }
            return Convert.ToDateTime(cf.GetSQLField(MirrorConn, "SELECT lastUpdate FROM " + ActiveDatabase + ".tabLog WHERE TableName='" + aTableName + "'"));
        }


        public bool UpdateTabAXOrderData(string OrderListNrs = "")  //OrderListNrs is a comma delimited list of norderlistnr values, if blank the entire table is deleted and re-created.
        {
            string tWhere = " WHERE (OL.nkassanr <= 121 OR OL.nkassanr IS NULL) AND (OL.nperskassanr <= 121 OR OL.nperskassanr IS NULL) AND (OL.nfirmenkassanr <= 121 OR OL.nfirmenkassanr IS NULL)";
            if (OrderListNrs != string.Empty) tWhere += " AND (OL.norderlistnr IN (" + OrderListNrs + "))";
            string tSQL = "SELECT OL.nprojnr, OL.norderlistnr, POS.norderlistpos, ED.NAUFTRAGNR, POS.dtgiltab, OL.norderstatusnr, POS.nkundenkartentypnr, POS.nperstypnr, POS.npoolnr, POS.nartikelnr, POS.feinzeltarif, POS.nkartennr, OL.nkassaprojnr, OL.nkassanr, POS.ntransnr, OL.nfirmenkassanr, OL.nfirmennr, OL.nperskassanr, OL.npersnr, ED.SZDELIVERYNAME, POS.neingegsortstk, POS.nrohlingstypnr, POS.ndattraegtypnr, POS.bwtp, NULL, P.SZVORNAME, P.SZNAME, TAB.DTAUSGABEDAT FROM " + ActiveDatabase + ".taborderlist AS OL LEFT JOIN " + ActiveDatabase + ".taborderlistpos AS POS ON OL.norderlistnr = POS.norderlistnr LEFT JOIN " + ActiveDatabase + ".tabedeauftrag AS ED ON ED.NORDERLISTNR = OL.norderlistnr LEFT JOIN " + ActiveDatabase + ".tabpersonen AS P ON P.NPERSKASSANR = OL.nperskassanr AND P.NPERSNR = OL.npersnr LEFT JOIN " + ActiveDatabase + ".tabkassatransaufbereit AS TAB ON TAB.nactkassanr=OL.nkassanr AND TAB.ntransnr=OL.ntransnr" + tWhere + " AND (POS.neingegsortstk<>0 ) AND ((ED.NAUFTRAGNR <> 0) OR (ED.NAUFTRAGNR IS NULL))";
            if (!cf.ExecuteSQL(MirrorConn, "DELETE FROM " + ActiveDatabase + ".tabaxorderdata" + tWhere.Replace("OL.", "") + " AND (testflag = 0)")) return false;
            DataSet myDataSet = new DataSet();
            myDataSet = cf.LoadDataSet(MirrorConn, tSQL, new DataSet(), "tabAXOrderData");
            foreach (DataRow tRow in myDataSet.Tables["tabAXOrderData"].Rows)
            {
                string sTempDate = tRow.Field<DateTime?>("dtgiltab").ToString();
                if (sTempDate == string.Empty || sTempDate == null)
                {
                    sTempDate = "0000-00-00";
                }
                else
                {
                    sTempDate = Convert.ToDateTime(sTempDate).ToString(Mirror.AxessDateTimeFormat);
                }
                if (!cf.RowExists(MirrorConn, ActiveDatabase + ".tabaxorderdata", "norderlistnr='" + tRow["norderlistnr"].ToString() + "' AND testflag=1"))
                {
                    int? nAufTragNr = tRow.Field<int?>("nauftragnr");
                    string tESDate = "NULL";
                    if (nAufTragNr != null) tESDate = Buy.GetEStoreSaleDate(tRow.Field<int>("nauftragnr"));
                    if (tESDate != "NULL") tESDate = "'" + tESDate + "'";
                    string tIssueDate = "NULL";
                    if (tRow.Field<DateTime?>("dtausgabedat") != null) tIssueDate = "'" + tRow.Field<DateTime?>("dtausgabedat").Value.ToString(Mirror.AxessDateTimeFormat) + "'";
                    tSQL = "INSERT INTO " + ActiveDatabase + ".tabaxorderdata (nprojnr, norderlistnr, norderlistpos, nauftragnr, dtgiltab, norderstatusnr, nkundenkartentypnr, nperstypnr, npoolnr, nartikelnr, feinzeltarif, nkartennr, nkassaprojnr, nkassanr, ntransnr, nfirmenkassanr, nfirmennr, nperskassanr, npersnr, SZDELIVERYNAME, neingegsortstk, nrohlingstypnr, ndattraegtypnr, bwtp, esdate, firstname, lastname, dtupdate, issuedate) VALUES ('";
                    tSQL += tRow.Field<short>("nprojnr").ToString() + "','";
                    tSQL += tRow.Field<int>("norderlistnr").ToString() + "','" + tRow.Field<Int16?>("norderlistpos").ToString() + "','";
                    tSQL += nAufTragNr.ToString() + "','" + sTempDate + "','";
                    tSQL += tRow.Field<short?>("norderstatusnr").ToString() + "','";
                    tSQL += tRow.Field<int?>("nkundenkartentypnr").ToString() + "','";
                    tSQL += tRow.Field<int?>("nperstypnr").ToString() + "','" + tRow.Field<Int16?>("npoolnr").ToString() + "','";
                    tSQL += tRow.Field<int?>("nartikelnr").ToString() + "','";
                    tSQL += tRow.Field<decimal?>("feinzeltarif").ToString() + "','";
                    tSQL += cf.EscapeChar(tRow.Field<String>("nkartennr")) + "','" + tRow.Field<Int16?>("nkassaprojnr").ToString() + "','" + tRow.Field<Int16?>("nkassanr").ToString() + "','";
                    tSQL += tRow.Field<int?>("ntransnr").ToString() + "','";
                    tSQL += tRow.Field<Int16?>("nfirmenkassanr").ToString() + "','" + tRow.Field<int?>("nfirmennr").ToString() + "','" + tRow.Field<Int16?>("nperskassanr").ToString() + "','";
                    tSQL += tRow.Field<Int32?>("npersnr").ToString() + "','" + cf.EscapeChar(tRow.Field<string>("szdeliveryname")) + "','" + tRow.Field<Int32?>("neingegsortstk").ToString() + "','";
                    tSQL += tRow.Field<Int32?>("nrohlingstypnr").ToString() + "','" + tRow.Field<Int16?>("ndattraegtypnr").ToString() + "','" + tRow.Field<Int16?>("bwtp").ToString() + "'," + tESDate + ",'";
                    tSQL += cf.EscapeChar(tRow.Field<string>("szvorname")) + "','" + cf.EscapeChar(tRow.Field<string>("szname")) + "','" + DateTime.Now.ToString(Mirror.AxessDateTimeFormat) + "'," + tIssueDate + ")";
                    cf.ExecuteSQL(MirrorConn, tSQL);
                }
            }
            return true;
        }


        //public bool RepairPersonen()
        //{
        //    return InitTable("tabpersonen", 1, 50000, true);
        //}
        //public bool RepairAdressen()
        //{
        //    return InitTable("tabadressen", 1, 50000, true);
        //}
        //public bool InitTabKundenKartenTypDef()
        //{
        //    return InitTable("tabkundenkartentypdef");
        //}

        //public bool InitTabPersTypDef()
        //{
        //    return InitTable("tabperstypdef");
        //}

        //public bool InitTabArtikelDef()
        //{
        //    return InitTable("tabartikeldef");
        //}

        //public bool InitTabKassierKonf()
        //{
        //    return InitTable("tabkassierkonf");
        //}

        //public bool InitTabFirmenTypDef()
        //{
        //    return InitTable("tabfirmentypdef");
        //}

        //public bool InitTabDattraegTypDef()
        //{
        //    return InitTable("tabdattraegtypdef");
        //}

        //public bool InitTabKundenBezahlartenDef()
        //{
        //    return InitTable("tabkundenbezahlartendef");
        //}

        //public bool InitTabPoolDef()
        //{
        //    return InitTable("tabpooldef");
        //}

        //public bool InitTabPromptDef()
        //{
        //    return InitTable("tabpromptdef");
        //}

        //    public bool InitTabPrompTelementeDef()
        //    {
        //        return InitTable("tabpromptelementedef");
        //    }

        //    public bool InitTabRohlingsTypeDef()
        //    {
        //        return InitTable("tabrohlingstypdef");
        //    }

        //    public bool InitTabTransStatusDef()
        //    {
        //        return InitTable("tabtransstatusdef");
        //    }

        //    public bool InitTabZutrKonf()
        //    {
        //        return InitTable("tabzutrkonf");
        //    }

        //    public bool InitTabBerechtTarife()
        //    {
        //        return InitTable("tabberechttarife");
        //    }

        //    public bool InitTabPrepaidTickets()
        //    {
        //        return InitTable("tabprepaidtickets");
        //    }

        //    public bool InitTabChipKarten()
        //    {
        //        return InitTable("tabchipkarten");
        //    }

        //    public bool InitTabKartenSperre()
        //    {
        //        return InitTable("tabkartensperre");
        //    }

        //    public bool InitTabFirmen()
        //    {
        //        return InitTable("tabfirmen");
        //    }

        //    public bool InitTabLeserTrans()
        //    {
        //        return InitTable("tablesertrans");
        //    }

        //public bool InitTabPersonen()
        //{
        //    return InitTable("tabpersonen");
        //}

        //    public bool InitTabAdressen()
        //    {
        //        return InitTable("tabadressen");
        //    }

        //    public bool InitAXCust()
        //    {
        //        //InitTabPersonen();
        //        //InitTabAdressen();
        //        //MergeCustAddTables("");
        //        //delete all records in TabAXCust
        //        //create new records for TabAXCust by looping through tabpersonen and finding any addresses in tabadressen
        //         return true;
        //    }

        //public bool InitTabOrderList()
        //{
        //    return InitTable("taborderlist");
        //}

        //public bool InitTabOrderListPOS()
        //{
        //    return InitTable("taborderlistpos", 1, 50000, true);
        //}

        //public bool InitTabEdeauftrag()
        //{
        //    return InitTable("tabedeauftrag");
        //}

        //public bool InitTabAXOrderData()
        //{
        //    InitTabOrderList();
        //    InitTabOrderListPOS();
        //    InitTabEdeauftrag();
        //    return UpdateTabAXOrderData();
        //}

        //    public bool InitTabStorePromptMaster()
        //    {
        //        return InitTable("tabstorepromptmaster");
        //    }

        //    public bool InitTabStorePromptDetail()
        //    {
        //        return InitTable("tabstorepromptdetail");
        //    }

        //    public void InitStorePrompts()
        //    {
        //        //InitTabStorePromptMaster();
        //        InitTabStorePromptDetail();
        //    }

        public void SalesFix()
        {
            string tDate = DateTime.Now.AddDays(-2).ToString(AxessDateFormat);

        }

        public DataTable UpdateSalesTables()
        {
            DataTable dtReturn = new DataTable("SalesTrans");
            dtReturn.Columns.Add(new DataColumn()
            {
                DataType = System.Type.GetType("System.String"),
                ColumnName = "TransKey"
            }
            );

            if (!AxessSource.Login()) return dtReturn;
            long logNo = GetLogNumber("salesmodify");
            bool isDone = false;
            do
            {
                isDone = true;
                XmlDocument SalesModificationsXML = AxessSource.GetSalesModifyList(logNo + 1);
                foreach (XmlNode tNode in SalesModificationsXML.GetElementsByTagName("ACTLOGLINE").Item(0).ChildNodes)
                {
                    isDone = false;
                    string lfdZaehler = string.Empty;
                    string dtUpdateStamp = DateTime.Now.ToString(AxessDateTimeFormat);  // cf.TimestampConvert(Convert.ToDouble(tNode.SelectSingleNode("DTTIMESTAMP").InnerText));
                    foreach (XmlNode lNode in tNode.SelectSingleNode("ACTLOGVALUESNEW")) //loop through all values in the row and add them to the correct temp strings
                    {
                        string fieldName = lNode.SelectSingleNode("SZFIELDNAME").InnerText.ToLower();  //get field name
                        string fieldValue = (lNode.SelectSingleNode("SZFIELDVALUE").InnerText.Length != 0) ? "'" + cf.EscapeChar(lNode.SelectSingleNode("SZFIELDVALUE").InnerText) + "'" : "NULL";  //get field value and format for mySQL
                        switch (fieldName)
                        {
                            case "nlfdzaehler":
                                lfdZaehler = fieldValue.Replace("'", "");
                                break;
                            case "nsalesmodifylognr":
                                logNo = Convert.ToInt64(fieldValue.Replace("'", ""));
                                break;
                        }
                    }
                    DataSet myDS = new DataSet();
                    myDS = cf.LoadDataSet(MirrorConn, "SELECT nactkassanr AS nkassanr, ntransnr, nactprojnr as nprojnr,  nkassierid, nkassierabrechnr FROM " + ActiveDatabase + ".tabkassatransaufbereit WHERE nlfdzaehler=" + lfdZaehler, myDS, "TAB");
                    if (myDS.Tables["TAB"].Rows.Count != 0)
                    {
                        string nKassierID = myDS.Tables["TAB"].Rows[0].Field<short>("nkassierid").ToString();
                        string nKassierAbrechNo = myDS.Tables["TAB"].Rows[0].Field<int?>("nkassierabrechnr").ToString();
                        if (nKassierAbrechNo == string.Empty || nKassierAbrechNo == "NULL") nKassierAbrechNo = "-1";
                        string nProjNo = myDS.Tables["TAB"].Rows[0].Field<short>("nprojnr").ToString();
                        string nTransNo = myDS.Tables["TAB"].Rows[0].Field<int>("ntransnr").ToString();
                        string nKassaNo = myDS.Tables["TAB"].Rows[0].Field<short>("nkassanr").ToString();
                        InitSalesTables(Convert.ToInt32(lfdZaehler), false, false);
                        DataRow tRow = dtReturn.NewRow();
                        tRow["TransKey"] = nKassaNo + "-" + nTransNo;
                        dtReturn.Rows.Add(tRow);

                        ResetPaymentData(nKassaNo, nTransNo, nKassierID, nKassierAbrechNo, nProjNo, dtUpdateStamp);
                        UpdateLog("salesmodify", logNo);
                    }
                    lfdZaehler = string.Empty;
                }
            }
            while (!isDone);
            logNo = GetLogNumber("paymentmodify");
            do
            {
                isDone = true;
                XmlDocument PmtModificationsXML = AxessSource.GetPaymentModifyList(logNo + 1);
                //long fieldCount = 0;
                foreach (XmlNode tNode in PmtModificationsXML.GetElementsByTagName("ACTLOGLINE").Item(0).ChildNodes)
                {
                    isDone = false;
                    string nProjNo = string.Empty;
                    string nKassaNo = string.Empty;
                    string nTransNo = string.Empty;
                    string nKassierID = string.Empty;
                    string nKassierAbrechNo = string.Empty;
                    string dtUpdateStamp = DateTime.Now.ToString(AxessDateTimeFormat);  //cf.TimestampConvert(Convert.ToDouble(tNode.SelectSingleNode("DTTIMESTAMP").InnerText));
                    foreach (XmlNode lNode in tNode.SelectSingleNode("ACTLOGVALUESNEW"))
                    {
                        string fieldName = lNode.SelectSingleNode("SZFIELDNAME").InnerText.ToLower();  //get field name
                        string fieldValue = (lNode.SelectSingleNode("SZFIELDVALUE").InnerText.Length != 0) ? "'" + cf.EscapeChar(lNode.SelectSingleNode("SZFIELDVALUE").InnerText) + "'" : "NULL";  //get field value and format for mySQL
                        if ((fieldName.Substring(0, 2) == "dt") && (fieldValue != "NULL")) fieldValue = "'" + cf.TimestampConvert(Convert.ToDouble(fieldValue.Replace("'", ""))) + "'";
                        switch (fieldName)
                        {
                            case "nprojnr":
                                nProjNo = fieldValue.Replace("'", "");
                                break;
                            case "npaymentmodifylognr":
                                logNo = Convert.ToInt64(fieldValue.Replace("'", ""));
                                break;
                            case "dtupdate":
                                fieldValue = dtUpdateStamp;
                                break;
                            case "nkassierid":
                                nKassierID = fieldValue.Replace("'", "");
                                break;
                            case "nkassierabrechnr":
                                nKassierAbrechNo = fieldValue.Replace("'", "");
                                break;
                            case "ntransnr":
                                nTransNo = fieldValue.Replace("'", "");
                                break;
                            case "nkassanr":
                                nKassaNo = fieldValue.Replace("'", "");
                                break;
                        }
                    }

                    ResetPaymentData(nKassaNo, nTransNo, nKassierID, nKassierAbrechNo, nProjNo, dtUpdateStamp);
                    UpdateLog("paymentmodify", logNo);
                }
                if (AxessSource.ErrorNo != 1401)
                {
                    isDone = true;
                }
            }
            while (!isDone);
            return dtReturn;
        }

        public bool FixTransAufBereit(int aKassaNr, int aSerialNr, int aUnicodeNr, bool retainDTUpdate = true)
        {
            long ZaehlerNo = Convert.ToInt64(cf.GetSQLField(MirrorConn, "SELECT nlfdzaehler FROM " + ActiveDatabase + ".tabkassatransaufbereit WHERE nkassanr=" + aKassaNr.ToString() + " AND nseriennr=" + aSerialNr.ToString() + " AND nunicodenr=" + aUnicodeNr));
            InitSalesTables(ZaehlerNo, true, retainDTUpdate);
            return true;
        }

        public DataTable InitSalesTables(long ZaehlerNo = 0, bool singleRec = false, bool retainDTUpdate = true)
        {
            DataTable dtReturn = new DataTable("SalesTrans");
            dtReturn.Columns.Add(new DataColumn()
            {
                DataType = System.Type.GetType("System.String"),
                ColumnName = "TransKey"
            }
            );
            DataColumn[] keys = new DataColumn[1];
            keys[0] = dtReturn.Columns["serialkey"];
            dtReturn.PrimaryKey = keys;

            if (!AxessSource.Login())
            {
                return dtReturn; //can't log in so return false to calling program
            }

            long loaded = 0;    //total number of records loaded so far from table
            long nProjNo = 0;
            long nKassaNo = 0;
            long nTransNo = 0;
            if (ZaehlerNo == 0)
                retainDTUpdate = false;
            long lfdZaehler = (ZaehlerNo == 0 ? Convert.ToInt64(cf.GetSQLField(MirrorConn, "SELECT MAX(nlfdzaehler) FROM " + ActiveDatabase + ".tabkassatransaufbereit")) : ZaehlerNo - 1);
            bool isDone = false; //'340871207'
            do                  //loops until no records returned, always does one pass
            {
                while (AxessSource.ErrorNo == 0 && !isDone)     //loop while there are records left to process
                {
                    XmlDocument localXML = AxessSource.GetSalesList(lfdZaehler + 1, (ZaehlerNo == 0 ? 500 : 1));    //get a batch from the snapshot, currently limited to 1000 records but code allows infinite
                    isDone = true;
                    foreach (XmlNode tNode in localXML.GetElementsByTagName("ACTLOGLINE").Item(0).ChildNodes)   //loop through each LogLine and copy it to memory, then to the mirror database
                    {
                        isDone = false || (ZaehlerNo != 0) || singleRec;
                        string myFunction = tNode.SelectSingleNode("SZKIND").InnerText.ToUpper();   //Log function, I=Insert, U=Update, D=Delete
                        string QualifiedTableName = ActiveDatabase + "." + tNode.SelectSingleNode("SZTABNAME").InnerText.ToLower(); //Qualified table name is database name (dot) table name, needed for SQL query
                        string tFields = string.Empty;  //temp string to hold field values for SQL query
                        string tValues = string.Empty;  //temp string to hold data values for SQL query
                        string myQuery = string.Empty;  //temp string to hold SQL query
                        string tMediaID = string.Empty; //holds media ID when available
                        XmlNode tXMLNode = tNode.SelectSingleNode("ACTLOGVALUESNEW");  //load the correct values based on function
                        string dtUpdateStamp = DateTime.Now.ToString(AxessDateTimeFormat);  //cf.TimestampConvert(Convert.ToDouble(tNode.SelectSingleNode("DTTIMESTAMP").InnerText));
                        foreach (XmlNode lNode in tXMLNode) //loop through all values in the row and add them to the correct temp strings
                        {
                            string fieldName = lNode.SelectSingleNode("SZFIELDNAME").InnerText.ToLower();  //get field name
                            string fieldValue = (lNode.SelectSingleNode("SZFIELDVALUE").InnerText.Length != 0) ? "'" + cf.EscapeChar(lNode.SelectSingleNode("SZFIELDVALUE").InnerText) + "'" : "NULL";  //get field value and format for mySQL
                            if ((fieldName.Substring(0, 2) == "dt") && (fieldValue != "NULL")) fieldValue = "'" + cf.TimestampConvert(Convert.ToDouble(fieldValue.Replace("'", ""))) + "'";
                            if (fieldName.ToUpper() == "NKARTENNR") tMediaID = fieldValue.Replace("'", "");
                            switch (fieldName)
                            {
                                case "nprojnr":
                                    nProjNo = Convert.ToInt64(fieldValue.Replace("'", ""));
                                    break;
                                case "nactkassanr":
                                    nKassaNo = Convert.ToInt64(fieldValue.Replace("'", ""));
                                    break;
                                case "ntransnr":
                                    if (fieldValue != "NULL")
                                        nTransNo = Convert.ToInt64(fieldValue.Replace("'", ""));
                                    break;
                                case "nlfdzaehler":
                                    lfdZaehler = Convert.ToInt64(fieldValue.Replace("'", ""));
                                    break;
                            }
                            switch (myFunction)     //build SQL query values based on function
                            {
                                case "I":   //for insert
                                    tFields += fieldName + ",";
                                    tValues += fieldValue + ",";
                                    break;
                                case "U":   //for update
                                    tValues += fieldName + "=" + fieldValue + ",";
                                    break;
                                case "D":   //TODO for delete
                                    //either delete based on all fields being the same or do a switch based on table name and delete based on primary key
                                    break;
                                default:
                                    myError.UpdateErrorLog("Mirror.InitSalesTables", -9999, "Invalid myFunction value: " + myFunction + "  (" + nKassaNo + "-" + nTransNo + ")");
                                    return dtReturn;
                            }
                        }
                        if ((cf.IsAltaPOS(nKassaNo)))
                        {
                            string sqlTemp = "nlfdzaehler='" + lfdZaehler + "'";
                            string tDTUpdate = dtUpdateStamp;
                            if (cf.RowExists(MirrorConn, ActiveDatabase + ".tabkassatransaufbereit", sqlTemp))
                            {
                                if (retainDTUpdate)
                                {
                                    try
                                    {
                                        tDTUpdate = Convert.ToDateTime(cf.GetSQLField(MirrorConn, $"SELECT dtinserttimestamp FROM {ActiveDatabase}.tabkassatransaufbereit WHERE {sqlTemp}")).ToString(AxessDateTimeFormat);
                                    }
                                    catch { }
                                }
                                cf.ExecuteSQL(MirrorConn, $"DELETE FROM {ActiveDatabase}.tabkassatransaufbereit WHERE {sqlTemp}");
                            }
                            if (Convert.ToDateTime(tDTUpdate) > Convert.ToDateTime(dtUpdateStamp))
                                tDTUpdate = dtUpdateStamp;
                            switch (myFunction) //now build SQL statement 
                            {
                                case "I":   //for insert
                                    tFields += "MEDIAID,WTP64,WTP32,PFLAG,DTUPDATE";
                                    if (tMediaID.ToUpper() != "NULL")
                                    {
                                        tValues += "'" + tMediaID + "','" + AxessSource.GetWebID(tMediaID) + "','" + AxessSource.MediaID_2_WTP32(tMediaID) + "','0','" + tDTUpdate + "'";
                                    }
                                    else
                                    {
                                        tValues += "null,null,null,'0','" + tDTUpdate + "'";      //removed three nulls 'NULL', maybe need null?
                                    }
                                    myQuery = "INSERT INTO " + QualifiedTableName + " (" + tFields + ") values (" + tValues + ")";
                                    break;
                                case "U":   //for update
                                    tValues += "MEDIAID='" + tMediaID + "',WTP64='" + AxessSource.GetWebID(tMediaID.Replace("'", "") + "',WTP32='" + AxessSource.MediaID_2_WTP32(tMediaID.Replace("'", "") + "'"));
                                    myQuery = "UPDATE " + QualifiedTableName + " SET " + tValues + " WHERE NLFDZAEHLER='" + tNode.SelectSingleNode("NLFDZAEHLER").InnerText.ToUpper() + "'";
                                    break;
                                case "D":   //TODO for delete
                                    break;
                            }
                            if (!cf.ExecuteSQL(MirrorConn, myQuery))
                                return dtReturn;
                            bool newTKey = (dtReturn.Rows.Count == 0);
                            if (!newTKey)
                            {
                                keys[0] = dtReturn.Columns["transkey"];
                                dtReturn.PrimaryKey = keys;
                                newTKey = (dtReturn.Rows.Find(nKassaNo.ToString() + "-" + nTransNo.ToString()) == null);
                            }
                            if (newTKey)
                            {
                                DataRow tRow = dtReturn.NewRow();
                                tRow["TransKey"] = nKassaNo.ToString() + "-" + nTransNo.ToString();
                                dtReturn.Rows.Add(tRow);
                            }
                            SetPaymentData(nKassaNo, nTransNo, nProjNo, "default", retainDTUpdate);
                        }
                        loaded++;  //increment table counter
                        //statusText("Updating Payment Information " + loaded.ToString() + " -- " + nKassaNo + " -- " + lfdZaehler);
                        System.Diagnostics.Debug.Print(lfdZaehler.ToString());
                        UpdateLog("tabkassatransaufbereit", lfdZaehler);
                    }
                }
            }
            while (!isDone); //repeat until last snapshot
            return (dtReturn);
        }
        //public bool FixBereitIDs()
        //{
        //    //find all records with invalid or null IDs
        //    DataSet tDS = cf.LoadDataSet(MirrorConn, "SELECT DISTINCT NLFDZAEHLER FROM " + ActiveDatabase + ".tabkassatransaufbereit where dtupdate >= '" + DateTime.Now.AddDays(-2).ToString(AxessDateFormat) + "' AND NSERIENNR > 0 AND mediaid is null",new DataSet(), "TAB");
        //    //update if ID now found.
        //    int i = 2;
        //    if (tDS.Tables.Contains("TAB"))
        //    {
        //        foreach (DataRow tRow in tDS.Tables["TAB"].Rows)
        //        {
        //            //fixTransAufBereit(Convert.ToInt32(tRow["nkassanr"].ToString()), Convert.ToInt32(tRow["nseriennr"].ToString()), false);
        //            InitSalesTables(Convert.ToInt64(tRow["nlfdzaehler"].ToString()), true, false);
        //            System.Diagnostics.Debug.Print(i++.ToString());
        //        }
        //    }
        //    return true;
        //}

        //public void LoadPayments()
        //{
        //    long curYear = 2016;
        //    long curMonth = 7;
        //    DataSet myDS = new DataSet();
        //    myDS = cf.LoadDataSet(MirrorConn, "SELECT DISTINCT NTRANSNR, NACTPROJNR, NACTKASSANR, DTUPDATE FROM " + ActiveDatabase + ".tabkassatransaufbereit WHERE NTRANSNR <> 0 AND feinzeltariftats <> 0 AND YEAR(dtausgabedat) = " + curYear.ToString() + " AND MONTH(dtausgabedat) >= " + curMonth.ToString(),myDS, "TAB");
        //    foreach (DataRow tRow in myDS.Tables["TAB"].Rows)
        //    {
        //        StatusText("Loading Payments " + (myDS.Tables["TAB"].Rows.IndexOf(tRow) + 1).ToString() + " of " + myDS.Tables["TAB"].Rows.Count.ToString());
        //        SetPaymentData(Convert.ToInt64(tRow.Field<short>("nactkassanr")), Convert.ToInt64(tRow.Field<int>("ntransnr")), Convert.ToInt64(tRow.Field<short>("nactprojnr")));
        //    }

        //}

        public bool ResetPaymentData(string nKassaNo, string nTransNo, string nKassierNo, string nKassierAbrechNo, string nProjNo, string dtUpdate)
        {
            string tQuery = "DELETE FROM " + ActiveDatabase + ".tabaxpmtdata WHERE nprojnr=" + nProjNo + " AND ntransnr=" + nTransNo + " AND nkassanr=" + nKassaNo;
            cf.ExecuteSQL(MirrorConn, tQuery);
            cf.ExecuteSQL(MirrorConn, tQuery.Replace("axpmtdata", "bezahlart"));
            cf.ExecuteSQL(MirrorConn, tQuery.Replace("bezahlart", "kassatransakt"));
            SetPaymentData(Convert.ToInt64(nKassaNo), Convert.ToInt64(nTransNo), Convert.ToInt64(nProjNo));
            return true;
        }

        //Type -- sales - change comes from a sales change, don't change payment dtupdate fields from their original.
        //        payment - change comes from a payment change, don't change sales dtupdate field from original
        //        default -sales, payment and axpmtdata all get their own most recent dtupdate meaning all get current until Axess fixes dttimestamp
        //tabaxpmtdata always gets the most current dttimestamp 

        public bool SetPaymentData(long nKassaNo, long nTransNo, long nProjNo, string Type = "default", bool retainDTUpdate = false)
        {
            long nKassierID = 0;
            string nKassierAbrechNo = string.Empty;
            string saledate = string.Empty;
            string ntranstatus = string.Empty;
            string nperskassanr = string.Empty;
            string npersnr = string.Empty;
            string nfirmenkassanr = string.Empty;
            string nfirmennr = string.Empty;
            string nlfdnr = string.Empty;
            string pmtamt = string.Empty;
            string pmtamt2 = string.Empty;
            string ngrundbezartnr = string.Empty;
            string nkundenbezartnr = string.Empty;
            string bhasbeleg = string.Empty;
            string bdeleted = string.Empty;
            string bbasisbezart = string.Empty;
            string bgesplittet = string.Empty;
            string transkey = string.Empty;
            string transpkey = string.Empty;
            string pmtkey = string.Empty;
            string saledtime = string.Empty;
            string newrec = string.Empty;
            string updateflag = string.Empty;
            string ptype = string.Empty;
            string cashier = string.Empty;
            string arcustid = string.Empty;
            string dtUpdate = DateTime.Now.ToString(AxessDateTimeFormat);
            string curUpdate = dtUpdate;
            string originalBereitDate = string.Empty;
            string originalTransaktDate = string.Empty;
            string originalBezahlartDate = string.Empty;



            string sqlTemp = " WHERE nprojnr=" + nProjNo.ToString() + " AND nkassanr=" + nKassaNo.ToString() + " AND ntransnr=" + nTransNo.ToString();
            cf.ExecuteSQL(MirrorConn, "DELETE FROM " + ActiveDatabase + ".tabaxpmtdata" + sqlTemp);
            if (Type.ToLower() == "sales")
            {
                originalTransaktDate = cf.GetSQLField(MirrorConn, "SELECT CAST(dtupdate) AS string FROM " + ActiveDatabase + ".tabkassatransakt" + sqlTemp);
                originalBezahlartDate = cf.GetSQLField(MirrorConn, "SELECT CAST(dtupdate) AS string FROM " + ActiveDatabase + ".tabbezahlart" + sqlTemp);
            }
            cf.ExecuteSQL(MirrorConn, "DELETE FROM " + ActiveDatabase + ".tabkassatransakt" + sqlTemp);
            cf.ExecuteSQL(MirrorConn, "DELETE FROM " + ActiveDatabase + ".tabbezahlart" + sqlTemp);
            DataSet myDS = new DataSet();
            string mySQL = "SELECT nkassanr, nkassierid, nkassierabrechnr, nlfdzaehler, nperskassanr, nfirmenkassanr, nfirmennr, dtausgabedat, nperspersnr AS npersnr, feinzeltariftats, dtupdate FROM " + ActiveDatabase + ".tabkassatransaufbereit WHERE nactprojnr=" + nProjNo.ToString() + " AND nactkassanr=" + nKassaNo + " AND ntransnr=" + nTransNo.ToString();
            myDS = cf.LoadDataSet(MirrorConn, mySQL, myDS, "TAB");
            if (!myDS.Tables.Contains("TAB")) return false;
            if (myDS.Tables["TAB"].Rows.Count == 0) return false;
            long lfdZaehler = Convert.ToInt64(myDS.Tables["TAB"].Rows[0].Field<int>("nlfdzaehler"));
            nKassierID = Convert.ToInt64(myDS.Tables["TAB"].Rows[0].Field<Int16>("nkassierid"));
            pmtamt2 = myDS.Tables["TAB"].Rows[0].Field<decimal>("feinzeltariftats").ToString();
            nKassierAbrechNo = myDS.Tables["TAB"].Rows[0].Field<Int32?>("nkassierabrechnr").ToString();
            if (nKassierAbrechNo == string.Empty || nKassierAbrechNo == "NULL") nKassierAbrechNo = "-1";
            nperskassanr = myDS.Tables["TAB"].Rows[0].Field<Int16?>("nperskassanr").ToString();
            nfirmenkassanr = myDS.Tables["TAB"].Rows[0].Field<Int16?>("nfirmenkassanr").ToString();
            if (nfirmenkassanr == string.Empty) nfirmenkassanr = "NULL";
            nfirmennr = myDS.Tables["TAB"].Rows[0].Field<Int32?>("nfirmennr").ToString();
            if (nfirmennr == string.Empty) nfirmennr = "NULL";
            saledate = myDS.Tables["TAB"].Rows[0].Field<DateTime>("dtausgabedat").ToString(Mirror.AxessDateFormat);
            npersnr = myDS.Tables["TAB"].Rows[0].Field<Int32?>("npersnr").ToString();
            originalBereitDate = myDS.Tables["TAB"].Rows[0].Field<DateTime>("dtupdate").ToString(Mirror.AxessDateTimeFormat);
            //load payment data records
            if (AxessSource.SessionID == 0) AxessSource.Login();
            XmlDocument PaymentXML = AxessSource.GetPaymentData(nProjNo, nKassaNo, nTransNo, nKassierID, Convert.ToInt64(nKassierAbrechNo));
            //start with tabkassatransakt record since it is required for all axpmtdata lines
            bool foundBezehlart = false;
            foreach (XmlNode payNode in PaymentXML.GetElementsByTagName("ACTLOGLINE").Item(0).ChildNodes)   //loop through each LogLine and copy it to tabAXPmtData
            {
                string tTable = payNode.SelectSingleNode("SZTABNAME").InnerText.ToLower();
                //curUpdate = DateTime.Now.ToString(AxessDateTimeFormat); //  cf.TimestampConvert(Convert.ToDouble(payNode.SelectSingleNode("DTTIMESTAMP").InnerText));
                switch (tTable)
                {
                    case "tabkassatransakt":
                        string mydFunction = payNode.SelectSingleNode("SZKIND").InnerText.ToUpper();   //Log function, I=Insert, U=Update, D=Delete
                        XmlNode ptXMLNode = payNode.SelectSingleNode("ACTLOGVALUESNEW");  //load the correct values based on function
                        string tFields = string.Empty;  //temp string to hold field values for SQL query
                        string tValues = string.Empty;  //temp string to hold data values for SQL query
                        foreach (XmlNode lNode in ptXMLNode) //loop through all values in the row and add them to the correct temp strings
                        {
                            string fieldName = lNode.SelectSingleNode("SZFIELDNAME").InnerText.ToLower();  //get field name
                            string fieldValue = (lNode.SelectSingleNode("SZFIELDVALUE").InnerText.Length != 0) ? "'" + cf.EscapeChar(lNode.SelectSingleNode("SZFIELDVALUE").InnerText) + "'" : "NULL";  //get field value and format for mySQL
                            if ((fieldName.Substring(0, 2) == "dt") && (fieldValue != "NULL")) fieldValue = "'" + cf.TimestampConvert(Convert.ToDouble(fieldValue.Replace("'", ""))) + "'";
                            switch (fieldName)
                            {
                                case "feinzeltariftats":
                                    pmtamt2 = fieldValue;
                                    break;
                                case "ntransstatus":
                                    ntranstatus = fieldValue;
                                    break;
                                case "bhasbeleg":
                                    bhasbeleg = fieldValue;
                                    break;
                                case "bgesplittet":
                                    bgesplittet = fieldValue;
                                    break;
                                case "ngrundbezartnr":
                                    ngrundbezartnr = fieldValue;
                                    break;
                                case "nkundenbezartnr":
                                    nkundenbezartnr = fieldValue;
                                    break;
                                case "nfirmenkassanr":
                                    nfirmenkassanr = fieldValue;
                                    break;
                                case "nfirmennr":
                                    nfirmennr = fieldValue;
                                    break;
                                case "dtausgabedat":
                                    saledtime = fieldValue;
                                    break;
                                case "dtupdate":
                                    if (retainDTUpdate)
                                    {
                                        dtUpdate = originalBereitDate;
                                    }
                                    else
                                    {
                                        if (Type.ToLower() == "sales")
                                        {
                                            dtUpdate = originalTransaktDate;
                                        }
                                        else
                                        {
                                            dtUpdate = curUpdate;
                                        }
                                    }
                                    if (Convert.ToDateTime(dtUpdate) > Convert.ToDateTime(curUpdate))
                                        dtUpdate = curUpdate;
                                    break;
                            }
                            switch (mydFunction)     //build SQL query values based on function
                            {
                                case "I":   //for insert
                                    tFields += fieldName + ",";
                                    tValues += fieldValue + ",";
                                    break;
                                case "U":   //for update
                                    tValues += fieldName + "=" + fieldValue + ",";
                                    break;
                                case "D":   //TODO for delete
                                    //either delete based on all fields being the same or do a switch based on table name and delete based on primary key
                                    break;
                                default:
                                    myError.UpdateErrorLog("Mirror.SetPaymentData", -9999, "Unknown mydFunction value: " + mydFunction + "  (" + nKassaNo + "-" + nTransNo + ")");
                                    return false;
                            }
                        }
                        string myQuery = string.Empty;
                        switch (mydFunction)
                        {
                            case "I":   //for insert
                                myQuery = "INSERT INTO " + ActiveDatabase + "." + tTable + " (" + tFields.Remove(tFields.Length - 1) + ") values (" + tValues.Remove(tValues.Length - 1) + ")";
                                break;
                            case "U":   //for update
                                myQuery = "UPDATE " + ActiveDatabase + "." + tTable + " SET " + tValues.Remove(tValues.Length - 1);
                                break;
                            case "D":   //TODO for delete
                                break;
                        }
                        if (!cf.ExecuteSQL(MirrorConn, myQuery))
                            return false;    //run the query to proccess single record
                        break;
                    case "tabbezahlart":
                        foundBezehlart = true;
                        mydFunction = payNode.SelectSingleNode("SZKIND").InnerText.ToUpper();   //Log function, I=Insert, U=Update, D=Delete
                        ptXMLNode = payNode.SelectSingleNode("ACTLOGVALUESNEW");  //load the correct values based on function
                        tFields = string.Empty;  //temp string to hold field values for SQL query
                        tValues = string.Empty;  //temp string to hold data values for SQL query
                        foreach (XmlNode lNode in ptXMLNode) //loop through all values in the row and add them to the correct temp strings
                        {
                            string fieldName = lNode.SelectSingleNode("SZFIELDNAME").InnerText.ToLower();  //get field name
                            string fieldValue = (lNode.SelectSingleNode("SZFIELDVALUE").InnerText.Length != 0) ? "'" + cf.EscapeChar(lNode.SelectSingleNode("SZFIELDVALUE").InnerText) + "'" : "NULL";  //get field value and format for mySQL
                            if ((fieldName.Substring(0, 2) == "dt") && (fieldValue != "NULL")) fieldValue = "'" + cf.TimestampConvert(Convert.ToDouble(fieldValue.Replace("'", ""))) + "'";
                            switch (fieldName)
                            {
                                case "nlfdnr":
                                    nlfdnr = fieldValue;
                                    break;
                                case "dbbetrag":
                                    pmtamt = fieldValue;
                                    break;
                                case "bdeleted":
                                    bdeleted = fieldValue;
                                    break;
                                case "bbasisbezart":
                                    bbasisbezart = fieldValue;
                                    break;
                                case "nkundenbezartnr":
                                    nkundenbezartnr = fieldValue;
                                    break;
                                case "ngrundbezartnr":
                                    ngrundbezartnr = fieldValue;
                                    break;
                                case "dtupdate":
                                    if (retainDTUpdate && (dtUpdate != null))
                                    {
                                        dtUpdate = originalBereitDate;
                                    }
                                    else
                                    {
                                        if (Type.ToLower() == "sales")
                                        {
                                            dtUpdate = originalTransaktDate;
                                        }
                                        else
                                        {
                                            dtUpdate = curUpdate;
                                        }
                                    }
                                    if (Convert.ToDateTime(dtUpdate) > Convert.ToDateTime(curUpdate))
                                        dtUpdate = curUpdate;
                                    break;
                            }
                            switch (mydFunction)     //build SQL query values based on function
                            {
                                case "I":   //for insert
                                    tFields += fieldName + ",";
                                    tValues += fieldValue + ",";
                                    break;
                                case "U":   //for update
                                    tValues += fieldName + "=" + fieldValue + ",";
                                    break;
                                case "D":   //TODO for delete
                                            //either delete based on all fields being the same or do a switch based on table name and delete based on primary key
                                    break;
                                default:
                                    myError.UpdateErrorLog("Mirror.SetPaymentData", -9999, "Unknown mydFunction value: " + mydFunction + "  (" + nKassaNo + "-" + nTransNo + ")");
                                    return false;
                            }
                        }
                        myQuery = string.Empty;
                        switch (mydFunction)
                        {
                            case "I":   //for insert
                                myQuery = "INSERT INTO " + ActiveDatabase + "." + tTable + " (" + tFields.Remove(tFields.Length - 1) + ") values (" + tValues.Remove(tValues.Length - 1) + ")";
                                break;
                            case "U":   //for update
                                myQuery = "UPDATE " + ActiveDatabase + "." + tTable + " SET " + tValues.Remove(tValues.Length - 1);
                                break;
                            case "D":   //TODO for delete
                                break;
                        }
                        if (!cf.ExecuteSQL(MirrorConn, myQuery)) return false;    //run the query to proccess single record
                        // now create tabaxpmtdata rows, one for each tabbezahlart record.
                        transkey = "'" + nKassaNo.ToString() + "-" + nTransNo.ToString() + "'";
                        transpkey = "'" + nKassaNo.ToString() + "-" + nTransNo.ToString() + '-' + nlfdnr.Replace("'", "") + "'";
                        if (npersnr == string.Empty) npersnr = "NULL";
                        if (ngrundbezartnr == "NULL") ngrundbezartnr = "0";
                        Int16 npmtkey = Convert.ToInt16(ngrundbezartnr.Replace("'", ""));
                        if (ngrundbezartnr == "0") ngrundbezartnr = "NULL";
                        pmtkey = "'" + (npmtkey + 100).ToString() + nkundenbezartnr.Replace("'", "") + "'";
                        if (npersnr == string.Empty) npersnr = "NULL";
                        myQuery = "INSERT INTO " + ActiveDatabase + ".tabaxpmtdata (nprojnr, nkassanr, ntransnr, saledate, nkassierid, ntranstatus, nperskassanr, npersnr, nfirmenkassanr, nfirmennr, nlfdnr, pmtamt, ngrundbezartnr, nkundenbezartnr, bhasbeleg, bdeleted, bbasisbezart, bgesplittet, transkey, transpkey, pmtkey, saledtime, newrec, updateflag, ptype, cashier, arcustid, email_flag, dtupdate) ";
                        myQuery += "VALUES ('" + nProjNo + "','" + nKassaNo + "','" + nTransNo + "','" + saledate + "','" + nKassierID + "'," + ntranstatus + ",'" + nperskassanr + "'," + npersnr + "," + nfirmenkassanr + "," + nfirmennr + "," + nlfdnr + "," + pmtamt + "," + ngrundbezartnr + "," + nkundenbezartnr + ",";
                        myQuery += bhasbeleg + "," + bdeleted + "," + bbasisbezart + "," + bgesplittet + "," + transkey + "," + transpkey + "," + pmtkey + "," + saledtime + ",'','','','','','','" + (retainDTUpdate ? originalBereitDate : curUpdate) + "')";
                        if (!cf.ExecuteSQL(MirrorConn, myQuery)) return false;
                        break;
                }
            }
            if (!foundBezehlart)
            {
                pmtamt = pmtamt2;
                nlfdnr = "0";
                bdeleted = "0";
                bbasisbezart = "NULL";
                transkey = "'" + nKassaNo.ToString() + "-" + nTransNo.ToString() + "'";
                transpkey = "'" + nKassaNo.ToString() + "-" + nTransNo.ToString() + '-' + nlfdnr.Replace("'", "") + "'";
                if (ngrundbezartnr == "NULL" || ngrundbezartnr == string.Empty) ngrundbezartnr = "0";
                Int16 npmtkey = Convert.ToInt16(ngrundbezartnr.Replace("'", ""));
                if (ngrundbezartnr == "0") ngrundbezartnr = "NULL";
                pmtkey = "'" + (npmtkey + 100).ToString() + nkundenbezartnr.Replace("'", "") + "'";
                if (npersnr == string.Empty) npersnr = "NULL";
                string myQuery = "INSERT INTO " + ActiveDatabase + ".tabaxpmtdata (nprojnr, nkassanr, ntransnr, saledate, nkassierid, ntranstatus, nperskassanr, npersnr, nfirmenkassanr, nfirmennr, nlfdnr, pmtamt, ngrundbezartnr, nkundenbezartnr, bhasbeleg, bdeleted, bbasisbezart, bgesplittet, transkey, transpkey, pmtkey, saledtime, newrec, updateflag, ptype, cashier, arcustid, email_flag, dtupdate) ";
                myQuery += "VALUES ('" + nProjNo + "','" + nKassaNo + "','" + nTransNo + "','" + saledate + "','" + nKassierID + "'," + ntranstatus + ",'" + nperskassanr + "'," + npersnr + "," + nfirmenkassanr + "," + nfirmennr + "," + nlfdnr + "," + pmtamt + "," + ngrundbezartnr + "," + nkundenbezartnr + ",";
                myQuery += bhasbeleg + "," + bdeleted + "," + bbasisbezart + "," + bgesplittet + "," + transkey + "," + transpkey + "," + pmtkey + "," + saledtime + ",'','','','','','','" + (retainDTUpdate ? originalBereitDate : curUpdate) + "')";
                if (!cf.ExecuteSQL(MirrorConn, myQuery)) return false;
            }
            //taltaSync.SyncPurchase(nKassaNo.ToString() + "-" + nTransNo.ToString());
            return true;
        }


        public void FixTransAufBereitIDs(int aKassaNr, int aSerienNr, string aMediaID)
        {

            //cf.ExecuteSQL("UPDATE " + activeDatabase + ".tabkassatransaufbereit SET ")
        }

        public void FixMediaIDUsinglesertrans()
        {
            DataSet tDS = cf.LoadDataSet(MirrorConn, "SELECT DISTINCT NKASSANR, nseriennr, NUNICODENRINTERN AS nunicodenr, NLFDLESERTRANSNR FROM " + ActiveDatabase + ".tablesertrans WHERE dtverwdat >= '2017-01-18'", new DataSet(), "lt");
            foreach (DataRow tRow in tDS.Tables["lt"].Rows)
            {
                if (cf.RowExists(MirrorConn, ActiveDatabase + ".tabkassatransaufbereit", "nkassanr=" + tRow["nkassanr"].ToString() + " AND nseriennr=" + tRow["nseriennr"].ToString() + " AND nunicodenr=" + tRow["nuincodenr"].ToString() + " AND mediaid is null"))
                {
                    FixTransAufBereit(Convert.ToInt32(tRow["nkassanr"].ToString()), Convert.ToInt32(tRow["nseriennr"].ToString()), Convert.ToInt32(tRow["nunicodenr"].ToString()), false);
                }
            }
        }

        public long UpdateTables(long aBeginLogNr, string aTableList = "")
        {
            if (!AxessSource.Login() || aBeginLogNr==0) return 0; //can't log in so return false to calling program
            DS = cf.LoadDataSet(MirrorConn, $"SELECT TableName, LogNo, KeyFields FROM {ActiveDatabase}.tabLog WHERE LogGroup ='A' ORDER BY TableName", DS, "KeyFields");
            int tRowCount = DS.Tables["KeyFields"].Rows.Count;
            if (aTableList == string.Empty)
            {
                foreach (DataRow tRow in DS.Tables["KeyFields"].Rows)
                {
                    aTableList += "," + tRow["TableName"].ToString();
                }
            }
            aTableList = aTableList.Substring(1);
            long curLogNo = aBeginLogNr;  //lowest log number for basic tables
            long lastLogNo = curLogNo;
            string OrderNrList = string.Empty;

            XmlDocument localXML = AxessSource.GetLogList(curLogNo, 1000);
            if (AxessSource.ErrorNo != 0)
            {
                goto cleanout;
            }
            foreach (XmlNode tNode in localXML.GetElementsByTagName("ACTLOGLINE").Item(0).ChildNodes)   //loop through each LogLine and copy it to memory, then to the mirror database
            {
                //Application.DoEvents();
                curLogNo = Convert.ToInt64(tNode.SelectSingleNode("NLOGNR").InnerText);
                string myFunction = tNode.SelectSingleNode("SZKIND").InnerText.Trim().ToUpper();   //Log function, I=Insert, U=Update, D=Delete
                string curTable = tNode.SelectSingleNode("SZTABNAME").InnerText.ToLower();
                string QualifiedTableName = $"{ActiveDatabase}.{curTable}"; //Qualified table name is database name (dot) table name, needed for SQL query
                string dtUpdateStamp = DateTime.Now.ToString(AxessDateTimeFormat);   //cf.TimestampConvert(Convert.ToDouble(tNode.SelectSingleNode("DTTIMESTAMP").InnerText));
                string pKassa = string.Empty;
                string pTrans = string.Empty;
                string pZaehler = string.Empty;
                string pPrompt = string.Empty;
                string pElement = string.Empty;
                try
                {
                    string tTableList = "'" + aTableList.Replace(",", "','") + "'";
                    if (((GetLogNumber(curTable) < curLogNo++)) & tTableList.Contains("'" + curTable + "'"))
                    {
                        string tFields = string.Empty;  //temp string to hold field values for SQL query
                        string tValues = string.Empty;  //temp string to hold data values for SQL query
                        string tWhere = string.Empty;   //temp string to hold where query values for update and delete functions
                        string myQuery = string.Empty;  //temp string to hold SQL query
                        XmlNode tXMLNode = tNode.SelectSingleNode("ACTLOGVALUESNEW");   //new row values
                        XmlNode oXMLNode = tNode.SelectSingleNode("ACTLOGVALUESOLD");   //old row values
                        if (myFunction != "I")  //build where statement for updates and deletes
                        {
                            foreach (DataRow tRow in DS.Tables["KeyFields"].Rows)
                            {
                                if (tRow["TableName"].ToString().Trim().ToUpper() == curTable)
                                {
                                    string tKeyFields = "'" + tRow["KeyFields"].ToString().ToLower() + "'";
                                    foreach (XmlNode tNode1 in oXMLNode)
                                    {
                                        string tName = tNode1.SelectSingleNode("SZFIELDNAME").InnerText.ToLower();  //get field name
                                        if (tKeyFields.Contains("'" + tName + "'"))
                                        {
                                            string tValue = (tNode1.SelectSingleNode("SZFIELDVALUE").InnerText.Length != 0) ? "'" + cf.EscapeChar(tNode1.SelectSingleNode("SZFIELDVALUE").InnerText) + "'" : "NULL";  //get field value and format for mySQL
                                            tWhere = (tWhere.Length != 0 ? " AND " : "") + tName + "=" + cf.EscapeChar(tValue);
                                        }
                                    }
                                }
                            }
                        }

                        string curCustNr = string.Empty;
                        string curCustKassa = string.Empty;
                        string curAddrNr = string.Empty;
                        string curAddrKass = string.Empty;
                        string promptZaehler = string.Empty;
                        if (myFunction != "D")  
                        {
                            foreach (XmlNode lNode in tXMLNode) //loop through all values in the row and add them to the correct temp strings
                            {
                                string fieldName = lNode.SelectSingleNode("SZFIELDNAME").InnerText.ToLower();  //get field name
                                string fieldValue = (lNode.SelectSingleNode("SZFIELDVALUE").InnerText.Length != 0) ? "'" + cf.EscapeChar(lNode.SelectSingleNode("SZFIELDVALUE").InnerText) + "'" : "NULL";  //get field value and format for mySQL
                                if (fieldName == "dtupdate")
                                {
                                    fieldValue = dtUpdateStamp;
                                }
                                else
                                {
                                    switch (curTable)
                                    {
                                        case "tabstorepromptmaster":
                                        case "tabstorepromptdetail":
                                            switch (fieldName)
                                            {
                                                case "npromptzaehlernr":
                                                    pZaehler = fieldValue;
                                                    break;
                                                case "nkassanr":
                                                    pKassa = fieldValue;
                                                    break;
                                                case "npromptnr":
                                                    pPrompt = fieldValue;
                                                    break;
                                                case "npromptelementnr":
                                                    pElement = fieldValue;
                                                    break;
                                                case "ntransnr":
                                                    pTrans = fieldValue;
                                                    break;
                                            }
                                            break;
                                        case "tabpersonen":
                                            if (fieldName == "nperskassanr")
                                            {
                                                curCustKassa = fieldValue;
                                            }
                                            if (fieldName == "npersnr")
                                            {
                                                curCustNr = fieldValue;
                                            }
                                            break;
                                        case "tabadressen":
                                            if (fieldName == "nadrkassanr")
                                            {
                                                curAddrKass = fieldValue;
                                            }
                                            if (fieldName == "nadrnr")
                                            {
                                                curAddrNr = fieldValue;
                                            }
                                            break;
                                        case "tabedeauftrag":           //nauftragnr
                                        case "taborderlist":
                                        case "taborderlistpos":
                                            if (fieldName == "norderlistnr")
                                            {
                                                OrderNrList = cf.AddToList(OrderNrList, fieldValue);
                                            }

                                            break;
                                    }
                                    if ((fieldName.Substring(0, 2) == "dt") && (fieldValue != "NULL"))
                                    {
                                        fieldValue = "'" + cf.TimestampConvert(Convert.ToDouble(fieldValue.Replace("'", ""))) + "'";
                                    }

                                    if (myFunction == "I")
                                    {
                                        tFields += fieldName + ",";
                                        tValues += fieldValue + ",";
                                    }
                                    else
                                    {
                                        tValues += fieldName + "=" + fieldValue + ",";
                                    }
                                }
                            }
                        }
                        if ((myFunction == "I") || (tWhere.Length != 0))
                        { 
                            switch (myFunction) //now build SQL statement 
                            {
                                case "I":   
                                    myQuery = $"INSERT INTO {QualifiedTableName} ({tFields}dtupdate) values ({tValues}'{dtUpdateStamp}')";
                                    break;
                                case "U":   
                                    myQuery = $"UPDATE {QualifiedTableName} SET {tValues}dtupdate='{dtUpdateStamp}' WHERE {tWhere}";
                                    break;
                                case "D":   
                                    myQuery = $"DELETE FROM {QualifiedTableName} WHERE {tWhere}";
                                    break;
                            }
                            if (cf.ExecuteSQL(MirrorConn, myQuery)) //run query
                            {
                                switch (curTable)    //deal with merged tables
                                {
                                    case "tabpersonen":
                                        MergeCustAddTables(curCustKassa.Replace("'", "") + '-' + curCustNr.Replace("'", ""), false);
                                        break;
                                    case "tabadressen":
                                        MergeCustAddTables(curAddrKass.Replace("'", "") + '-' + curAddrNr.Replace("'", ""), true);
                                        break;
                                    case "tabstorepromptdetail":
                                        MergePromptData(pKassa, string.Empty, pZaehler, pPrompt, pElement);
                                        break;
                                }
                            }
                        }
                        UpdateLog(curTable, curLogNo);
                    }
                }
                catch (MySqlException ex)
                {
                    myError.UpdateErrorLog("UpdateTables", ex.Number, ex.Message, curLogNo.ToString());
                    curLogNo -= 1;
                    goto cleanout;
                }
                UpdateLog("AxessMirror", curLogNo);
            }
            
cleanout:
            if (AxessSource.ErrorNo == 1401)    //no data found inside this range (Axess_CRM error)
            {
                AxessSource.ErrorNo = 0;
            }
            if (OrderNrList != string.Empty)
            {
                UpdateTabAXOrderData(OrderNrList);
            }
            return curLogNo - 1;
        }

        public bool InitTable(string tableName, long startNo = 1, long noRows = 50000, bool repair = false)
        {
            if (!AxessSource.Login()) return false; //can't log in so return false to calling program
            if (!repair)
            {
                if (!DeleteTable(tableName)) return false; //delete all records in table.
            }
            long totRecs = 0;   //total number or records in this snapshot
            long loaded = 0;    //total number of records loaded so far from table
            do                  //loops until AxessSoure.IncompleteSnapshot is false, always does one pass
            {
                totRecs += AxessSource.SetEntitySnapshot(tableName + "[" + (startNo + loaded) + ".." + (startNo + loaded + noRows).ToString() + "]");  //add number of records returned to totRecs.
                long curRec = 0;    //current record read from this snapshot, starts at 1 
                while (((AxessSource.ErrorNo == 0) | AxessSource.IncompleteSnapshot) && (loaded < totRecs))     //loop while there are records left to process
                {
                    long batchSize = 1000;
                    if (totRecs - loaded - curRec < 1000) batchSize = totRecs - loaded;
                    XmlDocument localXML = AxessSource.GetSnapshotList(curRec + 1, batchSize);    //get a batch from the snapshot, currently limited to 1000 records but code allows infinite
                    foreach (XmlNode tNode in localXML.GetElementsByTagName("ACTLOGLINE").Item(0).ChildNodes)   //loop through each LogLine and copy it to memory, then to the mirror database
                    {
                        System.Diagnostics.Debug.Print((repair ? "Repair" : "Init") + " Tables (" + tableName + ") " + Convert.ToString(loaded) + " of " + totRecs.ToString() + (AxessSource.IncompleteSnapshot ? " [INCOMPLETE]" : ""));
                        curRec++;   //increment snapshot counter
                        string myFunction = tNode.SelectSingleNode("SZKIND").InnerText.ToUpper();   //Log function, I=Insert, U=Update, D=Delete
                        string QualifiedTableName = ActiveDatabase + "." + tNode.SelectSingleNode("SZTABNAME").InnerText.ToLower(); //Qualified table name is database name (dot) table name, needed for SQL query
                        string dtUpdateStamp = cf.TimestampConvert(Convert.ToDouble(tNode.SelectSingleNode("DTTIMESTAMP").InnerText));
                        string tFields = string.Empty;  //temp string to hold field values for SQL query
                        string tValues = string.Empty;  //temp string to hold data values for SQL query
                        string myQuery = string.Empty;  //temp string to hold SQL query
                        string pKassa = string.Empty;
                        string pTrans = string.Empty;
                        string pZaehler = string.Empty;
                        string pPrompt = string.Empty;
                        string pElement = string.Empty;

                        string persnr = string.Empty;
                        string perskassa = string.Empty;
                        string adrnr = string.Empty;
                        string adrkassa = string.Empty;
                        string orderlistnr = string.Empty;
                        string orderlistpos = string.Empty;
                        XmlNode tXMLNode = tNode.SelectSingleNode("ACTLOGVALUES" + ((myFunction == "D") ? "OLD" : "NEW"));  //load the correct values based on function
                        foreach (XmlNode lNode in tXMLNode) //loop through all values in the row and add them to the correct temp strings
                        {
                            string fieldName = lNode.SelectSingleNode("SZFIELDNAME").InnerText.ToLower();  //get field name
                            string fieldValue = (lNode.SelectSingleNode("SZFIELDVALUE").InnerText.Length != 0) ? "'" + cf.EscapeChar(lNode.SelectSingleNode("SZFIELDVALUE").InnerText) + "'" : "NULL";  //get field value and format for mySQL
                            if (fieldName != "dtupdate")
                            {
                                switch (fieldName)
                                {
                                    case "npersnr":
                                        persnr = fieldValue;
                                        break;
                                    case "nperskassanr":
                                        perskassa = fieldValue;
                                        break;
                                    case "nadrnr":
                                        adrnr = fieldValue;
                                        break;
                                    case "nadrkassanr":
                                        adrkassa = fieldValue;
                                        break;
                                    case "nkassanr":
                                        pKassa = fieldValue;
                                        break;
                                    case "ntransnr":
                                        pTrans = fieldValue;
                                        break;
                                    case "npromptzaehlernr":
                                        pZaehler = fieldValue;
                                        break;
                                    case "npromptnr":
                                        pPrompt = fieldValue;
                                        break;
                                    case "npromptelementnr":
                                        pElement = fieldValue;
                                        break;
                                    case "norderlistnr":
                                        orderlistnr = fieldValue;
                                        break;
                                    case "norderlistpos":
                                        orderlistpos = fieldValue;
                                        break;
                                }
                                if ((fieldName.Substring(0, 2) == "dt") && (fieldValue != "NULL"))
                                {
                                    fieldValue = "'" + cf.TimestampConvert(Convert.ToDouble(fieldValue.Replace("'", ""))) + "'";
                                }

                                switch (myFunction)     //build SQL query values based on function
                                {
                                    case "I":   //for insert
                                        tFields += fieldName + ",";
                                        tValues += fieldValue + ",";
                                        break;
                                    case "U":   //for update
                                        tFields += fieldName + "=" + fieldValue + ",";
                                        break;
                                    case "D":   //TODO for delete
                                                //either delete based on all fields being the same or do a switch based on table name and delete based on primary key
                                        break;
                                    default:
                                        myError.UpdateErrorLog("Mirror.InitTable", -9999, "Unknown myFunction value: " + myFunction + "  (" + curRec + ")");
                                        return false;
                                }
                            }
                        }
                        bool skip = false;
                        switch (tableName)
                        {
                            case "tabpersonen":
                                skip = (cf.RowExists(MirrorConn, QualifiedTableName, $"nperskassanr={perskassa} AND npersnr={persnr}"));
                                break;
                            case "tabadressen":
                                skip = (cf.RowExists(MirrorConn, QualifiedTableName, $"nadrkassanr={adrkassa} AND nadrnr={adrnr}"));
                                break;
                            case "taborderlist":
                                skip = (cf.RowExists(MirrorConn, QualifiedTableName, $"norderlistnr={orderlistnr}"));
                                break;
                            case "taborderlistpos":
                                skip = (cf.RowExists(MirrorConn, QualifiedTableName, $"norderlistnr={orderlistnr} AND norderlistpos={orderlistpos}"));
                                break;
                        }
                        if (!(skip && repair))
                        {
                            switch (myFunction) //now build SQL statement 
                            {
                                case "I":   //for insert
                                    myQuery = $"INSERT INTO {QualifiedTableName} ({tFields} dtupdate) values ({tValues} '{dtUpdateStamp}')";
                                    break;
                                case "U":   //for update
                                    //myQuery = $"UPDATE {QualifiedTableName} SET {tFields} dtupdate='{dtUpdateStamp}' WHERE ";
                                    break;
                                case "D":   //TODO for delete
                                    //myQuery = $"DELETE FROM {QualifiedTablename} WHERE ";
                                    break;
                            }
                            cf.ExecuteSQL(MirrorConn, myQuery);    //run the query to proccess single record
                            switch (tableName)
                            {
                                case "tabpersonen":
                                    MergeCustAddTables(perskassa.Replace("'", "") + '-' + persnr.Replace("'", ""), false);
                                    break;
                                case "tabadressen":
                                    MergeCustAddTables(adrkassa.Replace("'", "") + '-' + adrnr.Replace("'", ""), true);
                                    break;
                                case "tabstorepromptdetail":
                                    MergePromptData(pKassa, pTrans, pZaehler, pPrompt, pElement);
                                    break;
                            }
                        }
                        loaded++;  //increment table counter
                    }
                }
            }
            while (AxessSource.IncompleteSnapshot); //repeat until last snapshot
            UpdateLog(tableName, AxessSource.LogNoStart);
            return (true);  //log out
        }

        public void UpdateLog(string aTableName, long aLogNo, bool aMirror = false)
        {
            string TableList = "'" + aTableName + "'";
            if (aMirror)
            {
                TableList += ",'AxessMirror'";
            }
            //does tablename already exist?
            if (cf.RowExists(MirrorConn, ActiveDatabase + ".tabLog", "TableName='" + aTableName + "'"))
            {
                cf.ExecuteSQL(MirrorConn, $"UPDATE {ActiveDatabase}.tabLog SET LogNo='{aLogNo}',lastUpdate='{DateTime.Now.ToString(Mirror.AxessDateTimeFormat)}' WHERE TableName IN ({TableList})");
            }
            else
            {       //no record, so insert to create one
                cf.ExecuteSQL(MirrorConn, $"INSERT INTO {ActiveDatabase}.tabLog (TableName, LogNo, lastUpdate) VALUES('{aTableName}','{aLogNo}', '{DateTime.Now}')");
            }
        }

        public string GetCustKey(string aLastName, string aFirstName, DateTime? aDateOfBirth)  //returns single CustKey (short ex: 20-15334)
        {
            DataSet myDS = cf.LoadDataSet(MirrorConn, $"SELECT concat_ws('-' ,nperskassa, npersnr) AS custkey, lastname, firstname, email, city, state FROM {ActiveDatabase}.tabaxcust WHERE (activeflag <> 0) AND {Mirror.AllAltaPOSFilter.Replace("POSFilter", "nperskassa")} AND (lastname='{cf.EscapeChar(aLastName)}' AND firstname='{cf.EscapeChar(aFirstName)}' AND dob='{aDateOfBirth.Value.ToString(Mirror.AxessDateFormat)}')", new DataSet(), "Customers");
            //return all matching values
            long z = myDS.Tables["Customers"].Rows.Count;
            switch (z)
            {
                case 0:
                    return null;
                case 1:
                    return myDS.Tables["Customers"].Rows[0].Field<string>("custkey");
                default:
                    foreach (DataRow myRow in myDS.Tables["Customers"].Rows)
                    {
                        if (aFirstName == myRow.Field<string>("firstname") && aLastName == myRow.Field<string>("lastname")) return myDS.Tables["Customers"].Rows[0].Field<string>("custkey");
                    }
                    return null;
            }
            //if only one row returned, keep it and hope you got the right one.
            //if more than one row look for matching case on names.
            //  otherwise look at city and email to try to determine.
        }

        public void FixPromptData()
        {
            DataSet tDS = cf.LoadDataSet(MirrorConn, "(SELECT recid FROM " + ActiveDatabase + ".tabpromptdata AS A WHERE (SELECT COUNT(*) FROM " + ActiveDatabase + ".tabpromptdata AS B WHERE A.nkassanr = B.nkassanr AND A.ntransnr = B.ntransnr AND A.npromptzaehlernr = B.npromptzaehlernr AND A.npromptnr = B.npromptnr AND A.npromptelementnr = B.npromptelementnr) > 1 AND recID > (SELECT MIN(recid) FROM " + ActiveDatabase + ".tabpromptdata AS C WHERE A.nkassanr = C.nkassanr AND A.ntransnr = C.ntransnr AND A.npromptzaehlernr = C.npromptzaehlernr AND A.npromptnr = C.npromptnr AND A.npromptelementnr = C.npromptelementnr))", new DataSet(), "PromptData");
            foreach (DataRow myRow in tDS.Tables["PromptData"].Rows)
            {
                cf.ExecuteSQL(MirrorConn, "DELETE FROM " + ActiveDatabase + ".tabpromptdata WHERE recid = " + myRow.Field<Int32>("recid").ToString());
            }
        }

        public void FixPersonen()
        {
            //start with init personen routine, but don't initialize
            if (!AxessSource.Login()) return; //can't log in so return false to calling program
            long totRecs = 0;   //total number or records in this snapshot
            long loaded = 0;    //total number of records loaded so far from table
            long startNo = 1;
            long noRows = 50000;
            do                  //loops until AxessSoure.IncompleteSnapshot is false, always does one pass
            {
                totRecs += AxessSource.SetEntitySnapshot("tabpersonen[" + (startNo + loaded) + ".." + (startNo + loaded + noRows).ToString() + "]");  //add number of records returned to totRecs.
                long curRec = 0;    //current record read from this snapshot, starts at 1 
                while (((AxessSource.ErrorNo == 0) | AxessSource.IncompleteSnapshot) && (loaded < totRecs))     //loop while there are records left to process
                {
                    long batchSize = 1000;
                    if (totRecs - loaded - curRec < 1000) batchSize = totRecs - loaded;
                    XmlDocument localXML = AxessSource.GetSnapshotList(curRec + 1, batchSize);    //get a batch from the snapshot, currently limited to 1000 records but code allows infinite
                    foreach (XmlNode tNode in localXML.GetElementsByTagName("ACTLOGLINE").Item(0).ChildNodes)   //loop through each LogLine and copy it to memory, then to the mirror database
                    {
                        curRec++;   //increment snapshot counter
                        string myFunction = tNode.SelectSingleNode("SZKIND").InnerText.ToUpper();   //Log function, I=Insert, U=Update, D=Delete
                        string QualifiedTableName = ActiveDatabase + "." + tNode.SelectSingleNode("SZTABNAME").InnerText.ToLower(); //Qualified table name is database name (dot) table name, needed for SQL query
                        string dtUpdateStamp = cf.TimestampConvert(Convert.ToDouble(tNode.SelectSingleNode("DTTIMESTAMP").InnerText));
                        string tFields = string.Empty;  //temp string to hold field values for SQL query
                        string tValues = string.Empty;  //temp string to hold data values for SQL query
                        string myQuery = string.Empty;  //temp string to hold SQL query
                        string pKassa = string.Empty;
                        string pTrans = string.Empty;
                        string pZaehler = string.Empty;
                        string pPrompt = string.Empty;
                        string pElement = string.Empty;

                        string persnr = string.Empty;
                        string perskassa = string.Empty;
                        string adrnr = string.Empty;
                        string adrkassa = string.Empty;
                        XmlNode tXMLNode = tNode.SelectSingleNode("ACTLOGVALUES" + ((myFunction == "D") ? "OLD" : "NEW"));  //load the correct values based on function
                        foreach (XmlNode lNode in tXMLNode) //loop through all values in the row and add them to the correct temp strings
                        {
                            string fieldName = lNode.SelectSingleNode("SZFIELDNAME").InnerText.ToLower();  //get field name
                            string fieldValue = (lNode.SelectSingleNode("SZFIELDVALUE").InnerText.Length != 0) ? "'" + cf.EscapeChar(lNode.SelectSingleNode("SZFIELDVALUE").InnerText) + "'" : "NULL";  //get field value and format for mySQL
                            if ((fieldName.Substring(0, 2) == "dt") && (fieldValue != "NULL")) fieldValue = "'" + cf.TimestampConvert(Convert.ToDouble(fieldValue.Replace("'", ""))) + "'";
                            if (fieldName != "dtupdate")
                            {
                                switch (fieldName)
                                {
                                    case "npersnr":
                                        persnr = fieldValue;
                                        break;
                                    case "nperskassanr":
                                        perskassa = fieldValue;
                                        break;
                                    case "nadrnr":
                                        adrnr = fieldValue;
                                        break;
                                    case "nadrkassanr":
                                        adrkassa = fieldValue;
                                        break;
                                    case "nkassanr":
                                        pKassa = fieldValue;
                                        break;
                                    case "ntransnr":
                                        pTrans = fieldValue;
                                        break;
                                    case "npromptzaehlernr":
                                        pZaehler = fieldValue;
                                        break;
                                    case "npromptnr":
                                        pPrompt = fieldValue;
                                        break;
                                    case "npromptelementnr":
                                        pElement = fieldValue;
                                        break;
                                }
                            }
                        }
                        myQuery = "INSERT INTO " + QualifiedTableName + " (" + tFields + "dtupdate) values (" + tValues + "'" + dtUpdateStamp + "')";
                    }
                }

            }
            while (AxessSource.IncompleteSnapshot); //repeat until last snapshot
            return;
            //InitTable("");
        }

        public void FixOrderDuplicates()
        {
            //tabpersonnen
            DataTable tDT = cf.LoadTable(MirrorConn, $"SELECT * FROM {ActiveDatabase}.tabpersonen AS A WHERE (SELECT COUNT(*) FROM {ActiveDatabase}.tabpersonen AS B WHERE B.NPERSKASSANR = A.NPERSKASSANR AND B.NPERSNR = A.NPERSNR) > 1 GROUP BY A.NPERSKASSANR, A.NPERSNR", "tabpersonen");
            string tQuery = string.Empty;
            foreach (DataRow tDR in tDT.Rows)
            {
                cf.ExecuteSQL(MirrorConn, $"DELETE FROM {ActiveDatabase}.tabpersonen WHERE nperskassanr={tDR["nperskassanr"].ToString()} AND npersnr={tDR["npersnr"].ToString()}");
                tQuery = "INSERT INTO axess_cwc.tabpersonen (NPERSPROJNR,NPERSKASSANR,NPERSNR,SZNAME,SZVORNAME,SZGESCHLECHT,DTGEBURT,NPERSTYPNR,SZTITEL,SZANREDE,SZMOBILNR,SZEMAIL,NFAMPROJNR,NFAMKASSANR,NFAMPERSNR,NFAMSORTNR,NADRPROJNR,NADRKASSANR,NADRNR,NADRTYPNR,NSPRACHID,DTUPDATE,NBMPPROJNR,NBMPKASSANR,NBMPNR,NTEXTPROJNR,NTEXTKASSANR,NTEXTNR,NTARIFBLATTPROJNR,NTARIFBLATTNR,SZMEMO,NREFPERSPROJNR,NREFPERSKASSANR,NREFPERSNR,BPERSAKTIV,SZWTPLOGINID,SZWTPPASSWORD,NWTPPROFILNR,BBEZARTLS,BBEZARTEC,BBEZARTKK,SZMATCHCODE,BBEZARTLSE,SZCRSID,SZCRSDATEN,SZEXTBESUCHERID,SZMIDDLENAME,SZPASSPORTID,SZPASSPORTISSUERNAME,DTPASSPORTISSUEDATE,fheight,nheightunit,fweight,nweightunit,fshoesize,nshoesizeunit,fsolelength,nsolelengthunit,nskilevel,szinformation1,szinformation2,szinformation3,szmessage) ";
                tQuery += $" VALUES ('{tDR["NPERSPROJNR"].ToString()}','{tDR["NPERSKASSANR"].ToString()}','{tDR["NPERSNR"].ToString()}','{tDR["SZNAME"].ToString()}','{tDR["SZVORNAME"].ToString()}','{tDR["SZGESCHLECHT"].ToString()}','{tDR.Field<DateTime>("DTGEBURT").ToString()}','{tDR["NPERSTYPNR"].ToString()}','{tDR["SZTITEL"].ToString()}','{tDR["SZANREDE"].ToString()}','{tDR["SZMOBILNR"].ToString()}','{tDR["SZEMAIL"].ToString()}','{tDR["NFAMPROJNR"].ToString()}','{tDR["NFAMKASSANR"].ToString()}','{tDR["NFAMPERSNR"].ToString()}','{tDR["NFAMSORTNR"].ToString()}','{tDR["NADRPROJNR"].ToString()}','{tDR["NADRKASSANR"].ToString()}','{tDR["NADRNR"].ToString()}','{tDR["NADRTYPNR"].ToString()}','{tDR["NSPRACHID"].ToString()}','{tDR.Field<DateTime>("DTUPDATE").ToString()}','{tDR["NBMPPROJNR"].ToString()}','{tDR["NBMPKASSANR"].ToString()}','{tDR["NBMPNR"].ToString()}','{tDR["NTEXTPROJNR"].ToString()}','{tDR["NTEXTKASSANR"].ToString()}','{tDR["NTEXTNR"].ToString()}','{tDR["NTARIFBLATTPROJNR"].ToString()}','{tDR["NTARIFBLATTNR"].ToString()}','{tDR["SZMEMO"].ToString()}','{tDR["NREFPERSPROJNR"].ToString()}','{tDR["NREFPERSKASSANR"].ToString()}','{tDR["NREFPERSNR"].ToString()}','{tDR["BPERSAKTIV"].ToString()}','{tDR["SZWTPLOGINID"].ToString()}','{tDR["SZWTPPASSWORD"].ToString()}','{tDR["NWTPPROFILNR"].ToString()}','{tDR["BBEZARTLS"].ToString()}','{tDR["BBEZARTEC"].ToString()}','{tDR["BBEZARTKK"].ToString()}','{tDR["SZMATCHCODE"].ToString()}','{tDR["BBEZARTLSE"].ToString()}','{tDR["SZCRSID"].ToString()}','{tDR["SZCRSDATEN"].ToString()}','{tDR["SZEXTBESUCHERID"].ToString()}','{tDR["SZMIDDLENAME"].ToString()}','{tDR["SZPASSPORTID"].ToString()}','{tDR["SZPASSPORTISSUERNAME"].ToString()}','{tDR.Field<DateTime>("DTPASSPORTISSUEDATE").ToString(AxessDateTimeFormat)}','{tDR["fheight"].ToString()}','{tDR["nheightunit"].ToString()}','{tDR["fweight"].ToString()}','{tDR["nweightunit"].ToString()}','{tDR["fshoesize"].ToString()}','{tDR["nshoesizeunit"].ToString()}','{tDR["fsolelength"].ToString()}','{tDR["nsolelengthunit"].ToString()}','{tDR["nskilevel"].ToString()}',{tDR["szinformation1"].ToString()}',{tDR["szinformation2"].ToString()}',{tDR["szinformation3"].ToString()}',{tDR["szmessage"].ToString()}')";
                cf.ExecuteSQL(MirrorConn, tQuery);
            }
            //tabadressen
            tDT = cf.LoadTable(MirrorConn, $"SELECT * FROM {ActiveDatabase}.tabadressen AS A (SELECT COUNT(*) FROM {ActiveDatabase}.tabadressen AS B WHERE B.NADRKASSANR=A.NADRKASSANR AND B.NADRNR=A.NADRNR) > 1 GROUP BY NADRKASSANR, NADRNR", "tabadressen");
            tQuery = string.Empty;
            foreach (DataRow tDR in tDT.Rows)
            {
                cf.ExecuteSQL(MirrorConn, $"DELETE FROM {ActiveDatabase}.tabadressen WHERE nadrkassanr={tDR["nadrkassanr"].ToString()} AND nadrnr={tDR["nadrnr"].ToString()}");
                tQuery = "INSERT INTO axess_cwc.tabadressen (NADRPROJNR,NADRKASSANR,NADRNR,SZSTRASSE,SZPLZ,SZORT,SZSTAATKURZNAME,SZTELNR,SZFAXNR,SZBESCH,DTUPDATE,SZPROVINZKURZNAME,SZSTRASSE2,SZSTRASSE3,SZCO) ";
                tQuery += $" VALUES ('{tDR["NADRPROJNR"].ToString()}','{tDR["NADRKASSANR"].ToString()}','{tDR["NADRNR"].ToString()}','{tDR["SZSTRASSE"].ToString()}','{tDR["SZPLZ"].ToString()}','{tDR["SZORT"].ToString()}','{tDR["SZSTAATKURZNAME"].ToString()}','{tDR["SZTELNR"].ToString()}','{tDR["SZFAXNR"].ToString()}','{tDR["SZBESCH"].ToString()}','{tDR.Field<DateTime>("DTUPDATE").ToString(AxessDateTimeFormat)}','{tDR["SZPROVINZKURZNAME"].ToString()}','{tDR["SZSTRASSE2"].ToString()}','{tDR["SZSTRASSE3"].ToString()}','{tDR["SZCO"].ToString()}')";
                cf.ExecuteSQL(MirrorConn, tQuery);
            }
            //taborderlist
            tDT = cf.LoadTable(MirrorConn, $"SELECT * FROM {ActiveDatabase}.taborderlist AS A (SELECT COUNT(*) FROM {ActiveDatabase}.taborderlist AS B WHERE B.norderlistnr = A.norderlistnr) > 1 GROUP BY A.norderlistnr)", "taborderlist");
            tQuery = string.Empty;
            foreach (DataRow tDR in tDT.Rows)
            {
                cf.ExecuteSQL(MirrorConn, $"DELETE FROM {ActiveDatabase}.taborderlist WHERE norderlistnr={tDR["norderlistnr"].ToString()}");
                tQuery = "INSERT INTO axess_cwc.taborderlist (nprojnr,norderlistnr,nfirmenprojnr,nfirmenkassanr,nfirmennr,npersprojnr,nperskassanr,npersnr,ngrundbezartnr,nkundenbezartnr,nkassaprojnr,nkassanr,ntransnr,norderstatusnr,dtupdate,nordertypnr,nswitchprojnr,nswitchsubprojnr,nswitchgesnr,ndeliveryadrprojnr,ndeliveryadrkassanr,ndeliveryadrnr,ndeliveryadrtypnr,nempffirmenprojnr,nempffirmenkassanr,nempffirmennr,nempfpersprojnr,nempfperskassanr,nempfpersnr,szextorderreference,nnamedusernr) ";
                tQuery += $" VALUES ('{tDR["NPROJNR"].ToString()}','{tDR["NORDERLISTNR"].ToString()}','{tDR["NFIRMENPROJNR"].ToString()}','{tDR["NFIRMENKASSANR"].ToString()}','{tDR["NFIRMENNR"].ToString()}','{tDR["npersprojnr"].ToString()}','{tDR["nperskassanr"].ToString()}','{tDR["npersnr"].ToString()}','{tDR["ngrundbezartnr"].ToString()}','{tDR["nkundenbezartnr"].ToString()}','{tDR["nkassaprojnr"].ToString()}','{tDR["nkassanr"].ToString()}','{tDR["ntransnr"].ToString()}','{tDR["norderstatusnr"].ToString()}','{tDR.Field<DateTime>("dtupdate").ToString(AxessDateTimeFormat)}','{tDR["norderstatusnr"].ToString()}','{tDR["nordertypnr"].ToString()}','{tDR["nswitchprojnr"].ToString()}','{tDR["nswitchsubprojnr"].ToString()}','{tDR["nswitchgesnr"].ToString()}','{tDR["ndeliveryadrprojnr"].ToString()}','{tDR["ndeliveryadrkassanr"].ToString()}','{tDR["ndeliveryadrnr"].ToString()}','{tDR["ndeliveryadrtypnr"].ToString()}','{tDR["nempffirmenprojnr"].ToString()}','{tDR["nempffirmenkassanr"].ToString()}','{tDR["nempffirmennr"].ToString()}','{tDR["nempfpersprojnr"].ToString()}','{tDR["nempfperskassanr"].ToString()}','{tDR["nempfpersnr"].ToString()}','{tDR["szextorderreference"].ToString()}','{tDR["nnamedusernr"].ToString()}')"; 
                cf.ExecuteSQL(MirrorConn, tQuery);
            }
            //taborderlistpos
            tDT = cf.LoadTable(MirrorConn, $"SELECT * FROM {ActiveDatabase}.taborderlistpos AS A (SELECT COUNT(*) FROM {ActiveDatabase}.taborderlistpos AS B WHERE B.norderlistnr = A.norderlistnr AND B.norderlistpos=A.norderlistpos) > 1 GROUP BY NORDERLISTNR, norderlistpos)", "taborderlistpos");
            tQuery = string.Empty;
            foreach (DataRow tDR in tDT.Rows)
            {
                cf.ExecuteSQL(MirrorConn, $"DELETE FROM {ActiveDatabase}.taborderlistpos WHERE norderlistnr={tDR["norderlistnr"].ToString()} AND norderlistpos={tDR["norderlistpos"].ToString()}");
                tQuery = "INSERT INTO axess_cwc.taborderlistpos (nprojnr,norderlistnr,norderlistpos,nkundenkartentypnr,nperstypnr,npoolnr,nartikelnr,feinzeltarif,neingegsortstk,dtgiltab,npersprojnr,nperskassanr,npersnr,bignorefoto,nrohlingstypnr,ndattraegtypnr,bwtp,nkassaprojnr,nkassanr,ntransnr,nblocknr,nkartennr,dtupdate) ";
                tQuery += $" VALUES ('{tDR["nprojnr"].ToString()}','{tDR["norderlistnr"].ToString()}','{tDR["norderlistpos"].ToString()}','{tDR["nkundenkartentypnr"].ToString()}','{tDR["nperstypnr"].ToString()}','{tDR["npoolnr"].ToString()}','{tDR["nartikelnr"].ToString()}','{tDR["feinzeltarif"].ToString()}','{tDR["neingegsortstk"].ToString()}','{tDR.Field<DateTime>("dtgiltab").ToString(AxessDateTimeFormat)}','{tDR["npersprojnr"].ToString()}','{tDR["nperskassanr"].ToString()}','{tDR["npersnr"].ToString()}','{tDR["bignorefoto"].ToString()}','{tDR["nrohlingstypnr"].ToString()}','{tDR["ndattraegtypnr"].ToString()}','{tDR["bwtp"].ToString()}','{tDR["nkassaprojnr"].ToString()}','{tDR["nkassanr"].ToString()}','{tDR["ntransnr"].ToString()}','{tDR["nblocknr"].ToString()}','{tDR["nkartennr"].ToString()}','{tDR.Field<DateTime>("dtupdate").ToString(AxessDateTimeFormat)}')";  
                cf.ExecuteSQL(MirrorConn, tQuery);
            }
            //tabbezahlart
            tDT = cf.LoadTable(MirrorConn, $"SELECT * FROM {ActiveDatabase}.tabbezahlart AS A (SELECT COUNT(*) FROM {ActiveDatabase}.tabbezahlart AS B WHERE B.NPROJNR=705 AND B.NKASSANR=A.NKASSANR AND B.NTRANSNR=A.NTRANSNR AND B.NLFDNR=A.NLFDNR) > 1 GROUP BY ntransnr, NKASSANR, NLFDNR", "tabadressen");
            tQuery = string.Empty;
            foreach (DataRow tDR in tDT.Rows)
            {
                cf.ExecuteSQL(MirrorConn, $"DELETE FROM {ActiveDatabase}.tabadressen WHERE nadrkassanr={tDR["nadrkassanr"].ToString()} AND nadrnr={tDR["nadrnr"].ToString()}");
                tQuery = "INSERT INTO axess_cwc.tabadressen (NADRPROJNR,NADRKASSANR,NADRNR,SZSTRASSE,SZPLZ,SZORT,SZSTAATKURZNAME,SZTELNR,SZFAXNR,SZBESCH,DTUPDATE,SZPROVINZKURZNAME,SZSTRASSE2,SZSTRASSE3,SZCO) ";
                tQuery += $" VALUES ('{tDR["NADRPROJNR"].ToString()}','{tDR["NADRKASSANR"].ToString()}','{tDR["NADRNR"].ToString()}','{tDR["SZSTRASSE"].ToString()}','{tDR["SZPLZ"].ToString()}','{tDR["SZORT"].ToString()}','{tDR["SZSTAATKURZNAME"].ToString()}','{tDR["SZTELNR"].ToString()}','{tDR["SZFAXNR"].ToString()}','{tDR["SZBESCH"].ToString()}','{tDR.Field<DateTime>("DTUPDATE").ToString(AxessDateTimeFormat)}','{tDR["SZPROVINZKURZNAME"].ToString()}','{tDR["SZSTRASSE2"].ToString()}','{tDR["SZSTRASSE3"].ToString()}','{tDR["SZCO"].ToString()}')";
                cf.ExecuteSQL(MirrorConn, tQuery);
            }
            //tabedeauftrag
            tDT = cf.LoadTable(MirrorConn, $"SELECT * FROM {ActiveDatabase}.tabadressen AS A (SELECT COUNT(*) FROM {ActiveDatabase}.tabadressen AS B WHERE B.NADRKASSANR=A.NADRKASSANR AND B.NADRNR=A.NADRNR) > 1 GROUP BY concat_ws('-',NADRKASSANR, NADRNR)", "tabadressen");
            tQuery = string.Empty;
            foreach (DataRow tDR in tDT.Rows)
            {
                cf.ExecuteSQL(MirrorConn, $"DELETE FROM {ActiveDatabase}.tabadressen WHERE nadrkassanr={tDR["nadrkassanr"].ToString()} AND nadrnr={tDR["nadrnr"].ToString()}");
                tQuery = "INSERT INTO axess_cwc.tabadressen (NADRPROJNR,NADRKASSANR,NADRNR,SZSTRASSE,SZPLZ,SZORT,SZSTAATKURZNAME,SZTELNR,SZFAXNR,SZBESCH,DTUPDATE,SZPROVINZKURZNAME,SZSTRASSE2,SZSTRASSE3,SZCO) ";
                tQuery += $" VALUES ('{tDR["NADRPROJNR"].ToString()}','{tDR["NADRKASSANR"].ToString()}','{tDR["NADRNR"].ToString()}','{tDR["SZSTRASSE"].ToString()}','{tDR["SZPLZ"].ToString()}','{tDR["SZORT"].ToString()}','{tDR["SZSTAATKURZNAME"].ToString()}','{tDR["SZTELNR"].ToString()}','{tDR["SZFAXNR"].ToString()}','{tDR["SZBESCH"].ToString()}','{tDR.Field<DateTime>("DTUPDATE").ToString(AxessDateTimeFormat)}','{tDR["SZPROVINZKURZNAME"].ToString()}','{tDR["SZSTRASSE2"].ToString()}','{tDR["SZSTRASSE3"].ToString()}','{tDR["SZCO"].ToString()}')";
                cf.ExecuteSQL(MirrorConn, tQuery);
            }
        }

        public void FixAXCust() //long running, goes through entire Axess customer database and inserts any that are missing, ignores updates.
        {
            DataSet tDS = cf.LoadDataSet(MirrorConn, "SELECT DISTINCT concat_ws('-', NPERSKASSANR, NPERSNR) AS perskey FROM " + ActiveDatabase + ".tabpersonen WHERE DATE(dtupdate)='2017-03-08)'", new DataSet(), "Cust");
            foreach (DataRow tRow in tDS.Tables["Cust"].Rows)
            {
                MergeCustAddTables(tRow.Field<string>("perskey"));
            }
        }
    }
}

//bool isComplete = false;
//long lCount = 0;
//foreach (XmlNode toNode in oXMLNode)    //loop through the old values and get the primary key values based on table
//{
//    string tofName = toNode.SelectSingleNode("SZFIELDNAME").InnerText.ToLower();  //get field name
//    string tofValue = (toNode.SelectSingleNode("SZFIELDVALUE").InnerText.Length != 0) ? "'" + cf.EscapeChar(toNode.SelectSingleNode("SZFIELDVALUE").InnerText) + "'" : "NULL";  //get field value and format for mySQL
//    switch (curTable)
//    {
//        case "tabkundenkartentypdef":   //nkundenkartentypnr
//            if (tofName == "nkundenkartentypnr")
//            {
//                tWhere = tofName + "=" + tofValue;
//                isComplete = true;
//            }
//            break;
//        case "tabperstypdef":           //nperstypnr
//            if (tofName == "nperstypnr")
//            {
//                tWhere = tofName + "=" + tofValue;
//                isComplete = true;
//            }
//            break;
//        case "tabartikeldef":           //nartikelnr
//            if (tofName == "nartikelnr")
//            {
//                tWhere = tofName + "=" + tofValue;
//                isComplete = true;
//            }
//            break;
//        case "tabkassierkonf":          //nprojnr,nkassierid,
//            switch (tofName)
//            {
//                case "nprojnr":
//                case "nkassierid":
//                    if (tWhere.Length != 0) tWhere += " AND ";
//                    tWhere += "(" + tofName + "=" + tofValue + ")";
//                    isComplete = (++lCount == 2);
//                    break;
//            }
//            break;
//        case "tabfirmentypdef":         //nfirmentypnr
//            if (tofName == "nfirmentypnr")
//            {
//                tWhere = tofName + "=" + tofValue;
//                isComplete = true;
//            }
//            break;
//        case "tabdattraegtypdef":       //ndattraegtypnr
//            if (tofName == "ndattraegtypnr")
//            {
//                tWhere = tofName + "=" + tofValue;
//                isComplete = true;
//            }
//            break;
//        case "tabkundenbezahlartendef": //ngrundebezartnr,nkundenbezartnr
//            switch (tofName)
//            {
//                case "ngrundebezartnr":
//                case "nkundenbezartnr":
//                    if (tWhere.Length != 0) tWhere += " AND ";
//                    tWhere += "(" + tofName + "=" + tofValue + ")";
//                    isComplete = (++lCount == 2);
//                    break;
//            }
//            break;
//        case "tabpooldef":              //nprojnr,npoolnr
//            switch (tofName)
//            {
//                case "nprojnr":
//                case "npoolnr":
//                    if (tWhere.Length != 0) tWhere += " AND ";
//                    tWhere += "(" + tofName + "=" + tofValue + ")";
//                    isComplete = (+lCount == 2);
//                    break;
//            }
//            break;
//        case "tabpromptdef":            //npromptnr
//            if (tofName == "npromptnr")
//            {
//                tWhere = tofName + "=" + tofValue;
//                isComplete = true;
//            }
//            break;
//        case "tabpromptelementedef":    //npromptnr,npromptelemntnr,ndlgelementnr
//            switch (tofName)
//            {
//                case "npromptnr":
//                case "npromptelementnr":
//                case "ndlgelementnr":
//                    if (tWhere.Length != 0) tWhere += " AND ";
//                    tWhere += "(" + tofName + "=" + tofValue + ")";
//                    isComplete = (++lCount == 3);
//                    break;
//            }
//            break;
//        case "tabrohlingstypdef":       //nrohlingstypnr
//            if (tofName == "nrohlingstypnr")
//            {
//                tWhere = tofName + "=" + tofValue;
//                isComplete = true;
//            }
//            break;
//        case "tabtransstatusdef":       //ntransstatusnr
//            if (tofName == "ntransstatusnr")
//            {
//                tWhere = tofName + "=" + tofValue;
//                isComplete = true;
//            }
//            break;
//        case "tabzutrkonf":             //nprojnr,nzutrnr
//            switch (tofName)
//            {
//                case "nprojnr":
//                case "nzutrnr":
//                    if (tWhere.Length != 0) tWhere += " AND ";
//                    tWhere += "(" + tofName + "=" + tofValue + ")";
//                    isComplete = (++lCount == 2);
//                    break;
//            }
//            break;
//        case "tabberechttarife":        //nprojnr,ntarifblattnr,ntarifblattguelnr,nkundenkartentypnr,npoolnr,nlfdnr,nzeitztafnr,nperstypnr
//            switch (tofName)
//            {
//                case "ntarifnr":
//                    tWhere += "(" + tofName + "=" + tofValue + ")";
//                    isComplete = true;
//                    if (myFunction != "U")
//                    {
//                        StatusText("Hit!!!");
//                    }
//                    break;
//            }
//            break;
//        case "taborderlist":            //nprojnr,norderlistnr
//            switch (tofName)
//            {
//                case "norderlistnr":
//                    OrderNrList = cf.AddToList(OrderNrList, tofValue);
//                    if (tWhere.Length != 0) tWhere += " AND ";
//                    tWhere += "(" + tofName + "=" + tofValue + ")";
//                    isComplete = (++lCount == 2);
//                    break;

//                case "nprojnr":
//                    if (tWhere.Length != 0) tWhere += " AND ";
//                    tWhere += "(" + tofName + "=" + tofValue + ")";
//                    isComplete = (++lCount == 2);
//                    break;
//            }
//            break;
//        case "taborderlistpos":         //nprojnr,norderlistnr,norderlistpos
//            switch (tofName)
//            {
//                case "norderlistnr":
//                    OrderNrList = cf.AddToList(OrderNrList, tofValue);
//                    if (tWhere.Length != 0) tWhere += " AND ";
//                    tWhere += "(" + tofName + "=" + tofValue + ")";
//                    isComplete = (++lCount == 3);
//                    break;
//                case "nprojnr":
//                case "norderlistpos":
//                    if (tWhere.Length != 0) tWhere += " AND ";
//                    tWhere += "(" + tofName + "=" + tofValue + ")";
//                    isComplete = (++lCount == 3);
//                    break;
//            }
//            break;
//        case "tabprepaidtickets":       //nprojnr,nkassanr,njournalnr,ntransnr,nblocknr,nseriennr
//            switch (tofName)
//            {
//                case "nprojnr":
//                case "nkassanr":
//                case "njournalnr":
//                case "ntransnr":
//                case "nblocknr":
//                case "nseriennr":
//                    if (tWhere.Length != 0) tWhere += " AND ";
//                    tWhere += "(" + tofName + "=" + tofValue + ")";
//                    isComplete = (++lCount == 6);
//                    break;
//            }
//            break;
//        case "tabchipkarten":           //nchipkartenregid,szkartennr
//            switch (tofName)
//            {
//                case "nchipkartenregid":
//                case "szkartennr":
//                    if (tWhere.Length != 0) tWhere += " AND ";
//                    tWhere += "(" + tofName + "=" + tofValue + ")";
//                    isComplete = (++lCount == 2);
//                    break;
//            }
//            break;
//        case "tabstorepromptmaster":    //recid
//            if (tofName == "recid")
//            {
//                tWhere = tofName + "=" + tofValue;
//                isComplete = true;
//            }
//            break;
//        case "tabstorepromptdetail":    //nprojnr,nkassanr,npromptzaehlernr,npromptnr,npromptelementnr,nlfdelementnr
//            switch (tofName)
//            {
//                case "nprojnr":
//                case "nkassanr":
//                case "npromptzaehlernr":
//                case "npromptnr":
//                case "npromptelementnr":
//                case "nlfdelementnr":
//                    if (tWhere.Length != 0) tWhere += " AND ";
//                    tWhere += "(" + tofName + "=" + tofValue + ")";
//                    isComplete = (++lCount == 6);
//                    break;
//            }
//            break;
//        case "tabkartensperre":         //nprojnr,nkassanr,nseriennr
//            switch (tofName)
//            {
//                case "nprojnr":
//                case "nkassanr":
//                case "nseriennr":
//                    if (tWhere.Length != 0) tWhere += " AND ";
//                    tWhere += "(" + tofName + "=" + tofValue + ")";
//                    isComplete = (++lCount == 3);
//                    break;
//            }
//            break;
//        case "tabfirmen":               //nfirmenprojnr,nfirmenkassanr,nfirmennr
//            switch (tofName)
//            {
//                case "nfirmenprojnr":
//                case "nfirmenkassanr":
//                case "nfirmennr":
//                    if (tWhere.Length != 0) tWhere += " AND ";
//                    tWhere += "(" + tofName + "=" + tofValue + ")";
//                    isComplete = (++lCount == 3);
//                    break;
//            }
//            break;
//        case "tabpersonen":             //npersprojnr,nperskassanr,npersnr
//            switch (tofName)
//            {
//                case "npersprojnr":
//                case "nperskassanr":
//                case "npersnr":
//                    if (tWhere.Length != 0) tWhere += " AND ";
//                    tWhere += "(" + tofName + "=" + tofValue + ")";
//                    isComplete = (++lCount == 3);
//                    break;
//            }
//            break;
//        case "tabadressen":             //nadrprojnr, nadrkassanr, nadrnr
//            switch (tofName)
//            {
//                case "nadrprojnr":
//                case "nadrkassanr":
//                case "nadrnr":
//                    if (tWhere.Length != 0) tWhere += " AND ";
//                    tWhere += "(" + tofName + "=" + tofValue + ")";
//                    isComplete = (++lCount == 3);
//                    break;
//            }
//            break;
//        case "tabedeauftrag":           //nauftragnr
//            switch (tofName)
//            {
//                case "nauftragnr":
//                    tWhere = tofName + "=" + tofValue;
//                    isComplete = (++lCount == 1);
//                    break;
//                    //case "norderlistnr":
//                    //    if (tWhere.Length != 0) tWhere += " AND ";
//                    //    tWhere += "(" + tofName + "=" + tofValue + ")";
//                    //    isComplete = (++lCount == 2);
//                    //    break;
//            }
//            break;
//        case "tablesertrans":           //ncpunr,nlfdcputransnr,nverkprojnr,nkassanr,nseriennr
//            switch (tofName)
//            {
//                case "ncpunr":
//                case "nlfdcputransnr":
//                case "nverkprojnr":
//                case "nkassanr":
//                case "nseriennr":
//                    if (tWhere.Length != 0) tWhere += " AND ";
//                    tWhere += "(" + tofName + "=" + tofValue + ")";
//                    isComplete = (++lCount == 5);
//                    break;
//            }
//            break;
//    }
//    if (isComplete) break;
//}

