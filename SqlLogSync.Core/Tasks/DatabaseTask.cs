using SqlLogSync.Storages;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlLogSync.Tasks
{

    public enum DatabaseTaskMode { 
        Backup,
        Restore
    }

    public abstract class DatabaseTask : SyncTask
    {

        

        public static DatabaseTask Create() {

            DatabaseTaskMode mode = DatabaseTaskMode.Backup;

            var settings = System.Configuration.ConfigurationManager.AppSettings;

            string mt = settings["DB.Mode"];
            if (string.IsNullOrWhiteSpace(mt))
            {
                mt = "backup";
            }
            else 
            {
                mt = mt.ToLower();
            }

            if (mt == "restore") {
                mode = DatabaseTaskMode.Restore;
            }

            DatabaseTask DBTask = null;

            if (mode == DatabaseTaskMode.Backup)
            {
                DBTask = new DatabaseBackupTask();
            }
            else {
                DBTask = new DatabaseRestoreTask();
            }

            DBTask.ConnectionString = new System.Data.SqlClient.SqlConnectionStringBuilder(settings["DB.ConnectionString"]);
            DBTask.IgnoreDatabases = settings["DB.IgnoreDatabases"];
            DBTask.BackupFolder = settings["DB.BackupFolder"];
            DBTask.ZipPassword = settings["DB.Zip.Password"];
            int val = int.Parse(settings["DB.Interval"]);

            string storage = settings["DB.Storage"];
            Storage sg = null;

            if (string.IsNullOrWhiteSpace(storage))
            {
                storage = "azure";
            }
            else
            {
                storage = storage.ToLower();
            }

            if (storage == "azure")
            {
                sg = new AzureStorage();

                sg.Container = settings["DB.Storage.Container"];
                sg.Host = settings["DB.Storage.Host"];
                sg.Key = settings["DB.Storage.Key"];
                sg.Secrete = settings["DB.Storage.Secrete"];

                DBTask.Storage = sg;
            }

            return DBTask;
        }

        public string ZipPassword { get; set; }

        public Storages.Storage Storage { get; set; }

        public SqlConnectionStringBuilder ConnectionString { get; set; }

        public string Database { get; set; }

        public string BackupFolder { get; set; }

        public string IgnoreDatabases { get; set; }

        public string FileDateKey(DateTime t)
        {
            return t.ToString("yyyy'-'MM'-'dd");
        }

        public string FileTimeKey(DateTime t)
        {
            return t.ToString("HH'-'mm'-'ss");
        }




        protected sealed override void Execute()
        {
            List<string> list = new List<string>();

            if (string.IsNullOrWhiteSpace(IgnoreDatabases))
            {
                IgnoreDatabases = "";
            }
            else
            {
                IgnoreDatabases = IgnoreDatabases.ToUpper();
            }

            ConnectionString.Execute("SELECT [name] FROM [master].[sys].[databases]", r =>
            {
                while (r.Read())
                {
                    string db = r.GetString(0);
                    if (IgnoreDatabases.Contains(db.ToUpper()))
                        continue;
                    list.Add(db);
                }
            });

            foreach (var item in list)
            {
                Database = item;
                //try
                //{
                    ExecuteBackup();
                //}
                //catch (Exception ex) {
                //    Utils.Log(ex.ToString());
                //}
            }
        }

        protected abstract void ExecuteBackup();

    }
}
