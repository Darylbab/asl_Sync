using System;
using System.Data;
using MySql.Data.MySqlClient;

namespace asl_SyncLibrary
{
    public class Shop
    {
        private CommonFunctions cf = new CommonFunctions();
        private ASLError myError = new ASLError();

        private const string Shop_Connection_String = "server=142.93.56.69; username=alta; password=vPCJpp63ED9BfduWqK; Convert Zero Datetime=True; ";
        public MySqlConnection Shop_Alta_ComConn = new MySqlConnection(Shop_Connection_String);

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
            if (cf.OpenConn(Shop_Alta_ComConn) == ConnectionState.Open)
            {
                cf.CloseConn(Shop_Alta_ComConn);
                tStat = Serverstatus.Active;
            }
            else
            {
                try
                {
                    Shop_Alta_ComConn.Ping();
                    tStat = Serverstatus.Service_Failed;
                }
                catch { tStat = Serverstatus.Server_Failed; }
            }
            return tStat;
        }




    }
}
