using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlLogSync.Storages
{
    public abstract class StorageFile
    {
        public virtual string Name { get; protected set; }
        public virtual string Folder { get; protected set; }
        public virtual DateTime LastModifiedUtc { get; protected set; }

        public string CloudPath
        {
            get
            {
                return Folder + "/" + Name;
            }
        }

    }
}
