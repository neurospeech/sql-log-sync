using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlLogSync.Tasks
{

    public class DatabaseBackupTask : DatabaseTask
    {
        #region private bool FullBackupExists()
        private async Task<bool> FullBackupExists()
        {
            string today = FileDateKey(DateTime.Today);
            if (Storage.FileExists(Database + "/" + today + ".bak.zip"))
                return true;

            DateTime older = DateTime.Today.AddDays(-30);
            DateTime till = DateTime.Today.AddDays(-10);
            for (DateTime i = older; i < till; i = i.AddDays(1))
            {
                Storage.DeleteFile(Database + "/" + FileDateKey(i));
            }

            for (DateTime i = till; i <= DateTime.Today.AddDays(-5); i = i.AddDays(1))
            {
                Storage.DeleteFile(Database + "/" + FileDateKey(i) + "/logs/");
            }

            

            await TakeBackup(backup, today);

            return false;
        }
        #endregion

        #region private void TakeBackup(string backup,string today)
        private async Task TakeBackup(string sql, string file)
        {
            string localFile = BackupFolder + "\\" + Database + "-" + file.Replace("/","-") + ".bak";
            sql = string.Format(sql, Database, localFile);

            if (File.Exists(localFile))
                File.Delete(localFile);

            Utils.Log(Database + ": Backup Started");

            await ConnectionString.ExecuteAsync(sql);

            Utils.Log(Database + ": Compressing Backup");

            string tmpFile = Path.GetTempFileName();

            using( var fs = System.IO.File.OpenWrite(tmpFile))
            using (var zip = new ZipOutputStream(fs))
            {
                zip.Password = ZipPassword;
                zip.SetLevel(9);
                zip.UseZip64 = UseZip64.On;
                var lf = new FileInfo(localFile);
                var entry = new ZipEntry(lf.Name);
                entry.AESKeySize = 256;
                entry.DateTime = DateTime.Now;
                await zip.PutNextEntryAsync(entry);

                using (var fin = System.IO.File.OpenRead(localFile))
                {
                    await fin.CopyToAsync(zip);
                }
            }

            Utils.Log(Database + ": Uploading Backup");
            Storage.UploadFile(Database + "/" +  file + ".bak.zip", new FileInfo(tmpFile));
            File.Delete(tmpFile);
            File.Delete(localFile);

            Utils.Log(Database + ": Backup Done");
            System.GC.Collect();
        }
        #endregion



        #region private void BackupLog()
        private Task BackupLog()
        {
            return TakeBackup(backupLog, FileDateKey(DateTime.Now) + "/logs/" + FileTimeKey(DateTime.Now));
        }
        #endregion

        string backupLog = "BACKUP LOG [{0}] TO  DISK = N'{1}' WITH NOFORMAT, NOINIT,  NAME = N'{0}-Transaction Log  Backup', SKIP, NOREWIND, NOUNLOAD,  STATS = 10";
        string backup = "BACKUP DATABASE [{0}] TO  DISK = N'{1}' WITH NOFORMAT, NOINIT,  NAME = N'{0}-Full Database Backup', SKIP, NOREWIND, NOUNLOAD,  STATS = 10";

        protected override async Task ExecuteBackupAsync()
        {
            if (await FullBackupExists())
            {
                await BackupLog();
            }
        }

    }
}
