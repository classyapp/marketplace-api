using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceStack.ServiceHost;

namespace classy.Manager
{
    public interface ITempMediaFileManager
    {
        string StoreTempFile(string appId, IFile file);
        void DeleteTempFile(string appId, string fileId);
    }
}