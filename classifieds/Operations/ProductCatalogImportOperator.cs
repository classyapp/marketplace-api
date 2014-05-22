using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using classy.Manager;
using Classy.Models;
using Classy.Repository;
using Classy.Interfaces.Managers;
using System.Net;

namespace classy.Operations
{
    public class ProductCatalogImportOperator : IOperator<ImportProductCatalogJob>
    {
        private readonly IStorageRepository _storageRepository; // AWS
        private readonly IListingRepository _listingRepository; //MONGO
        private readonly IJobRepository _jobRepository; // JOBS
        private readonly ICurrencyManager _currencyManager; // 

        public ProductCatalogImportOperator(IStorageRepository storageRepo, IListingRepository listingRepo, IJobRepository jobRepo, ICurrencyManager currencyManager)
        {
            _storageRepository = storageRepo;
            _listingRepository = listingRepo;
            _jobRepository = jobRepo;
            _currencyManager = currencyManager;
        }

        private void ReportError(Exception ex, Job job, IJobRepository jobRepo, int savedProducts, int errors)
        {
            job.ImportErrors.Add(ex.Message);
            job.ProgressInfo = savedProducts + " products saved, " + errors + " errors encountered";
            jobRepo.Save(job);
        }

        public void PerformOperation(ImportProductCatalogJob request)
        {

            var job = _jobRepository.GetById(request.AppId, request.JobId);
            bool overwriteListings;
            int catalogFormat;
            bool updateImages;
            string currencyCode = null;
            int numProductsSaved = 0;
            int numErrors = 0;

            try
            {
                overwriteListings = (bool)job.Properties["OverwriteListings"];
                updateImages = (bool)job.Properties["UpdateImages"];
                catalogFormat = (int)job.Properties["CatalogFormat"];
                currencyCode = (string)job.Properties["CurrencyCode"];
            }
            catch (Exception ex)
            {
                ++numErrors;
                ReportError(new Exception("One of the required properties for the job is missing: " + ex.Message), job, _jobRepository, numProductsSaved, numErrors);
            }

            if (job.Attachments.Count() > 0)
            {
                Stream file = _storageRepository.GetFile(job.Attachments[0].Key);
                StreamReader reader = new StreamReader(file);
                int lineNum = 0;
                Listing activeListing = null;
                bool skipToNextParent = false;
                Dictionary<string, string> variantProperties = new Dictionary<string, string>();

                try
                {
                    job.Status = "Executing";
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
                            PurchaseOption purchaseOption = null; // = new PurchaseOption();

                            string[] variants = null;

                            if (dataLine[2].ToLower().Equals("parent") || dataLine[2].ToLower().Trim().Equals(""))
                            {

                                if (activeListing != null)
                                {
                                    // persist the last listing and create a new one.
                                    activeListing.IsPublished = true;

                                    try
                                    {
                                        _listingRepository.Insert(activeListing);
                                    }
                                    catch (Exception ex)
                                    {
                                        throw new Exception("Error persisting listing: " + activeListing.Id + ". Error: " + ex.Message);
                                    }

                                    try
                                    {
                                        job.ProgressInfo = ++numProductsSaved + " products saved, " + numErrors + " errors encountered";
                                        _jobRepository.Save(job);
                                    }
                                    catch (Exception ex)
                                    {
                                        throw new Exception("Error persisting job status, error was: " + ex.Message);
                                    }
                                    activeListing = null;
                                }

                                skipToNextParent = false;

                                currListing = new Listing();
                                currListing.PricingInfo = new PricingInfo();
                                currListing.PricingInfo.PurchaseOptions = new List<PurchaseOption>();
                                purchaseOptions = currListing.PricingInfo.PurchaseOptions;


                                currListing.ProfileId = job.ProfileId;
                                currListing.AppId = job.AppId;
                                currListing.ListingType = "Product";

                                variants = dataLine[4].Split(',');

                                variantProperties = new Dictionary<string, string>();
                                foreach (string variation in variants)
                                {
                                    switch (variation.ToLower())
                                    {
                                        case "color":
                                            variantProperties.Add("Color", null);
                                            break;
                                        case "size":
                                            variantProperties.Add("Size", null);
                                            break;
                                        case "design":
                                            variantProperties.Add("Design", null);
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
                                    currListing.Metadata = new Dictionary<string, string>();

                                currListing.Metadata.Add("Style", dataLine[10]);
                                currListing.Metadata.Add("Width", dataLine[21]);
                                currListing.Metadata.Add("Depth", dataLine[22]);
                                currListing.Metadata.Add("Height", dataLine[23]);
                                currListing.Metadata.Add("Materials", dataLine[24]);
                                currListing.Metadata.Add("Manufacturer", dataLine[25]);
                                currListing.Metadata.Add("Designer", dataLine[26]);
                                currListing.Metadata.Add("ProductUrl", dataLine[7]);

                                string[] keywords = dataLine[36].Split(',');

                                if (currListing.SearchableKeywords == null)
                                    currListing.SearchableKeywords = new List<string>();

                                foreach (string keyword in keywords)
                                {
                                    if (keyword.Length > 0)
                                        currListing.SearchableKeywords.Add(keyword);
                                }


                                // special treatment of childless listings
                                if (dataLine[2].ToLower().Trim().Equals(""))
                                {
                                    purchaseOption = new PurchaseOption();
                                    if (purchaseOption.VariantProperties.Keys.Contains("Color"))
                                        purchaseOption.VariantProperties["Color"] = dataLine[18];
                                    if (purchaseOption.VariantProperties.Keys.Contains("Size"))
                                        purchaseOption.VariantProperties["Size"] = dataLine[19];
                                    if (purchaseOption.VariantProperties.Keys.Contains("Design"))
                                        purchaseOption.VariantProperties["Design"] = dataLine[20];

                                    purchaseOption.Title = dataLine[6];

                                    if (dataLine[7].Length > 0)
                                        purchaseOption.ProductUrl = dataLine[7];

                                    purchaseOption.SKU = dataLine[0];

                                    purchaseOption.Quantity = int.Parse(dataLine[11]);
                                    purchaseOption.Price = double.Parse(dataLine[12]);
                                    purchaseOption.NeutralPrice = purchaseOption.Price * _currencyManager.GetRate(currencyCode, "USD", 0);
                                }

                            }

                            else if (dataLine[2].ToLower().Equals("child"))
                            {
                                if (skipToNextParent)
                                {
                                    continue;
                                }

                                purchaseOption = new PurchaseOption();
                                purchaseOptions = activeListing.PricingInfo.PurchaseOptions;

                                purchaseOption.VariantProperties = variantProperties;

                                if (purchaseOption.VariantProperties.Keys.Contains("Color"))
                                    purchaseOption.VariantProperties["Color"] = dataLine[18];
                                if (purchaseOption.VariantProperties.Keys.Contains("Size"))
                                    purchaseOption.VariantProperties["Size"] = dataLine[19];
                                if (purchaseOption.VariantProperties.Keys.Contains("Design"))
                                    purchaseOption.VariantProperties["Design"] = dataLine[20];



                                //child title.
                                purchaseOption.Title = dataLine[6];

                                if (dataLine[7].Length > 0)
                                    purchaseOption.ProductUrl = dataLine[7];

                                purchaseOption.SKU = dataLine[0];

                                purchaseOption.Quantity = int.Parse(dataLine[11]);
                                purchaseOption.Price = double.Parse(dataLine[12]);
                                purchaseOption.NeutralPrice = purchaseOption.Price * _currencyManager.GetRate(currencyCode, "USD", 0);
                            }

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

                                    var client = new WebClient();
                                    byte[] img = null;

                                    try
                                    {
                                        img = client.DownloadData(mf.Url);
                                    }
                                    catch (Exception)
                                    {
                                        client.Dispose();
                                        throw new Exception("Error downloading image: " + mf.Url + ", line # " + lineNum + " : " + currLine);
                                    }
                                    client.Dispose();

                                    try
                                    {
                                        _storageRepository.SaveFileSync(mf.Key, img, mf.ContentType);
                                    }
                                    catch (Exception)
                                    {
                                        throw new Exception("Error saving image: " + mf.Url + ", line# " + lineNum + " : " + currLine);
                                    }
                                }
                            }


                            if (dataLine[2].ToLower().Equals("parent") || dataLine[2].Trim().Equals(""))
                            {
                                currListing.ExternalMedia = tmpList.ToArray();
                            }
                            else if (dataLine[2].ToLower().Equals("child"))
                            {
                                purchaseOption.MediaFiles = tmpList.ToArray();
                                purchaseOptions.Add(purchaseOption);
                                activeListing.PricingInfo.PurchaseOptions = purchaseOptions;
                            }



                            if (dataLine[2].ToLower().Equals("parent") || dataLine[2].Trim().Equals(""))
                            {
                                activeListing = currListing;
                            }

                        }

                        lineNum++;
                    }

                    // persist last product.
                    if (activeListing != null)
                    {
                        // persist the last listing and create a new one.
                        activeListing.IsPublished = true;

                        try
                        {
                            _listingRepository.Insert(activeListing);
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("Error persisting listing: " + activeListing.Id + ". Error: " + ex.Message);
                        }

                        try
                        {
                            job.ProgressInfo = ++numProductsSaved + " products saved, " + numErrors + " errors encountered";
                            _jobRepository.Save(job);

                            job.Status = "Finished";
                            _jobRepository.Save(job);
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("Error saving job status, error was : " + ex.Message);
                        }

                        activeListing = null;
                    }
                }
                catch (Exception exx)
                {
                    skipToNextParent = true;
                    ++numErrors;
                    ReportError(exx, job, _jobRepository, numProductsSaved, numErrors);

                }
            }
        }
    }
}