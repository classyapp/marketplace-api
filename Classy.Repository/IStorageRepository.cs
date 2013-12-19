using Classy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classy.Repository
{
    public interface IStorageRepository
    {
        string KeyToUrl(string key);
        void SaveFile(string key, byte[] content, string contentType);
        void SaveFileFromUrl(string key, string url, string contentType); 
        void DeleteFile(string key); // deletes the file located at the url
    }
}
