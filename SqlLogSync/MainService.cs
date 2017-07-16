using SqlLogSync.Tasks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SqlLogSync
{
    public partial class MainService : ServiceBase
    {
        public MainService()
        {
            InitializeComponent();

        }

        

        protected override void OnStart(string[] args)
        {
            //Task.Run(async () => await PingAsync());
            Task.Run(async () => await RunLoop());
        }

        private TimeSpan delay = TimeSpan.FromMinutes(5);

        private async Task RunLoop()
        {
            while (true)
            {
                try {
                    RunTask();
                }
                catch (Exception ex)
                {
                    LogText(ex.ToString(), EventLogEntryType.Error);
                }
                await Task.Delay(delay);
            }
        }

        protected override void OnStop()
        {
        }

        //private void pingTimer_Tick(object sender, EventArgs e)
        //{
        //    //Task.Run(async () => await PingAsync());
        //    //ThreadPool.QueueUserWorkItem(w => RunTask());
        //}

        private DatabaseTask currentTask = null;

        private void RunTask()
        {
            try
            {

                lock (this)
                {
                    if (currentTask == null)
                    {
                        currentTask = DatabaseTask.Create();
                    }
                    else
                    {
                        LogText("Previous backup is still running", EventLogEntryType.Information);
                        return;
                    }
                }



                currentTask.Run();
                LogText("Task completed successfully", EventLogEntryType.Information);
                lock (this) {
                    currentTask = null;
                }
            }
            catch (Exception ex)
            {
                LogText(ex.ToString(), EventLogEntryType.Error);
            }
            finally {
                lock (this)
                {
                    currentTask = null;
                }
            }

        }

        private void LogText(string v, EventLogEntryType error)
        {
            this.EventLog.BeginInit();
            this.EventLog.Source = this.ServiceName;
            if (!EventLog.SourceExists(EventLog.Source))
            {
                EventLog.CreateEventSource(EventLog.Source, "Application");
            }
            this.EventLog.EndInit();
            this.EventLog.Log = "Application";
            this.EventLog.WriteEntry(v, error);

        }

    }
}
