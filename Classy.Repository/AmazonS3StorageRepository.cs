using Amazon.S3.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;

namespace Classy.Repository
{
    public class AmazonS3StorageRepository : IStorageRepository
    {
        private readonly Amazon.S3.IAmazonS3 s3Client;
        private readonly string bucketName;

        public AmazonS3StorageRepository(Amazon.S3.IAmazonS3 s3Client, string bucketName)
        {
            this.s3Client = s3Client;
            this.bucketName = bucketName;
        }
        public void SaveFile(string key, byte[] content, string contentType)
        {
            PutObjectRequest request = new PutObjectRequest();
            request.BucketName = bucketName;
            request.ContentType = contentType;
            request.Key = key;
            request.InputStream = new MemoryStream(content);
            s3Client.PutObjectAsync(request);
        }

        public Stream GetFile(string key)
        {
            var request = new GetObjectRequest()
            {
                Key = key,
                BucketName = bucketName
            };
            var obj = s3Client.GetObject(request);
            byte[] buffer = new byte[obj.ContentLength];
            return obj.ResponseStream;
        }

        public void SaveFileFromUrl(string key, string url, string contentType)
        {
            var client = new WebClient();
            byte[] img = client.DownloadData(url);
            SaveFile(key, img, contentType);
        }

        public void DeleteFile(string key)
        {
            throw new NotImplementedException();
        }

        public string KeyToUrl(string key)
        {
            return string.Concat("http://", ConfigurationManager.AppSettings["CloudFrontDistributionUrl"].TrimEnd('/'), '/', key);
        }
    }
}