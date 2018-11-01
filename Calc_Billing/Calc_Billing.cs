using System;
using System.Data;
using System.Windows.Forms;
using asl_SyncLibrary;
using System.Data.OleDb;

namespace Calc_Billing
{
    public partial class Calc_Billing : Form
    {
        private bool inCalc = false;
        const bool POSSumFP = false;
        const string ARTransTable = "ARTransTest";  //ARTransTest  //use test for testing.

        private CommonFunctions CF = new CommonFunctions();
        private DataWarehouse DW = new DataWarehouse();
        OleDbConnection VFPConn = new OleDbConnection("Provider=VFPOLEDB.1");

        //private string merprg;
        private DateTime mxstart = DateTime.Today.AddDays(-1);
        private string sxStart = DateTime.Today.AddDays(-1).ToString(Mirror.AxessDateFormat);
        private DateTime mxend = DateTime.Today.AddDays(-1);
        private DateTime mdate;
        string mtranstype = "";
        string marcustid = string.Empty;
        string marname = string.Empty;
        string mdesc = string.Empty;
        decimal mdebitamt = 0;
        decimal mcreditamt = 0;
        string mauthcode = string.Empty;
        private string mfname = string.Empty;
        private string mlname = string.Empty;
        private string mbilldate;
        string mserialkey = string.Empty;
        int mticktype = 0;
        int mperstype = 0;
        int mtoken = 0;
        //private bool mchgflag = false;
        //private bool mlodgeccbill  = false;
        //private bool msbchg = false;
        private string msbcode = string.Empty;
        //private Int32 srhandlet = 0;
        //private string mupdate;
        //private DateTime mstart;
        //private bool mpreview = true;
        //private string mtestemail = "darylb@alta.com";
        //private bool memailflag = false;
        //private string MSPTOT;
        //private string MCCTOT;
        //private string MTSTOT;
        //private string MLDGTOT;
        //private string martot;
        //private bool memailtest;
        //private string mend;
        //private string mxday1;
        //private string mxday2;
        //private string mskeytext;
        //private string mtempdir = @"c:\acctg\temp\";
        //private string mspsales;
        //private string mtsales;
        //private string mccsales;
        //private string mb2bsales;
        //private string mtotsales;
        //private bool mcalcacc = false;
        private bool mcalcexp = false;
        //private bool mtest = false;
        //private string mcount;
        //private string mctotal;
        //private string mexcount;
        //private string mapcount;
        //private string msalesdetcount;
        //private bool mrefundflag;
        //private string mtickexpbeg;
        //private string mtickexpend;
        //private string mbcode;
        private string mkey;
        //private string mtkey;
        //private string mfampckref;
        private DateTime mcystart;
        //private string mfound_order;
        //private string mfound_serial;
        //private string mmissing_webid;
        //private string msql;
        //private string msql2;
        //private string mwhere;
        private string mmediaid;
        private string mcardacct;
        private string mauthnet_custid;

        //private DataTable ARTrans;
        //private DataTable AXCust;
        //private DataTable PersonType;
        //private DataTable ARCust;
        //private DataTable PromptDef;
        //private DataRow SalesData;
        private DataTable PmtData;
        private DataRow SPCards;

        public Calc_Billing()
        {
            InitializeComponent();
        }

        private void BtnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void DtpFromDate_ValueChanged(object sender, EventArgs e)
        {
            if ((dtpToDate.Value < dtpFromDate.Value) && !inCalc)
            {
                inCalc = true;
                dtpToDate.Value = dtpFromDate.Value;
                inCalc = false;
            }
        }

        private void DtpToDate_ValueChanged(object sender, EventArgs e)
        {
            if ((dtpToDate.Value > dtpFromDate.Value) && !inCalc)
            {
                inCalc = true;
                dtpFromDate.Value = dtpToDate.Value;
                inCalc = false;
            }
        }

        private void Calc_Billing_Shown(object sender, EventArgs e)
        {
            mcystart = Convert.ToDateTime(CF.GetSQLField(DW.dwConn, $"SELECT start_date FROM {DW.ActiveDatabase}.season_dates ORDER BY season DESC LIMIT 1"));
            dtpFromDate.Value = DateTime.Today.AddDays(-1);
            dtpToDate.Value = dtpFromDate.Value;
        }

        private void BtnRun_Click(object sender, EventArgs e)
        {
            mxstart = dtpFromDate.Value;
            mxend = dtpToDate.Value;
            for (DateTime a = mxstart; a <= mxend; a = a.AddDays(1))
            {
                sxStart = a.ToString(Mirror.AxessDateFormat);
                Main();    
            }
        }

        private bool UpdatePossum(DateTime SaleDate, int POS, string Office, decimal ARAmt, decimal TotRec, decimal AXAR, decimal AXTot)
        {
            string sSaleDate = SaleDate.ToString(Mirror.AxessDateFormat);
            if (CF.RowExists(DW.dwConn, $"{DW.ActiveDatabase}.possum", $"POS=33 AND saledate='{sSaleDate}'"))
            {
                return CF.ExecuteSQL(DW.dwConn, $"UPDATE {DW.ActiveDatabase}.possum SET office='EXP', aramt={ARAmt.ToString()}, totrec={TotRec.ToString()}, axar={AXAR.ToString()}, axtot={AXTot.ToString()} WHERE POS=33 AND saledate='{sxStart}'");
            }
            else
            {
                return CF.ExecuteSQL(DW.dwConn, $"INSERT INTO {DW.ActiveDatabase}.possum (saledate, pos, office, aramt, totrec, axar, axtot) VALUES ('{sSaleDate}', 33, 'EXP', {ARAmt.ToString()}, {TotRec.ToString()}, {AXAR.ToString()}, {AXTot.ToString()})");
            }

        }

        private void Main()
        {
            if (chkAddCharges.Checked)
            {
                tsStatusLabel1.Text = "Loading POS Charges and Pass Charges...";
                //Open_Files();
                //AXCust = CF.LoadTable(DW.dwConn, $"SELECT * FROM {DW.ActiveDatabase}.axcust", "axcust");
                //PersonType = CF.LoadTable(DW.dwConn, $"SELECT * FROM {DW.ActiveDatabase}.persontype", "persontype");
                //PromptDef = CF.LoadTable(DW.dwConn, $"SELECT * FROM {DW.ActiveDatabase}.promptdef", "promptdef");
                PmtData = CF.LoadTable(DW.dwConn, $"SELECT saledate, transkey, nkassanr, arcustid, arname, tfactor, pmtamt, ptype, szprompt1, szprompt2, szprompt3 FROM {DW.ActiveDatabase}.pmtdata WHERE saledate = '{sxStart}' AND ptype IN ('AR','AG','WE') AND (NOT nkassanr IN (20, 33)) AND pmtamt <> 0", "pmtdata");
                Load_POS_ARs();
                //Build_Lodge_Summary();
                DataTable LodgePmtData = CF.LoadTable(DW.dwConn, $"SELECT nkassanr, SUM(pmtamt) AS TotalAmt FROM {DW.ActiveDatabase}.pmtdata WHERE saledate = '{sxStart}' AND nkassanr IN (50,51,52,53,54) GROUP BY nkassanr", "PmtData");
                foreach (DataRow LodgePmtRow in LodgePmtData.Rows)
                {
                    decimal martot = LodgePmtRow.Field<decimal>("TotalAmt");
                    UpdatePossum(LodgePmtRow.Field<DateTime>("saledate"), LodgePmtRow.Field<int>("nkassanr"), "LDG", martot, martot, martot, martot);
                }
                //ARTrans_TokenUpdate();        //update zero tokenkeys
                CF.ExecuteSQL(DW.dwConn, $"UPDATE {DW.ActiveDatabase}.{ARTransTable} AS A INNER JOIN {DW.ActiveDatabase}.spcards AS B ON A.serialkey = B.serialkey SET A.tokenkey = B.tokenkey, A.authnet_custid = B.authnet_custid, A.arcustid = '11874', A.arname = 'ASL -PASS CHARGES' WHERE (A.authcode = '') AND ((A.debitamt - A.creditamt) > 0) AND (B.tokenkey > 0)"); 
            }
            tsStatusLabel1.Text = "Calculating Card Charges...";
            mcalcexp = true;
            if (chkUseBilling.Checked)
            {
                Calc_Use_Charges();
            }

            if (chkLoadARTrans.Checked)
            {
                //Update_Sales_Files();
                //*update salesdata
                //* update pmtdata
                //* update possum

                //*** select from artrans records ***
                string msqlcmd = $"SELECT * FROM {DW.ActiveDatabase}.{ARTransTable} WHERE transdate = '{sxStart}' AND transtype = 'C' AND posnbr = 33 ORDER BY serialkey";
                DataTable artransxt = CF.LoadTable(DW.dwConn, msqlcmd, "artransxt");

                CF.ExecuteSQL(DW.dwConn, $"DELETE FROM {DW.ActiveDatabase}.pmtdata WHERE saledate = '{sxStart}' AND nkassanr = 33");
                CF.ExecuteSQL(DW.dwConn, $"DELETE FROM {DW.ActiveDatabase}.salesdata WHERE saledate = '{sxStart}' AND nkassanr = 33");
                decimal mtot = 0;
                decimal mamt = 0;
                foreach (DataRow ARTransXTRow in artransxt.Rows)
                {
                    mamt = ARTransXTRow.Field<decimal>("debitamt") - ARTransXTRow.Field<decimal>("creditamt");
                    mtot += mamt;
                    int mtfactor = (mamt <= 0 ? mtfactor = -1 : 1);
                    string museticktype = (ARTransXTRow.Field<int?>("useticktype").HasValue ? museticktype = ARTransXTRow["useticktype"].ToString() : ")");
                    string musetickdesc = ARTransXTRow["usetickdesc"].ToString();
                    string mmedia = (!string.IsNullOrEmpty(ARTransXTRow.Field<string>("mediaia")) ? mmedia = ARTransXTRow.Field<string>("mediaia") : string.Empty);
                    string MFNAME = CF.EscapeChar(ARTransXTRow["firstname"].ToString());
                    string MLNAME = CF.EscapeChar(ARTransXTRow["lastname"].ToString());
                    //* **insert pmtdata reccord ***
                    msqlcmd = $"INSERT INTO {DW.dwConn}.pmtdata (transkey,transpkey,pmtkey,saledate,arcustid,nprojnr,nkassanr,office,ntransnr,nlfdnr,ntransstat,tfactor,pmtamt,ptype,arname,tgroup,fname,lname,pstatus) " +
                        $"VALUES ('{ARTransXTRow["transkey"].ToString()}', '{ARTransXTRow["transkey"].ToString()}', '106-1', '{ARTransXTRow.Field<DateTime>("transdate").ToString(Mirror.AxessDateFormat)}', " +
                        $"'{ARTransXTRow["arcustid"].ToString()}', 705, 33, 'POE', {ARTransXTRow.Field<int>("ntransnr").ToString()}, 0, 3, {mtfactor.ToString()}, " +
                        $"{mamt.ToString()}, 'AR', '{ARTransXTRow[".arname"].ToString()}', '{ARTransXTRow["tgroup"].ToString()}', '{MFNAME}', '{MLNAME}', 'Calculated Use Charge')";
                    CF.ExecuteSQL(DW.dwConn, msqlcmd);
                    //*** insert salesdata record ***
                    msqlcmd = $"INSERT INTO {DW.ActiveDatabase}.salesdata (transkey,serialkey,saledate,ntranstype,ttype,nkassanr,ntransnr,nblocknr,tgroup,tfactor,tariff,fname,lname,persdesc, nperstype, qty,mediaid,nticktype, tickdesc) " +
                        $" VALUES ('{ARTransXTRow["transkey"].ToString()}', '{ARTransXTRow["serialkey"].ToString()}', '{ARTransXTRow.Field<DateTime>("transdate").ToString(Mirror.AxessDateFormat)}', " +
                        $" 0, 'S', 33, {ARTransXTRow.Field<int>("ntransnr").ToString()}, 0, '{ARTransXTRow["tgroup"].ToString()}', {mtfactor.ToString()}, {mamt.ToString("#0.00")}, '{MFNAME}', " +
                        $"{MLNAME}, '{ARTransXTRow["persdesc"].ToString()}', {ARTransXTRow.Field<int>("nperstype").ToString()}, 1, '{mmedia}', '{museticktype}', '{musetickdesc}')";
                    CF.ExecuteSQL(DW.dwConn, msqlcmd);
                }
                //* MAKE POSSUM RECORD
                if (mtot != 0)
                {
                    UpdatePossum(mxstart, 33, "EXP", mtot, mtot, mtot, mtot);
                }

            }
            if (chkDisplayReport.Checked)
            {
                DataTable ARTrans = CF.LoadTable(DW.dwConn, "SELECT * FROM { DW.ActiveDatabase}.artrans WHERE(arcustid IN ('11874', '') OR (arcustid IS null)) AND (authcode = '' OR authcode IS null) AND ((debitamt + creditamt) > 0) AND (transdate > '{mcystart.ToString(Mirror.AxessDateFormat)}' ORDER BY tokenkey, transdate DESC", "ARTrans");
                //build and print "UNBILLED RESORT CHARGES"   (rcunbilled)
            }
        }

        private void Load_POS_ARs()
        {
            DateTime mtransdate = CF.defaultDOB;
            marname = string.Empty;
            mdesc = string.Empty;
            mdebitamt = 0;
            mcreditamt = 0;
            mauthcode = string.Empty;
            mtoken = 0;
            mserialkey = string.Empty;
            mticktype = 0;
            mperstype = 0;
            //*** load ar charges data***
            foreach (DataRow PmtRowX in PmtData.Rows)
            {
                // Load Salesdata matching pmtrow on transkey
                DataRow SalesRow = CF.LoadDataRow(DW.dwConn, $" SELECT fname, lname, tickdesc, persdesc, serialkey, transkey, nperstype, nticktype, nkassanr, tgroup FROM {DW.ActiveDatabase}.salesdata WHERE transkey = '{PmtRowX["transkey"].ToString()}' LIMIT 1");

                //****************
                //*BUILD MEMORY VARIABLES TO INSERT INTO ARTRANS***
                //****************
                mfname = string.Empty;
                mlname = string.Empty;
                mcardacct = string.Empty;
                mtranstype = string.Empty;
                mtransdate = CF.defaultDOB;
                mdebitamt = 0;
                mcreditamt = 0;
                mauthcode = string.Empty;
                mbilldate = PmtRowX.Field<DateTime>("saledate").ToString(Mirror.AxessDateFormat);
                mdesc = string.Empty;
                mtoken = 0;
                mauthnet_custid = "0";
                mmediaid = string.Empty;
                marname = string.Empty;
                mserialkey = string.Empty;
                mticktype = 0;
                mperstype = 0;
                marcustid = string.Empty;
                mkey = PmtRowX["transkey"].ToString();
                if (!string.IsNullOrEmpty(PmtRowX["arcustid"].ToString()))
                {
                    marcustid = PmtRowX["arcustid"].ToString();
                    marname = PmtRowX["arname"].ToString();
                }
                if (PmtRowX.Field<int>("tfactor") == -1)
                {
                    mcreditamt = (PmtRowX.Field<decimal>("pmtamt") * -1);
                }
                else
                {
                    mdebitamt = PmtRowX.Field<decimal>("pmtamt");
                }
                mbilldate = (PmtRowX.Field<DateTime>("saledate")).ToString(Mirror.AxessDateFormat);
                bool mcont = false;



                //data from SalesData or elsewhere
                mdesc = SalesRow["tickdesc"].ToString();
                if (SalesRow.Field<string>("fname").Length > 0)
                {
                    mfname = SalesRow.Field<string>("fname");
                }
                if (SalesRow.Field<string>("lname").Length > 0)
                {
                    mlname = SalesRow.Field<string>("lname");
                }
                if ((SalesRow.Field<string>("serialkey").Length > 0) && (mlname == string.Empty))
                {
                    SPCards = CF.LoadDataRow(DW.dwConn, $"SELECT firstname, lastname FROM {DW.ActiveDatabase}.spcards WHERE serialkey = '{SalesRow.Field<string>("serialkey")}'");
                    if (SPCards != null)
                    {
                        mfname = SPCards["firstname"].ToString();
                        mlname = SPCards["lastname"].ToString();
                    }
                }

                if (PmtRowX["ptype"].ToString() == "WE") //web sale
                {
                    mkey = SalesRow["transkey"].ToString();
                    mtranstype = "I";
                    mcont = true;
                    mserialkey = SalesRow["serialkey"].ToString();
                    marcustid = "1206";
                    marname = "ESTORE";
                    mauthcode = marcustid;
                    mticktype = 0;
                    if (SalesRow.Field<int?>("nticktype").HasValue)
                    {
                        mticktype = SalesRow.Field<int>("nticktype");
                    }
                    if (SalesRow.Field<int?>("nperstype").HasValue)
                    {
                        mperstype = SalesRow.Field<int>("nperstype");
                    }
                }
                else
                {
                    if ((PmtRowX["ptype"].ToString() == "AG") && (PmtRowX.Field<decimal>("pmtamt") != 0))
                    {
                        switch (PmtRowX["szprompt1"].ToString())
                        {
                            case "LC":

                                mkey = SalesRow["transkey"].ToString();
                                mtranstype = "Z";
                                mcont = true;
                                mfname = PmtRowX["szprompt2"].ToString().Substring(0, (PmtRowX["szprompt2"].ToString() + " ").ToString().IndexOf(' '));
                                mlname = PmtRowX["szprompt2"].ToString().Substring(mfname.Length);
                                if (mlname.Length > 20)
                                {
                                    mlname = mlname.Substring(0, 20);
                                }
                                mlname += "   " + PmtRowX["szprompt3"].ToString();
                                if (mlname.Length > 25)
                                {
                                    mlname = mlname.Substring(0, 25);
                                }
                                mdesc = "Lodge Room Charge  " + SalesRow["tickdesc"].ToString();
                                mauthcode = marcustid;
                                break;

                            case "PC":
                            case "FP":
                                mkey = SalesRow["transkey"].ToString();
                                mtranstype = "Z";
                                mcont = true;
                                mmediaid = PmtRowX["szprompt3"].ToString();
                                mfname = PmtRowX["szprompt2"].ToString().Substring(0, (PmtRowX["szprompt2"].ToString() + " ").ToString().IndexOf(' '));
                                mlname = PmtRowX["szprompt2"].ToString().Substring(mfname.Length);
                                if (mlname.Length > 20)
                                {
                                    mlname = mlname.Substring(0, 20);
                                }
                                if (mlname.Length > 20)
                                {
                                    mlname = mlname.Substring(0, 20);
                                }
                                mauthcode = string.Empty;
                                string mxkey = PmtRowX["szprompt3"].ToString();
                                SPCards = CF.LoadDataRow(DW.dwConn, $"SELECT firstname, lastname, tokenkey, authnet_custid,empid, serialkey, cardacct FROM {DW.ActiveDatabase}.spcards WHERE mediaid = '{mxkey}'");
                                if (SPCards != null)
                                {
                                    mfname = CF.EscapeChar(SPCards["firstname"].ToString());
                                    mlname = CF.EscapeChar(SPCards["lastname"].ToString());
                                    mtoken = SPCards.Field<Int32>("tokenkey");
                                    mauthnet_custid = SPCards["authnet_custid"].ToString();
                                    if (mtoken != 0)
                                    {
                                        mbilldate = "00/00/0000";
                                    }
                                    if (SPCards["empid"].ToString() != string.Empty && SPCards.Field<int>("tokenkey") == 0)
                                    {
                                        mauthcode = SPCards["empid"].ToString();
                                        marcustid = "11521";
                                        marname = "ASL - EMPLOYEE CHARGES";
                                    }
                                }
                                mserialkey = SalesRow["serialkey"].ToString();
                                mcardacct = SPCards["cardacct"].ToString();
                                if (PmtRowX["szprompt1"].ToString() == "FP")
                                {
                                    mdesc = "Forgot Pass Charge";
                                }
                                break;
                            case "RS":
                                mkey = SalesRow["transkey"].ToString();
                                mtranstype = "T";
                                mcont = true;
                                mfname = PmtRowX["szprompt2"].ToString().Substring(0, (PmtRowX["szprompt2"].ToString() + " ").ToString().IndexOf(' '));
                                mlname = PmtRowX["szprompt2"].ToString().Substring(mfname.Length);
                                if (mlname.Length > 20)
                                {
                                    mlname = mlname.Substring(0, 20);
                                }
                                if (mlname.Length > 20)
                                {
                                    mlname = mlname.Substring(0, 20);
                                }
                                mdesc = "Tickets/Ski School Purchase " + mkey;
                                mauthcode = marcustid;
                                break;
                        }
                    }
                    else
                    {
                        if (SalesRow.Field<int>("nticktype") == 4001)  //AR Payment
                        {
                            mkey = SalesRow["transkey"].ToString();
                            mtranstype = "X";
                            mcont = true;
                            mauthcode = marcustid;
                            if (PmtRowX.Field<int>("tfactor") == -1)
                            {
                                mcreditamt = (PmtRowX.Field<int>("pmtamt") * -1);
                            }
                            else
                            {
                                mdebitamt = PmtRowX.Field<int>("pmtamt");
                            }
                        }
                        else
                        {
                            if (PmtRowX.Field<decimal>("pmtamt") != 0)
                            {
                                mcont = true;
                                mauthcode = marcustid;
                                mserialkey = SalesRow["serialkey"].ToString();
                                mkey = SalesRow["transkey"].ToString();
                                mtranstype = (SalesRow.Field<int>("nkassanr") >= 50 && SalesRow.Field<int>("nkassanr") <= 54) ? "L" : "T";
                                mticktype = SalesRow.Field<int?>("nticktype").HasValue ? 0 :SalesRow.Field<int>("nticktype");
                                mperstype = SalesRow.Field<int?>("nperstype").HasValue ? 0 : SalesRow.Field<int>("nperstype");
                            }
                        }
                    }
                }
                mdesc = CF.EscapeChar(mdesc);
                mfname = CF.EscapeChar(mfname.Replace(",", " "));
                mlname = CF.EscapeChar(mlname.Replace(",", " "));
                if (mcont)
                {
                    DataRow ARTransRow = CF.LoadDataRow(DW.dwConn, $"SELECT arcustid, transtype, debitamt, creditamt, authcode, tokenkey from {DW.ActiveDatabase}.{ARTransTable} WHERE transtype <> 'C' AND transkey='{mkey}'");
                    if (ARTransRow["arcustid"].ToString() != string.Empty)
                    {
                        if ((marcustid != ARTransRow["arcustid"].ToString()) || (mtranstype != ARTransRow["transtype"].ToString()) || (mdebitamt != ARTransRow.Field<decimal>("debitamt")) || (mcreditamt != ARTransRow.Field<decimal>("creditamt")) ||  (ARTransRow["authcode"].ToString() == string.Empty  && mtoken != ARTransRow.Field<int>("tokenkey")))
                        {
                            ARTrans_Update(ARTransRow, PmtRowX, SalesRow);
                        }
                    }
                    else
                    {
                        mdate = PmtRowX.Field<DateTime>("saledate");
                        ARTrans_Insert(ARTransRow, PmtRowX, SalesRow);
                    }
                }
            }



            //** add ar payments **

            DataTable SalesData = CF.LoadTable(DW.dwConn, $"SELECT * FROM {DW.ActiveDatabase}.salesdata WHERE nticktype = 4001 AND saledate = '{sxStart}' AND NOT (nkassanr IN (20, 33))", "salesdata");
            foreach (DataRow SalesRow in SalesData.Rows)
            {
                if (SalesRow.Field<decimal>("tariff") != 0)
                {
                    mkey = SalesRow["transkey"].ToString();
                    DataRow PmtRow = CF.LoadDataRow(DW.dwConn, $"SELECT * FROM {DW.ActiveDatabase}.pmtdata WHERE transkey = '{mkey}'");
                    mtranstype = "X";
                    mdebitamt = 0;
                    mcreditamt = 0;
                    mbilldate = SalesRow.Field<DateTime>("saledate").ToString(Mirror.AxessDateFormat);
                    mtransdate = SalesRow.Field<DateTime>("saledate");
                    mdesc = SalesRow["tickdesc"].ToString();
                    mcardacct = string.Empty;
                    mtoken = 0;
                    mauthnet_custid = "0";
                    mmediaid = string.Empty;
                    mserialkey = SalesRow["serialkey"].ToString();
                    mticktype = SalesRow.Field<int>("nticktype");
                    mperstype = SalesRow.Field<int>("nperstype");
                    marcustid = string.Empty;
                    marname = string.Empty;
                    if (PmtRow.Field<string>("arcustid").Trim().Length != 0)
                    {
                        marcustid = PmtRow["arcustid"].ToString();
                        marname = PmtRow["arname"].ToString();
                    }
                    mdesc = SalesRow["tickdesc"].ToString();
                    mauthcode = marcustid;
                    mfname = CF.EscapeChar(SalesRow["fname"].ToString());
                    mlname = CF.EscapeChar(SalesRow["lname"].ToString());
                    if (SalesRow.Field<int>("tfactor") == 1)
                    {
                        mcreditamt = SalesRow.Field<decimal>("tariff");
                    }
                    else
                    {
                        mdebitamt = SalesRow.Field<decimal>("tariff");

                    }

                    DataRow ARTransRow = CF.LoadDataRow(DW.dwConn, $"SELECT arcustid, transtype, debitamt, creditamt, authcode, tokenkey from {DW.ActiveDatabase}.{ARTransTable} WHERE transtype <> 'C' AND transkey='{mkey}'");
                    if (ARTransRow["arcustid"].ToString() != string.Empty)
                    {
                        if ((marcustid != ARTransRow["arcustid"].ToString()) || (mtranstype != ARTransRow["transtype"].ToString()) || (ARTransRow["authcode"].ToString() == string.Empty && mtoken != ARTransRow.Field<int>("tokenkey")))
                        {
                            ARTrans_Update(ARTransRow, PmtRow, SalesRow);
                        }
                    }
                    else
                    {
                        mdate = PmtRow.Field<DateTime>("saledate");
                        ARTrans_Insert(ARTransRow, PmtRow, SalesRow);
                    }
                }
            }
        }

        //  * nzutrnr field in Axess
        //  * 1 - Collins
        //  * 2 - Collins Angle
        //  * 3 - Sunnyside
        //  * 4 - Albion
        //  * 5 - Wildcat
        //  * 6 - Supreme
        //  * 7 - Sugarloaf
        //  * 8 - Sugarpass to Alta
        //  * 9 - Sugarpass to Snowbird
        //  * 61-90 - Snowbird

        private void Calc_Use_Charges()
        {
            //DataTable Pmts = CF.LoadTable(DW.dwConn, $"SELECT * FROM {DW.ActiveDatabase}.pmtdata WHERE saledate = '{sxStart}' ORDER BY transkey", "PmtData");
            //DataTable Sales = CF.LoadTable(DW.dwConn, $"SELECT * FROM {DW.ActiveDatabase}.salesdata WHERE saledate = '{mxstart.ToString()}' ORDER BY serialkey", "SalesData");
            //DataTable SkiVisits = CF.LoadTable(DW.dwConn, $"SELECT * from {DW.ActiveDatabase}.skivisits WHERE readdate = '{sxStart}' AND nkassanr < 60 ORDER BY serialkey", "SkiVisits");
            //Update_SPCards_Flags
            //disabled until we figure out if needed.
            //CF.ExecuteSQL(DW.dwConn, $"UPDATE {DW.ActiveDatabase}.spcards SET amflag = 'N', begflag = 'N', altaflag = 'N', sbflag = 'N', useflag = 'N', lateflag = 'N', at3flag = 'N', aramt = 0, sbamt = 0, usedate=null, uticktype = 0, usedesc = '' WHERE tgroup IN ('CC', 'MC', 'EX')");
            //CF.ExecuteSQL(DW.dwConn, $"UPDATE {DW.ActiveDatabase}.spcards AS A INNER JOIN {DW.ActiveDatabase}.skivisits AS B ON A.serialkey = B.serialkey " +
            //    $"SET A.begflag = B.begflag, A.sbflag = B.sbflag, A.altaflag = B.altaflag, A.amflag=B.amflag, A.lateflag=B.lateflag, A.at3flag=B.at3flag, A.usedate = '{sxStart}' " +
            //    $"WHERE ((A.tgroup = 'EX' AND A.cardstatus = 'A') OR (A.tgroup = 'CC' AND A.cardstatus IN ('A', 'P', 'X')) OR (A.tgroup = 'MC' AND A.nticktype = 2025 AND A.mcpnbr <> '' AND A.cardstatus = 'A') " +
            //        $"AND B.mcpnbr LIKE 'MCP%' AND B.readdate <= '{sxStart}' GROUP BY B.mcpnbr, B.readdate");
            //CF.ExecuteSQL(DW.dwConn, $"UPDATE {DW.ActiveDatabase}.spcards SET useflag = 'Y' WHERE altaflag = 'Y' OR sbflag = 'Y'");
            if (mcalcexp)
            {
                //calc_charges  update express and goldcard files with usage
                DataTable SPCharges = CF.LoadTable(DW.dwConn, $"SELECT * FROM {DW.ActiveDatabase}.spcards WHERE tgroup IN ('CC','EX','MC') AND (saledate >= '{sxStart}' AND expdate >= '{sxStart}' AND useflag = 'Y' ORDER BY serialkey", "SPCharges");
                if (SPCharges != null)
                {
                    foreach (DataRow SPChargeRow in SPCharges.Rows)
                    {
                        if (SPChargeRow["cardstatus"].ToString() == "A" || (SPChargeRow["tgroup"].ToString() == "CC" && (SPChargeRow["cardstatus"].ToString().IndexOfAny(new char[] { 'A', 'P', 'X' }) > 0)))
                        {
                            mkey = (SPChargeRow.Field<int>("nticktype") + 10000).ToString() + (SPChargeRow.Field<int>("nperstype") + 1000);
                            DataRow ExpressType = CF.LoadDataRow(DW.dwConn, $"SELECT * FROM {DW.ActiveDatabase}.expresstype WHERE ticktype = {SPChargeRow["nticktype"].ToString()} AND perstype = {SPChargeRow["nperstype"].ToString()}");
                            if (ExpressType != null)
                            {
                                int mticktype = 0;
                                if ((SPChargeRow["cardstatus"].ToString() == "A" && SPChargeRow["useflag"].ToString() == "Y") || (SPChargeRow["tgroup"].ToString() == "CC" && (SPChargeRow["cardstatus"].ToString().IndexOfAny(new char[] { 'A', 'P', 'X' }) > 0)))
                                {
                                    DateTime msaledate = SPChargeRow.Field<DateTime>("saledate");
                                    //calc_amt_due
                                    string mski3free = "N";
                                    decimal mmaxamt = 0;
                                    decimal msbupamt = 0;
                                    string msbdesc = string.Empty;
                                    //SELECT spcards
                                    if (ExpressType.Field<bool>("dayonly"))
                                    {
                                        mticktype = 1; //&& Area Day
                                    }
                                    else
                                    {
                                        if ((SPChargeRow["lateflag"].ToString() == "Y") && (SPChargeRow["sbflag"].ToString() == "N"))
                                        {
                                            mticktype = 14; //&& Late pass only
                                            if ((SPChargeRow["at3flag"].ToString() == "Y") && SPChargeRow["begflag"].ToString() == "Y")
                                            {
                                                mticktype = 130;
                                                int nticktype = SPChargeRow.Field<int>("nticktype");
                                                if ((nticktype == 2016 || nticktype == 2018 || nticktype == 2019) && mticktype == 130)
                                                {
                                                    mski3free = "Y";
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if ((SPChargeRow["begflag"].ToString() == "Y") && (SPChargeRow["sbflag"].ToString() == "N"))
                                            {
                                                mticktype = 4; //&& Beginner Lifts Only
                                            }
                                            else
                                            {
                                                if ((SPChargeRow["amflag"].ToString() == "N") && (SPChargeRow["sbflag"].ToString() == "N"))
                                                {
                                                    mticktype = 3; //&& PM Only
                                                }
                                                else
                                                {
                                                    mticktype = 1; //&& Area Day
                                                    if (ExpressType.Field<bool>("multiflag"))
                                                    {
                                                        //DO check_multiday_use  
                                                        string msqlcmd = $"SELECT readdate, altaflag, amflag, sbflag FROM applications.skivisits WHERE serialkey = '{SPChargeRow["serialkey"].ToString()}' ORDER BY readdate DESC";
                                                        DataTable CardUse = CF.LoadTable(DW.dwConn, msqlcmd, "CardUse");
                                                        if (CardUse.Rows.Count > 0)
                                                        {
                                                            bool mckflag = true;
                                                            int mxdays = 0;
                                                            DateTime mcurdate = mxstart;
                                                            bool mfreedayused = false;
                                                            foreach (DataRow CardUseRow in CardUse.Rows)
                                                            {
                                                                if (!mckflag)
                                                                    break;
                                                                if (CardUseRow.Field<DateTime>("readdate") > (mcurdate.AddDays(-3)))
                                                                {
                                                                    if (CardUseRow.Field<DateTime>("readdate") == (mcurdate.AddDays(-1)))
                                                                    {
                                                                        if (CardUseRow["altaflag"].ToString() == "Y" && (CardUseRow["amflag"].ToString() == "Y" || CardUseRow["sbflag"].ToString() == "Y"))
                                                                        {
                                                                            mxdays += 1;
                                                                        }
                                                                        else
                                                                        {
                                                                            mfreedayused = true;
                                                                        }
                                                                        mcurdate = CardUseRow.Field<DateTime>("readdate");
                                                                    }
                                                                    else
                                                                    {
                                                                        //****allow free days ****
                                                                        if ((CardUseRow.Field<DateTime>("readdate") == mcurdate.AddDays(-2)) && !mfreedayused)
                                                                        {
                                                                            if (CardUseRow["altaflag"].ToString() == "Y" && (CardUseRow["amflag"].ToString() == "Y" || CardUseRow["sbflag"].ToString() == "Y"))
                                                                            {
                                                                                mxdays = mxdays + 1;
                                                                                mcurdate = CardUseRow.Field<DateTime>("readdate");
                                                                                mfreedayused = true;
                                                                            }
                                                                            else
                                                                            {
                                                                                mckflag = false;
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    mckflag = false;
                                                                }
                                                            }
                                                            if (mxdays > 1)
                                                            {
                                                                mticktype = 33; // && Multiday 3 - 4 Day Pass
                                                                if (mxdays == 4 || mxdays == 5)
                                                                {
                                                                    mticktype = 35; // && Multiday 5 - 6 Pass
                                                                }
                                                                if (mxdays > 5)
                                                                {
                                                                    mticktype = 37; // && Multiday 7 days plus
                                                                }
                                                            }
                                                        }
                                                    }   //end check_multiday_use
                                                }
                                            }
                                        }
                                    }
                                    //** Mountain Collective
                                    if (SPChargeRow["tgroup"].ToString() == "MC")
                                    {
                                        mticktype = 1; //&& Alta
                                    }
                                    DataTable ExpPrices = CF.LoadTable(DW.dwConn, $"SELECT nticktype, tickdesc, aprice, cprice, sba, sbc, pos, 0 AS amtdue, 0 as sbamtdue, aprice2, cprice2 FROM {DW.ActiveDatabase}.expprices", "ExpPrices");
                                    DataRow ExpCfg = CF.LoadDataRow(DW.dwConn, $"SELECT * FROM {DW.ActiveDatabase}.expcfg");
                                    //    ***load prices based on date range
                                    string mfield = $"{ExpressType["ratecode"].ToString()}price2";
                                    string mfield2 = "disc2amt";
                                    if (mxstart < ExpCfg.Field<DateTime>("sdate") || mxstart > ExpCfg.Field<DateTime>("edate"))
                                    {
                                        mfield.Replace("2", "");
                                        mfield2.Replace("2", "");
                                    }
                                    string msbfield = $"sb{ExpressType["ratecode"]}";
                                    DataRow ExpPrice = CF.LoadDataRow(DW.dwConn, $"SELECT * FROM {DW.ActiveDatabase}.expprices WHERE nticktype={mticktype.ToString()}");
                                    decimal Field1Val = ExpPrice.Field<decimal>(mfield);
                                    decimal Field2Val = ExpressType.Field<decimal>(mfield2);
                                    decimal maxdayrate = Field1Val * (Field2Val / 100);
                                    //*** caculate amount due ***
                                    decimal mamtdue = 0;
                                    if (ExpressType["disctype"].ToString() == "P")
                                    {
                                        mamtdue = maxdayrate;
                                    }
                                    else
                                    {
                                        mamtdue = Field1Val + Field2Val;
                                    }
                                    decimal msbamtdue = ExpPrice.Field<decimal>(msbfield);
                                    //* **check if Montain Collective Free Day***
                                    if (SPChargeRow.Field<int>("freedays") > 0 && SPChargeRow["tgroup"].ToString() == "MC")
                                    {
                                        // DO check_card_use
                                        mkey = SPChargeRow["mcpnbr"].ToString();
                                        int DaysSkied = Convert.ToInt32(CF.GetSQLField(DW.dwConn, $"SELECT COUNT(*) AS daysskied FROM (SELECT mcpnbr, readdate FROM {DW.ActiveDatabase}.skivisits WHERE mcpnbr LIKE 'MCP%' AND readdate <= '{sxStart}' WHERE mcpnbr='{mkey}' GROUP BY mcpnbr, readdate ORDER BY mcpnbr) AS mcpuses GROUP BY mcpnbr ORDER BY mcpnbr"));
                                        if (DaysSkied <= SPChargeRow.Field<int>("freedays"))
                                        {
                                            mamtdue = 0;
                                        }
                                    }
                                    //* deduct reload rate for convenience cards
                                    if (ExpressType.Field<int>("ticktype") == 30037 && (mticktype < 30 || mticktype > 39))
                                    {
                                        mamtdue = mamtdue - 3;
                                    }
                                    if (mamtdue < 0)
                                    {
                                        mamtdue = 0;
                                    }
                                    //* ski at three free
                                    if (mski3free == "Y")
                                    {
                                        mamtdue = 0;
                                    }
                                    //*** calcualte alta snowbird upgrades for convenience cards ****
                                    if (SPChargeRow["sbflag"].ToString() == "Y" && SPChargeRow["tgroup"].ToString() != "MC")
                                    {
                                        mticktype = (SPChargeRow["sbflag"].ToString() == "Y" && SPChargeRow["altaflag"].ToString() == "N") ? 50 : 30014;
                                        if (mxstart >= ExpCfg.Field<DateTime>("sdate") && mxstart <= ExpCfg.Field<DateTime>("edate"))
                                        {
                                            mfield = "expprices." + ExpressType.Field<decimal>("ratecode") + "price2";
                                            mfield2 = "expresstype.disc2amt";
                                        }
                                        else
                                        {
                                            mfield = "expprices." + ExpressType.Field<decimal>("ratecode") + "price";
                                            mfield2 = "expresstype.discamt";
                                        }
                                        msbfield = "expprices.sb" + ExpressType["ratecode"].ToString();
                                        maxdayrate = ExpPrice.Field<decimal>(mfield) * (ExpressType.Field<decimal>(mfield2) / 100);
                                        if (mticktype == 30014)
                                        {
                                            mmaxamt = maxdayrate; //&& altasnowbird max
                                            mticktype = 30032; //&& Alta Snowbird Upgrade
                                            ExpPrice = CF.LoadDataRow(DW.dwConn, $"SELECT * FROM {DW.ActiveDatabase}.expprices WHERE nticktype={mticktype}");
                                            if (!(ExpPrice is null))
                                            {
                                                msbupamt = maxdayrate;
                                                msbdesc = ExpPrice["tickdesc"].ToString();
                                            }
                                            mamtdue = mamtdue + msbupamt;
                                            if (mamtdue > mmaxamt)
                                            {
                                                mamtdue = mmaxamt;
                                            }
                                        }
                                        else
                                        {
                                            mamtdue = maxdayrate; //&& snowbird only
                                        }
                                    }
                                    //* copper card deal - spring of 2016 04 / 04 / 2016 - 04 / 08 / 2016 * *********************
                                    if (SPChargeRow.Field<int>("nticktype") == 2018 && (mticktype == 3 || mticktype == 4 || mticktype == 14) && (mxstart >= new DateTime(2016, 04, 04) && mxstart <= new DateTime(2016, 04, 08)))
                                    {
                                        mamtdue = 10;
                                    }
                                    CF.ExecuteSQL(DW.dwConn, $"UPDATE {DW.ActiveDatabase}.expprices SET amtdue={mamtdue.ToString()}, sbamtdue={msbamtdue.ToString()} WHERE nticktype = {mticktype}");
                                    string Expfield = string.Empty;
                                    if (SPChargeRow.Field<bool>("sbflag"))
                                    {
                                        Expfield = ", sb" + ExpressType["ratecode"].ToString();
                                    }
                                    CF.ExecuteSQL(DW.dwConn, $"UPDATE {DW.ActiveDatabase}.spcards SET aramt = {ExpPrice["amtdue"].ToString()}, sbamt={ExpPrice["sbamtdue"].ToString()}, usedesc='{ExpPrice["tickdesc"].ToString()}', usedate='{sxStart}', uticktype={ExpPrice["nticktype"].ToString()}{Expfield}");
                                }
                            }
                        }
                    }
                }
            }
        }

        private void ARTrans_Update(DataRow ARTran, DataRow PmtRow, DataRow SaleRow)
        {
            //**mark orginal AR as Adjusted**
            if (CF.ExecuteSQL(DW.dwConn, $"update {DW.ActiveDatabase}.{ARTransTable} set transkey='{ARTran["transkey"].ToString()}-A1', dtupdate='{DateTime.Now.ToString(Mirror.AxessDateTimeFormat)}' WHERE recid={ARTran["recid"].ToString()}"))
            {
                //**insert record for adjustment
                string mtranskey = $"{ARTran["transkey"].ToString()}-A2";
                string mxauthcode = ARTran["authcode"].ToString();
                DateTime? mbilldate = null;
                if (mxauthcode != string.Empty && ARTran.Field<int>("tokenkey") > 0 )
                {
                    mxauthcode = string.Empty;
                }
                else
                {
                    mbilldate = ARTran.Field<DateTime>("billdate");
                }
                string sBillDate = (mbilldate.HasValue ? "null" : $"'{mbilldate.Value.ToString(Mirror.AxessDateFormat)}'");
                if (CF.ExecuteSQL(DW.dwConn, $"INSERT INTO {DW.ActiveDatabase}.{ARTransTable} (transdate,posnbr,transkey,transtype,arcustid,arname,transdesc,debitamt,creditamt," +
                    $"authcode,billdate,serialkey,cardacct,mediaid,firstname,lastname,tokenkey,authnet_custid,tgroup,nticktype,tickdesc,nperstype,persdesc,dtupdate)" +
                    $" Values ('{ARTran.Field<DateTime>("transdate").ToString(Mirror.AxessDateFormat)}',{PmtRow.Field<int>("nkassanr").ToString()},'{mtranskey}'," +
                    $"'{ARTran["transtype"].ToString()}','{ARTran["arcustid"].ToString()}','{ARTran["arname"].ToString()}','{ARTran["transdesc"].ToString()}'," +
                    $"{ARTran.Field<decimal>("mcreditamt").ToString("0#.00")},{ARTran.Field<decimal>("mdebitamt").ToString("0#.00")},'{mxauthcode}',{sBillDate}," +
                    $"'{ARTran["serialkey"].ToString()}','{ARTran["cardacct"].ToString()}','{ARTran["mediaid"].ToString()}','{mfname}','{mlname}'," +
                    $"{ARTran.Field<int>("tokenkey").ToString()},{ARTran.Field<int>("authnet_custid").ToString()},'{ARTran["tgroup"].ToString()}'," +
                    $"{ARTran.Field<int>("nticktype").ToString()},'{ARTran["tickdesc"].ToString()}',{ARTran.Field<int>("nperstype").ToString()}, " +
                    $"'{ARTran["persdesc"].ToString()}','{DateTime.Now.ToString(Mirror.AxessDateFormat)}')"))
                {
                    ARTrans_Insert(ARTran, PmtRow, SaleRow);
                }
            }
        }

        private void ARTrans_Insert(DataRow ARTransRow, DataRow PmtRow, DataRow SaleRow)
        {
            string mtickdesc = SaleRow["tickdesc"].ToString();
            string mpersdesc = SaleRow["persdesc"].ToString();
            //* *insert artrans record **
            string msqlcmd = $"INSERT INTO {DW.ActiveDatabase}.{ARTransTable} (transdate, posnbr, transkey, transtype, arcustid, arname, transdesc, debitamt, creditamt, " +
                $"authcode, billdate, serialkey, cardacct, mediaid, firstname, lastname, tokenkey, authnet_custid, tgroup, nticktype, tickdesc, " +
                $"nperstype, persdesc, dtupdate) Values('{PmtRow.Field<DateTime>("saledate").ToString(Mirror.AxessDateFormat)}',{PmtRow.Field<int>("nkassanr").ToString()}," +
                $"'{mkey}','{mtranstype}','{marcustid}','{marname}','{mdesc}',{mdebitamt.ToString("0#.00")},{mcreditamt.ToString("0#.00")},'{mauthcode}'," +
                $"'{mbilldate}','{mserialkey}','{mcardacct}','{mmediaid}','{mfname}','{mlname}',{mtoken.ToString()},{mauthnet_custid.ToString()}," +
                $"'{SaleRow["tgroup"].ToString()}',{mticktype},'{mtickdesc}',{mperstype},'{mpersdesc}','{mdate.ToString(Mirror.AxessDateTimeFormat)}')";
            CF.ExecuteSQL(DW.dwConn, msqlcmd);
        }
    }
}
