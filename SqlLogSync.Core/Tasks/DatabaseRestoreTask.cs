using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlLogSync.Tasks
{
    public class DatabaseRestoreTask : DatabaseTask
    {

        protected override void ExecuteBackup()
        {

            if (FullBackupExists())
            {
                RestoreLogs();
            }
            else {

                DirectoryInfo dir = new DirectoryInfo(BackupFolder + "\\" + Database);
                DeleteFolder(dir);

            }

        }

        private static void DeleteFolder(DirectoryInfo dir)
        {
            DateTime utcNow = DateTime.UtcNow.AddDays(-2);
            foreach (var file in dir.EnumerateFiles().Where(x => x.CreationTimeUtc < utcNow).ToArray())
            {
                file.Delete();
            }
            foreach (var d in dir.EnumerateDirectories().ToArray())
            {
                DeleteFolder(d);
            }
            if (!dir.EnumerateFileSystemInfos().Any()) {
                dir.Delete();
            }
        }

        private void RestoreLogs()
        {
            DateTime now = DateTime.Now;
            Storage.EnumerateFiles(Database + "/" + FileDateKey(now) + "/logs/", x => {
                string backup = x.Folder + "/" + x.Name;
                FileInfo file = new FileInfo(BackupFolder + "\\" + backup.Replace("/", "\\"));
                if (file.Exists)
                    return;
                if (!file.Directory.Exists)
                    file.Directory.Create();
                Storage.DownloadFile(x.CloudPath, file);
                try
                {
                    RestoreBackup(file, restoreLog);
                }
                catch (Exception ex) {
                    file.Delete();
                    Utils.Log(ex.ToString());
                    throw new InvalidOperationException("Restore Log failed", ex);
                }
            });
        }

        private bool FullBackupExists()
        {
            DateTime now = DateTime.Now;

            string backup = Database + "/" + FileDateKey(now) + ".bak.zip";

            FileInfo zipFile = new FileInfo(BackupFolder + "\\" + backup.Replace("/", "\\"));
            if (zipFile.Exists)
                return true;

            if (!Storage.FileExists(backup)) {
                return false;
            }

            if (!zipFile.Directory.Exists)
                zipFile.Directory.Create();
            Storage.DownloadFile(backup, zipFile);

            RestoreBackup(zipFile, restoreFull);

            return true;            
        }

        private void RestoreBackup(FileInfo zipFile, params string[] scripts)
        {

            FileInfo tfile = new FileInfo(zipFile.FullName + ".t.bak");
            if (tfile.Exists)
                tfile.Delete();
            using (ZipFile zip = new ZipFile(zipFile.FullName)) {
                foreach (ZipEntry entry in zip)
                {
                    using (FileStream fsout = tfile.OpenWrite())
                    {
                        entry.ExtractWithPassword(fsout, ZipPassword);
                    }
                    break;
                }
            }

            try
            {
                foreach (string script in scripts)
                {
                    string sql = string.Format(script, Database, tfile.FullName, tfile.FullName + ".r");

                    ConnectionString.Execute(sql);
                }
                tfile.Delete();
                System.IO.File.Delete(tfile.FullName + ".r");
            }
            catch (Exception ex) {
                Utils.Log(ex.ToString());
                zipFile.Delete();
                throw new InvalidOperationException("Restore Failed for " + Database,ex);
            }
        }

        string restoreLog = "RESTORE LOG [{0}] FROM  DISK = N'{1}' WITH FILE=1, STANDBY=N'{2}',  NOUNLOAD, REPLACE, STATS = 10";
        string restoreFull = "RESTORE DATABASE [{0}] FROM  DISK = N'{1}' WITH FILE=1, STANDBY=N'{2}',  NOUNLOAD, REPLACE, STATS = 10";




    }
}
