using classy.Manager;
using Classy.Models.Request;
using Classy.Repository;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;

namespace classy.Operations
{
    public class ProductCatalogImportOperator : IOperator<ImportProductCatalogJob>
    {
        private readonly IStorageRepository _storageRepository; // AWS
        private readonly IListingRepository _listingRepository; //MONGO
        private readonly IJobRepository _jobRepository; // JOBS
        private readonly IAppManager _appManager;

        public ProductCatalogImportOperator(IStorageRepository storageRepo, IListingRepository listingRepo,IJobRepository jobRepo)
        {
            _storageRepository = storageRepo;
            _listingRepository = listingRepo;
            _jobRepository = jobRepo;
            //_appManager = appManager;

        }

        public void PerformOperation(ImportProductCatalogJob request)
        {
            // var app = _appManager.GetAppById(request.Environment.AppId);
            //var listing = _listingRepository.GetById(request.ProfileId, request.Environment.AppId, true, null);
            //var listingMediaFile = listing.ExternalMedia.Single(x => x.Key == request.AWSFileKey);
            var job = _jobRepository.GetById(request.AppId, request.JobId);
            bool overwriteListings = (bool)job.Properties["OverwriteListings"];
            bool updateImages = (bool)job.Properties["UpdateImages"];
            int catalogFormat = (int)job.Properties["CatalogFormat"];

            if (job.Attachments.Count() > 0)
            {
                Stream file = _storageRepository.GetFile(job.Attachments[0].Key);

                StreamReader reader = new StreamReader(file);

                int lineNum = 0;
                while (!reader.EndOfStream)
                {
                    
                    string currLine = reader.ReadLine();

                    if (lineNum != 0)
                    { 
                        // validations, update job, update database
                        Trace.WriteLine(currLine);
                    }

                    lineNum++;
                }

            }
            else { 
                // TODO: add error
            }

            throw new NotImplementedException();
        }
    }
}