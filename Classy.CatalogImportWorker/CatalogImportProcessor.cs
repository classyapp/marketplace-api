﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using classy.Manager;
using Classy.Models;
using Classy.Repository;
using Classy.Interfaces.Managers;
using System.Net;
using System.Threading;

namespace Classy.CatalogImportWorker
{
    public class CatalogImportProcessor
    {
        public enum Columns
        {
            SKU_0,
            ParentSKU_1,
            Parantage_2,
            RelationshipType_3,
            VariationTheme_4,
            UPC_5,
            Title_6,
            ProductUrl_7,
            Description_8,
            Category_9,
            Style_10,
            Quantity_11,
            Price_12,
            MSRP_13,
            StandardShipping_14,
            ExpeditedShipping_15,
            LeadTimeMin_16,
            LeadTimeMax_17,
            Color_18,
            Size_19,
            Design_20,
            Width_21,
            Depth_22,
            Height_23,
            Materials_24,
            Manufacturer_25,
            Designer_26,
            Image_27,
            Image2_28,
            Image3_29,
            Image4_30,
            Image5_31,
            BulkItem_32,
            BulkCurbsideShipping_33,
            BulkInsideShipping_34,
            Configuration_35,
            Keywords_36,
            All
        }

        private readonly IStorageRepository _storageRepository; // AWS
        private readonly IListingRepository _listingRepository; //MONGO
        private readonly IJobRepository _jobRepository; // Jobs
        private readonly ICurrencyManager _currencyManager; // Currencies
        private readonly IProfileRepository _profileRepository; // Profiles
        private readonly IAppManager _appManager;

        private App _app;

        public CatalogImportProcessor(IStorageRepository storageRepo, IListingRepository listingRepo, IJobRepository jobRepo,
            ICurrencyManager currencyManager, IProfileRepository profileRepository, IAppManager appManager)
        {
            _storageRepository = storageRepo;
            _listingRepository = listingRepo;
            _jobRepository = jobRepo;
            _currencyManager = currencyManager;
            _profileRepository = profileRepository;
            _appManager = appManager;
        }

        public void Process(Job job)
        {
            // delete all products with images
            //var _products = _listingRepository.GetByProfileId(job.AppId, "172", true, null);
            //foreach (var _listing in _products)
            //{
            //    if (_listing.ExternalMedia != null)
            //    {
            //        foreach (var f in _listing.ExternalMedia)
            //        {
            //            _storageRepository.DeleteFile(f.Key);
            //        }
            //    }
            //    if (_listing.ListingType == "Product" && _listing.PricingInfo.PurchaseOptions != null)
            //    {
            //        foreach (var po in _listing.PricingInfo.PurchaseOptions)
            //        {
            //            foreach (var f in _listing.ExternalMedia)
            //            {
            //                _storageRepository.DeleteFile(f.Key);
            //            }
            //        }
            //    }
            //    _listingRepository.Delete(_listing.Id, _listing.AppId);
            //}

            // Get CSV from job
            List<List<string>> productsData = GetProductsData(job);
            Profile profile = _profileRepository.GetById(job.AppId, job.ProfileId, false, null);
            string currencCode = (string)job.Properties["CurrencyCode"];
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(profile.DefaultCulture);
            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(profile.DefaultCulture);

            if (productsData == null)
            {
                job.Status = "Finished";
                job.Errors = new List<string>();
                job.Errors.Add("Invalid or missing products data.");
                _jobRepository.Save(job);
            }
            else
            {
                _app = _appManager.GetAppById(job.AppId);

                job.Status = "Executing";
                _jobRepository.Save(job);

                foreach (var productData in productsData)
                {
                    try
                    {
                        // Validate data
                        ValidateProductData(productData);

                        // Build listing
                        Listing product = BuildListing(productData, job, profile.DefaultCulture, currencCode);

                        // Save images
                        UploadProductImages(product);

                        // Save listing
                        _listingRepository.Insert(product);
                        job.Succeeded++;
                    }
                    catch (ImportException ex)
                    {
                        productData[ex.Line] += string.Format(";{0}", ex.Message);
                        job.Errors.AddRange(productData);
                        job.Failed++;
                    }
                    finally
                    {
                        _jobRepository.Save(job);
                    }
                }
                job.Status = "Finished";
                _jobRepository.Save(job);
            }
        }

        private List<List<string>> GetProductsData(Job job)
        {
            if (job.Attachments.Length == 0 || string.IsNullOrEmpty(job.Attachments[0].Key))
                return null;

            // Download csv
            Stream stream = null;
            string[] allLines = null;
            try
            {
                stream = _storageRepository.GetFile(job.Attachments[0].Key);
                using (StreamReader rd = new StreamReader(stream))
                {
                    allLines = rd.ReadToEnd().Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                }
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                    stream.Dispose();
                }
            }

            if (allLines.Length <= 1)
                return null;

            List<List<string>> data = new List<List<string>>();
            List<string> currentProduct = new List<string>();
            for (int i = 1; i < allLines.Length; i++)
            {
                string[] lineData = allLines[i].Split(';');
                if (string.IsNullOrWhiteSpace(lineData[(int)Columns.Parantage_2]) || lineData[(int)Columns.Parantage_2].Equals("Parent", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (currentProduct.Count > 0)
                    {
                        data.Add(currentProduct);
                        currentProduct = new List<string>();
                    }
                }
                currentProduct.Add(allLines[i]);
            }
            data.Add(currentProduct);

            return data;
        }

        private void ValidateProductData(List<string> productData)
        {
            if (productData.Count == 1) // Standalone Product
            {
                // validate mandatory fields
                ValidateMandatoryFields(productData[0], new Columns[] { 
                    Columns.SKU_0, Columns.Title_6, Columns.ProductUrl_7, Columns.Description_8, Columns.Category_9, Columns.Style_10, Columns.Quantity_11, 
                    Columns.Price_12, Columns.Width_21, Columns.Depth_22, Columns.Height_23, Columns.Image_27
                }, 0);
                ValidateStyle(productData[0]);
                ValidateCategories(productData[0]);
            }
            else
            {
                // validate parent mandatory fields
                ValidateMandatoryFields(productData[0], new Columns[] { 
                    Columns.SKU_0, Columns.Title_6, Columns.ProductUrl_7, Columns.Description_8, Columns.Category_9, Columns.Style_10, Columns.Image_27
                }, 0);
                ValidateStyle(productData[0]);
                ValidateCategories(productData[0]);

                Dictionary<string, int> variationKeys = new Dictionary<string, int>();
                for (int i = 1; i < productData.Count; i++)
                {
                    ValidateMandatoryFields(productData[i], new Columns[] { 
                        Columns.SKU_0, Columns.ParentSKU_1, Columns.VariationTheme_4, Columns.ProductUrl_7, Columns.Description_8, Columns.Quantity_11, 
                        Columns.Price_12, Columns.Width_21, Columns.Depth_22, Columns.Height_23, Columns.Image_27
                    }, i);

                    // check variations are supplied
                    string[] productFields = productData[i].Split(';');
                    string[] variations = productFields[(int)Columns.VariationTheme_4].Split(',');
                    List<string> values = new List<string>();
                    CheckVariationField(i, productFields, variations, values, "Color", (int)Columns.Color_18);
                    CheckVariationField(i, productFields, variations, values, "Design", (int)Columns.Design_20);
                    CheckVariationField(i, productFields, variations, values, "Size", (int)Columns.Size_19);
                    string variationKey = string.Join(";", values.ToArray());
                    if (variationKeys.ContainsKey(variationKey))
                    {
                        throw new ImportException("Duplicate variation detected", i);
                    }
                    else
                    {
                        variationKeys.Add(variationKey, 0);
                    }
                }
            }
        }

        private void ValidateCategories(string productData)
        {
            string[] categories = productData.Split(';')[(int)Columns.Category_9].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var category in categories)
            {
                if (!_app.ProductCategories.Any(pc => pc.Value == category))
                    throw new ImportException(string.Format("Invalid category: {0}", category), 0);
            }
        }

        private void ValidateStyle(string productData)
        {
            string style = productData.Split(';')[(int)Columns.Style_10];
            if (!_app.Styles.Any(st => st.Value == style))
                throw new ImportException(string.Format("Invalid style: {0}", style), 0);
        }

        private static void CheckVariationField(int lineIdx, string[] productFields, string[] variations, List<string> values, string variation, int fieldIdx)
        {
            if (variations.Any(v => v.Equals(variation, StringComparison.InvariantCultureIgnoreCase)))
            {
                values.Add(variation);
                if (string.IsNullOrEmpty(productFields[fieldIdx]))
                {
                    throw new ImportException("Missing variation theme field: " + variation, lineIdx);
                }
                values.Add(productFields[fieldIdx]);
            }
        }

        private void ValidateMandatoryFields(string data, Columns[] columns, int lineIdx)
        {
            foreach (var column in columns)
            {
                string[] lineData = data.Split(';');
                if (string.IsNullOrWhiteSpace(lineData[(int)column]))
                    throw new ImportException("One or more mandatory fields are missing", lineIdx);
            }
        }

        private Listing BuildListing(List<string> productData, Job job, string culture, string currency)
        {
            Listing listing = new Listing();
            listing.ListingType = "Product";
            listing.AppId = job.AppId;
            listing.ProfileId = job.ProfileId;
            listing.DefaultCulture = culture;
            listing.PricingInfo = new PricingInfo() { CurrencyCode = currency };
            listing.IsPublished = true;
            listing.Metadata.Add("JobId", job.Id);

            string[] productFields = productData[0].Split(';');

            listing.Title = productFields[(int)Columns.Title_6];
            listing.Content = productFields[(int)Columns.Description_8];
            listing.Categories = productFields[(int)Columns.Category_9].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            //listing.SearchableKeywords = productFields[(int)Columns.Keywords_36].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            listing.Metadata.Add("Style", productFields[(int)Columns.Style_10]);

            if (!string.IsNullOrEmpty(productFields[(int)Columns.Materials_24]))
                listing.Metadata.Add("Materials", productFields[(int)Columns.Materials_24]);
            if (!string.IsNullOrEmpty(productFields[(int)Columns.Designer_26]))
                listing.Metadata.Add("Designer", productFields[(int)Columns.Designer_26]);
            if (!string.IsNullOrEmpty(productFields[(int)Columns.Manufacturer_25]))
                listing.Metadata.Add("Manufacturer", productFields[(int)Columns.Manufacturer_25]);
            if (!string.IsNullOrEmpty(productFields[(int)Columns.ProductUrl_7]))
                listing.Metadata.Add("ProductUrl", productFields[(int)Columns.ProductUrl_7]);

            // Images
            for (int i = (int)Columns.Image_27; i <= (int)Columns.Image5_31; i++)
            {
                string url = productFields[i];
                if (!string.IsNullOrWhiteSpace(url))
                {
                    listing.ExternalMedia.Add(new MediaFile { ContentType = "image/jpeg", Key = Guid.NewGuid().ToString(), Type = MediaFileType.Image, Url = url });
                }
            }

            if (productData.Count == 1)
            {
                // Standalone product
                listing.PricingInfo.BaseOption = new PurchaseOption();
                listing.PricingInfo.BaseOption.SKU = productFields[(int)Columns.SKU_0];
                listing.PricingInfo.BaseOption.Title = productFields[(int)Columns.Title_6];
                listing.PricingInfo.BaseOption.ProductUrl = productFields[(int)Columns.ProductUrl_7];
                listing.PricingInfo.BaseOption.Content = productFields[(int)Columns.Description_8];
                listing.PricingInfo.BaseOption.Quantity = double.Parse(productFields[(int)Columns.Quantity_11]);
                listing.PricingInfo.BaseOption.Price = double.Parse(productFields[(int)Columns.Price_12]);
                if (!string.IsNullOrWhiteSpace(productFields[(int)Columns.MSRP_13]))
                    listing.PricingInfo.BaseOption.CompareAtPrice = double.Parse(productFields[(int)Columns.MSRP_13]);
                listing.PricingInfo.BaseOption.Width = productFields[(int)Columns.Width_21];
                listing.PricingInfo.BaseOption.Depth = productFields[(int)Columns.Depth_22];
                listing.PricingInfo.BaseOption.Height = productFields[(int)Columns.Height_23];
                listing.PricingInfo.BaseOption.NeutralPrice = _currencyManager.GetRate(currency, "USD", 0) * listing.PricingInfo.BaseOption.Price;
            }
            else
            {
                listing.PricingInfo.PurchaseOptions = new List<PurchaseOption>();
                listing.PricingInfo.BaseOption = new PurchaseOption();

                for (int i = 0; i < productData.Count; i++)
                {
                    if (i == 0) // Base product data
                    {
                        listing.PricingInfo.BaseOption.SKU = productFields[(int)Columns.SKU_0];
                        if (!string.IsNullOrWhiteSpace(productFields[(int)Columns.Price_12]))
                            listing.PricingInfo.BaseOption.Price = double.Parse(productFields[(int)Columns.Price_12]);
                        if (!string.IsNullOrWhiteSpace(productFields[(int)Columns.MSRP_13]))
                            listing.PricingInfo.BaseOption.CompareAtPrice = double.Parse(productFields[(int)Columns.MSRP_13]);
                        if (!string.IsNullOrWhiteSpace(productFields[(int)Columns.Width_21]))
                            listing.PricingInfo.BaseOption.Width = productFields[(int)Columns.Width_21];
                        if (!string.IsNullOrWhiteSpace(productFields[(int)Columns.Depth_22]))
                            listing.PricingInfo.BaseOption.Depth = productFields[(int)Columns.Depth_22];
                        if (!string.IsNullOrWhiteSpace(productFields[(int)Columns.Height_23]))
                            listing.PricingInfo.BaseOption.Height = productFields[(int)Columns.Height_23];
                        listing.PricingInfo.BaseOption.NeutralPrice = _currencyManager.GetRate(currency, "USD", 0) * listing.PricingInfo.BaseOption.Price;
                    }
                    else
                    {
                        PurchaseOption option = new PurchaseOption();
                        option.SKU = productFields[(int)Columns.SKU_0];
                        option.Title = productFields[(int)Columns.Title_6];
                        option.ProductUrl = productFields[(int)Columns.ProductUrl_7];
                        option.Content = productFields[(int)Columns.Description_8];
                        option.Price = double.Parse(productFields[(int)Columns.Price_12]);
                        if (!string.IsNullOrWhiteSpace(productFields[(int)Columns.MSRP_13]))
                            option.CompareAtPrice = double.Parse(productFields[(int)Columns.MSRP_13]);
                        option.Width = productFields[(int)Columns.Width_21];
                        option.Depth = productFields[(int)Columns.Depth_22];
                        option.Height = productFields[(int)Columns.Height_23];
                        option.NeutralPrice = _currencyManager.GetRate(currency, "USD", 0) * listing.PricingInfo.BaseOption.Price;
                        listing.PricingInfo.PurchaseOptions.Add(option);

                        // Variants
                        option.VariantProperties = new Dictionary<string, string>();
                        if (productFields[(int)Columns.VariationTheme_4].Contains("color"))
                            option.VariantProperties.Add("color", productFields[(int)Columns.Color_18]);
                        if (productFields[(int)Columns.VariationTheme_4].Contains("design"))
                            option.VariantProperties.Add("design", productFields[(int)Columns.Design_20]);
                        if (productFields[(int)Columns.VariationTheme_4].Contains("size"))
                            option.VariantProperties.Add("size", productFields[(int)Columns.Size_19]);

                        // Images
                        List<MediaFile> mediaFiles = new List<MediaFile>();
                        for (int j = (int)Columns.Image_27; j <= (int)Columns.Image5_31; j++)
                        {
                            string url = productFields[j];
                            if (!string.IsNullOrWhiteSpace(url))
                            {
                                mediaFiles.Add(new MediaFile { ContentType = "image/jpeg", Key = Guid.NewGuid().ToString(), Type = MediaFileType.Image, Url = url });
                            }
                        }
                        option.MediaFiles = mediaFiles.ToArray();
                    }
                }
            }

            return listing;
        }

        private void UploadProductImages(Listing product)
        {
            for (int i = 0; i < product.ExternalMedia.Count; i++)
            {
                SaveImageFromUrl(product.ExternalMedia[i].Url, product.ExternalMedia[i].Key, i);
            }

            // Upload variation images
            if (product.PricingInfo.PurchaseOptions != null)
            {
                for (int i = 0; i < product.PricingInfo.PurchaseOptions.Count; i++)
                {
                    for (int j = 0; j < product.PricingInfo.PurchaseOptions[i].MediaFiles.Length; j++)
                    {
                        SaveImageFromUrl(product.PricingInfo.PurchaseOptions[i].MediaFiles[j].Url, product.PricingInfo.PurchaseOptions[i].MediaFiles[j].Key, i + 1);
                    }
                }
            }
        }

        private void SaveImageFromUrl(string url, string key, int i)
        {
            WebClientWithTimeout wc = null;
            byte[] content;
            bool errorOccured = false;

            try
            {
                wc = new WebClientWithTimeout(30000);
                content = wc.DownloadData(url);
                _storageRepository.SaveFileSync(key, content, "image/jpeg");
                // Rescale??
            }
            catch (Exception)
            {
                errorOccured = true;
            }
            finally
            {
                wc.Dispose();
            }

            if (errorOccured)
            {
                throw new ImportException(string.Format("Error occured while trying to save image {0}", url), 0);
            }
        }
    }
}