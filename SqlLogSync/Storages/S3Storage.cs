using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Transfer;
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

    public S3Storage(string bucketName, string accessKeyId, string keySecret)
    {
        this.client = new AmazonS3Client(new AmazonS3Config
        {
            RegionEndpoint = Amazon.RegionEndpoint.USEast1,
            DefaultAWSCredentials = new BasicAWSCredentials(accessKeyId, keySecret)
        });
        this.BucketName = bucketName;
    }

    public override Task DeleteFile(string cloudPath)
    {
        throw new NotImplementedException();
    }

    public override async Task DownloadFile(string cloudPath, FileInfo localFile)
    {
        using var tr = new TransferUtility(this.client);
        await tr.DownloadAsync(localFile.FullName, this.BucketName, cloudPath);
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

    public override async Task UploadFile(string cloudPath, FileInfo localFile)
    {
        using var tr = new TransferUtility(this.client);
        using var fs = localFile.OpenRead();
        var r = new TransferUtilityUploadRequest {
            BucketName = this.BucketName,
            Key = cloudPath,
            AutoCloseStream= true,
            FilePath = localFile.FullName,
        };
        await tr.UploadAsync(r);
    }
}
