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
        public void SaveFile(string key, byte[] content, string contentType)
        {
            var ms = new MemoryStream(content);
            var img = System.Drawing.Image.FromStream(ms);
            var filename = string.Concat(@"c:\img\", key, ".gif");
        }

        public void SaveFileFromUrl(string key, string url, string contentType)
        {
            throw new NotImplementedException();
        }

        public void DeleteFile(string key)
        {
            File.Delete(string.Concat(@"c:\img\", key, ".gif"));
        }

        public string KeyToUrl(string key)
        {
            return string.Concat(@"c:\img\", key, ".gif");
        }
    }
}