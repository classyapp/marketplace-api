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
        private readonly IAppManager _appManager;

        public ProductCatalogImportOperator(IStorageRepository storageRepo, IListingRepository listingRepo, IJobRepository jobRepo, IAppManager appManager)
        {
            _storageRepository = storageRepo;
            _listingRepository = listingRepo;
            _jobRepository = jobRepo;
            _appManager = appManager;
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
                while (!reader.EndOfStream)
                {

                    string currLine = reader.ReadLine();

                    Dictionary<string, Listing> listingList = new Dictionary<string, Listing>();

                    if (lineNum != 0)
                    {
                        // validations, update job, update database
                        Trace.WriteLine(currLine);

                        string[] dataLine = currLine.Split(';');


                        Listing currListing = null;
                        PricingInfo PriceInfo = null;
                        PurchaseOption PO = new PurchaseOption();

                        // create new Listing object only if parent, otherwise get the parent to add a new PO to it.
                        if (dataLine[2].ToLower().Equals("parent"))
                        {
                            currListing = new Listing();
                            PriceInfo = new PricingInfo();
                        }
                        else
                        {
                            currListing = listingList[dataLine[0]];

                            if (currListing == null)
                                throw new Exception("No parent for child"); // TODO : handle better.

                            PriceInfo = currListing.PricingInfo;
                        }

                        currListing.ProfileId = job.ProfileId;

                        PO.SKU = dataLine[0];

                        string[] variants = dataLine[4].Split(',');

                        foreach (string variation in variants)
                        {
                            switch (variation.ToLower())
                            {
                                case "color":
                                    PO.VariantProperties.Add("Color", dataLine[18]);
                                    break;
                                case "size":
                                    PO.VariantProperties.Add("Size", dataLine[19]);
                                    break;
                                case "design":
                                    PO.VariantProperties.Add("Design", dataLine[20]);
                                    break;
                                default:
                                    break;
                            }
                        }


                        if (dataLine[2].ToLower().Equals("parent"))
                        {
                            // only fill out the listing parent once.
                            currListing.Title = dataLine[6];
                            currListing.Content = dataLine[8];

                            string[] categories = dataLine[9].Split(',');

                            foreach (string cat in categories)
                                currListing.Categories.Add(cat);

                            currListing.Metadata.Add("Style", dataLine[10]);
                            currListing.Metadata.Add("Width", dataLine[21]);
                            currListing.Metadata.Add("Depth", dataLine[22]);
                            currListing.Metadata.Add("Height", dataLine[23]);
                            currListing.Metadata.Add("Materials", dataLine[24]);
                            currListing.Metadata.Add("Manufacturer", dataLine[25]);
                            currListing.Metadata.Add("Designer", dataLine[26]);

                            string[] keywords = dataLine[36].Split(',');
                            foreach (string keyword in keywords)
                            {
                                currListing.SearchableKeywords.Add(keyword);
                            }

                        }
                        else
                        {
                            //child title.
                            PO.Title = dataLine[6];
                        }

                        PO.Quantity = int.Parse(dataLine[11]);
                        PO.Price = double.Parse(dataLine[12]);

                        List<MediaFile> tmpList = new List<MediaFile>();

                        // Media files
                        for (int i = 28; i < 32; i++)
                        {
                            if (dataLine[i].Length > 0)
                            {
                                // add media file
                                MediaFile mf = new MediaFile();
                                mf.Type = MediaFileType.File;
                                mf.ContentType = ""; // TODO ???
                                mf.Key = ""; // TODO
                                mf.Url = dataLine[i];
                                tmpList.Add(mf);
                            }
                        }
                        PO.MediaFiles = tmpList.ToArray();


                        PriceInfo.PurchaseOptions.Add(PO);
                        currListing.PricingInfo = PriceInfo;
                    }

                    lineNum++;
                }
            }
        }
    }
}