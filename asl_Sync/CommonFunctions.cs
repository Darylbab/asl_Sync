using System;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using MySql.Data.MySqlClient;



namespace asl_SyncLibrary
{
    public class CommonFunctions
    {

        private ASLError tError = new ASLError();

        internal MySqlConnection DWConn = new MySqlConnection(Properties.Settings.Default.MySQLDW);
        internal OleDbConnection VFPConn = new OleDbConnection("Provider=VFPOLEDB.1");
        internal Int32 timeDiff = 0; // = Convert.ToInt32(TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time").GetUtcOffset(DateTimeOffset.UtcNow) - TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time").GetUtcOffset(DateTimeOffset.UtcNow));

        private MySqlDataAdapter DA = new MySqlDataAdapter();
        private SqlDataAdapter SDA = new SqlDataAdapter();
        private OleDbDataAdapter ODA = new OleDbDataAdapter();

        public DateTime nullDate = new DateTime(1, 1, 1);
        public DateTime defaultDOB = Properties.Settings.Default.DefaultBirthdate;
        public string FileNameDateFormat = "yyyyMMdd";
        public string FileNameDatestampFormat = "yyyyMMdd_HHmmss";
        public string Season = Properties.Settings.Default.Season;
        public string SeasonShort = Properties.Settings.Default.Season.Substring(2);
        public DateTime SeasonStart = Properties.Settings.Default.SeasonStart;
        public DateTime SeasonEnd = Properties.Settings.Default.SeasonEnd;
        public DateTime YearEnd = Properties.Settings.Default.YearEnd;
        public int MTCMaxUse = Properties.Settings.Default.MaxUse_MC;
        public DateTime MTCYearEnd = Properties.Settings.Default.MtnColEnd;
        public string DataFolder = Properties.Settings.Default.SharedDataFolder;
        public string ITDataFolder = Properties.Settings.Default.ITFileCopyFolder;


        private int tErrorNo = 0;
        private string tErrorMsg = string.Empty;
        private string tErrorData = string.Empty;
        private long tErrorLogNo = 0;
        private string tErrorFunction = string.Empty;
        private string tErrorFunctionMessage = string.Empty;

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

        public long LastErrorNo
        {
            get
            {
                return tErrorNo;
            }
        }

        public string LastErrorMsg
        {
            get
            {
                return tErrorMsg;
            }
        }

        private bool tInit = false;

        public bool Initialized
        {
            get 
            {
                return tInit;
            }
        }

        private DateTime tLastInitTime;

        public DateTime LastInitTime
        {
            get
            {
                return tLastInitTime;
            }
        }


        public bool Init(bool canReInit = false)
        {
            tInit &= !canReInit;
            if (!tInit)
            {
                //ExternalDates = LoadDataRow(DWConn, "SELECT * FROM applications.asl_external_dates");
                //if (ExternalDates == DataRow))
                tLastInitTime = DateTime.Now;
            }
            return tInit;
        }


        public string ErrorData
        {
            get
            {
                return tErrorData;
            }
        }

        public string ErrorCallingFunction
        {
            get
            {
                return tErrorFunction;
            }
        }

        public long ErrorLogNo
        {
            get
            {
                return tErrorLogNo;
            }
        }

        public string ErrorFunction
        {
            get
            {
                return tErrorFunction;
            }
        }

        public string ErrorFunctionMessage
        {
            get
            {
                return tErrorFunctionMessage;
            }
        }

        public bool IsAltaPOS(long KassaNr) => (KassaNr >= 1 && KassaNr <= 60) || (KassaNr >= 100 && KassaNr <= 105) || (KassaNr >= 117 && KassaNr <= 121);

        public bool SaveFormData(string FormName, int Top, int Left, int Height, int Width, DateTime? LastStart = null, DateTime? LastStop = null, int StopReason = -1)
        {
            string MyQuery = string.Empty;
            string MyFilter = $"`user`='{Environment.UserName.ToUpper()}' AND form='{FormName}'";
            string LastStart_S = (LastStart == null ? "null" : $"'{LastStart.Value.ToString(Mirror.AxessDateTimeFormat)}'");
            string lastStop_S = (LastStop == null ? "null" : $"'{LastStop.Value.ToString(Mirror.AxessDateTimeFormat)}'");
            if (RowExists(DWConn, "alta_utilities.formData", $"{MyFilter}"))
            {
                MyQuery = $"UPDATE alta_utilities.formData SET top={Top}, `left`={Left}, height={Height}, width={Width}";
                if (LastStart != null) MyQuery += $", lastStart={LastStart_S}";
                if (LastStop != null) MyQuery += $", lastStop={lastStop_S}";
                
                if (StopReason != -1) MyQuery += $", stopReason={StopReason}";
                MyQuery +=$" WHERE {MyFilter}";
            }
            else
            {
                MyQuery = $"INSERT alta_utilities.formData (`user`, form, top, `left`, height, width, lastStart, lastStop, stopReason) VALUES ('{Environment.UserName.ToUpper()}', '{FormName}', {Top}, {Left}, {Height}, {Width}, {LastStart_S}, {lastStop_S}, {StopReason})";
            }
            return ExecuteSQL(DWConn, MyQuery);
        }

        public DataRow GetFormLocation(string FormName) => LoadDataRow(DWConn, $"SELECT * FROM alta_utilities.formData WHERE `user`='{Environment.UserName.ToUpper()}' AND form = '{FormName}'");

        public string GetKeyValue(string aSource, int aKey)
		{
			string[] tVals = aSource.Split('-');
			if (tVals.Length <= aKey || aKey < 0 || aSource.Length == 0)
			{
				return string.Empty;
			}

			return tVals[aKey];
		}

        public bool IsURLOnline(string address)
        {
            try
            {
                System.Net.WebClient client = new WebClient();
                byte[] testx = client.DownloadData(address);
                string testy = System.Text.Encoding.Default.GetString(testx);
                XmlDocument tDoc = new XmlDocument();
                tDoc.LoadXml(testy);
                return true;
            }
            catch(WebException ex)
            {
                bool ReturnVal = false;
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    switch (((HttpWebResponse)ex.Response).StatusCode.ToString())
                    {
                        case "CacheEntryNotFound":
                            tErrorNo = 18;
                            tErrorMsg = "The specified cache entry was not found.";
                            break;
                        case "ConnectFailure":
                            tErrorNo = 2;
                            tErrorMsg = "The remote service point could not be contacted at the transport level.";
                            break;
                        case "ConnectionClosed":
                            tErrorNo = 8;
                            tErrorMsg = "The connection was prematurely closed.";
                            ReturnVal = true;
                            break;
                        case "KeepAliveFailure":
                            tErrorNo = 12;
                            tErrorMsg = "The connection for a request that specifies the Keep - alive header was closed unexpectedly.";
                            ReturnVal = true;
                            break;
                        case "MessageLengthLimitExceeded":
                            tErrorNo = 17;
                            tErrorMsg = "A message was received that exceeded the specified limit when sending a request or receiving a response from the server.";
                            ReturnVal = true;
                            break;
                        case "NameResolutionFailure":
                            tErrorNo = 1; 
                            tErrorMsg = "The name resolver service could not resolve the host name.";
                            break;
                        case "Pending":
                            tErrorNo = 13;
                            tErrorMsg = "An internal asynchronous request is pending.";
                            ReturnVal = true;
                            break;
                        case "PipelineFailure":
                            tErrorNo = 5; 
                            tErrorMsg = "The request was a pipelined request and the connection was closed before the response was received.";
                            ReturnVal = true;
                            break;
                        case "ProtocolError":
                            tErrorNo = 7; 
                            tErrorMsg = "The response received from the server was complete but indicated a protocol-level error. For example, an HTTP protocol error such as 401 Access Denied would use this status.";
                            ReturnVal = true;
                            break;
                        case "ProxyNameResolutionFailure":
                            tErrorNo = 15; 
                            tErrorMsg = "The name resolver service could not resolve the proxy host name.";
                            break;
                        case "ReceiveFailure":
                            tErrorNo = 3;
                            tErrorMsg = "A complete response was not received from the remote server.";
                            break;
                        case "RequestCanceled":
                            tErrorNo = 6; 
                            tErrorMsg = "The request was canceled, the Abort() method was called, or an unclassifiable error occurred.This is the default value for Status.";
                            ReturnVal = true;
                            break;
                        case "RequestProhibitedByCachePolicy":
                            tErrorNo = 19;
                            tErrorMsg = "The request was not permitted by the cache policy.In general, this occurs when a request is not cacheable and the effective policy prohibits sending the request to the server.You might receive this status if a request method implies the presence of a request body, a request method requires direct interaction with the server, or a request contains a conditional header.";
                            ReturnVal = true;
                            break;
                        case "RequestProhibitedByProxy":
                            tErrorNo = 20;
                            tErrorMsg = "This request was not permitted by the proxy.";
                            ReturnVal = true;
                            break;
                        case "SecureChannelFailure":
                            tErrorNo = 10;
                            tErrorMsg = "An error occurred while establishing a connection using SSL.";
                            break;
                        case "SendFailure":
                            tErrorNo = 4; 
                            tErrorMsg = "A complete request could not be sent to the remote server.";
                            break;
                        case "ServerProtocolViolation":
                            tErrorNo = 11;
                            tErrorMsg = "The server response was not a valid HTTP response.";
                            break;
                        case "Success":
                            tErrorNo = 0;
                            tErrorMsg = "No error was encountered.";
                            ReturnVal = true;
                            break;
                        case "Timeout":
                            tErrorNo = 14;
                            tErrorMsg = "No response was received during the time-out period for a request.";
                            break;
                        case "TrustFailure":
                            tErrorNo = 9;
                            tErrorMsg = "A server certificate could not be validated.";
                            break;
                        case "UnknownError":
                            tErrorNo = 16;
                            tErrorMsg = "An exception of unknown type has occurred.";
                            break;
                        default:
                            tErrorNo = 99;
                            tErrorMsg = "An untrapped error occured.";
                            break;
                    }
                }
                return ReturnVal;
            }
        }


        public string GetRunningVersion()
        {
            try
            {
                //return System.Deployment.Application.ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();
                return "21";
            }
            catch
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        public string EscapeChar(string inputstring)
        {
            if (inputstring != null)
            {
                string outputstring = inputstring.Replace(@"\", "");
                outputstring = outputstring.Replace("'", "''");
                outputstring = outputstring.Replace('"', '~').Replace("~", "");
                //outputstring = inputstring.Replace("%", @"\%");
                //outputstring = inputstring.Replace("_", @"\_");
                return outputstring;
            }
            return inputstring;
        }

        private string StripPhoneCharacters(string displayphone)
        {
            if (displayphone == "-")
            {
                return "-";
            }
            else
            {
                return string.Join(null, Regex.Split(displayphone, "[^\\d]"));
            }
        }

        private string DisplayFormatPhone(string rawphone)
        {
            try
            {
                if (rawphone.Length == 10)
                {
                    string phoneinput = rawphone;
                    string phoneoutput = "(" + rawphone.Substring(0, 3) + ") " + rawphone.Substring(3, 3) + "-" + rawphone.Substring(6, 4);
                    return phoneoutput;
                }
                else
                {
                    return "-";
                }
            }
            catch (MySqlException ex)
            {
                long tt = ex.Number;
                return "-";
            }
        }

        public int GetAge(DateTime DOB)
        {
            int customerage = DateTime.Now.Year - DOB.Year;
            if (DOB > DateTime.Now.AddYears(-customerage)) customerage--;
            return customerage;
        }

        public int CalcAge(DateTime aDOB, DateTime? aCalcDate = null)
        {
            DateTime tCalcDate = aCalcDate == null ? DateTime.Now : aCalcDate.Value;
            int rtnValue = tCalcDate.Year - aDOB.Year;
            if (aDOB.AddYears(rtnValue) > tCalcDate)
            {
                rtnValue--;
            }
            return rtnValue;
        }

        public long GetJulianDate(DateTime aDate)
        {
            int m = aDate.Month;
            int d = aDate.Day;
            int y = aDate.Year;

            if (m < 3)
            {
                m = m + 12;
                y = y - 1;
            }
            return d + (153 * m - 457) / 5 + 365 * y + (y / 4) - (y / 100) + (y / 400) + 1721119;
        }

        public string SetFoxProDate(DateTime aDate)
        {
            return "{^" + aDate.ToString(Mirror.AxessDateFormat) + "}";
        }

        public string AddToList(string myList, string newValue)
        {
            string tString = "," + myList + ",";
            if (!tString.Contains("," + newValue + ","))
            {
                if (myList != string.Empty) myList += ",";
                myList += newValue;
            }
            return myList;
        }

        public bool IsServerResponding(string aURL)
        {
            PingReply pingReply;
            bool tResponds = false;
            using (var ping = new Ping())
                try
                {
                    pingReply = ping.Send(aURL);
                    tResponds = (pingReply.Status == IPStatus.Success);
                }
                catch {}
            return tResponds;

        }

        public void Table2CSV(DataTable aDataTable, string aFilePath, bool aHeader = true, bool aAppend = false)
        {
            if (!aAppend)                                                                   //if not appending
                if (File.Exists(aFilePath))                                                 //and the file exists
                    File.Delete(aFilePath);                                                 //delete the file

            StringBuilder fileContent = new StringBuilder();
            if (aHeader)                                                                    //if header included in CSV
            {
                foreach (var tCol in aDataTable.Columns)                                    //go through the columns
                {
                    fileContent.Append(tCol.ToString().Replace(",", " ") + ",");            //append each column name to the string with a following comma
                }

                fileContent.Replace(",", Environment.NewLine, fileContent.Length - 1, 1);   //replace the last comma with a newline character
            }

            foreach (DataRow dr in aDataTable.Rows)                                         //go through all rows in table
            {
                foreach (var column in dr.ItemArray)                                        //go through the columns in the row
                {
                    fileContent.Append("\"" + column.ToString().Replace(",", " ") + "\",");  //append the column data to the string with a following comma
                }
                fileContent.Replace(",", Environment.NewLine, fileContent.Length - 1, 1);   //replace the last comma with a newline character
            }
            File.WriteAllText(aFilePath, fileContent.ToString().Replace('"', '~').Replace("~", ""));                           //now write the file to disk
        }

        public DataTable CSV2Table(string aFileName, string aTableName = "new_table")
        {
            DataTable tDT = new DataTable();
            OleDbConnection tCon = new OleDbConnection("Provider=Microsoft.Jet.OleDb.4.0; Data Source = " + Path.GetDirectoryName(aFileName) + "; Extended Properties = \"Text;HDR=YES;FMT=Delimited\"");
            try
            {
                tCon.Open();
                OleDbDataAdapter adapter = new OleDbDataAdapter("SELECT * FROM " + Path.GetFileName(aFileName), tCon);
                adapter.Fill(tDT, aTableName);
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.Print(ex.Message);
            }
            if (tCon.State != ConnectionState.Closed)
                tCon.Close();
            tCon.Dispose();
            return tDT;
        }

        public DataSet CSV2DS(string aFileName, string aTableName, DataSet aDataSet, bool aAppend = false, bool aHeader = true)
        {
            if (!aAppend)
                if (aDataSet.Tables.Contains(aTableName))
                    aDataSet.Tables.Remove(aTableName);
            if (File.Exists(aFileName))
            {
                OleDbConnection tCon = new OleDbConnection("Provider=Microsoft.Jet.OleDb.4.0; Data Source = " + Path.GetDirectoryName(aFileName) + "; Extended Properties = \"Text;HDR=" + (aHeader ? "YES" : "NO") + ";FMT=Delimited\"");
                try
                {
                    tCon.Open();
                    OleDbDataAdapter adapter = new OleDbDataAdapter("SELECT * FROM " + Path.GetFileName(aFileName), tCon);
                    adapter.Fill(aDataSet, aTableName);
                }
                catch (MySqlException ex)
                {
                    System.Diagnostics.Debug.Print(ex.Message);
                }
                if (tCon.State != ConnectionState.Closed)
                    tCon.Close();
                tCon.Dispose();
            }
            return aDataSet;
        }

        public bool String2CSV(string aText, string aFilename, bool aAppend = false, bool aNewLine = true)
        {
            try
            {
                if (!aAppend && File.Exists(aFilename)) File.Delete(aFilename);
                if (aNewLine) aText = aText + Environment.NewLine;
                File.AppendAllText(aFilename, aText);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public bool GetSQLBool(SqlConnection aConnection, string aQuery, bool aYN = true, bool aTF = true) 
        {
            bool ReturnValue = false;
            string StringValue = GetSQLField(aConnection, aQuery);
            switch (StringValue.ToUpper())
            {
                case "0":
                    ReturnValue = false;
                    break;
                case "1":
                    ReturnValue = true;
                    break;
                case "Y":
                    if (aYN)
                    {
                        ReturnValue = true;
                    }
                    break;
                case "N":
                    if (aYN)
                    {
                        ReturnValue = false;
                    }
                    break;
                case "T":
                    if (aTF)
                    {
                        ReturnValue = true;
                    }
                    break;
                case "F":
                    if (aTF)
                    {
                        ReturnValue = false;
                    }
                    break;
            }
            return ReturnValue;
        }

        public string GetSQLField(SqlConnection aConnection, string aQuery)
        {
            string Result = string.Empty;
            ConnectionState myState = aConnection.State;
            try
            {
                if (myState == ConnectionState.Closed) OpenConn(aConnection);
				SqlCommand myCommand = new SqlCommand(aQuery, aConnection)
				{
					CommandType = CommandType.Text
				};
				var Result1 = myCommand.ExecuteScalar().ToString();
                Result = (Result1 is null ? string.Empty : Result1.ToString());
            }
            catch (Exception ex)
            {
                if (ex.HResult != -2147467261)  //no matching record so return an empty string with no error log.
                {
                    System.Diagnostics.Debug.Print(ex.Message);
                }
            }
            if (myState == ConnectionState.Closed)
                CloseConn(aConnection);
            aConnection.Dispose();
            return Result;
        }

        public string GetSQLField(OleDbConnection aConnection, string aQuery)
        {
            string Result = string.Empty;
            ConnectionState myState = aConnection.State;
            try
            {
                if (myState == ConnectionState.Closed)
                    OpenConn(aConnection);
                OleDbCommand myCommand = new OleDbCommand(aQuery, aConnection)
                {
                    CommandType = CommandType.Text
                };
                var Result1 = myCommand.ExecuteScalar().ToString();
                Result = (Result1 is null ? string.Empty : Result1.ToString());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Print(ex.Message);
            }

            if (myState == ConnectionState.Closed)
                CloseConn(aConnection);
            aConnection.Dispose();
            return Result;
        }

        public string GetSQLField(MySqlConnection aConnection, string aQuery)
        {
            string Result = string.Empty;
            ConnectionState myState = aConnection.State;
            try
            {
                if (myState == ConnectionState.Closed)
                    OpenConn(aConnection);
				MySqlCommand myCommand = new MySqlCommand(aQuery, aConnection)
				{
					CommandType = CommandType.Text
				};
                var Result1 = myCommand.ExecuteScalar().ToString();
                Result = (Result1 is null ? string.Empty : Result1.ToString());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Print(ex.Message);
            }
            if (myState == ConnectionState.Closed)
                CloseConn(aConnection);
            aConnection.Dispose();
            return Result;
        }

        public bool RowExists(OleDbConnection aConnection, string aTable, string aFilter = "")
        {
            bool Result = false;
            ConnectionState myState = aConnection.State;
            try
            {
                string Query = $"SELECT 1 FROM {aTable}";
                if (aFilter != string.Empty) Query += $" WHERE {aFilter}";
                using (OleDbCommand myCommand = new OleDbCommand($"SELECT EXISTS ({Query} LIMIT 1)", aConnection))
                {
                    myCommand.CommandType = CommandType.Text;
                    if (myState == ConnectionState.Closed)
                        OpenConn(aConnection);
                    Result = Convert.ToBoolean(myCommand.ExecuteScalar());
                }
            }
            catch (OleDbException ex)
            {
                System.Diagnostics.Debug.Print(ex.Message);
                tError.UpdateErrorLog("CommonFunctions.RowExists", 9996, ex.Message, ex.Data.ToString());
            }
            if (myState == ConnectionState.Closed)
                CloseConn(aConnection);
            aConnection.Dispose();
            return Result;
        }

        public bool RowExists(SqlConnection aConnection, string aTable, string aFilter = "")
        {
            bool Result = false;
            ConnectionState myState = aConnection.State;
            try
            {
                string Query = $"SELECT 1 FROM {aTable}";
                if (aFilter != string.Empty) Query += $" WHERE {aFilter}";
                using (SqlCommand myCommand = new SqlCommand($"SELECT EXISTS ({Query} LIMIT 1)", aConnection))
                {
                    myCommand.CommandType = CommandType.Text;
                    if (myState == ConnectionState.Closed)
                        OpenConn(aConnection);
                    Result = Convert.ToBoolean(myCommand.ExecuteScalar());
                }
            }
            catch (OleDbException ex)
            {
                System.Diagnostics.Debug.Print(ex.Message);
                tError.UpdateErrorLog("CommonFunctions.RowExists", 9997, ex.Message, ex.Data.ToString());
            }
            if (myState == ConnectionState.Closed)
                CloseConn(aConnection);
            aConnection.Dispose();

            return Result;
        }

        public bool RowExists(MySqlConnection aConnection, string aTable, string aFilter = "")
        {
            bool Result = false;
            ConnectionState myState = aConnection.State;
            try
            {
                string Query = $"SELECT 1 FROM {aTable}";
                if (aFilter != string.Empty) Query += $" WHERE {aFilter}";
                using (MySqlCommand myCommand = new MySqlCommand($"SELECT EXISTS ({Query} LIMIT 1)", aConnection))
                {
                    myCommand.CommandType = CommandType.Text;
                    if (myState == ConnectionState.Closed)
                        if (OpenConn(aConnection) == ConnectionState.Open)
                        {
                            string TTT = myCommand.ExecuteScalar().ToString();
                            Result = Convert.ToBoolean(myCommand.ExecuteScalar());
                        }
                    else
                        {
                            Result = false;
                        }
                }
            }
            catch (OleDbException ex)
            {
                System.Diagnostics.Debug.Print(ex.Message);
                tError.UpdateErrorLog("CommonFunctions.RowExists", 9998, ex.Message, aTable + "--" + aFilter);
            }
            if (myState == ConnectionState.Closed)
                CloseConn(aConnection);
            aConnection.Dispose();

            return Result;
        }

        public long RowCount(OleDbConnection aConnection, string aTable, string aFilter = "")
        {
            ConnectionState myState = aConnection.State;
            string Query = "SELECT * FROM " + aTable;
            if (aFilter != string.Empty)
            {
                Query += " WHERE " + aFilter;
            }
            DataTable tDT = LoadTable(aConnection, Query, "RC");
            long Result = tDT != null ? tDT.Rows.Count : 0;
            return Result;



            //try
            //{
            //    string Query = $"SELECT count(*) FROM {aTable}";
            //    if (aFilter != string.Empty) Query += $" WHERE {aFilter}";
            //    using (OleDbCommand myCommand = new OleDbCommand(Query, aConnection))
            //    {
            //        myCommand.CommandType = CommandType.Text;
            //        if (myState == ConnectionState.Closed)
            //            OpenConn(aConnection);
            //        Result = Convert.ToInt64(myCommand.ExecuteScalar());
            //    }
            //}
            //catch (OleDbException ex)
            //{
            //    System.Diagnostics.Debug.Print(ex.Message);
            //    tError.UpdateErrorLog("CommonFunctions.RowCount", 9999, ex.Message, ex.Data.ToString());
            //}
            //if (myState == ConnectionState.Closed)
            //    CloseConn(aConnection);
            //aConnection.Dispose();

            //return Result;

        }

        public long RowCount(SqlConnection aConnection, string aTable, string aFilter = "")
        {
            //long Result = 0;
            ConnectionState myState = aConnection.State;
            string Query = "SELECT * FROM " + aTable;
            if (aFilter != string.Empty)
            {
                Query += " WHERE " + aFilter;
            }
            DataTable tDT = LoadTable(aConnection, Query, "RC");
            long Result = tDT != null ? tDT.Rows.Count : 0;
            return Result;

            //try
            //{
            //            if (aFilter != string.Empty) Query += " WHERE " + aFilter;
            //SqlCommand myCommand = new SqlCommand(Query, aConnection)
            //{
            //	CommandType = CommandType.Text
            //};
            //if (myState == ConnectionState.Closed)
            //                OpenConn(aConnection);
            //            Result = Convert.ToInt64(myCommand.ExecuteScalar());
            //        }
            //        catch (MySqlException ex)
            //        {
            //            System.Diagnostics.Debug.Print(ex.Message);
            //            tError.UpdateErrorLog("CommonFunctions.RowCount", ex.Number, ex.Message, ex.Data.ToString());
            //        }
            //        if (myState == ConnectionState.Closed)
            //            CloseConn(aConnection);
            //        aConnection.Dispose();

            //        return Result;
        }

        public long RowCount(MySqlConnection aConnection, string aTable, string aFilter = "")
        {
            ConnectionState myState = aConnection.State;
            string Query = "SELECT * FROM " + aTable;
            if (aFilter != string.Empty)
            {
                Query += " WHERE " + aFilter;
            }
            DataTable tDT = LoadTable(aConnection, Query, "RC");
            long Result = tDT != null ? tDT.Rows.Count : 0;
            return Result;
            //         MySqlCommand myCommand = new MySqlCommand(Query, aConnection)
            //{
            //	CommandType = CommandType.Text,
            //	CommandTimeout = 60
            //};
            //try
            //         {
            //             if (myState == ConnectionState.Closed)
            //                 OpenConn(aConnection);
            //             Result = Convert.ToInt64(myCommand.ExecuteScalar());
            //         }
            //         catch (MySqlException ex)
            //         {
            //             System.Diagnostics.Debug.Print(ex.Message);
            //             tError.UpdateErrorLog("CommonFunctions.RowCount", ex.Number, ex.Message, ex.Data.ToString());
            //         }
            //         if (myState == ConnectionState.Closed)
            //             CloseConn(aConnection);
            //         aConnection.Dispose();
            //         return Result;
                 }

            public bool ExecuteSQL(OleDbConnection aConnection, string aQuery, int cmdTimeout = 30)
            {
            OleDbCommand dbCmdNull = new OleDbCommand("SET NULL OFF",aConnection);
            OleDbCommand myCommand = new OleDbCommand(aQuery + ";", aConnection);
                OleDbDataReader myReader;
                //Application.DoEvents();
                myCommand.CommandTimeout = cmdTimeout;
                bool rtnValue = false;
                try
                {
                    if (OpenConn(aConnection) == ConnectionState.Open)
                    {
                        dbCmdNull.ExecuteNonQuery();
                        myReader = myCommand.ExecuteReader();
                        while (myReader.Read()) { }
                        myReader.Close();
                        rtnValue = true;
                    }
                }
                catch (OleDbException ex)
                {
                    tError.UpdateErrorLog("CommonFunctions.ExecuteSQL(" + aConnection.DataSource + ")", 9999, ex.Message, aQuery.Replace("'", "''"));
                }
            if (aConnection.State != ConnectionState.Closed)
            {
                CloseConn(aConnection);
            }
            aConnection.Dispose();
            return rtnValue;
            }



        public bool ExecuteSQL(SqlConnection aConnection, string aQuery, int cmdTimeout = 30)
		{
            SqlCommand myCommand = new SqlCommand(aQuery + ";", aConnection);
            SqlDataReader myReader;
            //Application.DoEvents();
            myCommand.CommandTimeout = cmdTimeout;
            bool rtnValue = false;
            try
            {
                if (OpenConn(aConnection) == ConnectionState.Open)
                {

                    myReader = myCommand.ExecuteReader();
                    while (myReader.Read()) { }
                    myReader.Close();
                    rtnValue = true;
                }
            }
            catch (SqlException ex)
            {
                tError.UpdateErrorLog("CommonFunctions.ExecuteSQL(" + aConnection.DataSource + ")", ex.Number, ex.Message, aQuery.Replace("'", "''"));
            }
            if (aConnection.State != ConnectionState.Closed)
            {
                CloseConn(aConnection);
            }
            aConnection.Dispose();
            return rtnValue;
        }


        public bool ExecuteSQL(MySqlConnection aConnection, string aQuery, int cmdTimeout = 30)
		{
            MySqlCommand myCommand = new MySqlCommand(aQuery + ";", aConnection);
            MySqlDataReader myReader;
            //Application.DoEvents();
            myCommand.CommandTimeout = cmdTimeout;
            bool rtnValue = false;
            try
            {
                if (OpenConn(aConnection) == ConnectionState.Open)
                {
                    myReader = myCommand.ExecuteReader();
                    while (myReader.Read()) { }
                    myReader.Close();
                    rtnValue = true;
                }
            }
            catch (MySqlException ex)
            {
                tError.UpdateErrorLog("CommonFunctions.ExecuteSQL(" + aConnection.DataSource + ")", ex.Number, ex.Message, aQuery.Replace("'", "''"));
            }
            if (aConnection.State != ConnectionState.Closed)
            {
                CloseConn(aConnection);
            }
            aConnection.Dispose();
            return rtnValue;
        }

        public DataSet LoadDataSet(OleDbConnection aConnection, string aQuery, DataSet aDS, string aTableName, bool aAppend = false)
        {
            if (!aAppend) if (aDS.Tables.Contains(aTableName))
                    aDS.Tables.Remove(aTableName);
            OleDbCommand tCmd = new OleDbCommand(aQuery, aConnection)
            {
                CommandType = CommandType.Text
            };
            ODA.SelectCommand = tCmd;
            try
            {
                if (OpenConn(aConnection) == ConnectionState.Open)
                    ODA.Fill(aDS, aTableName);
            }
            catch (OleDbException ex)
            {
                System.Diagnostics.Debug.Print(ex.Message);
            }
            if (aConnection.State != ConnectionState.Closed)
                CloseConn(aConnection);
            aConnection.Dispose();
            return aDS;
        }

        public DataSet LoadDataSet(SqlConnection aConnection, string aQuery, DataSet aDS, string aTableName, bool aAppend = false)
        {
            if (!aAppend) if (aDS.Tables.Contains(aTableName))
                    aDS.Tables.Remove(aTableName);
			SqlCommand tCmd = new SqlCommand(aQuery, aConnection)
			{
				CommandType = CommandType.Text
			};
			SDA.SelectCommand = tCmd;
            try
            {
                if (OpenConn(aConnection) == ConnectionState.Open)
                    SDA.Fill(aDS, aTableName);
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.Print(ex.Message);
            }
            if (aConnection.State != ConnectionState.Closed)
                CloseConn(aConnection);
            aConnection.Dispose();
            return aDS;
        }

        public DataSet LoadDataSet(MySqlConnection aConnection, string aQuery, DataSet aDS, string aTableName, bool aAppend = false)
        {
            if (!aAppend) if (aDS.Tables.Contains(aTableName))
                    aDS.Tables.Remove(aTableName);
			MySqlCommand tCmd = new MySqlCommand(aQuery, aConnection)
			{
				CommandType = CommandType.Text,
				CommandTimeout = 60
			};
			DA.SelectCommand = tCmd;
			try
			{
                if (OpenConn(aConnection) == ConnectionState.Open)
                    DA.Fill(aDS, aTableName);
            }
            catch (MySqlException ex)
            {

                System.Diagnostics.Debug.Print(ex.Message);
            }
            if (aConnection.State != ConnectionState.Closed)
                CloseConn(aConnection);
            aConnection.Dispose();
            return aDS;
        }

        public DataTable LoadTable(OleDbConnection aConnection, string aQuery, string aTableName, bool Append = false)
        {

            DataSet tDS = LoadDataSet(aConnection, aQuery, new DataSet(), aTableName, Append);
            return tDS.Tables[aTableName];
        }

        public DataTable LoadTable(SqlConnection aConnection, string aQuery, string aTableName, bool Append = false)
		{

            DataSet tDS = LoadDataSet(aConnection, aQuery, new DataSet(), aTableName, Append);
            return tDS.Tables[aTableName];
        }

        public DataTable LoadTable(MySqlConnection aConnection, string aQuery, string aTableName, bool Append = false)
		{

            DataSet tDS = LoadDataSet(aConnection, aQuery, new DataSet(), aTableName, Append);
            return tDS.Tables[aTableName];
        }

        public bool TableHasData(DataTable aTable, Int32 aFieldNo = 0)
        {
            bool RtnVal = false;
            if (aTable != null)
            {
                if (aTable.Rows.Count > 0)
                {
                    RtnVal = (aTable.Columns[aFieldNo].ToString() != string.Empty);
                }
            }
            return RtnVal;
        }

        public bool RowHasData(DataRow aRow, Int32 aFieldNo = 0)
        {
            bool RtnVal = false;
            if (aRow != null)
            {
                RtnVal = (aRow[aFieldNo].ToString() != string.Empty);
            }
            return RtnVal;
        }


        //returns a single row.  If none found returns blank row, if more than one found returns first row.

        public DataRow LoadDataRow(OleDbConnection aConnection, string aQuery)
        {
            DataTable tDT = LoadTable(aConnection, aQuery, "temp");
            if (tDT == null) return null;
            if (tDT.Rows.Count == 0) return null;
            return tDT.Rows[0];
        }

        public DataRow LoadDataRow(SqlConnection aConnection, string aQuery)
		{
            DataTable tDT = LoadTable(aConnection, aQuery, "temp");
            if (tDT == null) return null;
            if (tDT.Rows.Count == 0) tDT.NewRow();
            return tDT.Rows[0];
        }

        public DataRow LoadDataRow(MySqlConnection aConnection, string aQuery)
		{
            DataTable tDT = LoadTable(aConnection, aQuery, "temp");
            if (tDT == null) return null;
            if (tDT.Rows.Count == 0) return tDT.NewRow();
            return tDT.Rows[0];
        }

        public ConnectionState OpenConn(OleDbConnection aConnection)
        {
            tErrorNo = 0;
            tErrorMsg = string.Empty;
            try
            {
                aConnection.Open();
            }
            catch (SqlException ex)
            {
                tErrorMsg = ex.Message.ToString().Trim();
                tErrorNo = ex.Number;
                tError.UpdateErrorLog("CommonFunctions.OpenConn", tErrorNo, tErrorMsg, aConnection.DataSource);
                return ConnectionState.Broken;
            }
            return aConnection.State;
        }

    public ConnectionState OpenConn(SqlConnection aConnection)
        {
            if (aConnection.State == ConnectionState.Closed)
            {
                tErrorNo = 0;
                tErrorMsg = string.Empty;
                try
                {
                    aConnection.Open();
                }
                catch (SqlException ex)
                {
                    tErrorMsg = ex.Message.ToString().Trim();
                    tErrorNo = ex.Number;
                    tError.UpdateErrorLog("CommonFunctions.OpenConn", tErrorNo, tErrorMsg, aConnection.DataSource);
                    return ConnectionState.Broken;
                }
            }
            return aConnection.State;
        }

        public ConnectionState OpenConn(MySqlConnection aConnection)
        {
            if (aConnection.State == ConnectionState.Closed)
            {
                tErrorNo = 0;
                tErrorMsg = string.Empty;
                try
                {
                    aConnection.Open();
                }
                catch (MySqlException ex)
                {
                    tErrorMsg = ex.Message.ToString().Trim();
                    tErrorNo = ex.Number;
                    //SetControl(aConnection.ConnectionString.se)
                    tError.UpdateErrorLog("CommonFunctions.OpenConn", tErrorNo, tErrorMsg, aConnection.DataSource);
                    return ConnectionState.Broken;
                }
            }
            return aConnection.State;
        }

        public bool CloseConn(OleDbConnection aConnection, long aWait = 10)
        {
            if (aConnection.State != ConnectionState.Open && aConnection.State != ConnectionState.Broken)
            {
                for (int i = 1; i <= aWait; i++)
                {
                    System.Threading.Thread.Sleep(1000);
                    if (aConnection.State != ConnectionState.Open && aConnection.State != ConnectionState.Broken) break;
                }
            }
            if (aConnection.State == ConnectionState.Open || aConnection.State == ConnectionState.Broken)
            {
                try
                {
                    aConnection.Close();
                }
                catch (SqlException ex)
                {
                    tErrorMsg = ex.Message.ToString().Trim();
                    tErrorNo = ex.Number;
                    tError.UpdateErrorLog("CommonFunctions.CloseConn", ex.Number, ex.Message, aConnection.DataSource);
                    return false;
                }
            }
            return (aConnection.State == ConnectionState.Closed);
        }

        public bool CloseConn(SqlConnection aConnection, long aWait = 10)
        {
            if (aConnection.State != ConnectionState.Open && aConnection.State != ConnectionState.Broken)
            {
                for (int i = 1; i <= aWait; i++)
                {
                    System.Threading.Thread.Sleep(1000);
                    if (aConnection.State != ConnectionState.Open && aConnection.State != ConnectionState.Broken) break;
                }
            }
            if (aConnection.State == ConnectionState.Open || aConnection.State == ConnectionState.Broken)
            {
                try
                {
                    aConnection.Close();
                }
                catch (SqlException ex)
                {
                    tErrorMsg = ex.Message.ToString().Trim();
                    tErrorNo = ex.Number;
                    tError.UpdateErrorLog("CommonFunctions.CloseConn", ex.Number, ex.Message, aConnection.DataSource);
                    return false;
                }
            }
            return (aConnection.State == ConnectionState.Closed); 
        }

        public bool CloseConn(MySqlConnection aConnection, long aWait = 10)
        {
            if (aConnection.State != ConnectionState.Open && aConnection.State != ConnectionState.Broken)
            {
                for (int i = 1; i <= aWait; i++)
                {
                    System.Threading.Thread.Sleep(1000);
                    if (aConnection.State != ConnectionState.Open && aConnection.State != ConnectionState.Broken) break;
                }
            }
            if (aConnection.State == ConnectionState.Open || aConnection.State == ConnectionState.Broken)
            {
                try
                {
                    aConnection.Close();
                }
                catch (MySqlException ex)
                {
                    tErrorMsg = ex.Message.ToString().Trim();
                    tErrorNo = ex.Number;
                    tError.UpdateErrorLog("CommonFunctions.CloseConn", ex.Number, ex.Message, aConnection.DataSource);
                }
            }
            bool RtnVal = (aConnection.State == ConnectionState.Closed);
            aConnection.ClearAllPoolsAsync();
            return RtnVal;
        }

        public XmlDocument ExecuteWeb(string aURL, XmlDocument aRequestDoc)
		{
            //Application.DoEvents();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(aURL);
            request.ContentType = "text/xml; charset=UTF-8; action =\"SOAP:Action\"";
            request.Method = "POST";
            using (Stream stream = request.GetRequestStream())
            {
                aRequestDoc.Save(stream);
            }

            using (WebResponse response = request.GetResponse())
            {
                using (StreamReader rd = new StreamReader(response.GetResponseStream()))
                {
                    string soapResult = rd.ReadToEnd();
                    XmlDocument localXML = new XmlDocument();
                    localXML.LoadXml(soapResult);
                    if (localXML.GetElementsByTagName("NERRORNO").Item(0).InnerText.Length != 0)
                    {
                        tErrorNo = Convert.ToInt32(localXML.GetElementsByTagName("NERRORNO").Item(0).InnerText);
                        tErrorMsg = localXML.GetElementsByTagName("SZERRORMESSAGE").Item(0).InnerText;
                    }
                    else
                    {
                        tErrorNo = -666;
                        tErrorMsg = "Axess API returned an invalid response.";
                    }
                    return localXML;
                }
            }
        }

        public string FirstChar(string aValue)
        {
            aValue = aValue.Trim();
            if (aValue == string.Empty)
            {
                return "";
            }
            return aValue.Substring(0, 1);
        }

        public string Quote(string aString, bool doubleQuote = false)
        {
            if (!doubleQuote) return "'" + aString + "'";
            return "\"" + aString + "\"";
        }

        public decimal Null2Val(decimal? aDecimalValue) => (aDecimalValue ?? 0);
        public Int16 Null2Val(Int16? aIntVal) => (aIntVal ?? 0);
        public Int32 Null2Val(Int32? aIntVal) => (aIntVal ?? 0);
        public Int64 Null2Val(Int64? aIntVal) => (aIntVal ?? 0);

        public string TimestampConvert(double timeStamp, bool dateOnly = false)
        {
            if (dateOnly) timeStamp = Math.Floor(timeStamp);
            DateTime lDate = Convert.ToDateTime("12/31/1899");
            lDate = lDate.AddDays(timeStamp - 1);
            return lDate.ToString(Mirror.AxessDateTimeFormat);
        }

    }


    // ****  Alta Ski Lifts Email ****

    public class ASLEmail
    {
        private ASLError tError = new ASLError();
        private CommonFunctions cf = new CommonFunctions();
        private DataWarehouse DW = new DataWarehouse();
        private MailMessage tMailMessage = new MailMessage();

        private string tFromName = string.Empty;
        private string tFromAddress = Properties.Settings.Default.EmailFromAddress;
        private string tSMTPHost = Properties.Settings.Default.SMTPHost;
        private string tSMTPObject = Properties.Settings.Default.SMTPObject;
        private string tSMTPUnlockCode = Properties.Settings.Default.SMTPUnlockCode;
        private string tSubject = string.Empty;
        private string tBody = string.Empty;
        private string tToName = string.Empty;
        private string tToAddress = string.Empty;
        private string tCCName = string.Empty;
        private string tCCAddress = string.Empty;
        private string tBCCName = string.Empty;
        private string tBCCAddress = string.Empty;
        private bool tIsHTML = false;

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

        public string FromName
        {
            get { return tFromName; }
        }

        public string FromAddress
        {
            get { return tFromAddress; }
            set { tFromAddress = value; }
        }

        public string SMTPHost
        {
            get { return tSMTPHost; }
            set { tSMTPHost = value; }
        }

        public string SMTPObject
        {
            get { return tSMTPObject; }
            set { tSMTPObject = value; }
        }

        public string SMTPUnlockCode
        {
            get { return tSMTPUnlockCode; }
            set { tSMTPUnlockCode = value; }
        }

        public string Subject
        {
            get { return tSubject; }
            set { tSubject = value; }
        }

        public string Body
        {
            get { return tBody; }
            set { tBody = value; }
        }

        public string ToAddress
        {
            get { return tToAddress; }
            set { tToAddress = value; }
        }

        public string ToName
        {
            get { return tToName; }
            set { tToName = value; }
        }

        public string CCAddress
        {
            get { return tCCAddress; }
            set { tCCAddress = value; }
        }

        public string CCName
        {
            get { return tCCName; }
            set { tCCName = value; }
        }

        public string BCCAddress
        {
            get { return tBCCAddress; }
            set { tBCCAddress = value; }
        }

        public string BCCName
        {
            get { return tBCCName; }
            set { tBCCName = value; }
        }

        public bool IsHTML
        {
            get { return tIsHTML; }
            set { tIsHTML = value; }
        }

        public void ResetAll()
        {
            tBCCAddress = string.Empty;
            tBCCName = string.Empty;
            tBody = string.Empty;
            tCCAddress = string.Empty;
            tCCName = string.Empty;
            tError = new ASLError();
            tFromAddress = string.Empty;
            tFromName = string.Empty;
            tMailMessage = new MailMessage();
            tSubject = string.Empty;
            tToAddress = string.Empty;
            tToName = string.Empty;
            tIsHTML = false;
        }

        public void CreateEmail()
        {
            string[] tAdr = tToAddress.Split(';');
            tMailMessage = new MailMessage(tFromAddress, tAdr[0].Trim(), tSubject, tBody);
            for (int i=1; i < tAdr.Length;  i++)
                {
                tMailMessage.To.Add(tAdr[i].Trim());
                }
            if (tCCAddress.Length != 0)
            {
                tAdr = tCCAddress.Split(';');
                for (int i=0; i < tAdr.Length; i++)
                {
                    tMailMessage.CC.Add(tAdr[i]);
                }
            }
            if (tBCCAddress.Length != 0)
            {
                tAdr = tBCCAddress.Split(';');
                for (int i=0; i < tAdr.Length; i++)
                {
                    tMailMessage.Bcc.Add(tAdr[i]);
                }
            }
        }

        public bool SendEmail()
        {
            SmtpClient client = new SmtpClient(tSMTPHost);
            tMailMessage.IsBodyHtml = tIsHTML;
            // Add credentials if the SMTP server requires them.
            //client.Credentials = CredentialCache.DefaultNetworkCredentials;

            try
            {
                client.Send(tMailMessage);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Print("Exception caught in aslEmail.SendEmail(): {0}", ex.Message);
                tError.UpdateErrorLog(ex.Source, -9999, ex.Message, ex.Data.ToString());
                return false;
            }
            // Display the values in the ContentDisposition for the attachment.
            //ContentDisposition cd = new ContentDisposition();
            //Console.WriteLine("Content disposition");
            //Console.WriteLine(cd.ToString());
            //Console.WriteLine("File {0}", cd.FileName);
            //Console.WriteLine("Size {0}", cd.Size);
            //Console.WriteLine("Creation {0}", cd.CreationDate);
            //Console.WriteLine("Modification {0}", cd.ModificationDate);
            //Console.WriteLine("Read {0}", cd.ReadDate);
            //Console.WriteLine("Inline {0}", cd.Inline);
            //Console.WriteLine("Parameters: {0}", cd.Parameters.Count);
            //foreach (DictionaryEntry d in cd.Parameters)
            //{
            //    Console.WriteLine("{0} = {1}", d.Key, d.Value);
            // }
            //data.Dispose();
            return true;
        }

        public string AttachJpeg(string aFileName)
        {
            Attachment tData = new Attachment(aFileName, MediaTypeNames.Image.Jpeg);
            ContentDisposition tDisposition = tData.ContentDisposition;
            tDisposition.CreationDate = System.IO.File.GetCreationTime(aFileName);
            tDisposition.ModificationDate = System.IO.File.GetLastWriteTime(aFileName);
            tDisposition.ReadDate = System.IO.File.GetLastAccessTime(aFileName);
            tDisposition.Inline = true;
            tMailMessage.Attachments.Add(tData);
            return tData.ContentId;
        }

        public bool Attach(string aFileName)
        {
            Attachment tData = new Attachment(aFileName);
            ContentDisposition tDisposition = tData.ContentDisposition;
            tDisposition.CreationDate = System.IO.File.GetCreationTime(aFileName);
            tDisposition.ModificationDate = System.IO.File.GetLastWriteTime(aFileName);
            tDisposition.ReadDate = System.IO.File.GetLastAccessTime(aFileName);
            tMailMessage.Attachments.Add(tData);
            return true;
        }

        public void SendNotification(string aNotificationName, string aExtraData = "", string aAttachment = "")
        {
            DataRow tDR = cf.LoadDataRow(DW.dwConn, $"SELECT * FROM applications.asl_notifications WHERE name='{aNotificationName}'");
            if (tDR != null)
            {

                ToAddress = tDR["to"].ToString();
                CCAddress = tDR["cc"].ToString();
                BCCAddress = tDR["bcc"].ToString();
                Subject = ("ASL Notification " + tDR["subject"].ToString());
                if (aExtraData.Contains(","))
                {
                    string[] vals = aExtraData.Split(',');
                    Body = string.Format(tDR["text"].ToString(), vals);
                }
                else
                {
                    Body = aExtraData;
                }
                FromAddress = "ASL_Notifications@alta.com";
                CreateEmail();
                if (aAttachment != string.Empty)
                    Attach(aAttachment);
                SendEmail();
            }
        }

    }

    public class ASLError
    {
        //private CommonFunctions cf = new CommonFunctions();
        //private DataWarehouse dw = new DataWarehouse();
        private MySqlConnection dwConn = new MySqlConnection(Properties.Settings.Default.MySQLDW);
        private int tErrorNo = 0;
        private string tErrorMsg = string.Empty;
        private string tErrorData = string.Empty;
        private long tErrorLogNo = 0;
        private string tErrorFunction = string.Empty;
        private string tErrorFunctionMessage = string.Empty;

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


        private const string ErrorInsert = "INSERT INTO applications.errorlog (ErrorNumber, ErrorMessage, Timestamp, CallingFunction, Info, Reported) VALUES({0},'{1}','{2}','{3}','{4}',{5})";


        public int LastErrorNo
        {
            get { return tErrorNo; }
        }

        public string LastErrorMsg
        {
            get { return tErrorMsg; }
        }

        public string ErrorData
        {
            get { return tErrorData; }
        }

        public long ErrorLogNo
        {
            get { return tErrorLogNo; }
        }

        public string ErrorFunction
        {
            get { return tErrorFunction; }
        }

        public string ErrorFunctionMessage
        {
            get { return tErrorFunctionMessage; }
        }

        public void UpdateErrorLog()
        {
            if (!(tErrorNo == 0 || tErrorFunction == string.Empty || tErrorMsg == string.Empty))
            {
                ExecuteSQL(string.Format(ErrorInsert, tErrorNo, tErrorMsg, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), tErrorFunction, tErrorData, 0));
            }
        }

        public void UpdateErrorLog(string aFunction, int aErrorNo, string aErrorMsg, string aErrorData = "")
        {
            tErrorNo = aErrorNo;
            tErrorMsg = aErrorMsg.Replace("'", "");
            tErrorFunction = aFunction;
            tErrorData = aErrorData;
            ExecuteSQL(string.Format(ErrorInsert, tErrorNo, tErrorMsg, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), tErrorFunction, tErrorData, 0));
        }

        public bool ErrorHandler(string aFunction, long aErrorNumber, string aErrorMessage, string aInfo = "")
        {
            if (aErrorNumber > 0)
            {
                //Microsoft errors
            }
            if ((aErrorNumber < 0) && (aErrorNumber < -10000))   //Third Party, currently used by Axess
            {
                switch (aFunction.Substring(0, aFunction.IndexOf(".")).ToUpper())
                {
                    case "AXESS":   //DCI4CRM
                        break;
                    case "WTPSI":
                        break;
                    case "BUY_ALTA_COM":
                        break;
                    case "MIRROR":
                        break;

                }
            }
            if ((aErrorNumber >= -20000) && (aErrorNumber >= -29999)) //Alta generated errors
            {
                //get values from applications.altaerrorcodes
                //log if needed
                //email or text notificai
            }
            return false;
        }

        private bool ExecuteSQL(string aQuery)
        {
            MySqlCommand tCmd = new MySqlCommand(aQuery, dwConn);
            MySqlDataReader tReader;
            //Application.DoEvents();
            bool rtnValue = false;
            try
            {
                dwConn.Open();
                if (dwConn.State == ConnectionState.Open)
                {
                    tReader = tCmd.ExecuteReader();
                    while (tReader.Read())
                    {
                    }
                    tReader.Close();
                    rtnValue = true;
                }
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.Print(ex.Number.ToString() + " -- " + ex.Message);
                File.AppendAllText(Properties.Settings.Default.TempErrLog, aQuery);
                File.AppendAllText(Properties.Settings.Default.TempErrLog, string.Format(ErrorInsert, ex.Number, ex.Message, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "aslError.ExecuteSQL", ex.Data.ToString(), 0));
            }
            if (dwConn.State != ConnectionState.Closed)
                dwConn.Close();
            dwConn.Dispose();
            return rtnValue;
        }

    }

//    class aslDataGridPrinter
//    {
//        public DataGridView dataGrid1;
//        public PrintDocument printDocument1;
//        public DataSet dataSet11;


//        //Once the DataGridPrinter is constructed, you can have it draw the DataGrid to the printer by calling its DrawDataGrid method in the Print Page event handler:
//        private void printDocument1_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
//        {
//            Graphics g = e.Graphics;
//            // Draw a label title for the grid
//            DrawTopLabel(g);
//            // draw the datagrid using the DrawDataGrid method passing the Graphics surface
// //           bool more = DrawDataGrid(g);
//            // if there are more pages, set the flag to cause the form to trigger another print page event
////            if (more == true)
//{
//            e.HasMorePages = true;
////            dataGridPrinter1.PageNumber++;
//        }
//        }

//        //The PrintPage event is triggered by both the Print method in the PrintDocument and the PrintPreviewDialog's ShowDialog method.  Below is the form's method for printing the DataGrid to the printer:

//        private void PrintMenu_Click(object sender, System.EventArgs e)
//        {
//            // Initialize the datagrid page and row properties
////            dataGridPrinter1.PageNumber = 1;
////            dataGridPrinter1.RowCount = 0;
//            // Show the Print Dialog to set properties and print the document after ok is pressed.
////            if (printDialog1.ShowDialog() == DialogResult.OK)
//            {
//                printDocument1.Print();
//            }
//        }

//        //Now let's take a look at the internals of the DataGridPrinter methods. There are two main methods in the DataGridPrinter class that do all the drawing: DrawHeader and DrawRows. Both these methods extract information from the DataGrid and the DataTable to draw the DataGrid. Below is the method for drawing the rows of the DataGrid:

//        public bool DrawRows(Graphics g)
//        {
//            try
//            {
//                int lastRowBottom = TopMargin;
//                // Create an array to save the horizontal positions for drawing horizontal gridlines
//                ArrayList Lines = new ArrayList();
//                // form brushes based on the color properties of the DataGrid
//                // These brushes will be used to draw the grid borders and cells
//                SolidBrush ForeBrush = new SolidBrush(dataGrid1.ForeColor);
//                SolidBrush BackBrush = new SolidBrush(dataGrid1.BackColor);
//                SolidBrush AlternatingBackBrush = new SolidBrush(TheDataGrid.AlternatingBackColor);
//                Pen TheLinePen = new Pen(dataGrid1.GridLineColor, 1);
//                // Create a format for the cell so that the string in the cell is cut off at the end of the column width
//                StringFormat cellformat = new StringFormat();
//                cellformat.Trimming = StringTrimming.EllipsisCharacter;
//                cellformat.FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.LineLimit;
//                // calculate the column width based on the width of the printed page and the # of
//                columns in the DataTable
//            // Note: Column Widths can be made variable in a future program by playing with the GridColumnStyles of the
//            // DataGrid
//                int columnwidth = PageWidth / TheTable.Columns.Count;
//                // set the initial row count, this will start at 0 for the first page, and be a different value for the 2nd, 3rd, 4th, etc.
//                // pages.
//                int initialRowCount = RowCount;
//                RectangleF RowBounds = new RectangleF(0, 0, 0, 0);
//                // draw the rows of the table
//                for (int i = initialRowCount; i < TheTable.Rows.Count; i++)
//                {
//                    // get the next DataRow in the DataTable
//                    DataRow dr = TheTable.Rows[i];
//                    int startxposition = TheDataGrid.Location.X;
//                    // Calculate the row boundary based on teh RowCount and offsets into the page
//                    RowBounds.X = TheDataGrid.Location.X; RowBounds.Y = TheDataGrid.Location.Y +
//                     TopMargin + ((RowCount - initialRowCount) + 1) * (TheDataGrid.Font.SizeInPoints +
//                     kVerticalCellLeeway);
//                    RowBounds.Height = TheDataGrid.Font.SizeInPoints + kVerticalCellLeeway;
//                    RowBounds.Width = PageWidth;
//                    // save the vertical row positions for drawing grid lines
//                    Lines.Add(RowBounds.Bottom);
//                    // paint rows differently for alternate row colors
//                    if (i % 2 == 0)
//                    {
//                        g.FillRectangle(BackBrush, RowBounds);
//                    }
//                    else
//                    {
//                        g.FillRectangle(AlternatingBackBrush, RowBounds);
//                    }
//                    // Go through each column in the row and draw the information from the
//                    DataRowfor(int j = 0; j < TheTable.Columns.Count; j++)
//                    {       
//                    RectangleF cellbounds = new RectangleF(startxposition, TheDataGrid.Location.Y + TopMargin + ((RowCount - initialRowCount) + 1) * (TheDataGrid.Font.SizeInPoints + kVerticalCellLeeway), columnwidth, TheDataGrid.Font.SizeInPoints + kVerticalCellLeeway);
//// draw the data at the next position in the row
//                    if (startxposition + columnwidth <= PageWidth)
//                    {
//                        g.DrawString(dr[j].ToString(), TheDataGrid.Font, ForeBrush, cellbounds, cellformat);
//                        lastRowBottom = (int)cellbounds.Bottom;
//                    }
//                    // increment the column position
//                    startxposition = startxposition + columnwidth;
//                }
//                RowCount++;
//                // when we've reached the bottom of the page, draw the horizontal and vertical
//                grid lines and return true
//                if (RowCount * (TheDataGrid.Font.SizeInPoints + kVerticalCellLeeway) > PageHeight * PageNumber) - (BottomMargin + TopMargin))
//                {
//                    DrawHorizontalLines(g, Lines); DrawVerticalGridLines(g, TheLinePen, columnwidth, lastRowBottom);
//                    return true;
//                }
//            }
//// when we've reached the end of the table, draw the horizontal and vertical grid lines and return false
//            DrawHorizontalLines(g, Lines);
//            DrawVerticalGridLines(g, TheLinePen, columnwidth, lastRowBottom);
//            return false;
//        }
//        catch (Exception ex)
//        {
//            MessageBox.Show(ex.Message.ToString());
//            return false;
//        }
//    }
//    }


}
