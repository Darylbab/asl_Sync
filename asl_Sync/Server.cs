using System;
using System.Net.NetworkInformation;
using System.Data;

namespace asl_SyncLibrary
{

    public enum Serverstatus
    {Unknown, Active, Server_Failed, Service_Failed};

    public enum Servertype
    {Unknown, Windows, Linux}

    public enum Servicetype
    {Unknown, MySQL, MSSQL, ORACLE, WEBSITE, WEBSERVICE, FTP}

    public class Server
    {

        //Asgard ASG = new Asgard();
        CommonFunctions cf = new CommonFunctions();
        DataWarehouse dw = new DataWarehouse();

        private string tName = string.Empty;
        public string Name
        {
            get { return tName; }
            set { tName = value; }
        }

        private Servertype tServerType = Servertype.Unknown;
        public Servertype ServerType
        {
            get { return tServerType; }
            set { tServerType = value; }
        }

        private string tDescription = string.Empty;
        public string Description
        {
            get { return tDescription; }
            set { tDescription = value; }
        }

        private string tServiceName = string.Empty;
        public string ServiceName
        {
            get { return tServiceName; }
            set { tServiceName = value; }
        }

        private Servicetype tServiceType = Servicetype.Unknown;
        public Servicetype ServiceType
        {
            get { return tServiceType; }
            set { tServiceType = value; }
        }

        private string tURL = string.Empty;
        public string URL
        {
            get { return tURL; }
            set { tURL = value; }
        }

        private Serverstatus tStatus = Serverstatus.Unknown;
        public Serverstatus Status
        {
            get { return tStatus; }
            set { tStatus = value; }
        }

        private DateTime? tLastChecked = null;
        public DateTime? LastChecked
        {
            get { return tLastChecked; }
            set { tLastChecked = value; }
        }

        private DateTime? tLastChanged = null;
        public DateTime? LastChanged
        {
            get { return tLastChanged; }
            set { tLastChanged = value; }
        }

        private Int32 tLogHistoryDays = 90;

        public Int32 LogHistoryDays
        {
            get { return tLogHistoryDays; }
            set { tLogHistoryDays = value; }
        }

        private string tSessionID = string.Empty;
        public string SessionID
        {
            get { return tSessionID; }
            set { tSessionID = value; }
        }

        private DateTime? tLastLogin = null;
        public DateTime? LastLogin
        {
            get { return tLastLogin; }
            set { tLastLogin = value; }
        }

        private string tServerResponse = string.Empty;
        public string ServerResponse
        {
            get { return tServerResponse; }
            set { tServerResponse = value; }
        }

        private bool tActive = false;
        public bool Active
        {
            get { return tActive; }
            set { tActive = value; }
        }

        private DataRow tServerRow = null;
        public DataRow ServerData
        {
            get
            {
                if (tServerRow == null)
                {
                    tServerRow = cf.LoadDataRow(Asgard.AsgardCon, $"SELECT * FROM sync.server WHERE name='{tName}'");
                }
                return tServerRow;
            }
        }

        private bool tStatusChanged = false;
        public bool StatusChanged
        {
            get { return tStatusChanged; }
        }

        private Serverstatus tPreviousStatus = Serverstatus.Unknown;
        public Serverstatus PreviousStatus
        {
            get { return tPreviousStatus; }
        }

        public Server(string aName, Servertype aServerType = Servertype.Unknown, string aDescription = "", string aServiceName = "", Servicetype aServiceType = Servicetype.Unknown, string aURL = "", Int32 aLogHistoryDays = 90, bool aActive = true)
        {
            tName = aName;
            if (aServerType != Servertype.Unknown)
            {
                //create new server
                tServerType = aServerType;
                tDescription = aDescription;
                tServiceName = aServiceName;
                tServiceType = aServiceType;
                tURL = aURL;
                tStatus = Serverstatus.Unknown;
                tLastChanged = DateTime.Now;
                tLastChecked = DateTime.Now;
                tLogHistoryDays = aLogHistoryDays;
                tSessionID = string.Empty;
                tLastLogin = null;
                tServerResponse = string.Empty;
                tActive = aActive;

            }
            GetServerData();
        }

        private bool GetServerData()
        {
            bool ServerDataFound = false;
            tServerRow = cf.LoadDataRow(Asgard.AsgardCon, $"SELECT * FROM sync.server WHERE name='{tName}'");
            if (tServerRow != null)
            {
                if (tServerRow["name"].ToString() != string.Empty)
                {
                    tServerType = (Servertype)Enum.Parse(typeof(Servertype), tServerRow["server_type"].ToString(), true);
                    tDescription = tServerRow["Description"].ToString().Trim();
                    tServiceName = tServerRow["service_name"].ToString().Trim();
                    tServiceType = (Servicetype)Enum.Parse(typeof(Servicetype), tServerRow["service_type"].ToString(), true);
                    tURL = tServerRow["url"].ToString().Trim();
                    tStatus = (Serverstatus)Enum.Parse(typeof(Serverstatus), tServerRow["status"].ToString(), true);
                    tLastChanged = tServerRow.Field<DateTime>("last_status_change");
                    tLastChecked = tServerRow.Field<DateTime>("last_status_check");
                    tLogHistoryDays = tServerRow.Field<Int32>("log_history_days");
                    tSessionID = tServerRow["sessionID"].ToString().Trim();
                    tLastLogin = tServerRow.Field<DateTime?>("last_login");
                    tServerResponse = tServerRow["server_response"].ToString().Trim();
                    tActive = (tServerRow["active"].ToString() == "1");
                    ServerDataFound = true;
                }
            }
            else
            {
                Reset();
            }
            return (ServerDataFound);
        }

        public Serverstatus CheckStatus()
        {
            PingReply pingReply;
            using (var ping = new Ping())
            {
                pingReply = ping.Send(tURL);
            }
            tLastChecked = DateTime.Now;

            bool isActive = (pingReply.Status == IPStatus.Success);
            tStatus = isActive ? Serverstatus.Active : Serverstatus.Server_Failed;
            if ((isActive && tStatus != Serverstatus.Active) || (!isActive && tStatus != Serverstatus.Server_Failed))
            {
                tLastChanged = DateTime.Now;
                Save();
            }
            return tStatus;
        }

        private void Reset()
        {
            tServerRow = null;
            tServerType = Servertype.Unknown;
            tDescription = string.Empty;
            tServiceName = string.Empty;
            tServiceType = Servicetype.Unknown;
            tURL = string.Empty;
            tStatus = Serverstatus.Unknown;
            tLastChanged = DateTime.Now;
            tLastChecked = DateTime.Now;
            tLogHistoryDays = 90;
            tSessionID = string.Empty;
            tLastLogin = null;
            tServerResponse = string.Empty;
            tActive = false;
        }

        public bool Refresh() => GetServerData();

        public bool Save()
        {
            if (tName != string.Empty)
            {
                string tQuery = string.Empty;
                //update or insert?
                DataRow tServer = cf.LoadDataRow(Asgard.AsgardCon, $"SELECT * FROM sync.server WHERE name = '{tName}'");
                if (tServer == null)
                {
                    //Insert
                    tQuery = "INSERT INTO sync.server (name,server_type,description,service_name,service_type,URL,status,last_status_check,last_status_change,log_history_days,sessionID,last_login,server_response,active) VALUES (";
                    tQuery += $"'{tName}','{tServerType.ToString()}','{tDescription}','{tServiceName}','{tServiceType}','{tURL}','{tStatus}','{tLastChecked.Value.ToString(Mirror.AxessDateTimeFormat)}','{tLastChanged.Value.ToString(Mirror.AxessDateTimeFormat)}',{tLogHistoryDays.ToString()},'{tSessionID}','{tLastLogin.Value.ToString(Mirror.AxessDateTimeFormat)}','{tServerResponse}',{tActive.ToString()})";
                }
                else
                {
                    //Update
                    tQuery = $"UPDATE sync.server SET server_type='{tServerType.ToString()}',description='{tDescription}',service_name='{tServiceName}',service_type='{tServiceType.ToString()}',URL='{tURL}',status='{tDescription}',last_status_check='{tLastChecked.Value.ToString(Mirror.AxessDateTimeFormat)}',last_status_change='{tLastChanged.Value.ToString(Mirror.AxessDateTimeFormat)}',log_history_days='{tLogHistoryDays.ToString()}',sessionID='{tSessionID}',last_login='{tLastLogin.Value.ToString(Mirror.AxessDateTimeFormat)}',server_response='{tServerResponse.ToString()}' WHERE name='{tName}'";
                }
                return cf.ExecuteSQL(Asgard.AsgardCon,tQuery);
            }
            return GetServerData();
        }
    }
}
