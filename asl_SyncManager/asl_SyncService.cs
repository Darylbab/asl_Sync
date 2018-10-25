using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using asl_SyncLibrary;



namespace asl_SyncManager
{
    public partial class Asl_SyncManager : ServiceBase
    {
        static Asgard ASG = new Asgard();

        private Task[] Tasks = null;

        public enum ServiceState
        {
            SERVICE_STOPPED = 0x00000001,
            SERVICE_START_PENDING = 0x00000002,
            SERVICE_STOP_PENDING = 0x00000003,
            SERVICE_RUNNING = 0x00000004,
            SERVICE_CONTINUE_PENDING = 0x00000005,
            SERVICE_PAUSE_PENDING = 0x00000006,
            SERVICE_PAUSED = 0x00000007,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ServiceStatus
        {
            public int dwServiceType;
            public ServiceState dwCurrentState;
            public int dwControlsAccepted;
            public int dwWin32ExitCode;
            public int dwServiceSpecificExitCode;
            public int dwCheckPoint;
            public int dwWaitHint;
        };

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(IntPtr handle, ref ServiceStatus serviceStatus);

        private int EventId = 1;        // used by log features


        public Asl_SyncManager(string[] args)
        {
            InitializeComponent();

            if (!EventLog.SourceExists("aslSyncManager"))
            {
                EventLog.CreateEventSource("aslSyncManager", "aslSyncLog");
            }
            SyncEventLog.Source = "aslSyncManager";
            SyncEventLog.Log = "aslSyncLog";
        }

        protected override void OnStart(string[] args)
        {
            //these lines must remain at the beginning of this function
            ServiceStatus serviceStatus = new ServiceStatus
            {
                dwCurrentState = ServiceState.SERVICE_START_PENDING,
                dwWaitHint = 100000
            };
            SetServiceStatus(ServiceHandle, ref serviceStatus);
            SyncEventLog.WriteEntry("Starting Service", EventLogEntryType.Information, EventId++);
            //end of beginning lines 

            System.Timers.Timer SyncTimer = new System.Timers.Timer
            {
                Interval = 10000 // 10 seconds  
            };
            SyncTimer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimer);
            SyncTimer.Start();



            //these lines must remain at the end of this function.
            serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
            SetServiceStatus(ServiceHandle, ref serviceStatus);
        }

        protected override void OnStop()
        {
            //these lines must remain at the beginning of this function
            ServiceStatus serviceStatus = new ServiceStatus
            {
                dwCurrentState = ServiceState.SERVICE_STOP_PENDING,
                dwWaitHint = 100000
            };
            SetServiceStatus(ServiceHandle, ref serviceStatus);
            SyncEventLog.WriteEntry("Stopping Service", EventLogEntryType.Information, EventId++);
            //end of beginning lines 


            // close all running tasks


            //these lines must remain at the end of this function.
            serviceStatus.dwCurrentState = ServiceState.SERVICE_STOPPED;
            SetServiceStatus(ServiceHandle, ref serviceStatus);
        }

        private bool Initialized = false;
        private bool Init()
        {
            if (!Initialized)
            {
                //
            };
            Initialized = (ASG.GetServerStatus() == Serverstatus.Active);
            return Initialized;
        }

        private void OnTimer(object sender, System.Timers.ElapsedEventArgs args)
        {
            DateTime CurrentTime = DateTime.Now;

            if (!Initialized)
            {
                Init();
            }
            // verify currently running tasks.
            foreach (Task tTask in Tasks)
            {
                switch (tTask.CheckStatus())
                    {
                    case -1:    //task has completed
                        break;
                    case 0:     //all is good
                        break;
                    case 1:     //task not responding
                        //close task
                        //restart task (try 3 times then error and send email)
                        break;
                    default:    //something unexpected happened.
                        //close task
                        //restart task (try 3 times then error and send email)
                        break;
                    }
            }
            // check for and start new tasks.
            // loop through all tasks currently in focus
            // if task not already running then start it
            
            // loop through all tasks in [Tasks] and close those outside of focus.
        }



         internal class Task
        {
            public Task(ProcessStartInfo aPSI = null, bool aStart = false)
            {
                if (aPSI != null)
                {
                    tProcess.StartInfo = aPSI;
                    if (aStart)
                    {
                        tProcess.Start();
                    }
                }
            }

            private Process tProcess = null;
            //private string name;
            //private DateTime startDate;
            private DateTime startTime;
            //private DateTime endDate;
            //private DateTime endTime;
            //private int tPeriod;
            //private TimeSpan maxRunTime;
            //private TimeSpan avgRunTime;
            private DateTime lastStatusCheck;
            private TimeSpan lastPrivilegeTime;
            private bool killRequested = false;
            //private int maxLocks;


            internal int CheckStatus()
            {
                lastStatusCheck = DateTime.Now;
                return GetProcessState(); ;
            }

            private int GetProcessState()
            {
                if (tProcess.HasExited)
                {
                    return -1;  //process has exited properly
                }
                try
                {
                    if (tProcess.Responding)
                    {
                        lastPrivilegeTime = tProcess.PrivilegedProcessorTime;
                        return 0;   //process responding properly
                    }
                    return 1;   //process not responding
                }
                catch { return 666; }
                //Task has not used any processor time, consider it locked up.
                //if (lastPrivilegeTime == tProcess.PrivilegedProcessorTime)
                //{
           
                //}
               // return 666;     //unknown state
            }

            internal bool StartProcess()
            {
                startTime = DateTime.Now;
                return true;
            }

            internal bool CloseProcess()
            {
                try
                {
                    tProcess.Kill();
                    killRequested = true;
                }
                catch
                {
                    tProcess.WaitForExit(5000);
                }
                if (killRequested)
                {

                }
                return tProcess.HasExited;
            }
        }
    }
}
