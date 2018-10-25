using System;
using System.Data;
using MySql.Data.MySqlClient;

namespace asl_SyncLibrary
{
    public class BUY_ALTA_COM
    {
        private CommonFunctions cf = new CommonFunctions();
        private ASLError myError = new ASLError();

        public MySqlConnection Buy_Alta_ComConn = new MySqlConnection(Properties.Settings.Default.MySQLWS);


        
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

        private const string prodDatabase = "asbshared";
        private const string testDatabase = "asbshared_test";

        public string ActiveDatabase
        {
            get
            {
                return cf.TestMode ? testDatabase : prodDatabase;
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

        public Serverstatus GetServerStatus()
        {
            Serverstatus tStat = Serverstatus.Unknown;
            if (cf.OpenConn(Buy_Alta_ComConn) == ConnectionState.Open)
            {
                cf.CloseConn(Buy_Alta_ComConn);
                tStat = Serverstatus.Active;
            }
            else
            {
                try
                {
                    Buy_Alta_ComConn.Ping();
                    tStat = Serverstatus.Service_Failed;
                }
                catch { tStat = Serverstatus.Server_Failed; }
            }
            return tStat;
        }

        public string GetEStoreSaleDate(int nAufTragNr)
        {
            //return Convert.ToDateTime(cf.GetSQLField(Buy_Alta_ComConn, "SELECT alta_orders.created AS eStoreDate FROM altaweb.alta_order_items INNER JOIN altaweb.alta_orders ON alta_order_items.order_ID = alta_orders.id WHERE alta_order_items.id =" + nAufTragNr.ToString())).ToString(Mirror.AxessDateTimeFormat);

            MySqlCommand tCmd = new MySqlCommand("SELECT alta_orders.created AS eStoreDate FROM altaweb.alta_order_items INNER JOIN altaweb.alta_orders ON alta_order_items.order_ID = alta_orders.id WHERE alta_order_items.id =" + nAufTragNr.ToString(), Buy_Alta_ComConn);
            DataSet ds = new DataSet();
            MySqlDataAdapter da = new MySqlDataAdapter();
            tCmd.CommandType = CommandType.Text;
            da.SelectCommand = tCmd;
            string rtnValue = "NULL";
            try
            {
                Buy_Alta_ComConn.Open();
                if (da.Fill(ds, "EStore") != 0) rtnValue = ds.Tables["EStore"].Rows[0].Field<DateTime>("eStoreDate").ToString(Mirror.AxessDateTimeFormat);
            }
            catch (MySqlException ex)
            {
                string msg = ex.Message;
                int no = ex.Number;
            }
            if (Buy_Alta_ComConn.State != ConnectionState.Closed) Buy_Alta_ComConn.Close();
            return rtnValue;
        }

        public bool SalesFromMTC(DataRow aData)
        {
            string tmpData = aData.Field<string>("custom response") + string.Empty;
            string tmpMCPNbr = aData.Field<string>("barcode");
            string sqlSales = string.Empty;
            if (tmpData != null)
            {
                tmpData = cf.EscapeChar(tmpData.ToLower());
            }
            else
            {
                tmpData = string.Empty;
            }
            //check to see if the record already exists
            string tTickType = aData["ticket type"].ToString();
            if (tTickType.Contains("Child"))
            {
                tTickType = "Child";
            }
            string tGFN = aData.Field<string>("guest first name").Trim().ToUpper();
            tGFN = (tGFN == null) ? "" : cf.EscapeChar(tGFN.Replace("(", "").Replace(")", "").Replace("â€™", "'").Replace("Ã©", "e").Replace("Ã¤", "a").Replace("/", ""));
            string tGLN = aData.Field<string>("guest last name").Trim().ToUpper();
            tGLN = (tGLN == null) ? "" : cf.EscapeChar(tGLN.Replace("(", "").Replace(")", "").Replace("â€™", "'").Replace("Ã©", "e").Replace("Ã¤", "a").Replace("/", ""));
            string tPFN = aData.Field<string>("purchaser first name");
            tPFN = (tPFN == null) ? "" : cf.EscapeChar(tPFN.Replace("(", "").Replace(")", "")).Trim().ToUpper();
            string tPLN = aData.Field<string>("purchaser last name");
            if (tPLN == null)
            {
                tPLN = string.Empty;
            }
            else
                tPLN = cf.EscapeChar(tPLN.Replace("(", "").Replace(")", "").Trim().ToUpper());
            DataRow tDR = cf.LoadDataRow(Buy_Alta_ComConn, $"SELECT * FROM {ActiveDatabase}.mtncol_sales WHERE mcpnbr='{tmpMCPNbr}'");
            if (tDR == null || tDR["recid"].ToString() == string.Empty)
            {
                sqlSales = $"INSERT INTO {ActiveDatabase}.mtncol_sales (order_id, mcpnbr, ticket_id, order_status, order_date, product, ticket_type, guest_first_name, guest_last_name,  guest_email, guest_birthdate, guest_height, guest_gender, guest_ability_level, guest_shoe_size, guest_shoe_style, guest_shoe_type, guest_equipment_choice, purchaser_first_name, purchaser_last_name, purchaser_email, marketing_opt_in, billing_address, billing_city, billing_state, billing_zip, billing_country, mailing_address, mailing_city, mailing_state, mailing_zip, mailing_country, image_url, store, avail_days, custom_response, custom_question, pre_arrival_approved, pre_arrival_submit_date, days_used) VALUES (";
                sqlSales += $"'{aData.Field<string>("order id").ToString()}',";
                sqlSales += $"'{aData.Field<string>("barcode")}',";
                sqlSales += $"'{aData.Field<string>("ticket id").ToString()}',";
                sqlSales += $"'{aData.Field<string>("order status")}',";
                sqlSales += $"'{aData.Field<string>("order date")}',";
                sqlSales += $"'{aData.Field<string>("product name")}',";
                sqlSales += $"'{tTickType}',";
                sqlSales += $"'{tGFN}',";
                sqlSales += $"'{tGLN}',";
                sqlSales += $"'{cf.EscapeChar(aData.Field<string>("guest email"))}',";
                sqlSales += $"'{aData.Field<string>("guest birth date")}',";
                sqlSales += $"'{cf.EscapeChar(aData.Field<string>("guest height"))}',";
                sqlSales += $"'{aData.Field<string>("guest gender")}',";
                sqlSales += $"'{aData.Field<string>("guest ability")}',";
                sqlSales += $"'{aData.Field<string>("guest shoe size")}',";
                sqlSales += $"'{aData.Field<string>("guest shoe style")}',";
                sqlSales += $"'{aData.Field<string>("guest shoe type")}',";
                sqlSales += $"'{aData.Field<string>("guest equipment choice")}',";
                sqlSales += $"'{tPFN}',";
                sqlSales += $"'{tPLN}',";
                sqlSales += $"'{cf.EscapeChar(aData.Field<string>("purchaser email"))}',";
                sqlSales += $"'{aData.Field<string>("marketing opt in")}',";
                sqlSales += $"'{cf.EscapeChar(aData.Field<string>("billing address"))}',";
                sqlSales += $"'{cf.EscapeChar(aData.Field<string>("billing city"))}',";
                sqlSales += $"'{aData.Field<string>("billing state")}',";
                sqlSales += $"'{aData.Field<string>("billing zip")}',";
                sqlSales += $"'{aData.Field<string>("billing country")}',";
                sqlSales += $"'{cf.EscapeChar(aData.Field<string>("mailing address"))}',";
                sqlSales += $"'{cf.EscapeChar(aData.Field<string>("mailing city"))}',";
                sqlSales += $"'{aData.Field<string>("mailing state")}',";
                sqlSales += $"'{aData.Field<string>("mailing zip")}',";
                sqlSales += $"'{aData.Field<string>("mailing country")}',";
                sqlSales += $"'{aData.Field<string>("image url")}',";
                sqlSales += $"'{aData.Field<string>("store")}',";
                sqlSales += tmpData.Contains("alta")  ? "'3'," : "'2',";
                sqlSales += $"'{tmpData}',";
                sqlSales += $"'{aData.Field<string>("custom question")}',";
                sqlSales += $"'{aData.Field<string>("pre arrival approved")}',";
                sqlSales += $"'{aData.Field<string>("pre arrival approved date")}',";
                sqlSales += $"'0')";
            }
            else
            {
                sqlSales = String.Empty;
                if (tTickType != tDR["ticket_type"].ToString())
                    sqlSales += $"ticket_type='{tTickType}',"; 
                if (aData.Field<string>("order status") != tDR["order_status"].ToString())
                    sqlSales += $"order_status='{aData.Field<string>("order status")}',";
                if (tGFN != tDR["guest_first_name"].ToString().ToUpper())
                    sqlSales += $"guest_first_name='{tGFN}',";
                if (tGLN != tDR["guest_last_name"].ToString().ToUpper())
                    sqlSales += $"guest_last_name='{tGLN}',";
                if (cf.EscapeChar(aData.Field<string>("guest email")) != tDR["guest_email"].ToString())
                    sqlSales += $"guest_email='{cf.EscapeChar(aData.Field<string>("guest email"))}',";
                if (aData.Field<string>("guest birth date") != tDR.Field<DateTime>("guest_birthdate").ToString(Mirror.AxessDateFormat))
                    sqlSales += $"guest_birthdate='{aData.Field<string>("guest birth date")}',";
                //if (aData.Field<string>("guest height") != tDR["guest_height"].ToString())
                //    sqlSales += $"guest_height='{aData.Field<string>("guest height")}',";
                //if (aData.Field<string>("guest gender") != tDR["guest_gender"].ToString())
                //    sqlSales += $"guest_gender='{aData.Field<string>("guest gender")}',";
                //if (aData.Field<string>("guest ability") != tDR["guest_ability_level"].ToString())
                //    sqlSales += $"guest_ability_level='{aData.Field<string>("guest ability")}',";
                //if (aData.Field<string>("guest shoe size") != tDR["guest_shoe_size"].ToString())
                //    sqlSales += $"guest_shoe_size='{aData.Field<string>("guest shoe size")}',";
                //if (aData.Field<string>("guest shoe style") != tDR["guest_shoe_style"].ToString())
                //    sqlSales += $"guest_shoe_style='{aData.Field<string>("guest shoe style")}',";
                //if (aData.Field<string>("guest shoe type") != tDR["guest_shoe_type"].ToString())
                //    sqlSales += $"guest_shoe_type='{aData.Field<string>("guest shoe type")}',";
                //if (aData.Field<string>("guest equipment choice") != tDR["guest_equipment_choice"].ToString())
                //    sqlSales += $"guest_equipment_choice='{aData.Field<string>("guest equipment choice")}',";
                if (tPFN != tDR["purchaser_first_name"].ToString().ToUpper().Trim())
                    sqlSales += $"purchaser_first_name='{cf.EscapeChar(tPFN.Trim())}',";
                if (tPLN != tDR["purchaser_last_name"].ToString().ToUpper().Trim())
                    sqlSales += $"purchaser_last_name='{cf.EscapeChar(tPLN.Trim())}',";
                if (cf.EscapeChar(aData.Field<string>("purchaser email")) != tDR["purchaser_email"].ToString().Trim())
                    sqlSales += $"purchaser_email='{cf.EscapeChar(aData.Field<string>("purchaser email"))}',";
                //sqlSales += $"marketing_opt_in='{aData.Field<string>("marketing opt in")}',";
                if (aData.Field<string>("image url") != null)
                    if (tDR["image_url"].ToString().Trim() != aData.Field<string>("image url").Trim())
                        sqlSales += $"image_url='{aData.Field<string>("image url").Trim()}',";
                //sqlSales += $"store='{aData.Field<string>("store")}',";
                if (tmpData.ToUpper().Trim() != tDR["custom_response"].ToString().ToUpper().Trim())
                {
                    sqlSales += $"custom_response='{tmpData.Trim()}',";
                    sqlSales += $"avail_days='{(tmpData.Contains("alta") ? "3" : "2")}',";
                }
                //sqlSales += $"custom_question='{aData.Field<string>("custom question")}',";
                if ((aData["pre arrival approved"].ToString().Trim() != tDR["pre_arrival_approved"].ToString().Trim()) || (aData["pre arrival approved"].ToString().Trim().ToUpper() == "YES"))
                {
                    sqlSales += $"pre_arrival_approved='{aData.Field<string>("pre arrival approved")}',";
                    sqlSales += $"pre_arrival_submit_date='{aData.Field<string>("pre arrival approved date")}',";
                    sqlSales += $"billing_address='{cf.EscapeChar(aData.Field<string>("billing address"))}',";
                    sqlSales += $"billing_city='{cf.EscapeChar(aData.Field<string>("billing city"))}',";
                    sqlSales += $"billing_state='{aData.Field<string>("billing state")}',";
                    sqlSales += $"billing_zip='{aData.Field<string>("billing zip")}',";
                    sqlSales += $"billing_country='{aData.Field<string>("billing country")}',";
                    sqlSales += $"mailing_address='{cf.EscapeChar(aData.Field<string>("mailing address"))}',";
                    sqlSales += $"mailing_city='{cf.EscapeChar(aData.Field<string>("mailing city"))}',";
                    sqlSales += $"mailing_state='{aData.Field<string>("mailing state")}',";
                    sqlSales += $"mailing_zip='{aData.Field<string>("mailing zip")}',";
                    sqlSales += $"mailing_country='{aData.Field<string>("mailing country")}',";
                }
                //sqlSales += $"days_used='0',";
                if (sqlSales != string.Empty)
                {
                    sqlSales = $"UPDATE {ActiveDatabase}.mtncol_sales SET {sqlSales.Substring(0,sqlSales.Length - 1)} WHERE mcpnbr='{tmpMCPNbr}'";
                }
            }
            if (sqlSales == string.Empty)
                return false;
            return cf.ExecuteSQL(Buy_Alta_ComConn, sqlSales);
        }
    }
}
