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

        public abstract void EnumerateFiles(string path, Action<StorageFile> fileAction);

        public abstract void UploadFile(string cloudPath,FileInfo localFile);

        public abstract void DownloadFile(string cloudPath, FileInfo localFile);

        public abstract void DeleteFile(string cloudPath);

        public abstract bool FileExists(string cloudPath);


        public void Sync() { 



        }
        

    }
}