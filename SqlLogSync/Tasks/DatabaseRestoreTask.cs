using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlLogSync.Tasks
{
    public class DatabaseRestoreTask : DatabaseTask
    {

        protected override async Task ExecuteBackupAsync()
        {

            if (await FullBackupExists())
            {
                await RestoreLogs();
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

        private async Task RestoreLogs()
        {
            DateTime now = DateTime.Now;
            await foreach(var x in Storage.EnumerateFilesAsync(Database + "/" + FileDateKey(now) + "/logs/")) {
                string backup = x.Folder + "/" + x.Name;
                FileInfo file = new FileInfo(BackupFolder + "\\" + backup.Replace("/", "\\"));
                if (file.Exists)
                    return;
                if (!file.Directory.Exists)
                    file.Directory.Create();
                await Storage.DownloadFile(x.CloudPath, file);
                try
                {
                    await RestoreBackup(file, restoreLog);
                }
                catch (Exception ex) {
                    file.Delete();
                    Utils.Log(ex.ToString());
                    throw new InvalidOperationException("Restore Log failed", ex);
                }
            }
        }

        private async Task<bool> FullBackupExists()
        {
            DateTime now = DateTime.Now;

            string backup = Database + "/" + FileDateKey(now) + ".bak.7z";

            FileInfo zipFile = new FileInfo(BackupFolder + "\\" + backup.Replace("/", "\\"));
            if (zipFile.Exists)
                return true;

            if (!await Storage.FileExists(backup)) {
                return false;
            }

            if (!zipFile.Directory.Exists)
                zipFile.Directory.Create();
            await Storage.DownloadFile(backup, zipFile);

            await RestoreBackup(zipFile, restoreFull);

            return true;            
        }

        private async Task RestoreBackup(FileInfo zipFile, params string[] scripts)
        {

            FileInfo tfile = new FileInfo(zipFile.FullName + ".t.bak");
            if (tfile.Exists)
                tfile.Delete();
            var p = System.Diagnostics.Process.Start(AppContext.BaseDirectory.TrimEnd('/') + "/7za.exe", new string[] {
                "x",
                "-p" + ZipPassword,
                zipFile.FullName,
                zipFile.DirectoryName
            });
            await p.WaitForExitAsync();

            try
            {
                foreach (string script in scripts)
                {
                    string sql = string.Format(script, Database, tfile.FullName, tfile.FullName + ".r");

                    await ConnectionString.ExecuteAsync(sql);
                }
                tfile.Delete();
                System.IO.File.Delete(tfile.FullName + ".r");
            }
            catch (Exception ex) {
                Utils.Log(ex.ToString());
                //zipFile.Delete();
                throw new InvalidOperationException("Restore Failed for " + Database,ex);
            }
        }

        string restoreLog = "RESTORE LOG [{0}] FROM  DISK = N'{1}' WITH FILE=1, STANDBY=N'{2}',  NOUNLOAD, REPLACE, STATS = 10";
        string restoreFull = "RESTORE DATABASE [{0}] FROM  DISK = N'{1}' WITH FILE=1, STANDBY=N'{2}',  NOUNLOAD, REPLACE, STATS = 10";




    }
}
