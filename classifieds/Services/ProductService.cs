using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using classy.Manager;
using Classy.Models.Request;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace classy.Services
{
    public class ProductService : Service
    {
        public IJobManager JobManager { get; set; }

        [CustomAuthenticate]
        public object Post(ImportPorductCatalogRequest request)
        {
            IFile file = Request.Files[0];
            byte[] content = new byte[file.ContentLength];
            file.InputStream.Read(content, 0, content.Length);
            JobManager.Environment = request.Environment;
            return JobManager.ScheduleCatalogImport(request.Environment.AppId, request.ProfileId, request.OverwriteListings, request.UpdateImages, content, file.ContentType, request.CatalogTemplateType);
        }

        [CustomAuthenticate]
        public object Get(JobsStatusRequest request)
        {
            JobManager.Environment = request.Environment;
            return JobManager.GetJobsStatus(request.Environment.AppId, request.ProfileID);
        }
    }
}