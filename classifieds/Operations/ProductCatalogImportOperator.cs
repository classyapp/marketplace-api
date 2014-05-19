using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using classy.Manager;
using Classy.Models;
using Classy.Repository;

namespace classy.Operations
{
    public class ProductCatalogImportOperator : IOperator<ImportProductCatalogJob>
    {
        private readonly IStorageRepository _storageRepository; // AWS
        private readonly IListingRepository _listingRepository; //MONGO
        private readonly IJobRepository _jobRepository; // JOBS
       // private readonly IAppManager _appManager;

        public ProductCatalogImportOperator(IStorageRepository storageRepo, IListingRepository listingRepo, IJobRepository jobRepo)
        {
            _storageRepository = storageRepo;
            _listingRepository = listingRepo;
            _jobRepository = jobRepo;
            //_appManager = appManager;
        }

        public void PerformOperation(ImportProductCatalogJob request)
        {
            var job = _jobRepository.GetById(request.AppId, request.JobId);
            bool overwriteListings = (bool)job.Properties["OverwriteListings"];
            bool updateImages = (bool)job.Properties["UpdateImages"];
            int catalogFormat = (int)job.Properties["CatalogFormat"];

            if (job.Attachments.Count() > 0)
            {
                Stream file = _storageRepository.GetFile(job.Attachments[0].Key);

                StreamReader reader = new StreamReader(file);

                int lineNum = 0;
                Dictionary<string, Listing> listingList = new Dictionary<string, Listing>();


                while (!reader.EndOfStream)
                {

                    string currLine = reader.ReadLine();

                    if (lineNum != 0)
                    {
                        // validations, update job, update database
                        Trace.WriteLine(currLine);

                        string[] dataLine = currLine.Split(';');


                        Listing currListing = null;
                        IList<PurchaseOption> purchaseOptions = null;
                        PurchaseOption purchaseOption = new PurchaseOption();

                        string[] variants = null;

                        if (dataLine[2].ToLower().Equals("parent"))
                        {
                            currListing = new Listing();
                            purchaseOptions = new List<PurchaseOption>();


                            currListing.ProfileId = job.ProfileId;
                            currListing.AppId = job.AppId;
                            currListing.ListingType = "Product";

                            variants = dataLine[4].Split(',');

                            purchaseOption.VariantProperties = new Dictionary<string, string>();
                            foreach (string variation in variants)
                            {
                                switch (variation.ToLower())
                                {
                                    case "color":
                                        purchaseOption.VariantProperties.Add("Color", dataLine[18]);
                                        break;
                                    case "size":
                                        purchaseOption.VariantProperties.Add("Size", dataLine[19]);
                                        break;
                                    case "design":
                                        purchaseOption.VariantProperties.Add("Design", dataLine[20]);
                                        break;
                                    default:
                                        break;
                                }
                            }


                            // only fill out the listing parent once.
                            currListing.Title = dataLine[6];
                            currListing.Content = dataLine[8];

                            string[] categories = dataLine[9].Split(',');


                            if (currListing.Categories == null)
                                currListing.Categories = new List<string>();
		  
                            foreach (string cat in categories)
                                currListing.Categories.Add(cat);


                            if (currListing.Metadata == null)
                                currListing.Metadata = new Dictionary<string,string>();
                            
                            currListing.Metadata.Add("Style", dataLine[10]);
                            currListing.Metadata.Add("Width", dataLine[21]);
                            currListing.Metadata.Add("Depth", dataLine[22]);
                            currListing.Metadata.Add("Height", dataLine[23]);
                            currListing.Metadata.Add("Materials", dataLine[24]);
                            currListing.Metadata.Add("Manufacturer", dataLine[25]);
                            currListing.Metadata.Add("Designer", dataLine[26]);

                            string[] keywords = dataLine[36].Split(',');

                            if (currListing.SearchableKeywords == null)
                                currListing.SearchableKeywords = new List<string>();

                            foreach (string keyword in keywords)
                            {
                                if (keyword.Length > 0)
                                    currListing.SearchableKeywords.Add(keyword);
                            }

                        }
                        else
                        {
                            currListing = listingList[dataLine[1]];
                            purchaseOptions = currListing.PurchaseOptions;

                            purchaseOption.VariantProperties = listingList[dataLine[1]].PurchaseOptions.First().VariantProperties;

                            //child title.
                            purchaseOption.Title = dataLine[6];
                        }

                        
                        purchaseOption.SKU = dataLine[0];

                        purchaseOption.Quantity = int.Parse(dataLine[11]);
                        purchaseOption.Price = double.Parse(dataLine[12]);

                        List<MediaFile> tmpList = new List<MediaFile>();

                        // Media files
                        for (int i = 28; i < 32; i++)
                        {
                            if (dataLine[i].Length > 0)
                            {
                                // add media file
                                MediaFile mf = new MediaFile();
                                mf.Type = MediaFileType.File;
                                mf.ContentType = "image/jpeg";
                                mf.Key = Guid.NewGuid().ToString();
                                mf.Url = dataLine[i];
                                tmpList.Add(mf);
                            }
                        }
                        purchaseOption.MediaFiles = tmpList.ToArray();


                        purchaseOptions.Add(purchaseOption);
                        currListing.PurchaseOptions = purchaseOptions;

                        if (dataLine[2].ToLower().Equals("parent"))
                        {
                            listingList.Add(dataLine[0], currListing);
                        }
                        
                    }

                    lineNum++;
                }

                // write data to storage.
                foreach (Listing currListing in listingList.Values)
                {
                    currListing.IsPublished = true;

                    _listingRepository.Insert(currListing);
                }
            }
        }
    }
}