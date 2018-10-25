using System;
using System.Data;
using System.Diagnostics;

namespace asl_SyncLibrary
{
    public class AltaSync
    {
        public event Action<Tuple<string, int, int, int>> ProgressChanged;

        private bool Initialized = false;

        private Mirror am = new Mirror();
        private DataWarehouse dw = new DataWarehouse();
        private BUY_ALTA_COM buy = new BUY_ALTA_COM();
        //private string appsDatabase = "";
        //private string buyDatabase = "";

        private DataSet altasyncDS = new DataSet();

        private CommonFunctions cf = new CommonFunctions();
        private ASLError er = new ASLError();

        private const string mdatadir = @"g:\fpdata\altaax\";

        private DateTime xDate = DateTime.Now.AddHours(-1);
        private DateTime mlastpmttime = DateTime.Now;

        private string merprg = string.Empty;
        private string mlodgeccbill = string.Empty;
        private string srhandlet = string.Empty;
        private string mupdate = string.Empty;
        private string mstart = string.Empty;
        private string memailtest = string.Empty;
        private string mend = string.Empty;
        private string mxday1 = string.Empty;
        private string mxday2 = string.Empty;
        private string mskeytext = string.Empty;
        private string mtempdir = string.Empty;
        private string mspsales = string.Empty;
        private string mtsales = string.Empty;
        private string mccsales = string.Empty;
        private string mb2bsales = string.Empty;
        private string mtotsales = string.Empty;
        private string mcalcacc = string.Empty;
        private string mcalcexp = string.Empty;
        private string mtest = string.Empty;
        private string mcount = string.Empty;
        private string mctotal = string.Empty;
        private string mexcount = string.Empty;
        private string mapcount = string.Empty;
        private string msalesdetcount = string.Empty;
        private string mrefundflag = string.Empty;
        private string mtickexpbeg = string.Empty;
        private string mtickexpend = string.Empty;
        private string mbcode = string.Empty;
        private string mkey = string.Empty;
        private string mtkey = string.Empty;
        private string mfampckref = string.Empty;
        private string mcystart = string.Empty;
        private string mfound_order = string.Empty;
        private string mfound_serial = string.Empty;
        private string mmissing_webid = string.Empty;
        private string msql = string.Empty;
        private string msql2 = string.Empty;
        private string mwhere = string.Empty;
        private string mmediaid = string.Empty;
        private string mcardacct = string.Empty;
        private string maxname = string.Empty;
        private string maltaname = string.Empty;
        private string mfields = string.Empty;
        private string mdata = string.Empty;
        private DateTime mlastprompttime = DateTime.Now;
        private DateTime mlastsalestime = DateTime.Now;
        private string mlastpmtstime = string.Empty;
        private string msbcode = string.Empty;
        private DateTime mstarttime = DateTime.Now;
        private string mtable = string.Empty;





        private const string pPOSFilter = "((nkassanr < 60 AND nkassanr <> 33) OR nkassanr = 120)";
        private const string sPOSFilter = "((nactkassanr < 60 AND nactkassanr <> 33) OR nkassanr = 120)";

        private struct AxessPromptData
        {
            public string PromptKey;
            public string PMatch;
            public Int16 PListNr;
            public string Prompt1;
            public string Prompt2;
            public string Prompt3;
            public bool Override;

            public AxessPromptData(string aPromptKey, string aPMatch, Int16 aPListNr, string aPrompt1, string aPrompt2, string aPrompt3, bool aOverride = false)
            {
                PromptKey = aPromptKey;
                PMatch = aPMatch;
                PListNr = aPListNr;
                Prompt1 = aPrompt1;
                Prompt2 = aPrompt2;
                Prompt3 = aPrompt3;
                Override = aOverride;
            }
        }


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
                am.TestMode = false;
                dw.TestMode = tTestMode;
                buy.TestMode = false;
            }
        }

        private void Init()
        {
            if (!Initialized)
            {
                altasyncDS.Reset();
                OpenFiles();
                Initialized = true;
            }
        }

        private void OnProgressChanged(Tuple<string, int, int, int> progress) => ProgressChanged?.Invoke(progress);

        public void SyncSale(string aTranskey)
        {
            Init();
            string Kassa = aTranskey.Substring(0, aTranskey.IndexOf("-"));
            string Trans = aTranskey.Replace(Kassa + "-", "");
            altasyncDS = cf.LoadDataSet(am.MirrorConn, $"SELECT CONCAT_WS('-', nactkassanr, ntransnr) AS transkey, IF (nseriennr > 0, CONCAT_WS('-', nkassanr, nseriennr, nunicodenr), '') AS serialkey, IF (nperspersnr>0,CONCAT_WS('-', nperskassanr, nperspersnr),'') AS perskey, if(NEINZELJOURNALNR>0, concat_ws('-', nactkassanr, neinzeljournalnr),'') AS journalkey, nlfdzaehler, nkassatransaktart AS ntranstype, date(dtinserttimestamp) AS saledate, '' AS ttype, nactkassanr AS nkassanr, ntransnr, nblocknr, nkassanr AS nskassanr, nseriennr, nunicodenr, neinzeljournalnr AS njournalnr, nstornojournalnr AS ncancelnr, '' AS tgroup, nkundenkartentypnr AS nticktype, nperstypnr AS nperstyp, npoolnr, nartikelnr AS nartnr, nstk AS qty, '' AS tfactor, feinzeltariftats AS tariff, '' AS tickdesc, '' AS persdesc, date(dtinserttimestamp) AS issuedate, DTGILTAB AS validfrom, dtgiltbis AS validtill, nkartennr, 0 AS uses, wtp32, wtp64, mediaid, nkassanralt AS rkassanr, nseriennralt AS rseriennr, nunicodenralt AS runicodenr, nperskassanr AS nperskassa, '' AS rserialkey, nperspersnr AS npersnr, '' AS lname, '' AS fname, '1969-12-31' AS DOB, 'X' AS pmatch, '' AS promptoverride, '' AS promptkey, '' AS szprompt1, '' AS szprompt2, '' AS tokenkey, '' AS pmtprofile, '' AS authnet_custid, '' AS axorderid, '' AS ESORDERID, NULL AS orderdate, '' AS updateflag, 'N' AS email_send, 'N' AS email_sent, '' AS email, dtinserttimestamp AS dtinsert, '' AS testflag, dtupdate, NPROJNR, NPERSPROJNR, NFIRMENPROJNR, NFIRMENKASSANR AS FIRMENKASSA, NFIRMENNR, FEINZELTARIFBLATT AS PRICE, NKASSIERID, NPOOLPERSTYPNR, NACTPROJNR, NACTGESNR, NPROJNRALT, pflag FROM {am.ActiveDatabase}.tabkassatransaufbereit WHERE nactkassanr='{Kassa}' AND ntransnr='{Trans}'",altasyncDS,"temp");
            if (altasyncDS.Tables["temp"].Rows.Count > 5)
                System.Diagnostics.Debug.Print(altasyncDS.Tables["temp"].Rows.Count.ToString());

            foreach (DataRow tRow in altasyncDS.Tables["temp"].Rows)
            {
                System.Diagnostics.Debug.Print(altasyncDS.Tables["temp"].Rows.IndexOf(tRow).ToString() + " of " + altasyncDS.Tables["temp"].Rows.Count.ToString());
                System.Diagnostics.Debug.Print(tRow["nlfdzaehler"].ToString());
                SyncSale(tRow);
            }
        }

        public void SyncSale(DataRow aRow)
        {
            Init();
            long xPOS = Convert.ToInt64(aRow["nkassanr"].ToString());
            if (xPOS > 59) return;

            aRow.SetField("email", "");
            aRow.SetField("email_send", "N");
            aRow.SetField("email_sent", "N");
            if (aRow["nkassanr"].ToString() == "20" && aRow.Field<DateTime>("dtinsert").Hour < cf.timeDiff)
            {
                aRow.SetField("saledate", aRow.Field<DateTime>("dtinsert").AddDays(-1).ToString(Mirror.AxessDateFormat));
            }
            DataRow tDR1 = cf.LoadDataRow(am.MirrorConn, $"SELECT DTPRODTIMESTAMP FROM {am.ActiveDatabase}.tabprepaidtickets WHERE nkassanr={aRow["nkassanr"].ToString()} AND nseriennr={aRow["nseriennr"].ToString()} AND nunicodenr={aRow["nunicodenr"].ToString()}");
            string tProdTimestamp = aRow.Field<DateTime>("saledate").ToString(Mirror.AxessDateFormat);
            if (tDR1.Field<DateTime?>("DTPRODTIMESTAMP") != null)
            {
                tProdTimestamp = tDR1.Field<DateTime>("DTPRODTIMESTAMP").ToString(Mirror.AxessDateFormat);
            }
            DataRow tRow;

            // update_passholder_name
            if (aRow["npersnr"].ToString() != string.Empty)
            {
                tRow = cf.LoadDataRow(am.MirrorConn, $"SELECT firstname, lastname, dob, email FROM {am.ActiveDatabase}.tabaxcust WHERE nperskassa='{aRow["nperskassa"].ToString()}' AND npersnr='{aRow["npersnr"].ToString()}' LIMIT 1");
                if (tRow != null)
                    if (tRow["firstname"].ToString().Trim() != string.Empty)
                    {
                        aRow.SetField("fname", tRow["firstname"].ToString());
                        aRow.SetField("lname", tRow["lastname"].ToString());
                        aRow.SetField<DateTime>("dob", Convert.ToDateTime(tRow["dob"].ToString()));
                        aRow.SetField("email", tRow["email"].ToString());
                    }
            }

            //update_orderid
            if (aRow["WTP32"].ToString() != String.Empty)
            {
                tRow = cf.LoadDataRow(am.MirrorConn, $"SELECT norderlistnr, nauftragnr, dtgiltab FROM {am.ActiveDatabase}.tabaxorderdata WHERE nkartennr='{aRow["wtp32"].ToString()}' AND ntransnr='{aRow["ntransnr"].ToString()}'");
                if (tRow != null)
                {
                    if (tRow["norderlistnr"].ToString().Length != 0)
                        aRow.SetField("axorderid", tRow["norderlistnr"].ToString());
                    if (tRow["nauftragnr"].ToString().Length != 0)
                        aRow.SetField("esorderid", tRow["nauftragnr"].ToString());
                    if (tRow["dtgiltab"].ToString().Length != 0)
                        aRow.SetField("orderdate", Convert.ToDateTime(tRow["dtgiltab"].ToString()));
                }
            }

            if (aRow["nkassanr"].ToString() == "20")
            {
                tRow = cf.LoadDataRow(am.MirrorConn, $"SELECT id AS SZAUFTRAGEXT FROM {am.ActiveDatabase}.tabprepaidtickets WHERE nkassanr=20 AND nseriennr={aRow["nseriennr"].ToString()}");
                if (tRow != null)
                {
                    if (tRow["esorderid"].ToString().Length != 0)
                        aRow.SetField("esorderid", tRow["ESOrderID"].ToString());
                }
            }

            //update_tokenkey
            if (aRow["mediaid"].ToString() != string.Empty)
            {
                tRow = cf.LoadDataRow(buy.Buy_Alta_ComConn, $"SELECT associatedtokenid, authnet_custid, customerpaymentprofileid FROM axessutility.tokenxref WHERE mediaid='{aRow["mediaid"].ToString().Trim()}' LIMIT 1");
                if (tRow != null)
                {
                    aRow.SetField("tokenkey", tRow["associatedtokenid"].ToString());
                    aRow.SetField("authnet_custid", tRow["authnet_custid"].ToString());
                    aRow.SetField("pmtprofile", tRow["customerpaymentprofileid"].ToString());
                }
            }

            //update_sales_prompts

            //***check for prompt
            mwhere = $" WHERE nkassanr='{aRow["nkassanr"].ToString()}' AND ntransnr = '{aRow["ntransnr"].ToString()}'";
            if (aRow["njournalnr"].ToString() != string.Empty)
            {
                mwhere += $"={aRow["njournalnr"].ToString()}";
            }
            else
            {
                mwhere += " is null";
            }
            //***get promptdata info * **
            altasyncDS = cf.LoadDataSet(am.MirrorConn, $"SELECT * FROM {am.ActiveDatabase}.tabpromptdata {mwhere} GROUP BY npromptelementnr", altasyncDS, "axprompt");
            string PromptKey = string.Empty;
            string tPMatch = string.Empty;
            altasyncDS = cf.LoadDataSet(dw.dwConn, $"SELECT * FROM {dw.ActiveDatabase}.promptdef", altasyncDS, "promptdef");
            DataColumn[] PromptDefKey = new DataColumn[2];
            PromptDefKey[0] = altasyncDS.Tables["promptdef"].Columns["Promptkey"];
            PromptDefKey[1] = altasyncDS.Tables["promptdef"].Columns["Tablename"];
            altasyncDS.Tables["promptdef"].PrimaryKey = PromptDefKey;

            if (altasyncDS.Tables["axprompt"].Rows.Count != 0)
            {
                foreach (DataRow pRow in altasyncDS.Tables["axprompt"].Rows)
                {
                    //cf.VFPConn.ConnectionString = $"provider=vfpoledb.1;data source = {mdatadir}promptdef.dbf";
                    //altasyncDS = cf.LoadDataSet(cf.VFPConn, $"SELECT * FROM promptdef", altasyncDS, "promptdef");
                    PromptKey = $"{pRow["NPROMPTNR"].ToString()}-{pRow["NPROMPTELEMENTNR"].ToString()}";
                    //DataColumn[] PromptDefKey = new DataColumn[2];
                    //PromptDefKey[0] = altasyncDS.Tables["promptdef"].Columns["Promptkey"];
                    //PromptDefKey[1] = altasyncDS.Tables["promptdef"].Columns["Tablename"];
                    //string[] myKey = new string[2];
                    //myKey[0] = PromptKey;
                    //myKey[1] = "salesday";
                    //DataRow dRow = altasyncDS.Tables["promptdef"].Rows.Find(myKey);
                    string[] myKey = new string[2];
                    myKey[0] = PromptKey;
                    myKey[1] = "salesday";
                    altasyncDS.Tables["promptdef"].PrimaryKey = PromptDefKey;
                    DataRow dRow = altasyncDS.Tables["promptdef"].Rows.Find(myKey);
                    if ((dRow != null))
                    {
                        string msqlcmd = string.Empty;
                        string mfieldname = dRow["fieldname"].ToString().Trim();
                        string mtablekey = dRow["tablekey"].ToString().Trim();
                        string mchecktable = string.Empty;
                        string mcheckfield = string.Empty;
                        if (dRow["checktable"].ToString() != string.Empty)
                        {
                            mchecktable = dRow["checktable"].ToString().Trim();
                            mcheckfield = dRow["checkfield"].ToString().Trim();
                            if (dRow["fieldtype"].ToString().ToUpper().Trim() == "C")
                            {
                                msqlcmd = $"'{pRow["sztext"].ToString().Trim()}'";
                            }
                            else
                            {
                                if (double.TryParse(pRow["sztext"].ToString(), out double isNumeric))
                                {
                                    msqlcmd = $"{pRow["sztext"].ToString().Replace("-", "")}";
                                }
                                else
                                {
                                    msqlcmd = "-1";
                                }
                            }
                            tPMatch = cf.RowExists(dw.dwConn, mchecktable, $"{mcheckfield} = {msqlcmd.ToUpper()}")  ? "N" : "Y";
                        }
                        else
                        {
                            tPMatch = "X";
                        }
                        string tPromptText = pRow["sztext"].ToString();
                        if (tPromptText.Length > 20)
                            tPromptText = tPromptText.Substring(0, 20);
                        aRow.SetField(mfieldname, cf.EscapeChar(tPromptText.ToUpper()));
                    }
                }
            }
            else
            {
                mwhere = $" WHERE nkassanr='{aRow["nkassanr"].ToString()}' AND ntransnr = '{aRow["ntransnr"].ToString()}' AND njournalnr=0 AND nprompttypnr=2";
                altasyncDS = cf.LoadDataSet(am.MirrorConn, $"SELECT * FROM {am.ActiveDatabase}.tabpromptdata {mwhere} GROUP BY npromptelementnr", altasyncDS, "axprompt");
                if (altasyncDS.Tables["axprompt"].Rows.Count != 0)
                {
                    DataRow pRow = altasyncDS.Tables["axprompt"].Rows[0];
                    aRow.SetField("szprompt2", "Group Coupon");
                }
            }
            aRow.SetField("promptkey", PromptKey);
            aRow.SetField("pmatch", tPMatch);
            DataRow sRow = cf.LoadDataRow(dw.dwConn, $"SELECT szprompt1, szprompt2, email_sent, email_send, testflag FROM {dw.ActiveDatabase}.salesdata WHERE nlfdzaehler='{aRow["nlfdzaehler"].ToString()}'");
            if (sRow.Table.Rows.Count != 0)
            {
                aRow.SetField("email_sent", sRow["email_sent"].ToString());
                aRow.SetField("email_send", sRow["email_send"].ToString());
                aRow.SetField("testflag", sRow["testflag"].ToString());
                cf.ExecuteSQL(dw.dwConn, $"DELETE FROM {dw.ActiveDatabase}.salesdata WHERE nlfdzaehler='{aRow["nlfdzaehler"].ToString()}'");
            }
            string tTGroup = "";
            string tTickDesc = "";
            if (aRow["nticktype"].ToString() != string.Empty)
            {
                DataRow ticktypeRow = altasyncDS.Tables["tickettypes"].Rows.Find(aRow["nticktype"].ToString());
                //ticktypeRow = altasyncDS.Tables["tickettypes"].Rows.Find(aRow["nticktype"].ToString());
                if (ticktypeRow != null)
                {
                    tTGroup = ticktypeRow["tgroup"].ToString().Trim();
                    tTickDesc = ticktypeRow["descrip"].ToString().Trim();
                }
            }
            if (tTickDesc.Length > 20)
                tTickDesc = tTickDesc.Substring(0, 20);

            //DataRow perstypeRow = altasyncDS.Tables["persontype"].Rows.Find(aRow["nperstype"].ToString());
            Int32 nTransType = Convert.ToInt32(aRow["ntranstype"].ToString().Trim());
            DataRow transtypeRow = altasyncDS.Tables["transtype"].Rows.Find(nTransType.ToString());
            string tTFactor = transtypeRow["tfactor"].ToString().Trim();
            string tPOS = aRow["nkassanr"].ToString().Trim();
            string tTType = transtypeRow["ttype"].ToString();
            string tTariff = (aRow.Field<decimal>("tariff") * Convert.ToInt32(tTFactor) * aRow.Field<Int16>("qty")).ToString().Trim();
            //update_passholder_name
            string tfname = cf.EscapeChar(aRow["fname"].ToString().Trim());
            string tlName = cf.EscapeChar(aRow["lname"].ToString().Trim());
            string tDOB = "null";
            string tEmail = aRow["email"].ToString().Trim();
            string tPersNr = aRow["npersnr"].ToString().Trim();
            string tPersKassaNr = aRow["nperskassa"].ToString().Trim();
            if (tPersNr.Length == 0)
                tPersNr = "null";
            if (tPersKassaNr.Length == 0)
                tPersKassaNr = "null";

            if (nTransType >= 2 && nTransType <= 7)
            {
                DataRow oldSale = cf.LoadDataRow(dw.dwConn, $"SELECT nperskassa, npersnr FROM {dw.ActiveDatabase}.salesdata WHERE serialkey = '{aRow["serialkey"].ToString()}' AND ntranstype=0 LIMIT 1");
                if (oldSale["npersnr"].ToString() != string.Empty)
                {
                    tPersNr = oldSale["npersnr"].ToString().Trim();
                    tPersKassaNr = oldSale["nperskassa"].ToString().Trim();
                }
            }

            if (tPersNr != "null")
            {
                DataRow custRow = cf.LoadDataRow(am.MirrorConn, $"SELECT firstname, lastname, dob, email FROM {am.ActiveDatabase}.tabaxcust WHERE nperskassa={tPersKassaNr} AND npersnr={tPersNr}");
                if (custRow != null)
                {
                    tfname = cf.EscapeChar(custRow["firstname"].ToString());
                    tlName = cf.EscapeChar(custRow["lastname"].ToString());
                    if ((tfname + tlName).Length != 0)
                    {
                        tDOB = $"'{custRow.Field<DateTime>("dob").ToString(Mirror.AxessDateFormat)}'";
                    }
                    tEmail = custRow["email"].ToString();
                }
            }

            string tPerskey = (tPersNr != "null") && (tPersKassaNr != "null") ? $"'{tPersKassaNr}-{tPersNr}'" : tPerskey = "null";
            string tTranskey = $"'{tPOS}-{aRow["ntransnr"].ToString()}'";
            if (tPOS.Length * aRow["ntransnr"].ToString().Length == 0)
                tTranskey = "null";
            string tSerialkey = string.Empty;
            string tUnicode = aRow["nunicodenr"].ToString();
            string tArtNr = aRow["nartnr"].ToString();
            if (tArtNr.Length != 0)
            {
                tSerialkey = $"'ART-{tArtNr}'";
            }
            else
            {
                if (tUnicode.StartsWith("-"))
                {
                    if (tTType == "C")
                        tUnicode = "MC";
                    if (tTType == "R")
                        tUnicode = "MR";
                }
                else
                {
                    tUnicode = aRow["nunicodenr"].ToString();
                }
            }

            if (tPOS.Length * aRow["nseriennr"].ToString().Length == 0)
            {
                tSerialkey = "null";
            }
            else
            {
                if (tSerialkey == string.Empty)
                    tSerialkey = $"'{aRow["nskassanr"].ToString()}-{aRow["nseriennr"].ToString()}-{tUnicode}'";
            }
            string tJournalkey = $"'{aRow["nkassanr"].ToString()}-{aRow["njournalnr"].ToString()}'";
            if (aRow["nkassanr"].ToString().Length * aRow["njournalnr"].ToString().Length == 0)
                tJournalkey = "null";
            string xtSQL = $"INSERT INTO {dw.ActiveDatabase}.salesdata (TRANSKEY,SERIALKEY,PERSKEY,JOURNALKEY,NLFDZAEHLER,NTRANSTYPE,SALEDATE,TTYPE,NKASSANR,NTRANSNR,NBLOCKNR,NSKASSANR,NSERIENNR,NUNICODENR,NJOURNALNR,NCANCELNR,TGROUP,NTICKTYPE,NPERSTYPE,NPOOLNR,NARTNR,QTY,TFACTOR,TARIFF,TICKDESC,PERSDESC,ISSUEDATE,VALIDFROM,VALIDTILL,NKARTENNR,USES,WTP32,WTP64,MEDIAID,RKASSANR,RSERIENNR,RUNICODENR,NPERSKASSA,RSERIALKEY,NPERSNR,LNAME,FNAME,DOB,PMATCH,PROMPTOVERRIDE,PROMPTKEY,SZPROMPT1,SZPROMPT2,TOKENKEY,PMTPROFILE,AUTHNET_CUSTid,AXORDERID,ESORDERID,ORDERDATE,UPDATEFLAG,email_send,email_sent,email,dtinsert,testflag,dtupdate) VALUES (";
            xtSQL += $"{tTranskey}, ";  //<{TRANSKEY: }>,
            xtSQL += $"{tSerialkey}, ";   //<{SERIALKEY: }>,
            xtSQL += $"{tPerskey}, "; //<{PERSKEY: }>,
            xtSQL += $"{tJournalkey}, ";    //<{JOURNALKEY: }>,
            xtSQL += $"'{aRow["nlfdzaehler"].ToString()}',";    //<{NLFDZAEHLER: }>,
            xtSQL += $"'{nTransType.ToString()}',";     //<{NTRANSTYPE: }>,
            xtSQL += $"'{tProdTimestamp}',";  // $"'{Convert.ToDateTime(aRow["saledate"].ToString()).ToString(Mirror.AxessDateFormat)}',";   //<{SALEDATE: }>,
            xtSQL += $"'{tTType}',"; //<{TTYPE: }>,
            xtSQL += $"'{(tPOS == "120" ? "20" : tPOS)}',";   //<{NKASSANR: }>,
            xtSQL += $"'{aRow["ntransnr"].ToString()}',";   //<{NTRANSNR: }>,
            xtSQL += $"'{aRow["nblocknr"].ToString()}',";   //<{NBLOCKNR: }>,
            xtSQL += $"'{aRow["nskassanr"].ToString()}',";  //<{NSKASSANR: }> serial pos
            xtSQL += $"'{aRow["nseriennr"].ToString()}',";//<{ NSERIENNR: }>,
            xtSQL += $"'{aRow["nunicodenr"].ToString()}',";//<{ NUNICODENR: }>,
            string tJournalNr = $"'{aRow["njournalnr"].ToString()}'";
            if (tJournalNr.Length == 2)
                tJournalNr = "null";
            xtSQL += $"{tJournalNr},";//<{ NJOURNALNR: }>,
            string tCancelNr = "null";
            if (aRow["ncancelnr"].ToString() != string.Empty)
                tCancelNr = $"'{aRow["ncancelnr"].ToString()}'";
            xtSQL += $"{tCancelNr},";//<{ NCANCELNR: }>,
            xtSQL += $"'{tTGroup}',";//<{ TGROUP: }>,
            string tTickType = "null";
            if (aRow["nticktype"].ToString() != string.Empty)
                tTickType = $"'{aRow["nticktype"].ToString()}'";
            xtSQL += $"{tTickType},";//<{ NTICKTYPE: }>,
            string tPersType = "null";
            if (aRow["nperstyp"].ToString() != string.Empty)
                tPersType = $"'{aRow["nperstyp"].ToString()}'";
            xtSQL += $"{tPersType},";//<{ NPERSTYPE: }>,
            string tPoolNr = "null";
            if (aRow["npoolnr"].ToString() != string.Empty)
                tPoolNr = $"'{aRow["npoolnr"].ToString()}'";
            xtSQL += $"{tPoolNr},";//<{ NPOOLNR: }>,
            if (cf.IsStringEmpty(tArtNr))
            {
                tArtNr = "null";
            }
            else
            {
                tArtNr = $"'{tArtNr}'";
            }
            xtSQL += $"{tArtNr},";//<{ NARTNR: }>,
            xtSQL += $"'{aRow["qty"].ToString()}',";//<{ QTY: }>,
            xtSQL += $"'{tTFactor}',";//<{ TFACTOR: }>,
            xtSQL += $"'{tTariff}',";//<{ TARIFF: }>,
            if (aRow.Field<Int32?>("nartnr") > 0)
            {
                DataRow articleRow = altasyncDS.Tables["articles"].Rows.Find(aRow["nartnr"].ToString());
                xtSQL += $"'{articleRow["szname"].ToString()}',";//<{ TICKDESC: }>,
            }
            else
            {
                xtSQL += $"'{cf.EscapeChar(tTickDesc)}',";//<{ TICKDESC: }>,
            }
            string tPersDesc = string.Empty;
            if (aRow["nperstyp"].ToString().Trim().Length != 0)
            {
                DataRow p1Row = altasyncDS.Tables["persontypes"].Rows.Find(aRow["nperstyp"].ToString());
                if (p1Row != null)
                    tPersDesc = p1Row["descrip"].ToString().Trim();
                if (tPersDesc.Length > 20)
                    tPersDesc = tPersDesc.Substring(0, 20);
            }
            xtSQL += $"'{tPersDesc}',";//<{ PERSDESC: }>,
            DateTime? IssueDate = aRow.Field<DateTime?>("issuedate");
            DataRow Prepaid = cf.LoadDataRow(am.MirrorConn, $"SELECT DTCODINGDAT FROM axess_cwc.tabprepaidtickets WHERE NKASSANR={aRow["nskassanr"].ToString()} AND NSERIENNR={aRow["nseriennr"].ToString()}");
            if (Prepaid != null)
            {
                IssueDate = Prepaid.Field<DateTime?>("DTCODINGDAT");
            }
            string tIssueDate = (IssueDate == null) ? "null" : $"'{IssueDate.Value.ToString(Mirror.AxessDateFormat)}'"; 
            xtSQL += $"{tIssueDate},";//<{ ISSUEDATE: }>,
            xtSQL += $"'{Convert.ToDateTime(aRow["validfrom"].ToString()).ToString(Mirror.AxessDateFormat)}',";//<{ VALIDFROM: }>,
            if (",520,521,522,523,123,127,30064,".Contains($",{tTickType.ToString()},"))
            {
                xtSQL += "'2019-07-14'";
            }
            else
            {
                xtSQL += $"'{Convert.ToDateTime(aRow["validtill"].ToString()).ToString(Mirror.AxessDateFormat)}',";//<{ VALIDTILL: }>,
            }
            string tKartenNr = aRow["nkartennr"].ToString();
            if (tKartenNr.Length > 10)
                tKartenNr = tKartenNr.Substring(0, 10);
            if (tKartenNr.Length == 0)
            {
                tKartenNr = "null";
            }
            else
            {
                tKartenNr = $"'{tKartenNr}'";
            }
            xtSQL += $"{tKartenNr},";//<{ NKARTENNR: }>,
            long tUses = 1;
            if (!tTranskey.StartsWith("33-"))
            {
                cf.RowCount(dw.dwConn, $"applications.skivisits", $"SERIALKEY={tSerialkey}");
            }
            xtSQL += $"{tUses},";//<{ USES: }>,
            string tTemp = aRow["wtp32"].ToString();
            if (tTemp == string.Empty)
                {
                    tTemp = "null";
                }
                else
                {
                    tTemp = $"'{tTemp}'";
                }
            xtSQL += $"{tTemp},";//<{ WTP32: }>,
            tTemp = aRow["wtp64"].ToString();
            if (tTemp == string.Empty)
            {
                tTemp = "null";
            }
            else
            {
                tTemp = $"'{tTemp}'";
            }
            xtSQL += $"{tTemp},";//<{ WTP64: }>,
            tTemp = aRow["mediaid"].ToString();
            if (tTemp == string.Empty)
            {
                tTemp = "null";
            }
            else
            {
                tTemp = $"'{tTemp}'";
            }

            xtSQL += $"{tTemp},";//<{ MEDIAID: }>,
            string tRkassanr = "null";
            if (aRow["rkassanr"].ToString() != string.Empty)
                tRkassanr = aRow["rkassanr"].ToString();
            xtSQL += $"{tRkassanr},";//<{ RKASSANR: }>,
            string tRSerialNr = "null";
            if (aRow["rseriennr"].ToString() != string.Empty)
                tRSerialNr = aRow["rseriennr"].ToString();
            xtSQL += $"{tRSerialNr},";//<{ RSERIENNR: }>,
            string tRUnicodeNr = "null";
            if (aRow["runicodenr"].ToString() != string.Empty)
                tRUnicodeNr = aRow["runicodenr"].ToString();
            xtSQL += $"{tRUnicodeNr},";//<{ RUNICODENR: }>,
            xtSQL += $"{tPersKassaNr},";//<{ NPERSKASSA: }>,
            xtSQL += $"'{aRow["rserialkey"].ToString()}',";//<{ RSERIALKEY: }>,
            xtSQL += $"{tPersNr},";//<{ NPERSNR: }>,
            xtSQL += $"'{tlName}',";//<{ LNAME: }>,
            xtSQL += $"'{tfname}',";//<{ FNAME: }>,
            xtSQL += $"{tDOB},";//<{ DOB: }>,
            xtSQL += $"'{aRow["pmatch"].ToString()}',";//<{ PMATCH: }>,
            xtSQL += $"'{aRow["promptoverride"].ToString()}',";//<{ PROMPTOVERRIDE: }>,
            xtSQL += $"'{aRow["promptkey"].ToString()}',";//<{ PROMPTKEY: }>,
            xtSQL += $"'{aRow["szprompt1"].ToString()}',";//<{ SZPROMPT1: }>,
            xtSQL += $"'{aRow["szprompt2"].ToString()}',";//<{ SZPROMPT2: }>,
            string tTokenkey = "0";
            if (aRow["tokenkey"].ToString() != string.Empty)
                tTokenkey = aRow["tokenkey"].ToString();
            xtSQL += $"'{tTokenkey}',";//<{ TOKENKEY: }>,
            string tPmtProfile = "0";
            if (aRow["pmtprofile"].ToString() != string.Empty)
                tPmtProfile = aRow["pmtprofile"].ToString();
            xtSQL += $"'{tPmtProfile}',";//<{ PMTPROFILE: }>,
            string tAuthnetCustID = "0";
            if (aRow["authnet_custid"].ToString() != string.Empty)
                tAuthnetCustID = aRow["authnet_custid"].ToString();
            xtSQL += $"'{tAuthnetCustID}',";//<{ AUTHNET_CUSTid: 0}>,
            string tAXOrderID = "0";
            if (aRow["axorderid"].ToString() != string.Empty)
                tAXOrderID = aRow["axorderid"].ToString();
            xtSQL += $"'{tAXOrderID}',";//<{ AXORDERID: }>,
            string tESOrderID = "0";
            if (aRow["esorderid"].ToString() != string.Empty)
                tESOrderID = aRow["esorderid"].ToString();
            xtSQL += $"'{tESOrderID}',";//<{ ESORDERID: }>,
            string tOrderDate = "null";
            if (DateTime.TryParse(aRow["orderdate"].ToString(), out DateTime tOut))
                tOrderDate = "'" + DateTime.Parse(aRow["orderdate"].ToString()).ToString(Mirror.AxessDateFormat) + "'";
            xtSQL += $"{tOrderDate},";//<{ ORDERDATE: }>,
            string tUpdateFlag = (aRow["updateflag"].ToString() == "1") ? "1" : "0";
            xtSQL += $"'{tUpdateFlag}',";//<{ UPDATEFLAG: }>,
            xtSQL += $"'{aRow["email_send"].ToString()}',";//<{ email_send: N}>,
            xtSQL += $"'{aRow["email_sent"].ToString()}',";//<{ email_sent: N}>,
            xtSQL += $"'{tEmail}',";//<{ email: }>,
            xtSQL += $"'{Convert.ToDateTime(aRow["dtinsert"].ToString()).ToString(Mirror.AxessDateTimeFormat)}',";//<{ dtinsert: }>,
            string tTestFlag = "0";
            if (aRow["testflag"].ToString() == "1")
                tTestFlag = "1";
            xtSQL += $"'{tTestFlag}',";//<{ testflag: 0}>,
            xtSQL += $"'{Convert.ToDateTime(aRow["dtupdate"].ToString()).ToString(Mirror.AxessDateTimeFormat)}')";//<{ dtupdate: }>);
            cf.ExecuteSQL(dw.dwConn, xtSQL);
        }
        

        public void SyncPurchase(DataRow aRow)
        {
            Init();
            SyncPurchase(aRow["transkey"].ToString());
        }

        public void SyncPurchase(string aTranskey)
        {
            Init();
            long xPOS = Convert.ToInt64(aTranskey.Substring(0,aTranskey.Replace("R","").IndexOf('-')));
            if (xPOS > 59) return;

            //fix KassaNr=120 (treat it as a KassaNr=20)
            if (aTranskey.StartsWith("120"))
            {
                aTranskey = aTranskey.Substring(1);
            }
            string mwhere = $" WHERE transkey = '{aTranskey}'";
            if (cf.RowExists(dw.dwConn, $"{dw.ActiveDatabase}.pmtdata", mwhere.Substring(6)))
                cf.ExecuteSQL(dw.dwConn, $"DELETE FROM {dw.ActiveDatabase}.pmtdata {mwhere}");
            altasyncDS = cf.LoadDataSet(am.MirrorConn, $"SELECT transkey, transpkey, pmtkey, saledate, if (nfirmennr IS NOT NULL,CONCAT(nfirmenkassanr + 100, nfirmennr),arcustid) AS arcustid, nprojnr, '' AS office, nkassanr, ntransnr, nlfdnr, nkassierid, ntranstatus AS ntransstat, 1 AS tfactor, IF (ngrundbezartnr IS NULL, 0, pmtamt) AS pmtamt, ptype, bhasbeleg, ngrundbezartnr AS ngrundbeza, nkundenbezartnr AS nkundenbez, bgesplittet AS begsplitte, NULL AS ntranstype, NULL AS nfirmenpro, nfirmenkassanr AS nfirmenkas, nfirmennr, NULL AS agtype, NULL AS refnum, NULL AS arname, NULL AS szname, NULL AS szroom, NULL AS szarcustid, '' AS tgroup, 'Sale' AS pstatus, '' AS ecoupon, '' AS credittype, 0.00 AS creditamt, '' AS lname, '' AS fname, cashier,'' AS pmatch, '' AS promptoverride, '' AS promptkey, 0 AS plistnr, '' AS szprompt1, '' AS szprompt2, '' AS szprompt3, 0 AS axorderid, 0 AS esorderid, null AS orderdate, 'N' AS matched, newrec, updateflag, saledtime AS dtinsert, IF (pmtkey IN ('120-1','10510','106-1'),'N','Y') AS email_flag, dtupdate, 0 AS testflag FROM {am.ActiveDatabase}.tabaxpmtdata {mwhere}", altasyncDS, "axpmts");
            //update_arcust
            string tCustID = string.Empty;
            string narname = string.Empty;
            bool tFound = false;
            foreach (DataRow zRow in altasyncDS.Tables["axpmts"].Rows)
            {

                if (!tFound)
                {
                    if (zRow["arcustid"].ToString() != string.Empty)
                    {
                        tCustID = zRow["arcustid"].ToString();
                        DataRow cRow = altasyncDS.Tables["arcust"].Rows.Find(zRow["arcustid"].ToString());
                        if (cRow != null)
                        {
                            foreach (DataRow tRow in altasyncDS.Tables["axpmts"].Rows)
                            {
                                narname = cf.EscapeChar(cRow["szname"].ToString());
                                tRow.SetField("arname", narname);
                            }
                            tFound = true;
                        }
                    }
                    else
                    {
                        Int16 tPOS = zRow.Field<Int16>("nkassanr");
                        string tFilter = string.Empty;
                        bool tInPOS = (tPOS >= 50 && tPOS <= 55);
                        if (zRow["ptype"].ToString() == "AG" || tInPOS)
                        {
                            if (zRow.Field<long>("plistnr") > 0)
                            {
                                tFilter = $"plistnr={zRow["plistnr"].ToString()}";
                            }
                            else
                            {
                                tFilter = $"remotepos={zRow["nkassanr"].ToString()}";
                            }
                            DataRow[] fRow = altasyncDS.Tables["arcust"].Select(tFilter);
                            if (fRow != null)
                            {
                                foreach (DataRow tRow in altasyncDS.Tables["axpmts"].Rows)
                                {
                                    tRow.SetField("arcustid", fRow[0].Field<string>("arcustid"));
                                    tRow.SetField("arname", fRow[0].Field<string>("szname"));
                                    if (tInPOS)
                                    {
                                        tRow.SetField("ptype", "AR");
                                    }
                                    tRow.SetField("matched", "N");
                                    tRow.SetField("updateflag", "Y");
                                }
                            }
                        }
                    }
                }


                //update_refunds_cancellations
                string tSalesAmt = cf.GetSQLField(dw.dwConn, $"SELECT SUM(tariff) FROM {dw.ActiveDatabase}.salesdata WHERE transkey='{aTranskey}'");
                string tTFactor = "1";
                foreach (DataRow tRow in altasyncDS.Tables["axpmts"].Rows)
                {
                    if (tSalesAmt.StartsWith("-"))
                    {
                        tTFactor = "-1";
                        tRow.SetField("tfactor", tTFactor);
                        if (tRow.Field<decimal>("pmtamt") > 0)
                        {
                            tRow.SetField("pmtamt", -tRow.Field<decimal>("pmtamt"));
                        }
                    }
                }

                //update_pmts
                DataRow cashierRow = altasyncDS.Tables["cashiers"].Rows.Find(zRow["nkassierid"].ToString());
                string transPKey = zRow["transpkey"].ToString();
                string tKassaNo = zRow["nkassanr"].ToString();
                //if KassaNr=120, treat as KassaNr=20
                if (tKassaNo == "120")
                {
                    transPKey = transPKey.Substring(1);
                    tKassaNo = "20";
                }
                //****** load data that need manipulated or are used in queries into variables and do manipulations
                DateTime tSaleDate = zRow.Field<DateTime>("saledate");
                //if online payment adjust the saledate to Utah date
                //if ((tKassaNo == "20" || tKassaNo == "21") && (pmtRow.Field<DateTime>("dtinsert").Hour < AxessTimeDiff))
                //    tSaleDate = (tSaleDate.AddDays(-1));
                string tPmtKey = zRow["pmtkey"].ToString();
                string tARCustID = zRow["arcustid"].ToString();
                tPmtKey = (zRow.Field<Int16?>("ngrundbeza") + 100).ToString().Substring(0, 3) + zRow["nkundenbez"].ToString();
                //decimal tPmtAmt = zRow.Field<Int16?>("ngrundbeza") == null ? 0 : zRow.Field<decimal>("PMTAMT");
                string tPmtType = (xPOS >= 50 && xPOS <= 55) ? zRow["ptype"].ToString() : altasyncDS.Tables["pmttype"].Rows.Find(tPmtKey)["ptype"].ToString();




                //update payment prompts
                string tUpdateFlag = "N";
                string nplistnr = string.Empty;
                string nprompt1 = zRow["szprompt1"].ToString();
                altasyncDS = cf.LoadDataSet(am.MirrorConn, $"SELECT * FROM {am.ActiveDatabase}.tabpromptdata WHERE NKASSANR={tKassaNo} AND NTRANSNR={zRow["NTRANSNR"].ToString()} AND NJOURNALNR=0 GROUP BY npromptelementnr", altasyncDS, "axprompt");

                DataColumn[] PromptDefKey = new DataColumn[1];
                PromptDefKey[0] = altasyncDS.Tables["promptdef"].Columns["npromptelementnr"];
                altasyncDS.Tables["promptdef"].PrimaryKey = PromptDefKey;
                string PromptKey = string.Empty;
                string tPMatch = string.Empty;
                if (altasyncDS.Tables.Contains("axprompt")) 
                {
                    foreach (DataRow pRow in altasyncDS.Tables["axprompt"].Rows)
                    {
                        PromptKey = pRow["npromptnr"].ToString() + "-" + pRow["npromptelementnr"].ToString();
                        foreach (DataRow dRow in altasyncDS.Tables["promptdef"].Rows)
                        if ((dRow["PromptKey"].ToString() == PromptKey))
                        {
                            string msqlcmd = string.Empty;
                            string mfieldname = dRow["fieldname"].ToString();
                            string mtablekey = dRow["tablekey"].ToString();
                            string mchecktable = string.Empty;
                            string mcheckfield = string.Empty;
                            if (dRow["checktable"].ToString() != string.Empty)
                            {
                                mchecktable = dRow["checktable"].ToString();
                                mcheckfield = dRow["checkfield"].ToString();
                                if (dRow["fieldtype"].ToString() == "C")
                                {
                                    msqlcmd = $"'{pRow["sztext"].ToString()}'";
                                }
                                else
                                {
                                    if (double.TryParse(pRow["sztext"].ToString(), out double isNumeric))
                                    {
                                        msqlcmd = $"{pRow["sztext"].ToString().Replace("-", "")}";
                                    }
                                    else
                                    {
                                        msqlcmd = "-1";
                                    }
                                }
                                tPMatch = cf.RowExists(dw.dwConn, $"{mchecktable}", $"{mcheckfield} = {msqlcmd}") ? "N" : "Y";
                            }
                            else
                            {
                                tPMatch = "X";
                            }
                            string tPromptText = pRow["sztext"].ToString();
                            if (tPromptText.Length > 20)
                                tPromptText = tPromptText.Substring(0, 20);
                            zRow.SetField(mfieldname, cf.EscapeChar(tPromptText));
                            foreach (DataRow tpRow in altasyncDS.Tables["axpmts"].Rows)
                            {
                                tpRow.SetField("promptkey", tPMatch);
                            }

                            if (PromptKey == "70-20") // pass and lodge room charges
                            {
                                nplistnr = pRow["nlfdelementnr"].ToString();
                                if (nplistnr == string.Empty)
                                {
                                    nplistnr = "null";
                                }
                                Int16 tPOS = zRow.Field<Int16>("nkassanr");
                                bool tInPOS = (tPOS >= 50 && tPOS <= 55);
                                DataRow custRow = cf.LoadDataRow(dw.dwConn, $"SELECT * FROM {dw.ActiveDatabase}.arcust WHERE plistnbr='{nplistnr}'");
                                if (custRow != null)
                                {
                                    int lkey = custRow.Field<int>("plistnbr");
                                    tARCustID = custRow["arcustid"].ToString();
                                    if (tARCustID != string.Empty)
                                    {
                                        narname= cf.GetSQLField(dw.dwConn, $"SELECT szname FROM {dw.ActiveDatabase}.arcust WHERE arcustid='{tARCustID}'");
                                        zRow.SetField("arcustid", narname);
                                    }
                                    else
                                    {
                                        string tWhere = string.Empty;
                                        if (zRow["ptype"].ToString().ToUpper() == "AG" || (tInPOS))
                                        {
                                            if (zRow.Field<int>("plistnr") > 0)
                                            {
                                                tWhere = "plistnr=" + zRow["plistnr"].ToString();
                                            }
                                            else
                                            {
                                                tWhere = "remotepos=" + zRow["nkassanr"].ToString();
                                            }
                                            if (tInPOS)
                                                tPmtType = "AR";
                                            tUpdateFlag = "Y";
                                        }
                                    }
                                    narname = custRow["szname"].ToString();
                                    switch (lkey)
                                    {
                                        case 40:
                                            nprompt1 = "PC";
                                            break;
                                        case 45:
                                            nprompt1 = "FP";
                                            break;
                                        case 48:
                                            nprompt1 = "RS";
                                            break;
                                        default:
                                            if (lkey < 39)
                                            {
                                                nprompt1 = "LC";
                                            }
                                            break;
                                    }
                                }
                            }
                            else
                            {
                                if (mfieldname.ToLower() == "szprompt1")
                                {
                                    nprompt1 = pRow["sztext"].ToString();
                                }
                                else
                                {
                                    zRow.SetField(mfieldname, pRow["sztext"].ToString());
                                }
                            }
                        }
                        zRow.SetField("pmatch", tPMatch);
                    }
                }

                if (narname.Length>25)
                {
                    narname = narname.Substring(0, 25).Trim();
                }
                string szprompt2 = $"{zRow["SZPROMPT2"].ToString().Trim()}";
                string szprompt3 = $"{zRow["SZPROMPT3"].ToString().Trim()}";
                if (szprompt2.Length > 20)
                {
                    szprompt2 = szprompt2.Substring(0, 20);
                }
                if (szprompt3.Length > 20)
                {
                    szprompt3 = szprompt3.Substring(0, 20);
                }
                string tSQL = $"INSERT INTO {dw.ActiveDatabase}.pmtdata (TRANSKEY, TRANSPKEY,PMTKEY,SALEDATE,ARCUSTID,NPROJNR,OFFICE,NKASSANR,NTRANSNR,NLFDNR,NKASSIERID,NTRANSSTAT,TFACTOR,PMTAMT,PTYPE,BHASBELEG,NGRUNDBEZA,NKUNDENBEZ,BEGSPLITTE,NTRANSTYPE,NFIRMENPRO,NFIRMENKAS,NFIRMENNR,AGTYPE,REFNUM,ARNAME,SZNAME,SZROOM,SZARCUSTID,TGROUP,PSTATUS,ECOUPON,CREDITTYPE,CREDITAMT,LNAME,FNAME,CASHIER,PMATCH,PROMPTOVERRIDE,PROMPTKEY,PLISTNR,SZPROMPT1,SZPROMPT2,SZPROMPT3,AXORDERID,ESORDERID,ORDERDATE,MATCHED,NEWREC,UPDATEFLAG,dtinsert,email_flag,dtupdate,testflag) VALUES (";
                tSQL += $"'{aTranskey}',";
                tSQL += $"'{transPKey}',";
                tSQL += $"'{tPmtKey}',";
                tSQL += $"'{tSaleDate.ToString(Mirror.AxessDateFormat)}',";
                tSQL += $"'{(tPmtType == "WE" ? "1206" : tARCustID)}',";
                tSQL += $"'{zRow["NPROJNR"].ToString()}',";
                tSQL += $"'{zRow["OFFICE"].ToString()}', ";
                tSQL += $"'{tKassaNo}',";
                tSQL += $"'{zRow["NTRANSNR"].ToString()}', ";
                tSQL += $"'{zRow["NLFDNR"].ToString()}', ";
                tSQL += (zRow["NKASSIERID"].ToString() == string.Empty ? "0, " : $"'{zRow["NKASSIERID"].ToString()}', ");
                tSQL += (zRow["NTRANSSTAT"].ToString() == string.Empty ? "0, " : $"'{zRow["NTRANSSTAT"].ToString()}', ");
                tSQL += $"{tTFactor}, "; //TFACTOR
                tSQL += $"'{zRow.Field<decimal>("PMTAMT").ToString()}', ";   //PMTAMT
                tSQL += $"'{tPmtType}', ";  //PTYPE
                tSQL += $"'{zRow["BHASBELEG"].ToString()}', ";
                tSQL += $"'{zRow["NGRUNDBEZA"].ToString()}', ";
                tSQL += $"'{zRow["NKUNDENBEZ"].ToString()}', ";
                tSQL += $"'{zRow["BEGSPLITTE"].ToString()}', ";
                tSQL += (zRow["NTRANSTYPE"].ToString() == string.Empty ? "0, " : $"'{zRow["NTRANSTYPE"].ToString()}', ");
                tSQL += (zRow["NFIRMENPRO"].ToString() == string.Empty ? "0, " : $"'{zRow["NFIRMENPRO"].ToString()}', ");
                tSQL += (zRow["NFIRMENKAS"].ToString() == string.Empty ? "null, " : $"'{zRow["NFIRMENKAS"].ToString()}', ");
                tSQL += (zRow["NFIRMENNR"].ToString() == string.Empty ? "null, " : $"'{zRow["NFIRMENNR"].ToString()}', ");
                tSQL += $"'{zRow["AGTYPE"].ToString()}', ";
                tSQL += $"'{zRow["REFNUM"].ToString()}', ";
                tSQL += $"'{cf.EscapeChar(narname.Trim())}', ";
                tSQL += $"'{zRow["SZNAME"].ToString()}', ";
                tSQL += $"'{zRow["SZROOM"].ToString()}', ";
                tSQL += $"'', ";
                tSQL += $"'{zRow["TGROUP"].ToString()}', ";
                tSQL += "'Sale', "; //pstatus
                tSQL += $"'{zRow["ECOUPON"].ToString()}', ";
                tSQL += $"'{zRow["CREDITTYPE"].ToString()}', ";
                tSQL += $"'{zRow["CREDITAMT"].ToString()}', ";
                tSQL += $"'{cf.EscapeChar(zRow["LNAME"].ToString())}', ";
                tSQL += $"'{cf.EscapeChar(zRow["FNAME"].ToString())}', ";
                string tCashierName = string.Empty;
                if (cashierRow != null)
                    tCashierName = $"{cf.EscapeChar(cashierRow["szname"].ToString())} {cf.EscapeChar(cashierRow["szvorname"].ToString())}";
                tSQL += $"'{tCashierName}', ";  //CASHIER
                tSQL += $"'{tPMatch}', ";
                tSQL += $"'{zRow["PROMPTOVERRIDE"].ToString()}', ";
                tSQL += $"'{PromptKey}', ";
                if (nplistnr==string.Empty)
                {
                    nplistnr = "0";
                }
                tSQL += $"{nplistnr}, ";
                tSQL += $"'{cf.EscapeChar(nprompt1)}', ";
                tSQL += $"'{cf.EscapeChar(szprompt2)}', ";
                tSQL += $"'{cf.EscapeChar(szprompt3)}', ";
                tSQL += $"'{zRow["AXORDERID"].ToString()}', ";
                tSQL += $"'{zRow["ESORDERID"].ToString()}', ";
                tSQL += (zRow["ORDERDATE"].ToString() == string.Empty ? "null" : $"'{Convert.ToDateTime(zRow["ORDERDATE"].ToString()).ToString(Mirror.AxessDateFormat)}'") + ", ";
                tSQL += "'N', ";   //MATCHED
                tSQL += "'N', ";    //NEWREC
                tSQL += $"'{tUpdateFlag}', "; //UPDATEFLAG
                tSQL += $"'{Convert.ToDateTime(zRow["dtinsert"].ToString()).ToString(Mirror.AxessDateTimeFormat)}', ";
                tSQL += $"'{zRow["email_flag"].ToString()}', ";
                tSQL += $"'{Convert.ToDateTime(zRow["dtupdate"].ToString()).ToString(Mirror.AxessDateTimeFormat)}',";
                tSQL += $"'{zRow["testflag"].ToString()}')";
                cf.ExecuteSQL(dw.dwConn, tSQL);
            }
        }

        public void SyncPrompt(string aTransKey)
        {
            if (cf.RowExists(am.MirrorConn, $"{am.ActiveDatabase}.tabpromptdata", "njournalnr=0"))
            {
                SyncSale(aTransKey);
            }
            if (cf.RowExists(am.MirrorConn, $"{am.ActiveDatabase}.tabpromptdata", "njournalnr<>0"))
            {
                SyncPurchase(aTransKey);
            }
        }

        public void SyncPrompt(DataRow aRow)
        {
            SyncPrompt(aRow["nkassanr"].ToString() + "-" + aRow["ntransnr"].ToString());
        }

        
        public void SetEmailFlag()
        {
            string tSQL = $"UPDATE {dw.ActiveDatabase}.salesdata AS A SET email_send='Y' WHERE LENGTH(email) <> 0 AND email_sent <> 'Y' AND transkey=(SELECT email_send FROM {dw.ActiveDatabase}.pmtdata WHERE transkey=A.transkey AND emailflag='Y' LIMIT 1)";
            cf.ExecuteSQL(dw.dwConn, tSQL, 90);
        }

        public void Update_Prompt_Changes()
        {

            string mptime = DateTime.Now.AddYears(-1).ToString(Mirror.AxessDateTimeFormat);
            altasyncDS = cf.LoadDataSet(dw.dwConn,$"SELECT * FROM {am.ActiveDatabase}.tabpromptdata WHERE dtupdate > '{mlastprompttime}'",altasyncDS,"axprompt");
            foreach (DataRow tRow in altasyncDS.Tables["axprompt"].Rows)
            {
                string PromptKey = tRow["npromptnr"].ToString() + "-" + tRow["npromptelementnr"].ToString();
                string TransKey = tRow["nkassanr"].ToString() + "-" + tRow["ntransnr"].ToString();
                DataRow pRow = altasyncDS.Tables["promptdef"].Rows.Find(PromptKey);
                bool tJournal = false;
                if (pRow != null)
                {
                    if (pRow["tablename"].ToString() == "salesday")
                    {
                        mtable = "axsales";
                        tJournal = (Convert.ToInt32(tRow["njournalnr"].ToString()) > 0);
                    }
                    else
                    {
                        mtable = "axpmts";
                    }
                    string tSQL = $"SELECT * FROM {dw.ActiveDatabase}.{mtable} WHERE transkey = '{pRow["mtranskey"].ToString()}'";
                    if (tJournal)
                    {
                        tSQL += $" AND njournalnr={tRow["njournalnr"].ToString()}";
                    }
                    altasyncDS = cf.LoadDataSet(dw.dwConn, tSQL, altasyncDS, mtable);
                    string mfieldname = pRow["fieldname"].ToString();
                    string mtablekey = pRow["tablekey"].ToString();
                    string mchecktable = string.Empty;
                    string mcheckfield = string.Empty;

                    bool matchflag = false;
                    if (pRow["checktable"].ToString() != string.Empty)
                    {
                        matchflag = true;
                        mchecktable = pRow["checktable"].ToString();
                        mcheckfield = pRow["checkfield"].ToString();
                    }
                    if (matchflag)
                    {
                        if (pRow["fieldtype"].ToString() == "C")
                        {
                            tSQL = $"'{pRow["sztext"].ToString()}'";
                        }
                        else
                        {
                            if (Convert.ToDecimal(pRow["sztext"].ToString()) > 0)
                            {
                                pRow.SetField("sztext", Convert.ToDecimal(pRow["sztext"].ToString()).ToString());
                                tSQL = $"'{pRow["sztext"].ToString().Replace(".","")}'";
                            }
                            else
                            {
                                tSQL = $"'-1'";
                            }
                        }
                        tSQL = $"SELECT * from ${dw.ActiveDatabase}.{mchecktable} WHERE {mcheckfield} = {tSQL})";
                        altasyncDS = cf.LoadDataSet(dw.dwConn, tSQL, altasyncDS, "promptcheck");
                        string tPMatch = altasyncDS.Tables.Contains("promptcheck") ? "Y" : "N";
                        foreach (DataRow zRow in altasyncDS.Tables[mtable].Rows)
                        {
                            if (zRow["ntranskey"].ToString() == PromptKey)
                            {
                                zRow.SetField("pmatch", tPMatch);
                            }
                        }
                    }
                    else
                    {
                        foreach (DataRow zRow in altasyncDS.Tables[mtable].Rows)
                        {
                            if (zRow["ntranskey"].ToString() == PromptKey)
                            {
                                zRow.SetField("pmatch", "X");
                            }
                        }
                    }

                }
            }
            /*
                    SELECT & mtable
                    mfield = mtable + ".promptkey"
                    replace ALL & mfield WITH mkey
                    replace ALL dtupdate WITH axprompt.dtupdate
                    IF mkey = "70-20" && pass and lodge room charges
                        SELECT arcust
                        GO top
                        LOCATE FOR arcust.plistnbr = axprompt.NLFDELEMENTNR
                        if !EOF()
                            lkey = arcust.plistnbr
                            replace axpmts.plistnr WITH axprompt.NLFDELEMENTNR
                            replace axpmts.arcustid WITH arcust.arcustid
                            replace axpmts.arname with arcust.szname
                            DO case						 
						        case lkey < 39
                                    replace axpmts.szprompt1 with "LC"
							    case lkey = 45
                                    replace axpmts.szprompt1 with "FP"
                                case lkey = 48
                                    replace axpmts.szprompt1 with "RS"
                                CASE lkey = 40
                                    replace axpmts.szprompt1 with "PC"
                                endcase
                        endif					
	    	        else
                        SELECT & mtable
                        mfield = mtable + "." + mfieldname
                        replace ALL &mfield WITH allTRIM(axprompt.sztext)
                    endif
                endif
                IF mtable = 'axsales'
                    SELECT axsales
                    GO top
                    thisform.update_sales
		        else
		            SELECT axpmts
                    GO top
                    thisform.update_pmts
                endif
                SELECT axprompt
                mpromptcnt = mpromptcnt + 1
                thisform.text10.Refresh
                mptime = axprompt.dtupdate
                SKIP
            enddo
            **** OPEN syscfg TABLE ****
            if USED('syscfg')
                SELECT syscfg
            else	
                mfname = mdatadir + 'syscfg'
                USE & mfname IN 0
                SELECT syscfg
            endif
            SELECT syscfg
            IF mptime > DATETIME() - (365 * 24 * 3600)
                REPLACE promptdt WITH mptime
            endif
            RETURN
*/
        }



        public void SyncPOS(string aPOSList, DateTime? aStartDate = null, DateTime? aEndDate = null)
        {
            string tQuery = $"SELECT nactkassanr, MIN(ntransnr) AS tMin, MAX(ntransnr) AS tMax FROM {am.ActiveDatabase}.tabkassatransaufbereit";
            string tWhere = "";
            if (aStartDate != null)
            {
                DateTime dStart = aStartDate.Value;
                string tStart = dStart.ToString(Mirror.AxessDateFormat);
                tWhere = $"date(dtinserttimestamp)='{tStart}' ";
                if (aEndDate != null)
                {
                    DateTime dEnd = aEndDate.Value;
                    string tEnd = dEnd.ToString(Mirror.AxessDateFormat);
                    tWhere = tWhere.Replace("=", ">=") + $"AND date(dtinserttimestamp)<='{tEnd}'";
                }
            }
            if (aPOSList != string.Empty && aPOSList != "*")
            {
                if (tWhere != string.Empty)
                {
                    tWhere += " AND ";
                }
                tWhere += $"nactkassanr IN ({aPOSList})";
            }
            if (tWhere != string.Empty)
            {
                tQuery += $" WHERE {tWhere} GROUP BY nactkassanr ORDER BY nactkassanr, NTRANSNR";
            }

            using (DataTable tDT = cf.LoadTable(am.MirrorConn, tQuery, "tempSync"))
            {
                foreach (DataRow dr in tDT.Rows)
                {
                    var TProgress = new Tuple<string, int, int, int>($"Updating POS {dr["nactkassanr"].ToString()}.", tDT.Rows.IndexOf(dr) + 1, tDT.Rows.Count, 2);
                    OnProgressChanged(TProgress);
                    Int32 tMin = Convert.ToInt32(dr["tMin"].ToString());
                    Int32 tMax = Convert.ToInt32(dr["tMax"].ToString());
                    Int32 TotRecs = tMax - tMin;
                    string tPOS = dr["nactkassanr"].ToString();
                    for (int i = tMin; i <= tMax; i++)
                    {
                        Int32 CurRec = i - tMin;
                        TProgress = new Tuple<string, int, int, int>($"Updating POS{dr["nactkassanr"].ToString()} ({CurRec} of {TotRecs}).", CurRec, TotRecs, 3);
                        OnProgressChanged(TProgress);
                        SyncSale($"{tPOS}-{i.ToString()}");
                        SyncPurchase($"{tPOS}-{i.ToString()}");
                    }
                }
            }
        }

        public void SyncAll()
        {
            Init();

            //get most recent salesdata and pmtdata timestamps
            mlastsalestime = Convert.ToDateTime(cf.GetSQLField(dw.dwConn, $"SELECT MAX(dtupdate) FROM {dw.ActiveDatabase}.salesdata WHERE {pPOSFilter}"));
            mlastpmttime = Convert.ToDateTime(cf.GetSQLField(dw.dwConn, $"SELECT MAX(dtupdate) FROM {dw.ActiveDatabase}.pmtdata WHERE {pPOSFilter}"));
            mlastprompttime = (mlastsalestime > mlastpmttime ? mlastsalestime : mlastpmttime);
            
            //run for 24 hours if 8PM hour otherwise run for 1 hour
            if (DateTime.Now.Hour == 20)
            {
                mlastsalestime = mlastsalestime.AddDays(-1);
                mlastpmttime = mlastpmttime.AddDays(-1);
            }
            else
            {
                mlastsalestime = mlastsalestime.AddHours(-1);
                mlastpmttime = mlastpmttime.AddHours(-1);
            }
            string sFilter = $" WHERE dtupdate > '{mlastsalestime.ToString(Mirror.AxessDateTimeFormat)}' AND {sPOSFilter}";
            string pFilter = $" WHERE dtupdate > '{mlastpmttime.ToString(Mirror.AxessDateTimeFormat)}' AND {pPOSFilter}";





            // *** get_sales_records (AXSALES)
            //altasyncDS = cf.LoadDataSet(dw.dwConn, "SELECT DISTINCT NLFDZAEHLER FROM applications.salesdata WHERE NOT (NLFDZAEHLER IN (SELECT NLFDZAEHLER FROM applications_test.salesdata)) LIMIT 5000", altasyncDS, "temp");
            //string t = string.Empty;
            //foreach(DataRow tRow in altasyncDS.Tables["temp"].Rows)
            //{
            //    t += "," + tRow["nlfdzaehler"].ToString();
            //}
            //sFilter = $" WHERE nlfdzaehler IN ({t.Substring(1)})";
            altasyncDS = cf.LoadDataSet(am.MirrorConn, $"SELECT nlfdzaehler, nkassatransaktart AS ntranstype, date(dtinserttimestamp) AS saledate, '' AS ttype, nactkassanr AS nkassanr, ntransnr, nblocknr, nkassanr AS nskassanr, nseriennr, nunicodenr, neinzeljournalnr AS njournalnr, nstornojournalnr AS ncancelnr, '' AS tgroup, nkundenkartentypnr AS nticktype, nperstypnr AS nperstyp, npoolnr, nartikelnr AS nartnr, nstk AS qty, '' AS tfactor, feinzeltariftats AS tariff, '' AS tickdesc, '' AS persdesc, date(dtinserttimestamp) AS issuedate, DTGILTAB AS validfrom, DTGILTBIS AS validtill, nkartennr, 0 AS uses, wtp32, wtp64, mediaid, nkassanralt AS rkassanr, nseriennralt AS rseriennr, nunicodenralt AS runicodenr, nperskassanr AS nperskassa, '' AS rserialkey, nperspersnr AS npersnr, '' AS lname, '' AS fname, '' AS DOB, '' AS pmatch, '' AS promptoverride, '' AS promptkey, '' AS szprompt1, '' AS szprompt2, '' AS tokenkey, '' AS pmtprofile, '' AS authnet_custid, '' AS axorderid, '' AS ESORDERID, NULL AS orderdate, '' AS updateflag, 'N' AS email_send, 'N' AS email_sent, '' AS email, dtinserttimestamp AS dtinsert, '' AS testflag, dtupdate, NPROJNR, NPERSPROJNR, NFIRMENPROJNR, NFIRMENKASSANR AS FIRMENKASSA, NFIRMENNR, FEINZELTARIFBLATT AS PRICE, NKASSIERID, NPOOLPERSTYPNR, NACTPROJNR, NACTGESNR, NPROJNRALT, pflag FROM {am.ActiveDatabase}.tabkassatransaufbereit {sFilter} ORDER BY NLFDZAEHLER", altasyncDS, "axsales");
            int tCount = altasyncDS.Tables["axsales"].Rows.Count;
            foreach (DataRow sdRow in altasyncDS.Tables["axsales"].Rows)
            {
                SyncSale(sdRow);
            }

            //load table with all distinct transkey values from tabaxpmtdata within the date range or using dwflag (final version)
            altasyncDS = cf.LoadDataSet(am.MirrorConn, $"SELECT transkey FROM {am.ActiveDatabase}.tabaxpmtdata WHERE dtupdate > '{mlastpmttime.ToString(Mirror.AxessDateTimeFormat)}' AND ({pPOSFilter}) GROUP BY transkey ORDER BY dtupdate", altasyncDS, "axpmtdata");
            //altasyncDS = cf.LoadDataSet(am.MirrorConn, $"SELECT DISTINCT(transkey) FROM {am.ActiveDatabase}.tabaxpmtdata WHERE dwflag=true AND ({pPOSFilter}) ORDER BY dtupdate", altasyncDS, "axpmtdata");

            //loop through all transkeys
            tCount = altasyncDS.Tables["axpmtdata"].Rows.Count;
            foreach (DataRow pmtdataRow in altasyncDS.Tables["axpmtdata"].Rows)
            {
                SyncPurchase(pmtdataRow["transkey"].ToString());
            }
        }
        
        private AxessPromptData GetPromptData(string aTransKey, string aDestinationTable = "", string aJournalFilter = "")
        {
            AxessPromptData tPromptData = new AxessPromptData();

            string tKassaNr = aTransKey.Substring(0, aTransKey.IndexOf("-"));
            string tTransNr = aTransKey.Replace(tKassaNr + "-", "");
            string mwhere = $" WHERE nkassanr='{tKassaNr}' AND ntransnr = '{tTransNr}'";

            if (aJournalFilter != string.Empty)
            {
                mwhere += $" AND njournalnr {aJournalFilter}";
            }
            //***get promptdata info * **
            altasyncDS = cf.LoadDataSet(am.MirrorConn, $"SELECT * FROM {am.ActiveDatabase}.tabpromptdata {mwhere} GROUP BY npromptelementnr", altasyncDS, "axprompt");




            string PromptKey = string.Empty;
            string tPMatch = string.Empty;
            if (altasyncDS.Tables["axprompt"].Rows.Count != 0)
            {
                foreach (DataRow pRow in altasyncDS.Tables["axprompt"].Rows)
                {
                    PromptKey = pRow["NPROMPTNR"].ToString() + "-" + pRow["NPROMPTELEMENTNR"].ToString();
                    object[] PromptDefKey = new object[2];
                    PromptDefKey[0] = PromptKey;
                    PromptDefKey[1] = "salesday";
                    DataRow dRow = altasyncDS.Tables["promptdef"].Rows.Find(PromptDefKey);
                    if ((dRow != null))
                    {
                        string msqlcmd = string.Empty;
                        string mfieldname = dRow["fieldname"].ToString();
                        string mtablekey = dRow["tablekey"].ToString();
                        string mchecktable = string.Empty;
                        string mcheckfield = string.Empty;
                        if (dRow["checktable"].ToString() != string.Empty)
                        {
                            mchecktable = dRow["checktable"].ToString();
                            mcheckfield = dRow["checkfield"].ToString();
                            if (dRow["fieldtype"].ToString() == "C")
                            {
                                msqlcmd = $"'{pRow["sztext"].ToString()}'";
                            }
                            else
                            {
                                if (double.TryParse(pRow["sztext"].ToString(), out double isNumeric))
                                {
                                    msqlcmd = $"{pRow["sztext"].ToString().Replace("-", "")}";
                                }
                                else
                                {
                                    msqlcmd = "-1";
                                }
                            }
                            tPMatch = cf.RowExists(dw.dwConn, $"{dw.ActiveDatabase}.{mchecktable}", $"{mcheckfield} = {msqlcmd}") ? "N" : "Y";
                        }
                        else
                        {
                            tPMatch = "X";
                        }
                        string tPromptText = pRow["sztext"].ToString();
                        if (tPromptText.Length > 20)
                            tPromptText = tPromptText.Substring(0, 20);
                        tPromptText = cf.EscapeChar(tPromptText);
                        switch (mfieldname)
                        {
                            case "szprompt1":
                                tPromptData.Prompt1 = tPromptText;
                                break;
                            case "szprompt2":
                                tPromptData.Prompt2 = tPromptText;
                                break;
                            case "szprompt3":
                                tPromptData.Prompt3 = tPromptText;
                                break;
                        }
                    }
                }
            }
            else if (aDestinationTable == "axsales")
            {
                mwhere = $" WHERE nkassanr='{tKassaNr}' AND ntransnr = '{tTransNr}' AND njournalnr=0 AND nprompttypnr=2";
                altasyncDS = cf.LoadDataSet(am.MirrorConn, $"SELECT * FROM {am.ActiveDatabase}.tabpromptdata {mwhere} GROUP BY npromptelementnr", altasyncDS, "axprompt");
                if (altasyncDS.Tables["axprompt"].Rows.Count != 0)
                {
                    tPromptData.Prompt2 = "Group Coupon";
                }
            }
            return tPromptData;
        }

        private void OpenFiles()
        {
            DataColumn[] keys = new DataColumn[1];
            DataColumn[] keys2 = new DataColumn[2];

            altasyncDS = cf.LoadDataSet(dw.dwConn, $"SELECT * FROM {dw.ActiveDatabase}.promptdef", altasyncDS, "promptdef");
            keys2[0] = altasyncDS.Tables["promptdef"].Columns["promptkey"];
            keys2[1] = altasyncDS.Tables["promptdef"].Columns["tablename"];
            altasyncDS.Tables["promptdef"].PrimaryKey = keys2;

            cf.VFPConn.ConnectionString = $"provider=vfpoledb.1;data source = {mdatadir}tickettypes.dbf";
            altasyncDS = cf.LoadDataSet(cf.VFPConn, $"SELECT * FROM tickettypes", altasyncDS, "tickettypes");
            keys[0] = altasyncDS.Tables["tickettypes"].Columns["Ticktype"];
            altasyncDS.Tables["tickettypes"].PrimaryKey = keys;

            cf.VFPConn.ConnectionString = $"provider=vfpoledb.1;data source = {mdatadir}persontype.dbf";
            altasyncDS = cf.LoadDataSet(cf.VFPConn, $"SELECT * FROM persontype WHERE Active=.T.", altasyncDS, "persontypes");
            keys[0] = altasyncDS.Tables["persontypes"].Columns["perskey"];
            altasyncDS.Tables["persontypes"].PrimaryKey = keys;

            altasyncDS = cf.LoadDataSet(am.MirrorConn, $"SELECT * FROM {am.ActiveDatabase}.tabartikeldef", altasyncDS, "articles");
            keys[0] = altasyncDS.Tables["articles"].Columns["nartikelnr"];
            altasyncDS.Tables["articles"].PrimaryKey = keys;

            altasyncDS = cf.LoadDataSet(dw.dwConn, $"SELECT * FROM {dw.ActiveDatabase}.cashiers", altasyncDS, "cashiers");
            keys[0] = altasyncDS.Tables["cashiers"].Columns["cashid"];
            altasyncDS.Tables["cashiers"].PrimaryKey = keys;

            altasyncDS = cf.LoadDataSet(dw.dwConn, $"SELECT * FROM {dw.ActiveDatabase}.transtype", altasyncDS, "transtype");
            keys[0] = altasyncDS.Tables["transtype"].Columns["trantype"];
            altasyncDS.Tables["transtype"].PrimaryKey = keys;

            altasyncDS = cf.LoadDataSet(dw.dwConn, $"SELECT * FROM {dw.ActiveDatabase}.pmttype", altasyncDS, "pmttype");
            keys[0] = altasyncDS.Tables["pmttype"].Columns["pkey"];
            altasyncDS.Tables["pmttype"].PrimaryKey = keys;

            altasyncDS = cf.LoadDataSet(dw.dwConn, $"SELECT * FROM {dw.ActiveDatabase}.arcust", altasyncDS, "arcust");
            keys[0] = altasyncDS.Tables["arcust"].Columns["arcustid"];
            altasyncDS.Tables["arcust"].PrimaryKey = keys;
        }

        public void FixPrepaids()
        {
            DataTable tTable = cf.LoadTable(am.MirrorConn, $"SELECT concat_ws('-',nkassanr,ntransnr) AS TransKey FROM {am.ActiveDatabase}.tabprepaidtickets", "Prepaids");
            foreach (DataRow tRow in tTable.Rows)
            {
                Debug.Print((tTable.Rows.IndexOf(tRow) + 1).ToString() + " of " + tTable.Rows.Count.ToString() + "  " + tRow["TransKey"].ToString());
                SyncSale(tRow["TransKey"].ToString());
            }
        }

    }
}
