using Amazon.S3;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SqlLogSync.Storages;

internal class S3Storage : Storage
{
    private readonly AmazonS3Client client;

    public string BucketName { get; private set; }

    public S3Storage()
    {
        this.client = new AmazonS3Client();
        this.BucketName = "";
    }

    public override Task DeleteFile(string cloudPath)
    {
        throw new NotImplementedException();
    }

    public override Task DownloadFile(string cloudPath, FileInfo localFile)
    {
        throw new NotImplementedException();
    }

    public override async IAsyncEnumerable<StorageFile> EnumerateFilesAsync(string path)
    {
        string token = null;
        do
        {
            var list = await client.ListObjectsV2Async(new Amazon.S3.Model.ListObjectsV2Request
            {
                BucketName = this.BucketName,
                Prefix = path,
                ContinuationToken = token
            });
            foreach(var item in list.S3Objects)
            {
                yield return new StorageFile(item.Key, item.LastModified ?? DateTime.Now);
            }
            token = list.ContinuationToken;
        } while (token != null);
    }

    public override async Task<bool> FileExists(string cloudPath)
    {
        try
        {
            var r = await client.GetObjectMetadataAsync(this.BucketName, cloudPath);
            return (int)r.HttpStatusCode <= 399;
        }
        catch (AmazonS3Exception)
        {
            return false;
        }
    }

    public override Task UploadFile(string cloudPath, FileInfo localFile)
    {
        throw new NotImplementedException();
    }
}
