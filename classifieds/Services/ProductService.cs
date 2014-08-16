using System;
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
            try
            {
                IFile file = Request.Files[0];
                byte[] content = new byte[file.ContentLength];
                file.InputStream.Read(content, 0, content.Length);
                JobManager.Environment = request.Environment;
                return JobManager.ScheduleCatalogImport(request.Environment.AppId, request.ProfileId, request.OverwriteListings, request.UpdateImages, content, file.ContentType, request.CatalogTemplateType);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine("Error occured while scheduling a catalog import");
                throw ex;
            }

        }

        [CustomAuthenticate]
        public object Get(JobsStatusRequest request)
        {
            JobManager.Environment = request.Environment;
            return JobManager.GetJobsStatus(request.Environment.AppId, request.ProfileID);
        }

        [CustomAuthenticate]
        public object Get(JobErrorsRequest request)
        {
            JobManager.Environment = request.Environment;
            return JobManager.GetJobErrors(request.Environment.AppId, request.JobId);
        }
    }
}