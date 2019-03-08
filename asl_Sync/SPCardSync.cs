using System;
using System.Data;

namespace asl_SyncLibrary
{

    public class SPCardSync
    {
        private double days_history = 2;
        private bool Initialized = false;

        private CommonFunctions cf = new CommonFunctions();
        private DataWarehouse dw = new DataWarehouse();
        private Mirror am = new Mirror();
        private Axess_DCI4CRM ax = new Axess_DCI4CRM();
        private BUY_ALTA_COM buy = new BUY_ALTA_COM();
        private ASLError myError = new ASLError();

        //init
        private DateTime mdatetime;
        private DataSet spcDS = new DataSet();
        private string mirrorDatabase = string.Empty;
        //private string dw.ActiveDatabase = string.Empty;
        private const string mdatadir = @"g:\fpdata\altaax\";

        private const string DependentTypes = "500,510,520,530,540,550";

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
                mirrorDatabase = am.ActiveDatabase;
                dw.TestMode = tTestMode;
                //dw.ActiveDatabase = dw.ActiveDatabase;
            }
        }

        private bool Init(DateTime? aRunStart = null)
        {
            if (!Initialized)
            {
                if (OpenFiles())
                {
                    mdatetime = ((aRunStart == null) ? (Convert.ToDateTime(cf.GetSQLField(dw.dwConn, $"SELECT MAX(dtupdate) FROM {dw.ActiveDatabase}.spcards WHERE saledate = DATE(dtupdate)")).AddDays(-days_history)) : (Convert.ToDateTime(aRunStart)));
                    Initialized = true;
                }
            }
            return Initialized;
        }

        private bool OpenFiles()
        {
            cf.VFPConn.ConnectionString = $"provider=vfpoledb.1;data source = {mdatadir}tickettypes.dbf";
            cf.LoadDataSet(cf.VFPConn, $"SELECT * FROM tickettypes", spcDS, "tickettypes");
            cf.VFPConn.ConnectionString = $"provider=vfpoledb.1;data source = {mdatadir}persontype.dbf";
            cf.LoadDataSet(cf.VFPConn, $"SELECT * FROM persontype  WHERE Active = .T.", spcDS, "persontypes");
            cf.LoadDataSet(dw.dwConn, $"SELECT * FROM applications.promptdef", spcDS, "promptdef");
            return (spcDS.Tables.Contains("tickettypes") && spcDS.Tables.Contains("persontypes") && spcDS.Tables.Contains("promptdef"));
        }

        public void RunAll()
        {
            Update_mediaid();
            Update_spcards();
            Update_blocked();
            Update_tokenkey();
        }

        public void Update_mediaid()
        {
            Init();
            DataTable tTable = cf.LoadTable(dw.dwConn, $"SELECT * FROM {dw.ActiveDatabase}.spcards WHERE cardstatus='A' AND arcustid='' AND LENGTH(mediaid) != 16 AND tgroup IN ('EX','SP','PC')", "spcheck");
            int tRowCount = tTable.Rows.Count;
            foreach (DataRow tRow in tTable.Rows)
            {
                DataRow sRow = cf.LoadDataRow(dw.dwConn, $"SELECT * FROM {dw.ActiveDatabase}.salesdata WHERE nlfdzaehler='{tRow["nlfdzaehle"].ToString()}'");
                if (sRow["mediaid"].ToString().Length == 16)
                {
                    cf.ExecuteSQL(dw.dwConn, $"UPDATE {dw.ActiveDatabase}.spcards SET mediaid='{sRow["mediaid"].ToString()}', wtp64='{sRow["wtp64"].ToString()}', wtp32='{sRow["wtp32"].ToString()}', cardacct='{ax.Hex2cardacct(sRow["wtp32"].ToString())}' WHERE nlfdzaehle='{tRow["nlfdzaehle"].ToString()}'");
                }
            }
            //remove dups in spcards
            tTable = cf.LoadTable(dw.dwConn, $"SELECT serialkey, MAX(recid) AS recid, COUNT(*) AS cnt FROM {dw.ActiveDatabase}.spcards GROUP BY serialkey HAVING cnt>1", "spcheck");
            tRowCount = tTable.Rows.Count;
            foreach (DataRow sRow in tTable.Rows)
            {
                cf.ExecuteSQL(dw.dwConn, $"DELETE FROM {dw.ActiveDatabase}.spcards WHERE serialkey='{sRow["serialkey"].ToString()}' AND recid < {sRow["recid"].ToString()}");
            }
            tTable.Dispose();
        }

        public void Update_spcards()
        {
            Init();
            DataColumn[] keys = new DataColumn[1];
            keys = new DataColumn[1];
            keys[0] = spcDS.Tables["tickettypes"].Columns["ticktype"];
            spcDS.Tables["tickettypes"].PrimaryKey = keys;
            keys[0] = spcDS.Tables["persontypes"].Columns["perskey"];
            spcDS.Tables["persontypes"].PrimaryKey = keys;
            //mdatetime = new DateTime(2018, 11, 1);
            try
            {
                string tQry = $"SELECT a.*, b.arcustid, b.arname FROM {dw.ActiveDatabase}.salesdata AS a INNER JOIN {dw.ActiveDatabase}.pmtdata AS b ON a.transkey = b.transkey WHERE a.dtupdate > '{mdatetime.ToString(Mirror.AxessDateTimeFormat)}' AND a.nkassanr != 33 AND a.tgroup in ('SP','EX','PC','CC','MC') AND NOT (a.TGROUP='SP' AND a.NTICKTYPE=123) ORDER BY a.dtinsert";
                DataTable tTable = cf.LoadTable(dw.dwConn, tQry, "salesdata");
                int tRowCount = tTable.Rows.Count;
                foreach (DataRow tRow in tTable.Rows)
                {
                    System.Diagnostics.Debug.Print(tTable.Rows.IndexOf(tRow).ToString() + " of " + tRowCount.ToString());
                    string mserialkey = tRow["serialkey"].ToString();
                    DataRow tSPCheckRow = cf.LoadDataRow(dw.dwConn, $"SELECT * FROM {dw.ActiveDatabase}.spcards WHERE serialkey ='{mserialkey}'");
                    bool mnewrec = (tSPCheckRow == null);

                    string nMediaID = tRow["mediaid"].ToString().Trim();
                    bool ValidMediaID = (nMediaID.Length != 8);
                    string nKassaNr = tRow["nkassanr"].ToString().Trim();
                    string nSerialNr = tRow["nseriennr"].ToString().Trim();
                    string nUnicodeNr = tRow["nunicodenr"].ToString().Trim();
                    string mtokenkey = "0"; // (tRow["tokenkey"].ToString().Length != 0) ? tRow["tokenkey"].ToString() : "0";
                    string mpmtprofile = "0"; // (tRow["pmtprofile"].ToString().Length != 0) ? tRow["pmtprofile"].ToString() : "0";
                    string mauthnet_custid = "0"; // (tRow["authnet_custid"].ToString().Length != 0) ? tRow["authnet_custid"].ToString() : "0";
                    string TokenWhere = (ValidMediaID ? $"mediaid='{nMediaID}'" : $"posnr={nKassaNr} AND serialnr={nSerialNr} AND nunicodenr={nUnicodeNr}");

                    //pass tap, find serial data


                    bool skiprec = false;
                    if (!mnewrec)
                    {
                        mnewrec |= (tSPCheckRow["serialkey"].ToString() != mserialkey);
                        skiprec = (tSPCheckRow["override"].ToString().Trim() == "Y");
                    }
                    bool mrecpresent = !mnewrec;
                    bool mupdate = false;
                    if (mrecpresent)
                    {
                        mupdate = (tSPCheckRow["tokenkey"].ToString() != mtokenkey);
                    }

                    //string testtrans = tRow["mediaid"].ToString();
                    //if (testtrans == "E00401503EF5495E")
                    //    testtrans = string.Empty;

                    DataRow tTickRow = spcDS.Tables["tickettypes"].Rows.Find(tRow["nticktype"].ToString());
                    bool hasTickType = (tTickRow != null);

                    DataRow tPersRow = spcDS.Tables["persontypes"].Rows.Find(tRow["nperstype"].ToString());
                    bool hasPersType = (tPersRow != null);

                    string mchrgflag = hasTickType ? tTickRow["chrgflag"].ToString() : "N";
                    string mmgmtflag = "N";
                    string mempchrg = "N";
                    string mdiscflag = "N";
                    string mutaflag = (skiprec ? tTickRow["utaflag"].ToString() : "N");
                    string mmtnrepflag = (skiprec ? tTickRow["mtnrepflag"].ToString() : "N");
                    string mwasbenflag = (skiprec ? tTickRow["wbflag"].ToString() : "N");
                    string mosaflag = (skiprec ? tTickRow["osaflag"].ToString() : "N");
                    string mmcpflag = hasTickType ? tTickRow["mcpflag"].ToString() : "N";
                    string mforgotflag = "N";
                    string mdisctype = hasPersType ? tPersRow["disctype"].ToString() : "N";
                    string mconvflag = hasTickType ? tTickRow["convflag"].ToString() : "N";
                    int mfreedays = 0;
                    string mcardstatus = string.Empty;
                    string museflag = string.Empty;
                    string mrptflag = string.Empty;
                    string mmcpnbr = string.Empty;
                    string mexpdate = string.Empty;
                    string marname = string.Empty;
                    string mpicfile = string.Empty;
                    string mlname = string.Empty;
                    string mfname = string.Empty;
                    string mfamkey = string.Empty;
                    string mdob = cf.defaultDOB.ToString(Mirror.AxessDateFormat);
                    string memail = string.Empty;
                    string mheight = string.Empty;
                    string mstreet = string.Empty;
                    string mcity = string.Empty;
                    string mstate = string.Empty;
                    string mzip = string.Empty;
                    string mgender = string.Empty;
                    string mperskey = string.Empty;
                    string mempid = string.Empty;
                    string marcustid = string.Empty;
                    string mwtp32 = "NULL";
                    string mwtp64 = "NULL";
                    string mmediaid = "NULL";
                    string mcardacct = "NULL";

                    if (tRow["nticktype"].ToString().Length != 0)
                    {
                        if (tRow["ttype"].ToString().ToUpper() == "S")
                        {
                            string mdatetime = tRow.Field<DateTime>("dtupdate").ToString(Mirror.AxessDateTimeFormat);
                            if (tRow.Field<int?>("npersnr") > 0)
                            {
                                //get_axcust
                                DataRow custRow = cf.LoadDataRow(am.MirrorConn, $"SELECT * FROM {am.ActiveDatabase}.tabaxcust WHERE nperskassa={tRow["nperskassa"].ToString()} AND npersnr={tRow["npersnr"].ToString()}");
                                if (custRow != null)
                                {
                                    mfamkey = custRow["famkassa"].ToString() + "-" + custRow["fampersnr"].ToString();
                                    if (custRow["email"].ToString().Length != 0)
                                        memail = cf.EscapeChar(custRow["email"].ToString());
                                    if (custRow["cheight"].ToString().Length != 0)
                                        mheight = cf.EscapeChar(@custRow["cheight"].ToString());
                                    if (custRow["gender"].ToString().Length != 0)
                                        mgender = custRow["gender"].ToString();
                                    if (custRow["city"].ToString().Length != 0)
                                        mcity = cf.EscapeChar(custRow["city"].ToString());
                                    if (mcity.Length > 30)
                                        mcity = mcity.Substring(0, 30);
                                    if (custRow["state"].ToString().Length != 0)
                                        mstate = custRow["state"].ToString();
                                    if (mstate.Length > 10)
                                        mstate = mstate.Substring(0, 10);
                                    if (custRow["zip"].ToString().Length != 0)
                                        mzip = custRow["zip"].ToString();
                                    if (custRow["street"].ToString().Length != 0)
                                        mstreet = cf.EscapeChar(custRow["street"].ToString());
                                    if (custRow["firstname"].ToString().Length != 0)
                                        mfname = cf.EscapeChar(custRow["firstname"].ToString());
                                    if (custRow["lastname"].ToString().Length != 0)
                                        mlname = cf.EscapeChar(custRow["lastname"].ToString());
                                    //if (custRow.Field<DateTime>("dob").Year >= 1900)
                                    //{
                                    if (tRow["dob"].ToString().Length != 0)
                                    {
                                        mdob = tRow.Field<DateTime>("dob").ToString(Mirror.AxessDateFormat);
                                    }
                                    //}
                                    //else
                                    //{
                                    //    if (custRow.Field<DateTime>("dob").Year >= 1900)
                                    //        mdob = custRow.Field<DateTime>("dob").ToString(Mirror.AxessDateFormat);
                                    //}
                                }
                                //end get_axcust
                                mperskey = tRow["nperskassa"].ToString() + "-" + tRow["npersnr"].ToString();
                                if (mrecpresent)
                                {
                                    mupdate |= (tSPCheckRow["perskey"].ToString() != mperskey);
                                }
                            }
                            if (tTickRow["picflag"].ToString() == "Y")
                            {
                                string tSN = tRow["nseriennr"].ToString().PadLeft(5, '0');
                                mpicfile = tRow["nkassanr"].ToString().PadLeft(3, '0') + @"\" + tSN.Substring(0, 2) + @"\" + tSN.Substring(2, 3) + "_" + (tRow.Field<DateTime>("validtill") - new DateTime(1997, 12, 31)).Days.ToString() + ".jpg";
                                if (mrecpresent)    //  017\15\544_7608
                                {
                                    mupdate |= (tSPCheckRow["pic"].ToString() != mpicfile);
                                }
                            }
                            if (tRow.Field<int?>("tokenkey") > 0)
                            {
                                mchrgflag = "Y";
                                marcustid = "11874";
                                marname = "ASL - PASS CHARGES";
                                if (mrecpresent)
                                    mupdate |= (tSPCheckRow["tokenkey"].ToString() != tRow["tokenkey"].ToString());
                            }
                            if (hasPersType)
                            {
                                if ((tTickRow["disctype"].ToString().Length != 0) || ((tTickRow["empchrg"].ToString() == "Y") && (tTickRow["discflag"].ToString() == "Y")))
                                {
                                    mdiscflag = "Y";
                                    mdisctype = tPersRow["disctype"].ToString();
                                    if (mrecpresent)
                                        mupdate |= (tSPCheckRow["discflag"].ToString() != mdiscflag);
                                }
                                if ((!skiprec && tTickRow["mtnrepflag"].ToString() == "Y") && (tPersRow["mtnflag"].ToString() == "True") || (tPersRow["mtnflag"].ToString() == "Y"))
                                {
                                    mmtnrepflag = "Y";
                                    if (mrecpresent)
                                        mupdate |= (tSPCheckRow["mtnrepflag"].ToString() != mmtnrepflag);
                                }
                                if (((!skiprec && tPersRow["utaflag"].ToString() == "True") && ((tTickRow["utaflag"].ToString() == "Y") || (DependentTypes.Contains(tPersRow["perskey"].ToString())))) || ((mdisctype == "E") && (tRow.Field<int>("tokenkey") > 0)))
                                {
                                    mutaflag = "Y";
                                    if (mrecpresent)
                                        mupdate |= (tSPCheckRow["utaflag"].ToString() != mutaflag);
                                }
                                if ((!skiprec && tTickRow["osaflag"].ToString() == "Y") && tPersRow["osaflag"].ToString() == "True")
                                {
                                    mosaflag = "Y";
                                    if (mrecpresent)
                                        mupdate |= (tSPCheckRow["osaflag"].ToString() != mosaflag);
                                }
                            }
                            if (tRow["rserialkey"].ToString() != "NULL")
                            {
                                mserialkey = tRow["rserialkey"].ToString();
                                if (mrecpresent)
                                    mupdate |= (tSPCheckRow["serialkey"].ToString() != mserialkey);
                            }
                            mexpdate = tRow.Field<DateTime>("validtill").ToString(Mirror.AxessDateFormat);
                            string tSzPrompt1 = tRow["szprompt1"].ToString().Trim();
                            //*** employee prompt Key & Charge Check
                            if ((tRow["promptkey"].ToString() == "3-1") || (tRow["promptkey"].ToString() == "2008-1"))
                            {
                                string mkey = tSzPrompt1;
                                if (mkey == "DAV2106")
                                    mkey = mkey.Trim();
                                DataRow empRow = cf.LoadDataRow(dw.dwConn, $"SELECT empchrg FROM payroll.empfile WHERE emp_id='{cf.EscapeChar(mkey)}' LIMIT 1");
                                if (empRow != null)
                                {
                                    mempid = mkey;
                                    if (empRow["empchrg"].ToString() == "Y")
                                    {
                                        mempchrg = "Y";
                                        marcustid = "11521";
                                        marname = "ASL - EMPLOYEE CHARGES";
                                    }
                                    else
                                    {
                                        mempchrg = "N";
                                        marcustid = string.Empty;
                                        marname = string.Empty;
                                    }
                                    if (mrecpresent)
                                        mupdate |= ((tSPCheckRow["empchrg"].ToString() != mempchrg) || (tSPCheckRow["empid"].ToString() != mkey));
                                }
                                else
                                    mempchrg = "N";
                            }


                            //*** CONVENIENCE CARD PROMPT CVHECK
                            if ((tRow["promptkey"].ToString() == "2010-3") || (tRow["promptkey"].ToString() == "2010-4"))
                            {
                                if (tSzPrompt1.Contains(" "))
                                {
                                    mfname = tSzPrompt1.Substring(0, tSzPrompt1.IndexOf(' ') - 1).Replace("'", "''");
                                    mlname = tSzPrompt1.Substring(tSzPrompt1.IndexOf(' ')).Replace("'", "''");
                                }
                                else
                                {
                                    mlname = tSzPrompt1.Replace("'", "''");
                                }
                                if (mrecpresent)
                                    mupdate |= (tSPCheckRow["lastname"].ToString() != mlname);
                            }

                            //*** BUS PASS CHECK ***
                            if ((tRow["promptkey"].ToString() == "23-2") && (tRow["nticktype"].ToString() == "3000"))
                                mlname = tSzPrompt1.Replace("'", "''");

                            //*** mountain collective free days check ***
                            if ((tRow["nticktype"].ToString().Trim() == "170") || (tRow["nticktype"].ToString().Trim() == "171") || (tRow["nticktype"].ToString().Trim() == "172") || (tRow["nticktype"].ToString().Trim() == "2025"))
                            {
                                mfreedays = 2;
                                if (tRow["promptkey"].ToString().Trim() == "11-1")
                                //if (tRow["nticktype"].ToString() == "2025")  //Alta-Bird MTC-CC No longer selling
                                { mmcpnbr = tSzPrompt1; }
                                else
                                { mmcpnbr = tRow["szprompt2"].ToString().Trim(); }
                                if (cf.RowExists(dw.dwConn, $"{dw.ActiveDatabase}.willcall", $"res_no='{mmcpnbr}'"))
                                {
                                    mfreedays = Convert.ToInt32(cf.GetSQLField(dw.dwConn, $"SELECT qty FROM {dw.ActiveDatabase}.willcall WHERE res_no='{mmcpnbr}'"));
                                }
                                if (mrecpresent)
                                    mupdate |= (tSPCheckRow["mcpnbr"].ToString() != mmcpnbr);
                                if (!mmcpnbr.StartsWith("MCP"))
                                {
                                    if (mmcpnbr.Contains("MCP"))
                                    {
                                        mmcpnbr = mmcpnbr.Substring(mmcpnbr.IndexOf('M'));

                                    }
                                }
                                if (mmcpnbr.Length > 16) mmcpnbr = mmcpnbr.Substring(0, 16);
                            }

                            //**** forgot flag testing ****
                            if (tTickRow["forgotflag"].ToString() == "Y")
                            {
                                mforgotflag = "Y";
                                if (mrecpresent)
                                    mupdate |= (tSPCheckRow["forgotflag"].ToString() != mforgotflag);
                            }

                            //*** wasatch benefit testing  ***
                            if (!skiprec && tTickRow["wbflag"].ToString() == "Y")
                            {
                                mwasbenflag = "Y";
                                if (mrecpresent)
                                    mupdate |= (tSPCheckRow["wasbenflag"].ToString() != mforgotflag);
                            }

                            //
                            if (tRow["wtp32"].ToString() != "NULL")
                            {
                                mwtp32 = "'" + tRow["wtp32"].ToString() + "'";
                                mcardacct = "'" + ax.Hex2cardacct(tRow["wtp32"].ToString()) + "'";
                            }
                            if (tRow["wtp64"].ToString() != "NULL")
                                mwtp64 = "'" + tRow["wtp64"].ToString() + "'";
                            if (tRow["mediaid"].ToString() != "NULL")
                                mmediaid = "'" + tRow["mediaid"].ToString() + "'";

                            //switch (tRow["ntranstype"].ToString())
                            //{
                            //    case "S":
                            //        mcardstatus = "A";
                            //        break;
                            //    default:
                            //        mcardstatus = tRow["ttype"].ToString();
                            //        break;
                            //}

                        }
                        string mtGroup = tRow["tgroup"].ToString().Trim();
                        string mTickType = tRow["nticktype"].ToString().Trim();
                        bool isReplacement = (tRow["ntranstype"].ToString().Trim() == "8");

                        if (isReplacement)
                        {
                            mcardstatus = "A";
                            string OldSerialKey = $"{ tRow["RKASSANR"].ToString().Trim()}-{ tRow["RSERIENNR"].ToString().Trim()}-{tRow["RUNICODENR"].ToString().Trim()}";
                            cf.ExecuteSQL(dw.dwConn, $"UPDATE {dw.ActiveDatabase}.spcards SET cardstatus='B', utasent=0 WHERE serialkey = '{OldSerialKey}'");
                        }
                        else
                        {
                            DataRow tDR = cf.LoadDataRow(am.MirrorConn, $"SELECT baktiv FROM {am.ActiveDatabase}.tabkartensperre WHERE nkassanr={tRow["nkassanr"].ToString()} AND nseriennr={tRow["nseriennr"].ToString()} AND nunicodenr={tRow["nunicodenr"].ToString()} LIMIT 1");
                            if (tDR[0].ToString().Trim() == "-1")
                            {
                                mcardstatus = "B";
                            }
                            else
                            {
                                mcardstatus = cf.GetSQLField(dw.dwConn, $"SELECT spcardstatus FROM {dw.ActiveDatabase}.transtype WHERE trantype={tRow["ntranstype"].ToString().Trim()}");
                            }
                        }
                        if (mrecpresent)
                        {
                            mupdate |= (tSPCheckRow["cardstatus"].ToString() != mcardstatus);
                            string tTK = tSPCheckRow["tokenkey"].ToString();
                            if (tTK.Length == 0)
                                tTK = "0";
                            if ((Convert.ToInt32(tTK) > 0) && (tSPCheckRow["cardstatus"].ToString() == "A"))
                                if (tSPCheckRow["chrgflag"].ToString() == "N" && tSPCheckRow["override"].ToString() == "N")
                                {
                                    mchrgflag = "Y";
                                    if (mrecpresent)
                                        mupdate |= (tSPCheckRow["chrgflag"].ToString() != mchrgflag);
                                }
                        }

                        string msqlcmd = string.Empty;
                        mdatetime = tRow.Field<DateTime>("dtupdate");
                        if (mupdate)
                        {
                            if (tRow["ttype"].ToString() == "S")
                            {
                                msqlcmd = $"UPDATE {dw.ActiveDatabase}.spcards SET ";
                                msqlcmd += $"tgroup = '{tTickRow["tgroup"].ToString()}', ";
                                msqlcmd += $"cardstatus = '{mcardstatus}', ";
                                msqlcmd += $"mcpnbr = '{mmcpnbr.Replace("'","")}', ";
                                msqlcmd += $"wtp32 = {mwtp32}, ";
                                msqlcmd += $"wtp64 = {mwtp64}, ";
                                msqlcmd += $"dob = '{mdob}', ";
                                msqlcmd += $"cardacct = {mcardacct},";
                                msqlcmd += $"nlfdzaehle = '{tRow["nlfdzaehler"].ToString()}', ";
                                msqlcmd += $"mediaid = {mmediaid}, ";
                                msqlcmd += $"familykey = '{mfamkey}', ";
                                msqlcmd += $"firstname = '{cf.EscapeChar(mfname)}', ";
                                msqlcmd += $"lastname = '{cf.EscapeChar(mlname)}', ";
                                msqlcmd += $"chrgflag = '{mchrgflag}', ";
                                msqlcmd += $"empchrg = '{mempchrg}', ";
                                msqlcmd += $"convflag = '{mconvflag}', ";
                                msqlcmd += $"mcpflag = '{mmcpflag}', ";
                                msqlcmd += $"forgotflag = '{mforgotflag}', ";
                                msqlcmd += $"tokenkey = '{(mtokenkey != string.Empty ? mtokenkey : "0").ToString()}', ";
                                msqlcmd += $"authnet_custid = '{(mauthnet_custid != string.Empty ? mauthnet_custid : "0").ToString()}', ";
                                msqlcmd += $"pmtprofile = '{(mpmtprofile != string.Empty ? mpmtprofile : "0").ToString()}', ";
                                msqlcmd += $"discflag = '{mdiscflag}', ";
                                msqlcmd += $"disctype = '{mdisctype}', ";
                                msqlcmd += $"utaflag = '{mutaflag}', ";
                                msqlcmd += $"mtnrepflag = '{mmtnrepflag}', ";
                                msqlcmd += $"wasbenflag = '{mwasbenflag}', ";
                                msqlcmd += $"chrgflag = '{mchrgflag}', ";
                                msqlcmd += $"osaflag = '{mosaflag}', ";
                                msqlcmd += $"height = '{cf.EscapeChar(mheight)}', ";
                                msqlcmd += $"gender = '{mgender}', ";
                                msqlcmd += $"street = '{cf.EscapeChar(mstreet)}', ";
                                msqlcmd += $"city = '{cf.EscapeChar(mcity)}', ";
                                msqlcmd += $"state = '{mstate}', ";
                                msqlcmd += $"email = '{cf.EscapeChar(memail)}', ";
                                msqlcmd += $"empid = '{cf.EscapeChar(mempid)}', ";
                                msqlcmd += $"arcustid = '{marcustid}', ";
                                msqlcmd += $"arname = '{cf.EscapeChar(marname)}', ";
                                msqlcmd += $"testflag = '{tRow["testflag"].ToString()}', ";
                                msqlcmd += $"dtupdate = '{mdatetime.ToString(Mirror.AxessDateTimeFormat)}', ";
                                msqlcmd += $"pic = '{mpicfile.Replace(@"\", @"\\")}', ";
                                msqlcmd += $"freedays = {mfreedays.ToString()}, ";
                                msqlcmd += $"nkassanr = {tRow["nkassanr"].ToString()} ";
                                msqlcmd += $" where serialkey = '{tSPCheckRow["serialkey"].ToString()}'";
                            }
                            else
                            {
                                msqlcmd = $"UPDATE {dw.ActiveDatabase}.spcards set cardstatus = '{tRow["ttype"].ToString()}' where serialkey = '{tRow["serialkey"].ToString()}'";
                            }
                        }
                        else
                        {
                            if (mnewrec)
                            {
                                //if (cf.RowExists(dw.dwConn, $"{dw.ActiveDatabase}.SPCARDS", $"SERIALKEY='{tRow["serialkey"].ToString()}'"))
                                //    cf.ExecuteSQL(dw.dwConn, $"DELETE FROM {dw.ActiveDatabase}.spcards WHERE serialkey = '{tRow["serialkey"].ToString()}'");
                                if (tRow["ttype"].ToString() == "S")
                                {
                                    msqlcmd = $"INSERT INTO {dw.ActiveDatabase}.spcards (";
                                    msqlcmd += "serialkey,nkassanr,saledate,transkey,transtype,cardstatus,acctnbr,tgroup,mcpnbr,";
                                    msqlcmd += "lastname,firstname,tokenkey,authnet_custid,pmtprofile,empid,nticktype,tickdesc,nperstype,persdesc,";
                                    msqlcmd += "perskey,wtp32,wtp64,mediaid,cardacct,nlfdzaehle,familykey,rserialkey,";
                                    msqlcmd += "chrgflag,empchrg,mgmtflag,convflag,forgotflag,mcpflag,discflag,disctype,";
                                    msqlcmd += "utaflag,mtnrepflag,wasbenflag,osaflag,expdate,";
                                    msqlcmd += "dob,height,gender,street,city,state,zip,email,pic,dtupdate,arcustid,arname,";
                                    msqlcmd += "freedays,useflag,testflag,rptkey) Values('";
                                    msqlcmd += $"{tRow["serialkey"].ToString()}','{tRow["nkassanr"].ToString()}','{tRow.Field<DateTime>("saledate").ToString(Mirror.AxessDateFormat)}','{tRow["transkey"].ToString()}','{tRow["ttype"].ToString()}','A','{tTickRow["acct"].ToString()}','{tTickRow["tgroup"].ToString()}','{mmcpnbr.Replace("'","")}'";
                                    msqlcmd += $",'{mlname}','{mfname}','{mtokenkey}','{mauthnet_custid}','{mpmtprofile}','{cf.EscapeChar(mempid)}','{tRow["nticktype"].ToString()}','{tRow["tickdesc"].ToString()}','{tRow["nperstype"].ToString()}','{tRow["persdesc"].ToString()}'";
                                    msqlcmd += $",'{mperskey}',{mwtp32}, {mwtp64},{mmediaid},{mcardacct},'{tRow["nlfdzaehler"].ToString()}','{mfamkey}','{mserialkey}'";
                                    msqlcmd += $",'{mchrgflag}','{mempchrg}','{mmgmtflag}','{mconvflag}','{mforgotflag}','{mmcpflag}','{mdiscflag}','{mdisctype}'";
                                    msqlcmd += $",'{mutaflag}','{mmtnrepflag}','{mwasbenflag}','{mosaflag}','{mexpdate}'";
                                    msqlcmd += $",'{mdob}','{mheight}','{mgender}','{mstreet}','{mcity}','{mstate}','{mzip}','{memail}'";
                                    msqlcmd += $",'{mpicfile.Replace(@"\", @"\\")}','{mdatetime.ToString(Mirror.AxessDateTimeFormat)}','{marcustid}','{marname}'";
                                    msqlcmd += $",{mfreedays},'{museflag}','{tRow["testflag"].ToString()}','{mrptflag}')";
                                }
                            }
                        }
                        if (msqlcmd != string.Empty)
                            cf.ExecuteSQL(dw.dwConn, msqlcmd.Replace("\n", ""));
                    }
                }
            }
            catch { }
        }

        public void Update_blocked()
        {
            Init();
            string mcardstatus = string.Empty;
            string mblockdate = (new DateTime(DateTime.Now.Year - (DateTime.Now.Month <= 6 ? 1 : 0), 7 , 1)).ToString(Mirror.AxessDateFormat);
            spcDS = cf.LoadDataSet(dw.dwConn, $"SELECT * FROM {dw.ActiveDatabase}.spcards WHERE saledate > '{mblockdate}' GROUP BY serialkey", spcDS, "spcards");
            DataColumn[] keys = new DataColumn[1];
            keys[0] = spcDS.Tables["spcards"].Columns["serialkey"];
            try
            {
                spcDS.Tables["spcards"].PrimaryKey = keys;
                spcDS = cf.LoadDataSet(am.MirrorConn, $"SELECT * FROM {am.ActiveDatabase}.tabkartensperre WHERE DATE(dtsperrzeitpunkt) >= '{mblockdate}' AND dtsperrzeitpunkt <= '{DateTime.Now.ToString(Mirror.AxessDateTimeFormat)}' AND nkassanr < 100", spcDS, "axblkd");
                int tRowCount = spcDS.Tables["axblkd"].Rows.Count;
                foreach (DataRow tRow in spcDS.Tables["axblkd"].Rows)
                {
                    if (tRow["baktiv"].ToString() == "0")
                    {
                        mcardstatus = "A";
                    }
                    else
                    {
                        if (tRow["dtgueltdatum"].ToString() == "NULL")
                        {
                            mcardstatus = "B";
                        }
                        else
                        {
                            if (tRow.Field<DateTime?>("dtgiltbis") == null)
                            {
                                mcardstatus = "A";
                            }
                            else
                            {
                                if (DateTime.Now < tRow.Field<DateTime>("dtgiltbis"))
                                {
                                    mcardstatus = "B";
                                }
                                else
                                {
                                    mcardstatus = "A";
                                }
                            }
                        }
                    }
                    string mkey = tRow["nkassanr"].ToString() + "-" + tRow["nseriennr"].ToString() + "-" + tRow["nunicodenr"].ToString();
                    DataRow sRow = spcDS.Tables["spcards"].Rows.Find(mkey);
                    if (sRow != null)
                        if ((sRow["cardstatus"].ToString() != mcardstatus) && (sRow["cardstatus"].ToString() != "C"))
                            cf.ExecuteSQL(dw.dwConn, $"UPDATE {dw.ActiveDatabase}.spcards SET cardstatus='{sRow["cardstatus"].ToString()}' WHERE serialkey='{mkey}'");
                }
            }
            finally
            {
            }
        }

        public void Update_tokenkey()
        {
            Init();
            spcDS = cf.LoadDataSet(dw.dwConn, $"SELECT * FROM {dw.ActiveDatabase}.spcards WHERE cardstatus='A' AND LENGTH(mediaID)=16", spcDS, "spcards");
            spcDS = cf.LoadDataSet(buy.Buy_Alta_ComConn, "SELECT * FROM axessutility.tokenxref WHERE (serialnr > 99) AND (LENGTH(mediaID)=16) GROUP BY mediaID", spcDS, "tokendata");
            DataColumn[] keys = new DataColumn[1];
            keys[0] = spcDS.Tables["tokendata"].Columns["mediaid"];
            try
            {
                spcDS.Tables["tokendata"].PrimaryKey = keys;
                spcDS = cf.LoadDataSet(dw.dwConn, $"SELECT * FROM {dw.ActiveDatabase}.arcust WHERE remotepos > 0", spcDS, "arcust");
                keys[0] = spcDS.Tables["arcust"].Columns["remotepos"];
                spcDS.Tables["arcust"].PrimaryKey = keys;
                int tRowCount = spcDS.Tables["spcards"].Rows.Count;
                foreach (DataRow tRow in spcDS.Tables["spcards"].Rows)
                {
                    DataRow sRow = spcDS.Tables["tokendata"].Rows.Find(tRow["mediaid"].ToString());
                    string tVals = string.Empty;
                    if (sRow != null)
                    {
                        if (((tRow["chrgflag"].ToString() == "N") && (Convert.ToInt32(tRow["tokenkey"].ToString()) > 0)) || (tRow["tokenkey"].ToString() != sRow["associatedtokenid"].ToString()))
                        {
                            if (tRow["override"].ToString() == "N")
                            {
                                tVals = $"tokenkey='{sRow["associatedtokenid"].ToString()}', pmtprofile='{sRow["CustomerPaymentProfileID"].ToString()}', authnet_custid='{sRow["authnet_custid"].ToString()}', arcustid='11874', arname='ASL-PASS CHARGES', chrgflag='Y'";
                            }
                        }
                    }
                    else
                    {
                        if (tRow["nkassanr"].ToString() == "51" || tRow["nkassanr"].ToString() == "52" || tRow["nkassanr"].ToString() == "53")  //rustler, alta and snowpine lodges
                        {
                            sRow = spcDS.Tables["arcust"].Rows.Find(tRow["nkassanr"].ToString());
                            if (sRow != null)
                            {
                                tVals = $"arcustid='{sRow["arcustid"].ToString()}', arname='{sRow["szname"].ToString().Trim()}'";
                            }
                        }
                    }
                    if (tVals != string.Empty)
                    {
                        cf.ExecuteSQL(dw.dwConn, $"UPDATE {dw.ActiveDatabase}.spcards SET {tVals} WHERE recid={tRow["recid"].ToString()}");
                        System.Diagnostics.Debug.Print("XXX");
                    }
                }
            }
            finally
            {
            }
        }
    }
}
