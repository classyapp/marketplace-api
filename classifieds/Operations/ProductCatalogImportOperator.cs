using classy.DTO.Request;
using classy.Manager;
using Classy.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace classy.Operations
{
    public class ProductCatalogImportOperator : IOperator<ImportProductCatalogRequest>
    {
        private readonly IStorageRepository _storageRepository;
        private readonly IListingRepository _listingRepository;
        private readonly IAppManager _appManager;

        public ProductCatalogImportOperator(IStorageRepository storageRepo, IListingRepository listingRepo, IAppManager appManager)
        {
            _storageRepository = storageRepo;
            _listingRepository = listingRepo;
            _appManager = appManager;
        }

        public void PerformOperation(ImportProductCatalogRequest request)
        {
            throw new NotImplementedException();
        }
    }
}