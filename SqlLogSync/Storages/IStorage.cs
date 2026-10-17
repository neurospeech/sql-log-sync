using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlLogSync.Storages
{
    public abstract class Storage
    {
        public Storage()
        {
        }

        public string Host { get; set; }
        public string Container { get; set; }
        public string Key { get; set; }
        public string Secrete { get; set; }
        public string LocalFolder { get; set; }

        public abstract IAsyncEnumerable<StorageFile> EnumerateFilesAsync(string path);

        public abstract Task UploadFile(string cloudPath,FileInfo localFile);

        public abstract Task DownloadFile(string cloudPath, FileInfo localFile);

        public abstract Task DeleteFile(string cloudPath);

        public abstract Task<bool> FileExists(string cloudPath);


        public void Sync() { 



        }
        

    }
}