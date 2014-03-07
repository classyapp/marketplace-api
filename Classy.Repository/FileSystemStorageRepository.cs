using Classy.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Classy.Repository
{
    public class FileSystemStorageRepository : IStorageRepository
    {
        private string GetPathFromKey(string key)
        {
            return string.Concat(@"c:\img\", key, ".gif");
        }
        public void SaveFile(string key, byte[] content, string contentType)
        {
            SaveFile(key, content, contentType, false);
        }
        public void SaveFile(string key, byte[] content, string contentType, bool cacheStream)
        {
            var ms = new MemoryStream(content);
            var img = System.Drawing.Image.FromStream(ms);
            var filename = GetPathFromKey(key);
        }

        public void SaveFileFromUrl(string key, string url, string contentType)
        {
            throw new NotImplementedException();
        }

        public void DeleteFile(string key)
        {
            File.Delete(GetPathFromKey(key));
        }

        public string KeyToUrl(string key)
        {
            return GetPathFromKey(key);
        }


        public Stream GetFile(string key)
        {
            FileStream stream = new FileStream(GetPathFromKey(key), FileMode.Open);
            return stream;
        }
    }
}