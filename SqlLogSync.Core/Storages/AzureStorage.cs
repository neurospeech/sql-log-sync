using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlLogSync.Storages
{
    public class AzureStorage : Storage
    {

        public AzureStorage()
        {

        }

        private CloudBlobClient _Client = null;
        public CloudBlobClient Client {
            get {
                if (_Client == null) { 
                    _Client = new CloudBlobClient(new Uri("https://" + Host), new StorageCredentials(Key, Secrete));;
                }
                return _Client;
            }
        }

        public override void EnumerateFiles(string path, Action<StorageFile> fileAction)
        {
            var c = Client;

            BlobContinuationToken token = null;

            path = path.ToLower();

            while(true){
                var results = c.ListBlobsSegmented(Container + "/" + path, true, BlobListingDetails.Metadata, 100, token, null,null);
                //var results = c.ListBlobsWithPrefixSegmented(Container + "/" + path, 100, token, new BlobRequestOptions { UseFlatBlobListing = true });
                foreach (var item in results.Results)
                {
                    CloudBlob cb = item as CloudBlob;
                    if (cb != null) {
                        AzureStorageFile asf = new AzureStorageFile(cb, Container);
                        fileAction(asf);
                    }
                }
                if (results.ContinuationToken == null)
                    break;
                token = results.ContinuationToken;
            }
        }

        public override void UploadFile(string cloudPath, System.IO.FileInfo localFile)
        {
            cloudPath = cloudPath.ToLower();
            var client = Client;
            var blob = client.GetBlobReferenceFromServer(new Uri(client.BaseUri, Container + "/" + cloudPath));
            //blob.UploadFile(localFile.FullName, new BlobRequestOptions { Timeout = TimeSpan.FromMinutes(10) });
            blob.UploadFromFile(localFile.FullName);
        }
        public override void DownloadFile(string cloudPath, System.IO.FileInfo localFile)
        {
            cloudPath = cloudPath.ToLower();
            var client = Client;
            var blob = client.GetBlobReferenceFromServer(new Uri(client.BaseUri,Container + "/" + cloudPath));
            if (localFile.Exists)
                localFile.Delete();

            System.IO.FileInfo t = new System.IO.FileInfo(localFile.FullName + ".t");
            if (t.Exists)
                t.Delete();
            using (var fs = t.OpenWrite())
            {
                blob.DownloadToStream(fs);
            }
            t.MoveTo(localFile.FullName);
        }
        public override void DeleteFile(string cloudPath)
        {
            cloudPath = cloudPath.ToLower();
            var client = Client;
            foreach (var item in client.ListBlobsSegmented(Container + "/" + cloudPath, true, BlobListingDetails.Metadata, 500,null,null,null).Results)
            {
                var cb = item as CloudBlob;
                if (cb != null)
                    cb.Delete();
            }
        }

        public override bool FileExists(string cloudPath)
        {
            cloudPath = cloudPath.ToLower();
            var client = Client;
            
            var b = client.GetBlobReferenceFromServer(new Uri(client.BaseUri, Container + "/" + cloudPath));
            return b.Exists();
        }
    }


    public class AzureStorageFile : StorageFile {

        public AzureStorageFile(CloudBlob cb, string container)
        {
            this.Name = cb.Name;

            string path = cb.Uri.AbsolutePath;
            path = System.IO.Path.GetDirectoryName(path).Replace("\\","/");

            if (path.StartsWith("/"))
                path = path.Substring(1);

            int index = path.IndexOf('/');
            path = path.Substring(index + 1);

            Folder = path;

            index = Name.LastIndexOf('/');
            if (index != -1) {
                Name = Name.Substring(index + 1);
            }

            LastModifiedUtc = cb.Properties.LastModified.Value.UtcDateTime;
        }

    }
}
