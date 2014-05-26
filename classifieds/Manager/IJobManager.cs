using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Classy.Models.Response;

namespace classy.Manager
{
    public interface IJobManager : IManager
    {
        JobView ScheduleCatalogImport(string appId, string profile, bool overwriteListings, bool updateImages, byte[] catalog, string contentType, int catalogFormat);
        IList<JobView> GetJobsStatus(string appId, string profileId);
    }
}