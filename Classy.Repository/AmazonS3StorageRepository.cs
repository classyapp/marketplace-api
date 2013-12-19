using Amazon.S3.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace Classy.Repository
{
    public class AmazonS3StorageRepository : IStorageRepository
    {
        public void SaveFile(string key, byte[] content, string contentType)
        {
            var s3Client = Amazon.AWSClientFactory.CreateAmazonS3Client(
                ConfigurationManager.AppSettings["S3AccessKey"], 
                ConfigurationManager.AppSettings["S3SecretKey"]
            );
            PutObjectRequest request = new PutObjectRequest();
            request.BucketName = ConfigurationManager.AppSettings["S3BucketName"];
            request.ContentType = contentType;
            request.Key = key;
            request.InputStream = new MemoryStream(content);
            s3Client.PutObject(request);
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