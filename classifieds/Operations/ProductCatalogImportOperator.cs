using classy.Manager;
using Classy.Models.Request;
using Classy.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace classy.Operations
{
    public class ProductCatalogImportOperator : IOperator<ImportProductCatalogJob>
    {
        private readonly IStorageRepository _storageRepository; // AWS
        private readonly IListingRepository _listingRepository; //MONGO
        private readonly IAppManager _appManager;

        public ProductCatalogImportOperator(IStorageRepository storageRepo, IListingRepository listingRepo, IAppManager appManager)
        {
            _storageRepository = storageRepo;
            _listingRepository = listingRepo;
            _appManager = appManager;
        }

        public void PerformOperation(ImportProductCatalogJob request)
        {
            //var app = _appManager.GetAppById(request.Environment.AppId);
            //var listing = _listingRepository.GetById(request.ProfileId, request.Environment.AppId, true, null);
            //var listingMediaFile = listing.ExternalMedia.Single(x => x.Key == request.AWSFileKey);


            throw new NotImplementedException();
        }
    }
}
