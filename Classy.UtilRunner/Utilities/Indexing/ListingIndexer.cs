using System;
using System.Collections.Generic;
using System.Linq;
using classy.Extentions;
using Classy.Interfaces.Search;
using Classy.Models;
using Classy.Models.Search;
using Funq;
using MongoDB.Driver;
using Nest;

namespace Classy.UtilRunner.Utilities.Indexing
{
    public class ListingIndexer : IUtility
    {
        private const int BatchSize = 100;
        private readonly MongoCollection<Listing> _listings;
        private readonly ISearchClientFactory _searchClientFactory;

        public ListingIndexer(Container container)
        {
            _searchClientFactory = container.Resolve<ISearchClientFactory>();
            var mongoDatabase = container.Resolve<MongoDatabase>();
            _listings = mongoDatabase.GetCollection<Listing>("classifieds");
        }
        
        public StatusCode Run(string[] args)
        {
            var client = _searchClientFactory.GetClient("listings");
            client.DeleteIndex<ListingIndexDto>();
            client.CreateIndex("listings", new IndexSettings());
            client.MapFromAttributes<ListingIndexDto>();

            client = _searchClientFactory.GetClient("listings");

            var i = 0;
            var cursor = _listings.FindAll().SetBatchSize(BatchSize);
            foreach (var listingsBulk in cursor.Bulks(BatchSize))
            {
                var toIndex = new List<ListingIndexDto>(BatchSize);

                foreach (var listing in listingsBulk)
                {
                    string[] metadata;
                    if (!listing.Metadata.IsNullOrEmpty())
                        metadata = listing.Metadata.Select(x => x.Value).ToArray();
                    else 
                        metadata = new string[0];

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
                        Keywords =
                            listing.SearchableKeywords != null ? listing.SearchableKeywords.ToArray() : new string[0],
                        ImageUrl = 
                            !listing.ExternalMedia.IsNullOrEmpty() ? listing.ExternalMedia[0].Url : null,
                        ListingType = listing.ListingType,
                        Metadata = metadata,
                        PurchaseCount = listing.PurchaseCount,
                        Title = listing.Title,
                        ViewCount = listing.ViewCount
                    });
                }

                client.IndexMany(toIndex);
                
                Console.WriteLine("Indexed {0} documents", ++i*BatchSize);
            }

            return StatusCode.Success;
        }
    }
}