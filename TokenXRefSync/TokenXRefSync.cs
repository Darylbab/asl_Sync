using System;
using System.Data;
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
        Shop SH = new Shop();

        DataTable TblSales;

        private DateTime msaledate = new DateTime(DateTime.Now.Year - (DateTime.Now.Month <= 6 ? 1 : 0), 7, 1);

        public TokenXRefSync()
        {
            InitializeComponent();
            DataRow FormLocation = CF.GetFormLocation("TokenXRefSync");
            int StopReason = -1;
            if (CF.RowHasData(FormLocation))
            {
                StartPosition = FormStartPosition.Manual;
                Location = new System.Drawing.Point(FormLocation.Field<int>("left"), FormLocation.Field<int>("top"));
                Height = FormLocation.Field<int>("height");
                Width = FormLocation.Field<int>("width");
                StopReason = FormLocation.Field<int>("stopReason");
            }
            CF.SaveFormData("TokenXRefSync", Location.Y, Location.X, Height, Width, DateTime.Now, null, StopReason);
        }

        public void Update_All(System.Windows.Forms.StatusStrip aStatusStrip = null, System.Windows.Forms.ToolStripProgressBar aProgressBar = null)
        {
            Cursor OldCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            LblStartTime.Text = $"Current run started {DateTime.Now.ToString("MMM dd, yyyy hh:mm:ss")}";
            Update99s();
            MirrorTokenXRef();
            Load_From_eStore();
            Update_mediaid();
            Update_tokenxref();
            Update_salesdata();
            LblStartTime.Text = string.Empty;
            Cursor.Current = OldCursor;
        }

        //id, associatedTokenId, customerPaymentProfileId, orderId, projNr, posNr, serialNr, nunicodenr, authnet_custid, activeflag, journalNr, workPos, lastUpdated, firstname, lastname, dateInserted, webId, mediaID, WTP64
        public void MirrorTokenXRef()
        {
            DateTime StartMirror = DateTime.Now;
            Cursor OldCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            //update Applications.TokenXref from AxessUtility.TokenXRef
            DataRow LastUpdate = CF.LoadDataRow(DW.dwConn, $"SELECT MAX(lastUpdated) FROM {DW.ActiveDatabase}.tokenxref");
            string UpdateWhere = string.Empty;
            if (CF.RowHasData(LastUpdate))
            {
                UpdateWhere = $"WHERE lastUpdated > '{LastUpdate.Field<DateTime>(0).ToString(Mirror.AxessDateTimeFormat)}'";
            }
            using (DataTable AUTokenXRef = CF.LoadTable(BUY.Buy_Alta_ComConn, $"SELECT * FROM axessutility.tokenxref {UpdateWhere} ORDER BY id","AUTokenXRef"))
            {
                pbSyncTokenFiles.Maximum = AUTokenXRef.Rows.Count;
                foreach (DataRow ATR in AUTokenXRef.Rows)
                {
                    pbSyncTokenFiles.Value = AUTokenXRef.Rows.IndexOf(ATR);
                    SslStatus.Text = $"Mirroring TokenXRef {pbSyncTokenFiles.Value + 1} of {pbSyncTokenFiles.Maximum}";
                    Refresh();
                    Int32 ID = ATR.Field<Int32>("id");
                    DateTime lastUpdated = ATR.Field<DateTime>("lastUpdated");
                    string LastUpdatedS = lastUpdated.ToString(Mirror.AxessDateTimeFormat);
                    DateTime dateInserted = ATR.Field<DateTime>("dateInserted");
                    string DateInsertedS = dateInserted.ToString(Mirror.AxessDateTimeFormat);
                    string UnicodeNrS = ATR["nunicodenr"].ToString().Trim();
                    if (UnicodeNrS == string.Empty)
                    {
                        UnicodeNrS = "null";
                    }
                    //lblSyncTokenFiles.Text = "Pass 1 of 2";
                    DataRow AppTokenRow = CF.LoadDataRow(DW.dwConn, $"SELECT * FROM {DW.ActiveDatabase}.TokenXRef WHERE id={ID}");
                    if (!CF.RowHasData(AppTokenRow))  //Row not in Applications so insert and check for a salesdata match based on SerialKey
                    {
                        string TQ = $"INSERT INTO {DW.ActiveDatabase}.TokenXRef (id, associatedTokenId, customerPaymentProfileId, orderId, projNr, posNr," +
                            $" serialNr, nunicodenr, authnet_custid, activeflag, journalNr, workPos," +
                            $" lastUpdated, firstname, lastname, dateInserted, webId, mediaID, WTP64) VALUES (" +
                            $"{ID}, {ATR["associatedTokenId"].ToString()}, {ATR["customerPaymentProfileId"].ToString()}, {ATR["orderId"].ToString()}, {ATR["projNr"].ToString()}, {ATR["posNr"].ToString()}, " +
                            $"{ATR["serialNr"].ToString()}, {UnicodeNrS}, {ATR["authnet_custid"].ToString()}, {ATR["activeflag"].ToString()}, {ATR["journalNr"].ToString()}, {ATR["workPos"].ToString()}, " +
                            $"'{LastUpdatedS}', '{CF.EscapeChar(ATR["firstname"].ToString().Trim())}', '{CF.EscapeChar(ATR["lastname"].ToString().Trim())}', '{DateInsertedS}', '{ATR["webId"].ToString().Trim()}', '{ATR["mediaID"].ToString().Trim()}', '{ATR["WTP64"].ToString().Trim()}')";
                        CF.ExecuteSQL(DW.dwConn, TQ);
                    }
                    else
                    {
                        if (ATR.Field<DateTime>("lastUpdated") != AppTokenRow.Field<DateTime>("lastUpdated"))
                        {
                            string TQ = $"UPDATE {DW.ActiveDatabase}.tokenxref SET associatedTokenId = {ATR["associatedTokenId"].ToString()}, customerPaymentProfileId = {ATR["customerPaymentProfileId"].ToString()}, " +
                                $"orderId = {ATR["orderId"].ToString()}, projNr = {ATR["projNr"].ToString()}, posNr = {ATR["posNr"].ToString()}, serialNr = {ATR["serialNr"].ToString()}, nunicodenr = {UnicodeNrS}, " +
                                $"authnet_custid = {ATR["authnet_custid"].ToString()}, activeflag = {ATR["activeflag"].ToString()}, journalNr = {ATR["journalNr"].ToString()}, workPos = {ATR["workPos"].ToString()}, " +
                                $"lastUpdated = '{LastUpdatedS}', firstname = '{CF.EscapeChar(ATR["firstname"].ToString().Trim())}', lastname = '{CF.EscapeChar(ATR["lastname"].ToString().Trim())}', dateInserted = '{DateInsertedS}', webId = '{ATR["webId"].ToString().Trim()}', " +
                                $"mediaID = '{ATR["mediaID"].ToString().Trim()}', WTP64 = '{ATR["WTP64"].ToString().Trim()}'";
                            CF.ExecuteSQL(DW.dwConn, TQ);
                        }
                    }
                }
                if (CF.RowCount(BUY.Buy_Alta_ComConn, $"axessutility.tokenxref") < CF.RowCount(DW.dwConn, $"{DW.ActiveDatabase}.tokenxref"))
                {
                    using (DataTable AUTokenXRef2 = CF.LoadTable(BUY.Buy_Alta_ComConn, $"SELECT id FROM axessutility.tokenxref order by id", "AUTokenXRef"))
                    {
                        AUTokenXRef2.PrimaryKey = new DataColumn[1] { AUTokenXRef2.Columns["id"] };
                        DataTable AppTokenXRef = CF.LoadTable(DW.dwConn, $"SELECT id FROM {DW.ActiveDatabase}.tokenxref", "AppTokenXRef");
                        if (CF.TableHasData(AppTokenXRef))
                        {
                            foreach (DataRow AppRow in AppTokenXRef.Rows)
                            {
                                SslStatus.Text = $"{AppTokenXRef.Rows.IndexOf(AppRow)} of {AppTokenXRef.Rows.Count}";
                                Refresh();
                                if (!CF.RowHasData(AUTokenXRef2.Rows.Find(AppRow.Field<Int32>("id"))))
                                {
                                    CF.ExecuteSQL(DW.dwConn, $"DELETE FROM {DW.ActiveDatabase}.tokenxref WHERE id={AppRow["id"].ToString()}");
                                }
                            }
                        }
                    }
                }
            }
            TimeSpan ts = TimeSpan.FromTicks(DateTime.Now.Ticks - StartMirror.Ticks);
            lblSyncTokenFiles.Text = string.Format($"{ts.Hours.ToString("0#")}:{ts.Minutes.ToString("0#")}:{ts.Seconds.ToString("0#")}");
            lblSyncTokenFiles.Visible = true;
        }

        public void Load_From_eStore()
        {
            DateTime FStartTime = DateTime.Now;
            Cursor OldCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            SslStatus.Text = "Loading Tokens from Shop.Alta.COM";
            Refresh();
            DateTime LastRecord = AM.GetLogTimestamp("EStoreTokenSync");
            string Query = "SELECT P.id AS AssociatedTokenID, P.authnet_id AS CustomerPaymentProfileID, T.nprojno, T.nposno, T.nserialno, T.nunicodeno, " +
                "C.authnet_id AS AuthnetCustID, T.njournalno, I.axess_wtp AS WebID, I.person_firstname, I.person_lastname, T.created_at " +
                "FROM altashop.order_items AS I " +
                "INNER JOIN altashop.orders AS O ON O.id = I.order_id " +
                "INNER JOIN altashop.customers AS C ON O.customer_id = C.id " +
                "INNER JOIN altashop.customer_profiles AS P ON P.customer_id = C.id " +
                "INNER JOIN altashop.axess_transactions AS T ON I.id = T.order_item_id " +
                $"WHERE I.resort_card = 1 AND (C.deleted_at IS NULL OR P.deleted_at IS NULL) AND T.created_at >= '{LastRecord.ToString(Mirror.AxessDateTimeFormat)}' ORDER BY T.created_at";
            using (DataTable WebSales2 = CF.LoadTable(SH.Shop_Alta_ComConn, Query, "WebSales2"))
            {
                pbLoadFromWeb.Maximum = WebSales2.Rows.Count;
                pbLoadFromWeb.Value = 0;
                foreach (DataRow WebRow in WebSales2.Rows)
                {
                    pbLoadFromWeb.Value = WebSales2.Rows.IndexOf(WebRow) + 1;
                    SslStatus.Text = $"Loading Token {pbLoadFromWeb.Value.ToString()} of {pbLoadFromWeb.Maximum.ToString()} from Shop.Alta.COM.";
                    Refresh();
                    if (WebRow.Field<DateTime>("created_at") > LastRecord)
                    {
                        LastRecord = WebRow.Field<DateTime>("created_at");
                    }
                    DataRow TokenXRow = CF.LoadDataRow(BUY.Buy_Alta_ComConn, $"SELECT * FROM axessutility.tokenxref WHERE projnr={WebRow["nprojno"].ToString()} AND posnr={WebRow["nposno"].ToString()} AND serialnr={WebRow["nserialno"].ToString()} AND nunicodenr={WebRow["nunicodeno"].ToString()}");
                    if (TokenXRow["serialnr"].ToString() == string.Empty)       //empty record returned, no match found.
                    {   //insert new TokenXRef record
                        DateTime CurTime = DateTime.Now;
                        string TTime = CurTime.ToString(Mirror.AxessDateTimeFormat);
                        Query = $"INSERT INTO axessutility.tokenxref (associatedTokenId, customerPaymentProfileId, orderId, projNr, posNr, serialNr, nunicodenr, " +
                            $"authnet_custid, activeflag, journalNr, workPos, lastUpdated, firstname, lastname, dateInserted, webId, mediaID, WTP64) " +
                            $"VALUES ({WebRow["associatedtokenid"].ToString()}, '{WebRow["customerpaymentprofileid"].ToString()}', " +
                            $"'', {WebRow["nprojno"].ToString()}, {WebRow["nposno"].ToString()}, {WebRow["nserialno"].ToString()}, " +
                            $"{WebRow["nunicodeno"].ToString()}, {WebRow["authnetcustid"].ToString()}, 1, {WebRow["njournalno"].ToString()}, 20, " +
                            $"'{TTime}', '{CF.EscapeChar(WebRow["person_firstname"].ToString())}', '{CF.EscapeChar(WebRow["person_lastname"].ToString())}', '{TTime}', '{WebRow["webid"].ToString()}', '', '')";
                        CF.ExecuteSQL(BUY.Buy_Alta_ComConn, Query);
                    }
                    else
                    {   //Update salesdata record new token data
                        string SerialKey = $"{WebRow["nposno"].ToString()}-{WebRow["nserialno"].ToString()}-{WebRow["nunicodeno"].ToString()}";
                        if (CF.RowExists(DW.dwConn, $"{DW.ActiveDatabase}.salesdata", $"serialkey='{SerialKey}' AND (tokenkey=0 OR pmtprofile=0 OR authnet_custid=0)"))
                        {
                            Query = $"UPDATE applications.salesdata SET tokenkey={TokenXRow["associatedtokenid"].ToString()}, pmtprofile = {TokenXRow["customerpaymentprofileid"].ToString()}, authnet_custid={TokenXRow["authnet_custid"].ToString()} WHERE serialkey = '{SerialKey}'";
                            CF.ExecuteSQL(DW.dwConn, Query);
                        }
                    }
                }
            }
            CF.ExecuteSQL(AM.MirrorConn, $"UPDATE {AM.ActiveDatabase}.tabLog SET LogNo='0',lastUpdate='{LastRecord.ToString(Mirror.AxessDateTimeFormat)}' WHERE TableName = 'EStoreTokenSync'");
            TimeSpan ts = TimeSpan.FromTicks(DateTime.Now.Ticks - FStartTime.Ticks);
            lblLoadFromWeb.Text = string.Format($"{ts.Hours.ToString("0#")}:{ts.Minutes.ToString("0#")}:{ts.Seconds.ToString("0#")}");
            lblLoadFromWeb.Visible = true;
        }

        public void Update_mediaid()
        {
            DateTime FStartTime = DateTime.Now;
            Cursor OldCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            SslStatus.Text = "Cleaning up IKON placeholders.";
            statusStrip1.Refresh();
            CF.ExecuteSQL(DW.dwConn, $"update {DW.ActiveDatabase}.spcards SET wtp32 = '', mediaid='', wtp64='' WHERE wtp32='215B5F7F'");
            CF.ExecuteSQL(DW.dwConn, $"UPDATE {DW.ActiveDatabase}.salesdata SET wtp32 = '', wtp64 = '', MEDIAID='' WHERE wtp32 = '215B5F7F'");
            CF.ExecuteSQL(AM.MirrorConn, $"UPDATE {AM.ActiveDatabase}.tabkassatransaufbereit SET wtp32 = '', wtp64 = '', MEDIAID='' WHERE wtp32 = '215B5F7F'");
            SslStatus.Text = "Loading SalesData for MediaID Update...";
            statusStrip1.Refresh();
            using (DataTable TblSalesData = CF.LoadTable(DW.dwConn, $"SELECT nlfdzaehler, mediaid, wtp64, wtp32, nkartennr FROM {DW.ActiveDatabase}.salesdata WHERE saledate>='2018-04-01' AND (tokenkey=0)  AND (tgroup IN ('EX','SP')) AND nlfdzaehler IS NOT NULL ORDER BY nlfdzaehler", "axsales"))
            {
                if (CF.TableHasData(TblSalesData))
                {
                    PbUpdateMediaID.Value = 0;
                    PbUpdateMediaID.Maximum = TblSalesData.Rows.Count;
                    foreach (DataRow SalesRow in TblSalesData.Rows)
                    {
                        PbUpdateMediaID.Value = TblSalesData.Rows.IndexOf(SalesRow) + 1;
                        SslStatus.Text = $"Updating MediaIDs ({PbUpdateMediaID.Value} of {TblSalesData.Rows.Count})";
                        statusStrip1.Refresh();
                        DataRow RowTransAufBereit = CF.LoadDataRow(AM.MirrorConn, $"SELECT nlfdzaehler, mediaid, wtp64, wtp32 FROM {AM.ActiveDatabase}.tabkassatransaufbereit " +
                            $"WHERE nlfdzaehler='{SalesRow["nlfdzaehler"].ToString()}'" +
                            $" AND (mediaid<>'{SalesRow["mediaid"].ToString().Trim()}' OR wtp32<>'{SalesRow["wtp32"].ToString().Trim()}' OR wtp64<>'{SalesRow["wtp64"].ToString().Trim()}')");
                        if (CF.RowHasData(RowTransAufBereit))
                        {
                            string tMediaID = RowTransAufBereit["mediaid"].ToString().Trim();
                            if (tMediaID == "215B5F7F") tMediaID = string.Empty;
                            string tWTP32 = RowTransAufBereit["wtp32"].ToString().Trim();
                            if (tWTP32 == "215B5F7F") tWTP32 = string.Empty;
                            string tWTP64 = RowTransAufBereit["wtp64"].ToString().Trim();
                            if (tWTP64 == "215B5F7F") tWTP64 = string.Empty;
                            string UpdateQuery = $"UPDATE {DW.ActiveDatabase}.salesdata SET " +
                                $"mediaid='{tMediaID}', nkartennr='{(tMediaID + "          ").Substring(0, 10).Trim()}', " +
                                $"wtp32='{tWTP32}', " +
                                $"wtp64='{tWTP64}' " +
                                $"WHERE nlfdzaehler='{SalesRow["nlfdzaehler"].ToString()}'";
                            CF.ExecuteSQL(DW.dwConn, UpdateQuery);
                            Application.DoEvents();
                        }
                    }
                }
            }
            SslStatus.Text = "COMPLETE";
            TimeSpan ts = TimeSpan.FromTicks(DateTime.Now.Ticks - FStartTime.Ticks);
            lblUpdateMediaIDs.Text = string.Format($"{ts.Hours.ToString("0#")}:{ts.Minutes.ToString("0#")}:{ts.Seconds.ToString("0#")}");
            lblUpdateMediaIDs.Visible = true;
            Cursor.Current = OldCursor;
            Application.DoEvents();
        }

        public void Update_tokenxref()
        {
            DateTime FStartTime = DateTime.Now;
            Cursor OldCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            //deactivate all expired (X), cancelled (C) or refunded/Replaced (R) cards.
            //SslStatus.Text = "Deactivating CC info on invalid cards.";
            //CF.ExecuteSQL(DW.dwConn, $"UPDATE {DW.ActiveDatabase}.spcards SET TOKENKEY = 0, AUTHNET_CUSTid=0, PMTPROFILE=0 WHERE cardstatus IN ('X', 'C', 'R') AND tokenkey <> 0");

            SslStatus.Text = "Loading TokenXRef (1 of 3)";
            using (DataTable TblTokens = CF.LoadTable(BUY.Buy_Alta_ComConn, "SELECT id, firstname, lastname, webid, mediaid, associatedtokenid, customerpaymentprofileid, authnet_custid, serialnr, posnr, nunicodenr, wtp64, workpos FROM axessutility.tokenxref WHERE posnr <> 99 AND serialnr > 0", "tokens"))
            {
                if (CF.TableHasData(TblTokens))
                {
                    SslStatus.Text = "Loading SalesData (2 of 3)";
                    TblSales = CF.LoadTable(DW.dwConn, $"SELECT * FROM {DW.ActiveDatabase}.salesdata WHERE saledate >= '2018-04-01' AND nkassanr NOT IN (27) ORDER BY nlfdzaehler", "axsales");
                    PbUpdateTokenXRef.Maximum = TblTokens.Rows.Count;
                    foreach (DataRow TokenRow in TblTokens.Rows)
                    {
                        PbUpdateTokenXRef.Value = TblTokens.Rows.IndexOf(TokenRow) + 1;
                        PbUpdateTokenXRef.Refresh();
                        SslStatus.Text = $"TokenXRefSync  (TokenXRef -- {PbUpdateTokenXRef.Value} of {PbUpdateTokenXRef.Maximum})";
                        statusStrip1.Refresh();
                        SetTokens(TokenRow);
                    }

                }
            }
            DataTable SalesWithTokens = CF.LoadTable(DW.dwConn, $"SELECT tokenkey, mediaid, nkassanr, nseriennr, nunicodenr FROM {DW.ActiveDatabase}.salesdata WHERE tokenkey > 0", "SalesWithTokens");
            if (CF.TableHasData(SalesWithTokens))
            {
                foreach (DataRow Tokens in SalesWithTokens.Rows)
                {
                    SslStatus.Text = $"Checking dropped tokens ({SalesWithTokens.Rows.IndexOf(Tokens)+1} of {SalesWithTokens.Rows.Count})";
                    Refresh();
                    if (!CF.RowExists(BUY.Buy_Alta_ComConn, "axessutility.tokenxref",$"mediaid = '{Tokens["mediaid"].ToString()}' AND associatedtokenid= {Tokens["tokenkey"].ToString()}"))
                    {
                        string SN = Tokens["nseriennr"].ToString();
                        if (SN == string.Empty) SN = "null";
                        CF.ExecuteSQL(DW.dwConn, $"UPDATE {DW.ActiveDatabase}.salesdata SET tokenkey=0, pmtprofile=0, authnet_custid=0 WHERE nseriennr = {SN} AND tokenkey= {Tokens["tokenkey"].ToString()}");
                        CF.ExecuteSQL(DW.dwConn, $"UPDATE {DW.ActiveDatabase}.SPCards SET tokenkey=0, pmtprofile=0, authnet_custid=0 WHERE mediaid = '{Tokens["nseriennr"].ToString()}' AND tokenkey= {Tokens["tokenkey"].ToString()}");
                    }
                }
            }
            SslStatus.Text = "COMPLETE";
            TimeSpan ts = TimeSpan.FromTicks(DateTime.Now.Ticks - FStartTime.Ticks);
            lblUpdateTokenXRef.Text = string.Format($"{ts.Hours.ToString("0#")}:{ts.Minutes.ToString("0#")}:{ts.Seconds.ToString("0#")}");
            lblUpdateTokenXRef.Visible = true;
            Cursor.Current = OldCursor;
        }

        public void SetTokens(DataRow TokenRow)
        {
            string mrecid = TokenRow["id"].ToString();
            bool UpdateMedia = false;
            string mkassanr = TokenRow["posnr"].ToString().Trim();
            int nkassanr = TokenRow.Field<int>("posnr");
            string mserialnr = TokenRow["serialnr"].ToString().Trim();
            int nserialnr = TokenRow.Field<int>("serialnr");
            string municodenr = TokenRow["nunicodenr"].ToString().Trim();
            string mfirstname = TokenRow["firstname"].ToString().Trim();
            string mlastname = TokenRow["lastname"].ToString().Trim();
            string mtoken = TokenRow["associatedtokenid"].ToString().Trim();
            string mpmtprofile = TokenRow["customerpaymentprofileid"].ToString().Trim();
            string mauthnet_custid = TokenRow["authnet_custid"].ToString().Trim();
            string mmediaid = TokenRow["mediaID"].ToString().Trim();
            string mwtp32 = TokenRow["webid"].ToString().ToUpper().Trim();
            string mwtp64 = TokenRow["wtp64"].ToString().Trim();

            bool ValidTokenMediaID = (mmediaid.Length == 16);
            bool ValidTokenSerialKey = (mkassanr != "0" && mkassanr != "99");
            string UpdateTime = DateTime.Now.ToString(Mirror.AxessDateTimeFormat);

            string SaleWhere = string.Empty;
            if (ValidTokenMediaID)
            {
                SaleWhere = $"mediaid = '{mmediaid}'";
            }
            if (ValidTokenSerialKey)
            {
                if (SaleWhere != string.Empty)
                {
                    SaleWhere += " AND ";
                }
                if (municodenr != string.Empty)
                {
                    SaleWhere += $"serialkey = '{mkassanr}-{mserialnr}-{municodenr}'";
                }
                else
                {
                    SaleWhere += $"nkassanr={mkassanr} AND nseriennr={mserialnr}";
                }
            }
            if (SaleWhere != string.Empty)
            { 
                DataRow SaleRow = CF.LoadDataRow(DW.dwConn, $"SELECT mediaid, wtp32, wtp64, fname, lname, tokenkey, pmtprofile, authnet_custid, serialkey, nkassanr,  nseriennr, nunicodenr, ttype FROM {DW.ActiveDatabase}.salesdata WHERE {SaleWhere} ORDER BY nlfdzaehler DESC LIMIT 1");
                if (CF.RowHasData(SaleRow))
                {
                    if (SaleRow["ttype"].ToString().Trim() == "S")
                    {
                        //if unicode is missing look for it in salesdata
                        bool ResetSerialKey = false;
                        if (municodenr == string.Empty)
                        {
                            string tUni = SaleRow["nunicodenr"].ToString();
                            if (tUni == string.Empty)
                            {
                                tUni = CF.GetSQLField(DW.dwConn, $"SELECT MAX(nunicodenr) FROM {DW.ActiveDatabase}.salesdata WHERE nkassanr={nkassanr}");
                            }
                            if (tUni != string.Empty)
                            {
                                municodenr = mserialnr != "1" ? tUni : (Convert.ToInt32(tUni) + 1).ToString();
                                ValidTokenSerialKey = true;
                                ResetSerialKey = true;
                            }
                        }
                        UpdateMedia = (nkassanr == 99 && ValidTokenMediaID);
                        string SerialKey = $"{mkassanr}-{mserialnr}-{municodenr}";
                        if (!ValidTokenMediaID)
                        {
                            string SalesMediaID = SaleRow["mediaid"].ToString().Trim();
                            if (SalesMediaID.Length == 16)
                            {
                                mmediaid = SalesMediaID;
                                UpdateMedia = true;
                                ValidTokenMediaID = true;
                            }
                        }
                        if (ValidTokenMediaID || ValidTokenSerialKey)
                        {
                            string TokenSet = "";
                            if (mlastname == string.Empty)  //ReadOnlyException change name fields if the last name are empty
                            {
                                TokenSet = $"firstname='{CF.EscapeChar(SaleRow["fname"].ToString().Trim())}', lastname='{CF.EscapeChar(SaleRow["lname"].ToString().Trim())}'";
                            }
                            if (ResetSerialKey || nkassanr != SaleRow.Field<Int32>("nkassanr") || nserialnr != SaleRow.Field<Int32>("nseriennr"))
                            {
                                if (TokenSet != string.Empty)
                                {
                                    TokenSet += ", ";
                                }
                                if (!ResetSerialKey)
                                {
                                    municodenr = SaleRow["nunicodenr"].ToString().Trim();
                                }
                                TokenSet += $"posnr={SaleRow["nkassanr"].ToString()}, serialnr={SaleRow["nseriennr"].ToString()}, nunicodenr={(municodenr != string.Empty ? municodenr : "null")}";
                            }
                            if (mwtp32 != SaleRow["wtp32"].ToString().Trim() || mwtp64 != SaleRow["wtp64"].ToString().Trim() || mmediaid != SaleRow["mediaid"].ToString().Trim())
                            {
                                if (TokenSet != string.Empty)
                                {
                                    TokenSet += ", ";
                                }
                                TokenSet += $"webid='{SaleRow["wtp32"].ToString().Trim()}', wtp64='{SaleRow["wtp64"].ToString()}', mediaid='{SaleRow["mediaid"].ToString().Trim()}'";
                            }
                            if (TokenSet != string.Empty)
                            {
                                CF.ExecuteSQL(BUY.Buy_Alta_ComConn, $"UPDATE axessutility.tokenxref SET {TokenSet}, lastUpdated='{UpdateTime}' WHERE id='{mrecid}'");
                            }
                            if (mtoken != SaleRow["tokenkey"].ToString().Trim() || mauthnet_custid != SaleRow["authnet_custid"].ToString().Trim() || mpmtprofile != SaleRow["pmtprofile"].ToString().Trim())
                            {
                                //string SalesWhere = ValidTokenMediaID ? $"mediaid = '{mmediaid}'" : $"serialkey='{mserialkey}'";
                                CF.ExecuteSQL(DW.dwConn, $"UPDATE {DW.ActiveDatabase}.salesdata SET tokenkey={mtoken}, pmtprofile={mpmtprofile}, authnet_custid={mauthnet_custid}, dtupdate = '{UpdateTime}' WHERE {SaleWhere} AND ttype = 'S'");
                                if (ValidTokenMediaID)
                                {
                                    CF.ExecuteSQL(DW.dwConn, $"UPDATE {DW.ActiveDatabase}.spcards SET tokenkey={mtoken}, pmtprofile={mpmtprofile}, authnet_custid={mauthnet_custid}, dtupdate = '{UpdateTime}' WHERE mediaid = '{mmediaid}' AND cardstatus IN ('A', 'B', 'P')");
                                }
                            }
                        }
                    }
                    else
                    {
                        //Clancel or Replacement
                        CF.ExecuteSQL(DW.dwConn, $"UPDATE {DW.ActiveDatabase}.salesdata SET tokenkey = 0, pmtprofile = 0, authnet_custid = 0, dtupdate = '{UpdateTime}' WHERE {SaleWhere}");
                        if (ValidTokenMediaID)
                        { 
                            CF.ExecuteSQL(DW.dwConn, $"UPDATE {DW.ActiveDatabase}.spcards  SET tokenkey = 0, pmtprofile = 0, authnet_custid = 0, dtupdate = '{UpdateTime}' WHERE mediaid = '{mmediaid}'");
                        }
                        CF.ExecuteSQL(BUY.Buy_Alta_ComConn, $"DELETE FROM axessutility.tokenxref WHERE id='{mrecid}'");
                    }
                }
            }
            Application.DoEvents();
        }

        private void Update99s()
        {
            using (DataTable Tokens = CF.LoadTable(BUY.Buy_Alta_ComConn, "SELECT id, mediaid  FROM axessutility.tokenxref WHERE LENGTH(mediaID) = 16 AND posNr = 99", "Tokens"))
            {
                foreach (DataRow TokenRow in Tokens.Rows)
                {
                    SslStatus.Text = $"TokenXRefSync  (Tapped MediaID -- {(Tokens.Rows.IndexOf(TokenRow) + 1).ToString()} of {Tokens.Rows.Count.ToString()})";
                    statusStrip1.Refresh();
                    string RecID = TokenRow["id"].ToString().Trim();
                    string RecMediaID = TokenRow["mediaid"].ToString().Trim();
                    DataRow SaleRow = CF.LoadDataRow(DW.dwConn, $"SELECT nkassanr, nseriennr, nunicodenr, wtp64, wtp32, fname, lname FROM {DW.ActiveDatabase}.salesdata WHERE mediaid='{RecMediaID}' AND nseriennr > 0 ORDER BY saledate DESC LIMIT 1");
                    if (CF.RowHasData(SaleRow))
                    {
                        string SerialKey = $"{SaleRow["nkassanr"].ToString().Trim()}-{SaleRow["nseriennr"].ToString().Trim()}-{SaleRow["nunicodenr"].ToString().Trim()}";
                        string UpdateTime = DateTime.Now.ToString(Mirror.AxessDateTimeFormat);
                        CF.ExecuteSQL(BUY.Buy_Alta_ComConn, $"UPDATE axessutility.tokenxref SET lastname = '{CF.EscapeChar(SaleRow["lname"].ToString().Trim())}', firstname = '{CF.EscapeChar(SaleRow["fname"].ToString().Trim())}', webId = '{SaleRow["wtp32"].ToString().Trim()}', wtp64 = '{SaleRow["wtp64"].ToString().Trim()}', posnr = {SaleRow["nkassanr"].ToString().Trim()}, serialnr = {SaleRow["nseriennr"].ToString().Trim()}, nunicodenr = {SaleRow["nunicodenr"].ToString().Trim()}, lastupdated = '{UpdateTime}' WHERE id = '{RecID}'");
                        //CF.ExecuteSQL(DW.dwConn, $"UPDATE {DW.ActiveDatabase}.tokenxref SET lastname = '{SaleRow["lname"].ToString().Trim()}', firstname = '{SaleRow["fname"].ToString().Trim()}', webId = '{SaleRow["wtp32"].ToString().Trim()}', wtp64 = '{SaleRow["wtp64"].ToString().Trim()}', posnr = {SaleRow["nkassanr"].ToString().Trim()}, serialnr = {SaleRow["nseriennr"].ToString().Trim()}, nunicodenr = {SaleRow["nunicodenr"].ToString().Trim()}, lastupdated = '{UpdateTime}' WHERE id = '{RecID}'");
                    }
                }
            }
        }

        public void Update_salesdata()
        {
            DateTime FStartTime = DateTime.Now;
            Cursor OldCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            SslStatus.Text = "Update Salesdata Pass 1 of 3";
            
            DataColumn[] keys = new DataColumn[1];
            using (DataTable TblTokenXRef = CF.LoadTable(BUY.Buy_Alta_ComConn, "SELECT mediaid, associatedtokenid, authnet_custid FROM axessutility.tokenxref WHERE POSNr <> 99 AND LENGTH(mediaID) = 16 GROUP BY mediaID", "tokens"))
            {
                keys[0] = TblTokenXRef.Columns["mediaid"];
                TblTokenXRef.PrimaryKey = keys;
                using (DataTable TblSalesData = CF.LoadTable(DW.dwConn, $"SELECT nlfdzaehler, mediaid, tokenkey, authnet_custid FROM {DW.ActiveDatabase}.salesdata WHERE saledate > '{msaledate}' AND tokenkey <> 0", "axsalesx"))
                {
                    PbUpdateSalesData.Value = 0;
                    PbUpdateSalesData.Maximum = TblSalesData.Rows.Count;
                    foreach (DataRow RowSalesData in TblSalesData.Rows)
                    {
                        PbUpdateSalesData.Value = TblSalesData.Rows.IndexOf(RowSalesData);
                        if (RowSalesData["mediaid"].ToString().Trim().Length != 0 && RowSalesData["nlfdzaehler"].ToString() != "NULL")
                        {
                            DataRow RowToken = TblTokenXRef.Rows.Find(RowSalesData["mediaid"].ToString());
                            if (CF.RowHasData(RowToken))
                                if (RowSalesData["tokenkey"].ToString() != RowToken["associatedtokenid"].ToString() || RowSalesData["authnet_custid"].ToString() != RowToken["authnet_custid"].ToString())
                                    CF.ExecuteSQL(DW.dwConn, $"UPDATE {DW.ActiveDatabase}.salesdata SET tokenkey='{RowToken["associatedtokenkey"].ToString()}', pmtprofile='{RowToken["customerpaymentprofileid"].ToString()}', authnet_custid='{RowToken["authnet_custid"].ToString()}', dtupdate='{DateTime.Now.ToString(Mirror.AxessDateTimeFormat)}' WHERE nlfdzaehler={RowSalesData["nlfdzaehler"].ToString()}");
                        }
                        Application.DoEvents();
                    }
                }
                //verify that all active, blocked and Presale cards with a token in SPCards have a matching token in TokenXRef, reset to no token in SPCards if not matched.
                SslStatus.Text = "Update Salesdata Pass 2 of 3";
                using (DataTable TblSPCards = CF.LoadTable(DW.dwConn, $"SELECT recid, tokenkey FROM {DW.ActiveDatabase}.spcards WHERE cardstatus IN ('A','B','P') AND tokenkey<>0", "spcards"))
                {
                    if (TblSPCards != null)
                    {
                        PbUpdateSalesData.Value = 0;
                        PbUpdateSalesData.Maximum = TblSPCards.Rows.Count;
                        keys[0] = TblTokenXRef.Columns["tokenkey"];
                        TblTokenXRef.PrimaryKey = keys;
                        foreach (DataRow RowSPCards in TblSPCards.Rows)
                        {
                            PbUpdateSalesData.Value = TblSPCards.Rows.IndexOf(RowSPCards);
                            DataRow RowToken = TblTokenXRef.Rows.Find(RowSPCards["tokenkey"].ToString());
                            if (CF.RowHasData(RowToken))
                            {
                                CF.ExecuteSQL(DW.dwConn, $"UPDATE {DW.ActiveDatabase}.spcards SET tokenkey=0, pmtprofile=0, authnet_custid=0 WHERE recid={RowSPCards["recid"].ToString()}");
                            }
                            SslStatus.Text = $"Row {TblSPCards.Rows.IndexOf(RowSPCards).ToString()} of {TblSPCards.Rows.Count.ToString()}";
                            Application.DoEvents();
                        }
                    }
                }
            }
            //*** update artrans ***
            //
            SslStatus.Text = "Update Salesdata Pass 3 of 3";
            using (DataTable TblARTrans = CF.LoadTable(DW.dwConn, $"SELECT serialkey, tokenkey FROM {DW.ActiveDatabase}.artrans WHERE authcode='' AND (arcustid='11874' OR (tokenkey=0 AND arcustid=''))", "artrans"))
            {
                using (DataTable TblSPCards = CF.LoadTable(DW.dwConn, $"SELECT serialkey, tokenkey, authnet_custid FROM {DW.ActiveDatabase}.spcards WHERE tokenkey>0 GROUP BY serialkey", "spcards"))
                {
                    PbUpdateSalesData.Value = 0;
                    PbUpdateSalesData.Maximum = TblSPCards.Rows.Count;
                    keys[0] = TblSPCards.Columns["serialkey"];
                    TblSPCards.PrimaryKey = keys;
                    foreach (DataRow RowARTrans in TblARTrans.Rows)
                    {
                        PbUpdateSalesData.Value = TblARTrans.Rows.IndexOf(RowARTrans);
                        DataRow RowSPCards = TblSPCards.Rows.Find(RowARTrans["serialkey"].ToString());
                        if (CF.RowHasData(RowSPCards))
                        {
                            if (RowARTrans["tokenkey"].ToString() != RowSPCards["tokenkey"].ToString())
                            {
                                CF.ExecuteSQL(DW.dwConn, $"UPDATE {DW.ActiveDatabase}.artrans SET tokenkey='{RowSPCards["tokenkey"].ToString()}', authnet_custid='{RowSPCards["authnet_custid"].ToString()}' WHERE serialkey='{RowARTrans["serialkey"].ToString()}' AND authcode = ''");
                            }
                        }
                        Application.DoEvents();
                    }
                }
            }
            SslStatus.Text = "COMPLETE";
            TimeSpan ts = TimeSpan.FromTicks(DateTime.Now.Ticks - FStartTime.Ticks);
            lblUpdateSalesData.Text = string.Format($"{0:D2}:{1:D2}:{2:D2}", ts.Hours, ts.Minutes, ts.Seconds);
            lblUpdateSalesData.Visible = true;
            Cursor.Current = OldCursor;
        }

 

        private void Run()
        {
            timer1.Enabled = false;
            Cursor OldCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            LblStartTime.Text = $"Current run started {DateTime.Now.ToString("MMM dd, yyyy hh:mm:ss")}";
            lblLoadFromWeb.Visible = false;
            lblUpdateMediaIDs.Visible = false;
            lblUpdateSalesData.Visible = false;
            lblUpdateTokenXRef.Visible = false;
            BtnRunUpdates.Enabled = false;
            // add code to update sync status table for each run.
            pbLoadFromWeb.Value = 0;
            PbUpdateMediaID.Value = 0;
            PbUpdateTokenXRef.Value = 0;
            PbUpdateSalesData.Value = 0;

            if (cbLoadFromWeb.Checked) Load_From_eStore();
            if (CbUpdateMediaID.Checked)  Update_mediaid();
            Update99s();

            //if (cbSyncTokenFiles.Checked) MirrorTokenXRef();
            if (CbUpdateTokenXRef.Checked)
            {
                Update_tokenxref();
            }
            //if (CbUpdateSalesData.Checked) Update_salesdata();
            // add code to update sync status table for each run.
            LblStartTime.Text = $"Next run at {DateTime.Now.AddTicks(timer1.Interval).ToString("MM dd, yyyy hh:mm:ss")}";
            Cursor.Current = OldCursor;
            Int32 CurHour = DateTime.Now.Hour;
            if (CurHour < 6 || CurHour > 17)
            {
                Application.Exit();
            }
            timer1.Enabled = true;
        }

        private void BtnRunUpdates_Click(object sender, EventArgs e) => Run();

        private void Timer1_Tick(object sender, EventArgs e) => Run();

        private void TokenXRefSync_FormClosed(object sender, FormClosedEventArgs e)
        {
            CF.SaveFormData("TokenXRefSync", Location.Y, Location.X, Height, Width, null, DateTime.Now, (int)e.CloseReason);
        }
    }

}

