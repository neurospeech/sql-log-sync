using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlLogSync.Storages
{
    public class StorageFile
    {
        public string Name { get; protected set; }
        public string Folder { get; protected set; }
        public DateTime LastModifiedUtc { get; protected set; }

        public string CloudPath { get; protected set; }

        public StorageFile(string cloudPath, DateTime lastModified)
        {
            this.CloudPath = cloudPath;
            this.LastModifiedUtc = lastModified;
            this.Name = Path.GetFileName(cloudPath);
            this.Folder = Path.GetDirectoryName(cloudPath);
        }
    }
}
