using System;
using System.Data;

namespace asl_SyncLibrary
{
    public class TokenXRefSync
    {
        private CommonFunctions cf = new CommonFunctions();
        private DataWarehouse dw = new DataWarehouse();
        private Mirror am = new Mirror();
        private Axess_DCI4CRM ax = new Axess_DCI4CRM();
        private BUY_ALTA_COM buy = new BUY_ALTA_COM();

        private DataSet txrDS = new DataSet();
        private string mirrorDatabase = "";
        private string appsDatabase = "";

        private DateTime msaledate = new DateTime(DateTime.Now.Year - (DateTime.Now.Month <= 6 ? 1 : 0), 7, 1);

        // private bool Initialized = false;

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
                appsDatabase = dw.ActiveDatabase;
            }
        }

        public void Update_All()
        {
            Update_mediaid();
            Update_tokenxref();
            Update_salesdata();
            txrDS.Dispose();
        }

        public void Update_mediaid()
        {
            DateTime xDate = DateTime.Now.AddDays(-300);
            txrDS = cf.LoadDataSet(dw.dwConn, $"SELECT * FROM {appsDatabase}.salesdata WHERE saledate>='{xDate.ToString(Mirror.AxessDateFormat)}' AND (tokenkey=0)  AND (tgroup IN ('EX','SP')) ORDER BY nlfdzaehler", txrDS, "axsales");
            int tRowCount = txrDS.Tables["axsales"].Rows.Count;

            foreach (DataRow tRow in txrDS.Tables["axsales"].Rows)
            {
                //if (tRow["serialkey"].ToString() == "18-58272-2")
                //    xDate = xDate;
                string mKey = tRow["nlfdzaehler"].ToString();
                DataRow sRow = cf.LoadDataRow(am.MirrorConn, $"SELECT mediaid, wtp64, wtp32 FROM {mirrorDatabase}.tabkassatransaufbereit WHERE nlfdzaehler='{mKey}'");
                if (sRow != null)
                {
                    string mmediaid = string.Empty;
                    string mwtp32 = string.Empty;
                    string mwtp64 = string.Empty;
                    string mkartennr = string.Empty;
                    if (sRow["mediaid"].ToString().Trim().Length > 10)
                    {
                        mmediaid = sRow["mediaid"].ToString().Trim();
                        mwtp32 = sRow["wtp32"].ToString().Trim();
                        mwtp64 = sRow["wtp64"].ToString().Trim();
                        mkartennr = mmediaid.Substring(0, 10);
                    }
                    cf.ExecuteSQL(dw.dwConn, $"UPDATE {appsDatabase}.salesdata SET mediaid='{mmediaid}', wtp64='{mwtp64}', wtp32='{mwtp32}', nkartennr='{mkartennr}' WHERE nlfdzaehler='{mKey}'");
                }
            }
        }

        private void BuildTokenRow(DataRow TokenRow)
        {
            string mrecid = TokenRow["id"].ToString();
            bool msales_update = false;
            bool mtoken_update = false;
            bool mtoken_cancel = false;
            string mposnr = string.Empty;
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
            DataRow sRow = txrDS.Tables["axsales2"].Rows.Find(TokenRow["orderid"].ToString());
            if (sRow != null)
            {
                mserialkey = sRow["serialkey"].ToString();
            }
            else
            {

            }
            //if (tRow["mediaid"].ToString() == "E004010801B7AAF4")
            //    mserialkey = mserialkey + string.Empty;

            switch (TokenRow["webid"].ToString().ToUpper().Trim())
            {
                case "WEBORDER":    //*** web orders ***
                    if (sRow != null)
                    {
                        if (sRow["tokenkey"].ToString() == "0" || sRow["tokenkey"].ToString() == null)
                        {
                            msales_update = true;
                            mtoken = TokenRow["associatedtokenid"].ToString();
                            mpmtprofile = TokenRow["customerpaymentprofileid"].ToString();
                            mauthnet_custid = TokenRow["authnet_custid"].ToString();
                        }
                        if (TokenRow["serialnr"].ToString() == "0" || TokenRow["serialnr"].ToString() == null)
                        {
                            mwtp32 = sRow["wtp32"].ToString();
                            mmediaid = sRow["mediaid"].ToString();
                            mwtp64 = sRow["wtp64"].ToString().Replace(" ", "");
                            mserialnr = sRow["nseriennr"].ToString();
                            mkassanr = sRow["nkassanr"].ToString();
                            mposnr = mkassanr;
                            municodenr = sRow["nunicodenr"].ToString();
                            if (sRow["lname"].ToString() != mlastname)
                            {
                                mfirstname = sRow["fname"].ToString().Replace("'", "''");
                                mlastname = sRow["lname"].ToString().Replace("'", "''");
                            }
                            mtoken_update = true;
                        }
                    }
                    break;
                case "PREORDER":  // *** prepaids and sales at the POS entered via serial number * ***
                    if (sRow == null)
                        sRow = cf.LoadDataRow(dw.dwConn, $"SELECT * FROM {appsDatabase}.salesdata WHERE serialkey LIKE ('{TokenRow["posNr"].ToString()}-{TokenRow["serialNr"].ToString()}-%')");
                    if (sRow != null)
                    {
                        if (sRow["mediaid"].ToString().Length > 10)
                        {
                            mkassanr = TokenRow["posnr"].ToString();
                            mserialnr = TokenRow["serialnr"].ToString();
                            mtoken_update = true;
                            mwtp32 = sRow["wtp32"].ToString();
                            mmediaid = sRow["mediaid"].ToString();
                            mwtp64 = sRow["wtp64"].ToString().Replace(" ", "");
                            mfirstname = sRow["fname"].ToString().Replace("'", "''");
                            mlastname = sRow["lname"].ToString().Replace("'", "''");
                        }
                        if (sRow["tokenkey"].ToString() == "0")
                        {
                            mserialkey = sRow["serialkey"].ToString();
                            msales_update = true;
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
                            foreach (DataRow xRow in txrDS.Tables["axsales"].Rows)
                            {
                                bool tMatch = false;
                                if (tVal == 1)
                                {
                                    tMatch = ((xRow["wtp32"].ToString() == TokenRow["webid"].ToString().Substring(0, 8)) && (xRow["nkassanr"].ToString() == TokenRow["workpos"].ToString()));
                                }
                                else
                                    if (tVal == 2)
                                    tMatch = ((xRow["wtp64"].ToString() == TokenRow["wtp64"].ToString()) && (xRow["nkassanr"].ToString() == TokenRow["workpos"].ToString()));
                                if (tMatch)
                                {
                                    mserialkey = xRow["serialkey"].ToString();
                                    mwtp32 = xRow["wtp32"].ToString();
                                    mmediaid = xRow["mediaid"].ToString();
                                    mwtp64 = xRow["wtp64"].ToString().Replace(" ", "");
                                    mserialnr = xRow["nseriennr"].ToString();
                                    mkassanr = xRow["nkassanr"].ToString();
                                    mposnr = xRow["nkassanr"].ToString();
                                    municode = xRow["nunicodenr"].ToString();
                                    mtoken_update = true;

                                    if (xRow["lname"].ToString() != mlastname)
                                    {
                                        mfirstname = xRow["fname"].ToString().Replace("'", "''");
                                        mlastname = xRow["lname"].ToString().Replace("'", "''");
                                    }

                                    if (xRow["tokenkey"].ToString() == "0")
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
                            foreach (DataRow xRow in txrDS.Tables["axsales"].Rows)
                            {
                                if (xRow["mediaid"].ToString() == TokenRow["mediaid"].ToString())
                                {
                                    mtoken_update = true;
                                    mserialkey = xRow["serialkey"].ToString();
                                    mwtp32 = xRow["wtp32"].ToString();
                                    mmediaid = xRow["mediaid"].ToString();
                                    mwtp64 = xRow["wtp64"].ToString().Replace(" ", "");
                                    mserialnr = xRow["nseriennr"].ToString();
                                    mkassanr = xRow["nkassanr"].ToString();
                                    mfirstname = xRow["fname"].ToString().Replace("'", "''");
                                    mlastname = xRow["lname"].ToString().Replace("'", "''");

                                    if (xRow["tokenkey"].ToString() == "0")
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
                            if ((TokenRow.Field<int>("posnr") > 0) && (TokenRow.Field<int>("posnr") != 99) && (TokenRow.Field<int>("serialnr") > 0) && (TokenRow.Field<int>("serialnr") != 99) && (TokenRow["serialnr"].ToString() == "NULL"))
                            {
                                foreach (DataRow xRow in txrDS.Tables["axsales"].Rows)
                                {
                                    if ((xRow["nseriennr"].ToString() == TokenRow["serialnr"].ToString()) && (xRow["nkassanr"].ToString() == TokenRow["posnr"].ToString()) && (xRow["lname"].ToString() == TokenRow["lastname"].ToString()))
                                    {
                                        mtoken_update = true;
                                        mserialkey = xRow["serialkey"].ToString();
                                        mwtp32 = xRow["wtp32"].ToString();
                                        mmediaid = xRow["mediaid"].ToString();
                                        mwtp64 = xRow["wtp64"].ToString().Replace(" ", "");
                                        mserialnr = xRow["nseriennr"].ToString();
                                        mkassanr = xRow["nkassanr"].ToString();
                                        mfirstname = xRow["fname"].ToString().Replace("'", "''");
                                        mlastname = xRow["lname"].ToString().Replace("'", "''");

                                        if (TokenRow["tokenkey"].ToString() == "0")
                                        {
                                            msales_update = true;
                                            mtoken = TokenRow["associatedtokenid"].ToString();
                                            mpmtprofile = xRow["customerpaymentprofileid"].ToString();
                                            mauthnet_custid = xRow["authnet_custid"].ToString();
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
                                    foreach (DataRow xRow in txrDS.Tables["axsales"].Rows)
                                    {
                                        if ((xRow["nseriennr"].ToString() == TokenRow["serialnr"].ToString()) && (xRow["nkassanr"].ToString() == TokenRow["posnr"].ToString()) && (xRow["ttype"].ToString() == "C"))
                                        {
                                            mtoken_cancel = true;
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
                cf.ExecuteSQL(buy.Buy_Alta_ComConn, $"UPDATE axessutility.tokenxref SET firstname='{mfirstname}', lastname='{mlastname}', posnr={mkassanr}, serialnr={mserialnr},webid='{mwtp32}', wtp64='{mwtp64}', mediaid='{mmediaid}' WHERE id='{mrecid}'");

            if (mtoken_cancel)
                cf.ExecuteSQL(buy.Buy_Alta_ComConn, $"DELETE FROM axessutility.tokenxref WHERE id='{mrecid}'");

            if (msales_update)
                cf.ExecuteSQL(dw.dwConn, $"UPDATE {appsDatabase}.salesdata SET tokenkey='{mtoken}', pmtprofile='{mpmtprofile}', authnet_custid='{mauthnet_custid}' WHERE serialkey='{mserialkey}'");
        }

        public void Update_tokenxref()
        {
            txrDS = cf.LoadDataSet(buy.Buy_Alta_ComConn, "SELECT * FROM axessutility.tokenxref ", txrDS, "tokens");
            txrDS = cf.LoadDataSet(dw.dwConn, $"SELECT * FROM {appsDatabase}.salesdata WHERE saledate>'{msaledate.ToString(Mirror.AxessDateFormat)}' AND LENGTH(mediaid) > 0 ORDER BY nlfdzaehler DESC", txrDS, "axsales");
            txrDS = cf.LoadDataSet(dw.dwConn, $"SELECT * FROM {appsDatabase}.salesdata WHERE saledate>'{msaledate.ToString(Mirror.AxessDateFormat)}' AND LENGTH(mediaid) > 0 AND esorderid > 0 AND ntranstype=0 ORDER BY nlfdzaehler DESC", txrDS, "axsales2");

            DataColumn[] keys = new DataColumn[1];
            keys[0] = txrDS.Tables["axsales2"].Columns["esorderid"];
            txrDS.Tables["axsales2"].PrimaryKey = keys;
            int tRowCount = txrDS.Tables["tokens"].Rows.Count;
;           foreach (DataRow tRow in txrDS.Tables["tokens"].Rows)
            {
                BuildTokenRow(tRow);
            }
        }

        public void Update_salesdata()
        {
            //if (!txrDS.Tables.Contains("axsales"))
            //    Load_tables();
            txrDS = cf.LoadDataSet(buy.Buy_Alta_ComConn, "SELECT * FROM axessutility.tokenxref WHERE serialNr > 99 AND LENGTH(mediaID) = 16 GROUP BY mediaID", txrDS, "tokens");
            DataColumn[] keys = new DataColumn[1];
            keys[0] = txrDS.Tables["tokens"].Columns["mediaid"];
            txrDS.Tables["tokens"].PrimaryKey = keys;
            txrDS = cf.LoadDataSet(dw.dwConn, $"SELECT * FROM {appsDatabase}.salesdata WHERE saledate > '{msaledate}' AND tokenkey > 0 ORDER BY nlfdzaehler desc", txrDS, "axsalesx");
            foreach (DataRow sRow in txrDS.Tables["axsalesx"].Rows)
            {
                if (sRow["mediaid"].ToString().Trim().Length != 0 && sRow["nlfdzaehler"].ToString() != "NULL")
                {
                    DataRow tRow = txrDS.Tables["tokens"].Rows.Find(sRow["mediaid"].ToString());
                    if (tRow != null)
                        if (sRow["tokenkey"].ToString() != tRow["associatedtokenid"].ToString() || sRow["authnet_custid"].ToString() != tRow["authnet_custit"].ToString())
                            cf.ExecuteSQL(dw.dwConn, $"UPDATE {appsDatabase}.salesdata SET tokenkey='{tRow["associatedtokenkey"].ToString()}', pmtprofile='{tRow["customerpaymentprofileid"].ToString()}', authnet_custid='{tRow["authnet_custid"].ToString()}', dtupdate='{DateTime.Now.ToString(Mirror.AxessDateTimeFormat)}' WHERE nlfdzaehler={sRow["nlfdzaehler"].ToString()}");
                }
            }
            //***fix broken SPCards to TokenXRef links*** SELECT recid, tokenkey FROM applications.spcards WHERE cardstatus='A' AND tokenkey>0
            txrDS = cf.LoadDataSet(dw.dwConn, $"SELECT recid, tokenkey FROM {dw.ActiveDatabase}.spcards WHERE cardstatus='A' AND tokenkey>0", txrDS, "spcards");
            int tRowCount = txrDS.Tables["SPCards"].Rows.Count;
            foreach (DataRow tRow in txrDS.Tables["spcards"].Rows)
            {
                if (!cf.RowExists(buy.Buy_Alta_ComConn, "axessutility.tokenxref",$"associatedtokenid='{tRow["tokenkey"].ToString()}'"))
                {
                    cf.ExecuteSQL(dw.dwConn, $"UPDATE {dw.ActiveDatabase}.spcards SET tokenkey=0, pmtprofile=0, authnet_custid=0 WHERE recid={tRow["recid"].ToString()}");
                }
            }

            //*** update artrans ***
            txrDS = cf.LoadDataSet(dw.dwConn, $"SELECT * FROM {appsDatabase}.artrans WHERE authcode='' AND (arcustid='11874' OR (tokenkey=0 AND arcustid=''))", txrDS, "artrans");
            txrDS = cf.LoadDataSet(dw.dwConn, $"SELECT * FROM {appsDatabase}.spcards WHERE tokenkey>0 GROUP BY serialkey", txrDS, "spcards");
            keys[0] = txrDS.Tables["spcards"].Columns["serialkey"];
            txrDS.Tables["spcards"].PrimaryKey = keys;
            foreach(DataRow tRow in txrDS.Tables["artrans"].Rows)
            {
                string mkey = tRow["serialkey"].ToString();
                DataRow sRow = txrDS.Tables["spcards"].Rows.Find(mkey);
                if (sRow != null)
                {
                    if (sRow["tokenkey"].ToString() != tRow["tokenkey"].ToString())
                    {
                        cf.ExecuteSQL(dw.dwConn, $"UPDATE {appsDatabase}.artrans SET tokenkey='{sRow["tokenkey"].ToString()}', authnet_custid='{sRow["authnet_custid"].ToString()}' WHERE serialkey='{mkey}' AND authcode = ''");
                    }
                }
            }
        }
    }
}
