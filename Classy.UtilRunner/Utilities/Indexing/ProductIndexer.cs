using System;
using System.Collections.Generic;
using System.Linq;
using classy.Extentions;
using Classy.Interfaces.Search;
using Classy.Models;
using Classy.Models.Search;
using Funq;
using MongoDB.Driver;

namespace Classy.UtilRunner.Utilities.Indexing
{
    public class ProductIndexer : IUtility
    {
        private const int BatchSize = 100;
        private readonly MongoCollection<Listing> _listings;
        private readonly ISearchClientFactory _searchClientFactory;

        public ProductIndexer(Container container)
        {
            _searchClientFactory = container.Resolve<ISearchClientFactory>();
            var mongoDatabase = container.Resolve<MongoDatabase>();
            _listings = mongoDatabase.GetCollection<Listing>("classifieds");
        }
        
        public StatusCode Run(string[] args)
        {
            var indexName = "listings" + "_v1.0";
            
            var client = _searchClientFactory.GetClient("listings", "v1.0");
            //client.Map<ListingIndexDto>(m => m.MapFromAttributes());
                
            var i = 0;
            var cursor = _listings.FindAll().SetBatchSize(BatchSize);
            foreach (var listingsBulk in cursor.Bulks(BatchSize))
            {
                var toIndex = new List<ListingIndexDto>(BatchSize);

                foreach (var listing in listingsBulk)
                {
                    if (listing.ListingType != "Product")
                        continue;

                    string[] metadata;
                    if (!listing.Metadata.IsNullOrEmpty())
                        metadata = listing.Metadata.Select(x => x.Value).ToArray();
                    else 
                        metadata = new string[0];

                    var price = GetProductPrice(listing);

                    toIndex.Add(new ListingIndexDto
                    {
                        Id = listing.Id,
                        AddToCollectionCount = listing.AddToCollectionCount,
                        BookingCount = listing.BookingCount,
                        ClickCount = listing.ClickCount,
                        CommentCount = listing.CommentCount,
                        Content = listing.Content,
                        FavoriteCount = listing.FavoriteCount,
                        FlagCount = listing.FlagCount,
                        EditorRank = listing.EditorsRank,
                        Categories = listing.Categories.ToArray(),
                        AnalyzedCategories = listing.Categories.ToArray(),
                        Keywords =
                            listing.SearchableKeywords != null ? listing.SearchableKeywords.Union(listing.Hashtags).ToArray() : listing.Hashtags.EmptyIfNull().ToArray(),
                        ImageUrl = 
                            !listing.ExternalMedia.IsNullOrEmpty() ? listing.ExternalMedia[0].Url : null,
                        ListingType = listing.ListingType,
                        Metadata = metadata,
                        PurchaseCount = listing.PurchaseCount,
                        Title = listing.Title,
                        AnalyzedTitle = listing.Title,
                        ViewCount = listing.ViewCount,
                        Price = price
                    });
                }

                if (toIndex.Any()) client.IndexMany(toIndex);
                
                Console.WriteLine("Indexed {0} documents", ++i*BatchSize);
            }

            return StatusCode.Success;
        }

        public static decimal GetProductPrice(Listing listing)
        {
            if (listing.ListingType != "Product") return 0;

            if (listing.PricingInfo == null) return 0;
            if (listing.PricingInfo.BaseOption != null)
                return listing.PricingInfo.BaseOption.Price;

            if (listing.PricingInfo.PurchaseOptions.Any())
                return listing.PricingInfo.PurchaseOptions[0].Price;

            return 0;
        }
    }
}