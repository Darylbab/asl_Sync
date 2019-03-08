using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;


namespace asl_SyncLibrary
{
    public class Asgard
    {
        public static MySqlConnection AsgardCon;

        private CommonFunctions cf = new CommonFunctions();
        private ASLError myError = new ASLError();

        private bool ReadWrite = false;

        public Asgard(bool aReadWrite = false)
        {
            //AsgardServer = new Server("ASGARD");
            ReadWrite = aReadWrite;
#if DEBUG
            AsgardCon = new MySqlConnection(Properties.Settings.Default.AsgardDebug);
#else
            if (aReadWrite)
            {
                AsgardCon = new MySqlConnection(Properties.Settings.Default.AsgardRW);
            }
            else
            {
                AsgardCon = new MySqlConnection(Properties.Settings.Default.AsgardRO);
            }
#endif
        }


        public Serverstatus GetServerStatus()
        {
            Serverstatus tStat = Serverstatus.Unknown;
            if (cf.OpenConn(AsgardCon) == ConnectionState.Open)
            {
                cf.CloseConn(AsgardCon);
                tStat = Serverstatus.Active;
            }
            else
            {
                try
                {
                    AsgardCon.Ping();
                    tStat = Serverstatus.Service_Failed;
                }
                catch { tStat = Serverstatus.Server_Failed; }
            }
            return tStat;
        }

        public DataRow GetServer(string aName) => cf.LoadDataRow(AsgardCon, $"SELECT * FROM sync.server WHERE name='{aName}'");

        public DataTable GetServers() => cf.LoadTable(AsgardCon, "SELECT * FROM sync.server", "Server");

        public DataRow GetTaskTemplate(string aName) => cf.LoadDataRow(AsgardCon, $"SELECT * FROM sync.task_template WHERE Name = '{aName}'");

        public DataRow GetTaskHistory(string aName) => cf.LoadDataRow(AsgardCon, $"SELECT * FROM sync.task_history WHERE name='{aName}'");

        public DataTable GetTaskStatusAll() => cf.LoadTable(AsgardCon, "SELECT Name AS Task_Name, Timestamp AS Last_Update, Function, concat(current_record, ' of ', max_records) AS Function_Status, Action, concat(current_record_2, ' of ', max_records_2) AS Action_Status FROM sync.task_status ORDER BY name", "TaskStatus");

        public DataRow GetTaskStatus(string aName) => cf.LoadDataRow(AsgardCon, $"SELECT * FROM sync.task_status WHERE name='{aName}'");

        public DataTable GetTasks() => cf.LoadTable(AsgardCon, "SELECT * FROM sync.task", "Tasks");

        public DataRow GetTask(string aName) => cf.LoadDataRow(AsgardCon, $"SELECT * FROM sync.task WHERE name='{aName}'");
        
        //return values
        //0 running and responding 
        //1 not found in task_status
        //2 not responding (updating record within 2 minutes)
        //3 reporting error
        public int CheckTaskStatus(string aName, int aResponseTime = 2)
        {
            if (aResponseTime < 1) { aResponseTime = 2; }

            //get the current status for this task.
            DataRow tDR = GetTaskStatus(aName);

            //is the task running?
            if (tDR == null) { return 1; }

            //has it updated within the past 2 minutes?
            DateTime.TryParse(tDR["timestamp"].ToString(), out DateTime tLastCheck);
            if (tLastCheck.AddMinutes(aResponseTime) > DateTime.Now) { return 2; }

            //add in other checks here

            //everything seems to be working fine.
            return 0;
        }

        public bool SetTaskStatus(string aName, string aFunction = "", string aAction = "", string aDescription = "", string aExtra = "", Int32? aCurrentRecord = null, Int32? aMaxRecords = null, Int32? aCurrentRecord2 = null, Int32? aMaxRecords2 = null)
        {
            string bCurrentRecord = (aCurrentRecord == null ? "NULL": aCurrentRecord.ToString());
            string bMaxRecords = (aMaxRecords == null ? "NULL" : aMaxRecords.ToString());
            string bCurrentRecord2 = (aCurrentRecord2 == null ? "NULL" : aCurrentRecord2.ToString());
            string bMaxRecords2 = (aMaxRecords2 == null ? "NULL" : aMaxRecords2.ToString());
            bool RtnVal = false;

            DataRow tDR = cf.LoadDataRow(AsgardCon, $"SELECT * FROM sync.task_status WHERE name='{aName}'");

            if (tDR == null)
            {
                //insert
                RtnVal = cf.ExecuteSQL(AsgardCon, $"INSERT INTO sync.task_status (name, timestamp, function, action, description, extra, current_record, max_records, current_record_2, max_records_2) VALUES ('{aName}', '{DateTime.Now.ToString(Mirror.AxessDateTimeFormat)}', '{aFunction}', '{aAction}', '{aDescription}', '{aExtra}', {bCurrentRecord}, {bMaxRecords}, {bCurrentRecord2}, {bMaxRecords2})");
            }
            else
            {
                //update  --  only update fields which are changed (timestamp always changes).
                string tQuery = $"UPDATE sync.task_status SET timestamp='{DateTime.Now.ToString(Mirror.AxessDateTimeFormat)}'";
                if (aFunction != tDR["function"].ToString().Trim())
                {
                    tQuery += $", function='{aFunction}'";
                }
                if (aAction != tDR["action"].ToString().Trim())
                {
                    tQuery += $", action='{aAction}'";
                }
                if (aDescription != tDR["description"].ToString().Trim())
                {
                    tQuery += $", description='{aDescription}'";
                }
                if (aExtra != tDR["extra"].ToString().Trim())
                {
                    tQuery += $", extra='{aExtra}'";
                }
                if (bCurrentRecord.Replace("NULL","") != tDR["current_record"].ToString().Trim())
                {
                    tQuery += $", current_record={bCurrentRecord}";
                }
                if (bCurrentRecord2.Replace("NULL", "") != tDR["current_record_2"].ToString().Trim())
                {
                    tQuery += $", current_record_2={bCurrentRecord2}";
                }
                if (bMaxRecords.Replace("NULL", "") != tDR["max_records"].ToString().Trim())
                {
                    tQuery += $", max_records={bMaxRecords}";
                }
                if (bMaxRecords2.Replace("NULL", "") != tDR["max_records_2"].ToString().Trim())
                {
                    tQuery += $", max_records_2={bMaxRecords2}";
                }
                tQuery += $" WHERE name='{aName}'";
                RtnVal = cf.ExecuteSQL(AsgardCon, tQuery);
            }
            return RtnVal;
        }
        private bool ValidName(string aName, string aType = "task", string aFunction = "")
        {
            if (string.IsNullOrWhiteSpace(aName))
            {
                throw new ArgumentException($"{aFunction} requires a valid {aType.ToLower()} name.");
            }
            return true;
        }

    }
}
