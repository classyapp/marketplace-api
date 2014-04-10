﻿using Amazon.S3.Model;
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
        private static Dictionary<string, Stream> memoryCache;
        private static Dictionary<int, string> keys;

        static AmazonS3StorageRepository()
        {
            memoryCache = new Dictionary<string, Stream>();
            keys = new Dictionary<int, string>();
        }

        public AmazonS3StorageRepository(Amazon.S3.IAmazonS3 s3Client, string bucketName)
        {
            this.s3Client = s3Client;
            this.bucketName = bucketName;
        }

        public void SaveFile(string key, byte[] content, string contentType)
        {
            SaveFile(key, content, contentType, false, null);
        }

        public void SaveFile(string key, byte[] content, string contentType, bool cacheStream, IListingRepository listingRepository)
        {
            PutObjectRequest request = new PutObjectRequest();
            request.BucketName = bucketName;
            request.ContentType = contentType;
            request.Key = key;
            request.InputStream = new MemoryStream(content);
            request.CannedACL = Amazon.S3.S3CannedACL.PublicReadWrite;
            if (cacheStream)
            {
                memoryCache.Add(key, new MemoryStream(content));
            }

            Task<PutObjectResponse> response = s3Client.PutObjectAsync(request);
            keys.Add(response.Id, key);
            response.ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    if (listingRepository != null)
                    {
                        listingRepository.SetListingErrorForMediaFile(keys[t.Id], t.Exception.ToString());
                    }
                    throw t.Exception;
                }
                else if (t.IsCompleted)
                {
                    if (cacheStream)
                    {
                        memoryCache.Remove(keys[t.Id]);
                    }
                }
            });
        }

        public Stream GetFile(string key)
        {
            if (memoryCache.ContainsKey(key))
            {
                return memoryCache[key];
            }

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
            DeleteObjectRequest request = new DeleteObjectRequest();
            request.BucketName = bucketName;
            request.Key = key;
            s3Client.DeleteObjectAsync(request);
        }

        public string KeyToUrl(string key)
        {
            return string.Concat("//", ConfigurationManager.AppSettings["CloudFrontDistributionUrl"].TrimEnd('/'), '/', key);
        }

        private void CacheStream(string originKey, Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            if (memoryCache.ContainsKey(originKey))
            {
                memoryCache[originKey].Dispose();
                memoryCache.Remove(originKey);
            }
            memoryCache.Add(originKey, stream);
        }
    }
}