using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using asl_SyncLibrary;

namespace TokenXRefSync
{
    public partial class TokenXRefSync : Form
    {
        Mirror AM = new Mirror();
        BUY_ALTA_COM BUY = new BUY_ALTA_COM();
        CommonFunctions CF = new CommonFunctions();
        DataWarehouse DW = new DataWarehouse();

        private DateTime msaledate = new DateTime(DateTime.Now.Year - (DateTime.Now.Month <= 6 ? 1 : 0), 7, 1);

        public TokenXRefSync()
        {
            InitializeComponent();
        }

        public void Update_All(System.Windows.Forms.StatusStrip aStatusStrip = null, System.Windows.Forms.ToolStripProgressBar aProgressBar = null)
        {
            Cursor OldCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            Update_mediaid();
            Update_tokenxref();
            Update_salesdata();
            Cursor.Current = OldCursor;
        }

        public void Update_mediaid()
        {
            Cursor OldCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            SslStatus.Text = "Loading SalesData for MediaID Update...";
            using (DataTable TblSalesData = CF.LoadTable(DW.dwConn, $"SELECT nlfdzaehler, mediaid, wtp64, wtp32, nkartennr FROM {DW.ActiveDatabase}.salesdata WHERE saledate>='{DateTime.Now.AddDays(-300).ToString(Mirror.AxessDateFormat)}' AND (tokenkey=0)  AND (tgroup IN ('EX','SP')) ORDER BY nlfdzaehler", "axsales"))
            {
                if (TblSalesData != null)
                {
                    PbUpdateMediaID.Value = 0;
                    PbUpdateMediaID.Maximum = TblSalesData.Rows.Count;
                    SslStatus.Text = $"Updating MediaIDs ({TblSalesData.Rows.Count})";
                    foreach (DataRow SalesRow in TblSalesData.Rows)
                    {
                        PbUpdateMediaID.Value = TblSalesData.Rows.IndexOf(SalesRow) + 1;
                        DataRow RowTransAufBereit = CF.LoadDataRow(AM.MirrorConn, $"SELECT mediaid, wtp64, wtp32 FROM {AM.ActiveDatabase}.tabkassatransaufbereit WHERE nlfdzaehler='{SalesRow["nlfdzaehler"].ToString()}'");
                        if (RowTransAufBereit != null)
                        {
                            string UpdateQuery = string.Empty;
                            if (SalesRow["mediaid"].ToString().Trim() != RowTransAufBereit["mediaid"].ToString().Trim())
                            {
                                UpdateQuery += $"mediaid='{SalesRow["mediaid"].ToString().Trim()}', nkartennr='{(SalesRow["mediaid"].ToString() + "          ").Substring(0, 10).Trim()}'";
                            }
                            if (SalesRow["wtp32"].ToString().Trim() != RowTransAufBereit["wtp32"].ToString().Trim())
                            {
                                UpdateQuery += $"{(UpdateQuery != string.Empty ? "," : "")}wtp32='{RowTransAufBereit["wtp32"].ToString().Trim()}'";
                            }
                            if (SalesRow["wtp64"].ToString().Trim() != RowTransAufBereit["wtp64"].ToString().Trim())
                            {
                                UpdateQuery += $"{(UpdateQuery != string.Empty ? "," : "")}wtp64='{RowTransAufBereit["wtp64"].ToString().Trim()}'";
                            }
                            if (UpdateQuery != string.Empty)
                            {
                                CF.ExecuteSQL(DW.dwConn, $"UPDATE {DW.ActiveDatabase}.salesdata SET {UpdateQuery} WHERE nlfdzaehler='{SalesRow["nlfdzaehler"].ToString()}'");
                            }
                        }
                    }
                }
            }
            SslStatus.Text = "COMPLETE";
            Cursor.Current = OldCursor;
        }

        public void Update_tokenxref()
        {
            Cursor OldCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            SslStatus.Text = "Loading TokenXRef (1 of 3)";
            using (DataTable TblTokens = CF.LoadTable(BUY.Buy_Alta_ComConn, "SELECT id, firstname, lastname, orderid, webid, mediaid, associatedtokenid, customerpaymentprofileid, authnet_custid, serialnr, posnr, wtp64, workpos, tokenkey FROM axessutility.tokenxref", "tokens"))
            {
                if (TblTokens != null)
                { 
                SslStatus.Text = "Loading SalesData (2 of 3)";
                DataTable TblSales = CF.LoadTable(DW.dwConn, $"SELECT * FROM {DW.ActiveDatabase}.salesdata WHERE saledate>'{msaledate.ToString(Mirror.AxessDateFormat)}' AND LENGTH(mediaid) > 0 ORDER BY nlfdzaehler DESC", "axsales");
                SslStatus.Text = "Loading SalesData2 (3 of 3)";
                DataTable TblSales2 = CF.LoadTable(DW.dwConn, $"SELECT * FROM {DW.ActiveDatabase}.salesdata WHERE saledate>'{msaledate.ToString(Mirror.AxessDateFormat)}' AND LENGTH(mediaid) > 0 AND esorderid > 0 AND ntranstype=0 ORDER BY nlfdzaehler DESC", "axsales2");
                DataColumn[] keys = new DataColumn[1];
                keys[0] = TblSales2.Columns["esorderid"];
                TblSales2.PrimaryKey = keys;
                PbUpdateTokenXRef.Maximum = TblTokens.Rows.Count;
                foreach (DataRow TokenRow in TblTokens.Rows)
                {
                    string mrecid = TokenRow["id"].ToString();
                    SslStatus.Text = $"TokenXRefSync  (TokenXRef -- {TblTokens.Rows.Count} records)";
                    PbUpdateTokenXRef.Value = TblTokens.Rows.IndexOf(TokenRow) + 1;
                    bool msales_update = false;
                    bool mtoken_update = false;
                    string mserialkey = string.Empty;
                    string mkassanr = string.Empty;
                    string mserialnr = string.Empty;
                    string municodenr = string.Empty;
                    string mfirstname = TokenRow["firstname"].ToString().Trim();
                    string mlastname = TokenRow["lastname"].ToString().Trim();
                    string mtoken = string.Empty;
                    string mpmtprofile = string.Empty;
                    string mauthnet_custid = string.Empty;
                    string municode = string.Empty;
                    string mmediaid = string.Empty;
                    string mwtp32 = string.Empty;
                    string mwtp64 = string.Empty;
                    DataRow Sales2Row = TblSales2.Rows.Find(TokenRow["orderid"].ToString());
                    if (Sales2Row != null)
                    {
                        mserialkey = Sales2Row["serialkey"].ToString();
                    }
                    else
                    {

                    }
                    switch (TokenRow["webid"].ToString().ToUpper().Trim())
                    {
                        case "WEBORDER":    //*** web orders ***
                            if (Sales2Row != null)
                            {
                                if (Sales2Row["tokenkey"].ToString() == "0" || Sales2Row["tokenkey"].ToString() == null)
                                {
                                    msales_update = true;
                                    mtoken = TokenRow["associatedtokenid"].ToString();
                                    mpmtprofile = TokenRow["customerpaymentprofileid"].ToString();
                                    mauthnet_custid = TokenRow["authnet_custid"].ToString();
                                }
                                if (TokenRow["serialnr"].ToString() == "0" || TokenRow["serialnr"].ToString() == null)
                                {
                                    mtoken_update = true;
                                    mwtp32 = Sales2Row["wtp32"].ToString();
                                    mmediaid = Sales2Row["mediaid"].ToString();
                                    mwtp64 = Sales2Row["wtp64"].ToString().Replace(" ", "");
                                    mserialnr = Sales2Row["nseriennr"].ToString();
                                    mkassanr = Sales2Row["nkassanr"].ToString();
                                    municodenr = Sales2Row["nunicodenr"].ToString();
                                    if (Sales2Row["lname"].ToString() != mlastname)
                                    {
                                        mfirstname = Sales2Row["fname"].ToString().Replace("'", "''");
                                        mlastname = Sales2Row["lname"].ToString().Replace("'", "''");
                                    }
                                }
                            }
                            break;
                        case "PREORDER":  // *** prepaids and sales at the POS entered via serial number * ***
                            if (Sales2Row == null)
                                Sales2Row = CF.LoadDataRow(DW.dwConn, $"SELECT mediaid, wtp32, wtp64, fname, lname, tokenkey, serialkey FROM {DW.ActiveDatabase}.salesdata WHERE serialkey LIKE ('{TokenRow["posNr"].ToString()}-{TokenRow["serialNr"].ToString()}-%')");
                            if (Sales2Row != null)
                            {
                                if (Sales2Row["mediaid"].ToString().Length > 10)
                                {
                                    mtoken_update = true;
                                    mkassanr = TokenRow["posnr"].ToString();
                                    mserialnr = TokenRow["serialnr"].ToString();
                                    mwtp32 = Sales2Row["wtp32"].ToString();
                                    mmediaid = Sales2Row["mediaid"].ToString();
                                    mwtp64 = Sales2Row["wtp64"].ToString().Replace(" ", "");
                                    mfirstname = Sales2Row["fname"].ToString().Replace("'", "''");
                                    mlastname = Sales2Row["lname"].ToString().Replace("'", "''");
                                }
                                if (Sales2Row["tokenkey"].ToString() == "0")
                                {
                                    msales_update = true;
                                    mserialkey = Sales2Row["serialkey"].ToString();
                                    mtoken = TokenRow["associatedtokenid"].ToString();
                                    mpmtprofile = TokenRow["customerpaymentprofileid"].ToString();
                                    mauthnet_custid = TokenRow["authnet_custid"].ToString();
                                }
                            }
                            break;
                        default:
                            //**** web reloads 32 bit ****
                            if ((TokenRow["posnr"].ToString() == "20") && (TokenRow["webid"].ToString().Trim().Length != 0) && (TokenRow["mediaID"].ToString().Trim().Length == 0))
                            {
                                int tVal = 0;
                                if (TokenRow["webid"].ToString().Trim().Length != 0)
                                    tVal = 1;
                                if (TokenRow["wtp64"].ToString().Trim().Length != 0)
                                    tVal = 2;
                                if (tVal != 0)
                                {
                                    foreach (DataRow RowSales in TblSales.Rows)
                                    {
                                        bool tMatch = false;
                                        if (tVal == 1)
                                        {
                                            tMatch = ((RowSales["wtp32"].ToString() == (TokenRow["webid"].ToString() + "        ").Substring(0, 8)) && (RowSales["nkassanr"].ToString() == TokenRow["workpos"].ToString()));
                                        }
                                        else
                                            if (tVal == 2)
                                            tMatch = ((RowSales["wtp64"].ToString() == TokenRow["wtp64"].ToString()) && (RowSales["nkassanr"].ToString() == TokenRow["workpos"].ToString()));
                                        if (tMatch)
                                        {
                                            mserialkey = RowSales["serialkey"].ToString();
                                            mwtp32 = RowSales["wtp32"].ToString();
                                            mmediaid = RowSales["mediaid"].ToString();
                                            mwtp64 = RowSales["wtp64"].ToString().Replace(" ", "");
                                            mserialnr = RowSales["nseriennr"].ToString();
                                            mkassanr = RowSales["nkassanr"].ToString();
                                            municode = RowSales["nunicodenr"].ToString();
                                            mtoken_update = true;

                                            if (RowSales["lname"].ToString() != mlastname)
                                            {
                                                mfirstname = RowSales["fname"].ToString().Replace("'", "''");
                                                mlastname = RowSales["lname"].ToString().Replace("'", "''");
                                            }

                                            if (RowSales["tokenkey"].ToString() == "0")
                                            {
                                                msales_update = true;
                                                mtoken = TokenRow["associatedtokenid"].ToString();
                                                mpmtprofile = TokenRow["customerpaymentprofileid"].ToString();
                                                mauthnet_custid = TokenRow["authnet_custid"].ToString();
                                            }
                                            break;
                                        }
                                    }
                                }
                                break;
                            }
                            else
                            {
                                //**** pos sales with media id captured via tap ****
                                if ((TokenRow["mediaid"].ToString().Trim().Length > 0) && (TokenRow["serialnr"].ToString() == "99"))
                                {
                                    foreach (DataRow RowSales in TblSales.Rows)
                                    {
                                        if (RowSales["mediaid"].ToString() == TokenRow["mediaid"].ToString())
                                        {
                                            mtoken_update = true;
                                            mserialkey = RowSales["serialkey"].ToString();
                                            mwtp32 = RowSales["wtp32"].ToString();
                                            mmediaid = RowSales["mediaid"].ToString();
                                            mwtp64 = RowSales["wtp64"].ToString().Replace(" ", "");
                                            mserialnr = RowSales["nseriennr"].ToString();
                                            mkassanr = RowSales["nkassanr"].ToString();
                                            mfirstname = RowSales["fname"].ToString().Replace("'", "''");
                                            mlastname = RowSales["lname"].ToString().Replace("'", "''");

                                            if (RowSales["tokenkey"].ToString() == "0")
                                            {
                                                msales_update = true;
                                                mtoken = TokenRow["associatedtokenid"].ToString();
                                                mpmtprofile = TokenRow["customerpaymentprofileid"].ToString();
                                                mauthnet_custid = TokenRow["authnet_custid"].ToString();
                                            }
                                            break;
                                        }
                                    }
                                    break;
                                }
                                else
                                {
                                    //****pos sales with media id captured via serial number ****
                                    if ((TokenRow.Field<int>("posnr") > 0) && (TokenRow.Field<int>("posnr") != 99) && (TokenRow.Field<int>("serialnr") > 0) && (TokenRow.Field<int>("serialnr") != 99) && (TokenRow["serialnr"].ToString() != "NULL"))
                                    {
                                        foreach (DataRow RowSales in TblSales.Rows)
                                        {
                                            if ((RowSales["nseriennr"].ToString() == TokenRow["serialnr"].ToString()) && (RowSales["nkassanr"].ToString() == TokenRow["posnr"].ToString()) && (RowSales["lname"].ToString() == TokenRow["lastname"].ToString()))
                                            {
                                                mtoken_update = true;
                                                mserialkey = RowSales["serialkey"].ToString();
                                                mwtp32 = RowSales["wtp32"].ToString();
                                                mmediaid = RowSales["mediaid"].ToString();
                                                mwtp64 = RowSales["wtp64"].ToString().Replace(" ", "");
                                                mserialnr = RowSales["nseriennr"].ToString();
                                                mkassanr = RowSales["nkassanr"].ToString();
                                                mfirstname = RowSales["fname"].ToString().Replace("'", "''");
                                                mlastname = RowSales["lname"].ToString().Replace("'", "''");

                                                if (RowSales["tokenkey"].ToString() == "0")
                                                {
                                                    msales_update = true;
                                                    mtoken = RowSales["associatedtokenid"].ToString();
                                                    mpmtprofile = RowSales["customerpaymentprofileid"].ToString();
                                                    mauthnet_custid = RowSales["authnet_custid"].ToString();
                                                }
                                                break;
                                            }
                                        }
                                        break;
                                    }
                                    else
                                    {
                                        //**** check FOR CANCELLED name ***
                                        if ((TokenRow["lastname"].ToString().Length == 0) && (TokenRow["mediaid"].ToString().Trim().Length != 0))
                                        {
                                            foreach (DataRow xRow in TblSales.Rows)
                                            {
                                                if ((xRow["nseriennr"].ToString() == TokenRow["serialnr"].ToString()) && (xRow["nkassanr"].ToString() == TokenRow["posnr"].ToString()) && (xRow["ttype"].ToString() == "C"))
                                                {
                                                        CF.ExecuteSQL(BUY.Buy_Alta_ComConn, $"DELETE FROM axessutility.tokenxref WHERE id='{mrecid}'");
                                                        break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        }
                        if (mtoken_update)
                            CF.ExecuteSQL(BUY.Buy_Alta_ComConn, $"UPDATE axessutility.tokenxref SET firstname='{mfirstname}', lastname='{mlastname}', posnr={mkassanr}, serialnr={mserialnr},webid='{mwtp32}', wtp64='{mwtp64}', mediaid='{mmediaid}' WHERE id='{mrecid}'");
                        if (msales_update)
                            CF.ExecuteSQL(DW.dwConn, $"UPDATE {DW.ActiveDatabase}.salesdata SET tokenkey='{mtoken}', pmtprofile='{mpmtprofile}', authnet_custid='{mauthnet_custid}' WHERE serialkey='{mserialkey}'");
                    }
                }
            }
            SslStatus.Text = "COMPLETE";
            Cursor.Current = OldCursor;
        }

        public void Update_salesdata()
        {

            Cursor OldCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            SslStatus.Text = "Loading TokenXRef (1 of 2)";
            using (DataTable TblTokenXRef = CF.LoadTable(BUY.Buy_Alta_ComConn, "SELECT mediaid, associatedtokenid, authnet_custid FROM axessutility.tokenxref WHERE serialNr > 99 AND LENGTH(mediaID) = 16 GROUP BY mediaID", "tokens"))
            {
                DataColumn[] keys = new DataColumn[1];
                keys[0] = TblTokenXRef.Columns["mediaid"];
                TblTokenXRef.PrimaryKey = keys;
                SslStatus.Text = "Loading SalesData (2 of 2)";
                using (DataTable TblSalesData = CF.LoadTable(DW.dwConn, $"SELECT nlfdzaehler, mediaid, tokenkey, authnet_custid FROM {DW.ActiveDatabase}.salesdata WHERE saledate > '{msaledate}' AND tokenkey <> 0", "axsalesx"))
                {
                    foreach (DataRow RowSalesData in TblSalesData.Rows)
                    {
                        if (RowSalesData["mediaid"].ToString().Trim().Length != 0 && RowSalesData["nlfdzaehler"].ToString() != "NULL")
                        {
                            DataRow RowToken = TblTokenXRef.Rows.Find(RowSalesData["mediaid"].ToString());
                            if (RowToken != null)
                                if (RowSalesData["tokenkey"].ToString() != RowToken["associatedtokenid"].ToString() || RowSalesData["authnet_custid"].ToString() != RowToken["authnet_custid"].ToString())
                                    CF.ExecuteSQL(DW.dwConn, $"UPDATE {DW.ActiveDatabase}.salesdata SET tokenkey='{RowToken["associatedtokenkey"].ToString()}', pmtprofile='{RowToken["customerpaymentprofileid"].ToString()}', authnet_custid='{RowToken["authnet_custid"].ToString()}', dtupdate='{DateTime.Now.ToString(Mirror.AxessDateTimeFormat)}' WHERE nlfdzaehler={RowSalesData["nlfdzaehler"].ToString()}");
                        }
                    }
                }
            }
            //verify that all active cards with a token in SPCards have a matching token in TokenXRef, reset to no token in SPCards if not matched.
            using (DataTable TblSPCards = CF.LoadTable(DW.dwConn, $"SELECT recid, tokenkey FROM {DW.ActiveDatabase}.spcards WHERE cardstatus='A' AND tokenkey<>0", "spcards"))
            {
                if (TblSPCards != null)
                {
                    foreach (DataRow RowSPCards in TblSPCards.Rows)
                    {
                        if (!CF.RowExists(BUY.Buy_Alta_ComConn, "axessutility.tokenxref", $"associatedtokenid='{RowSPCards["tokenkey"].ToString()}'"))
                        {
                            CF.ExecuteSQL(DW.dwConn, $"UPDATE {DW.ActiveDatabase}.spcards SET tokenkey=0, pmtprofile=0, authnet_custid=0 WHERE recid={RowSPCards["recid"].ToString()}");
                        }
                        SslStatus.Text = $"Row {TblSPCards.Rows.IndexOf(RowSPCards).ToString()} of {TblSPCards.Rows.Count.ToString()}";
                    }
                }
            }
            //*** update artrans ***
            //
            using (DataTable TblARTrans = CF.LoadTable(DW.dwConn, $"SELECT serialkey, tokenkey FROM {DW.ActiveDatabase}.artrans WHERE authcode='' AND (arcustid='11874' OR (tokenkey=0 AND arcustid=''))", "artrans"))
            {
                using (DataTable TblSPCards = CF.LoadTable(DW.dwConn, $"SELECT serialkey, tokenkey, authnet_custid FROM {DW.ActiveDatabase}.spcards WHERE tokenkey>0 GROUP BY serialkey", "spcards"))
                {
                    DataColumn[] keys = new DataColumn[1];
                    keys[0] = TblSPCards.Columns["serialkey"];
                    TblSPCards.PrimaryKey = keys;
                    foreach (DataRow RowARTrans in TblARTrans.Rows)
                    {
                        DataRow RowSPCards = TblSPCards.Rows.Find(RowARTrans["serialkey"].ToString());
                        if (RowSPCards != null)
                        {
                            if (RowARTrans["tokenkey"].ToString() != RowSPCards["tokenkey"].ToString())
                            {
                                CF.ExecuteSQL(DW.dwConn, $"UPDATE {DW.ActiveDatabase}.artrans SET tokenkey='{RowSPCards["tokenkey"].ToString()}', authnet_custid='{RowSPCards["authnet_custid"].ToString()}' WHERE serialkey='{RowARTrans["serialkey"].ToString()}' AND authcode = ''");
                            }
                        }
                        SslStatus.Text = $"Row {TblARTrans.Rows.IndexOf(RowARTrans).ToString()} of {TblARTrans.Rows.Count.ToString()}";
                    }
                }
            }
            SslStatus.Text = "COMPLETE";
            Cursor.Current = OldCursor;
        }
        private void Run()
        {
            timer1.Enabled = false;
            BtnRunUpdates.Enabled = false;
            Cursor OldCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            if (CbUpdateMediaID.Checked) Update_mediaid();
            if (CbUpdateTokenXRef.Checked) Update_tokenxref();
            if (CbUpdateSalesData.Checked) Update_salesdata();
            Cursor.Current = OldCursor;
            timer1.Enabled = true;
        }

        private void BtnRunUpdates_Click(object sender, EventArgs e) => Run();

        private void Timer1_Tick(object sender, EventArgs e) => Run();
    }

}

