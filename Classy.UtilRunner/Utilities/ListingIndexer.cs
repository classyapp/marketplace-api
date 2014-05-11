using System.Collections.Generic;
using System.Linq;
using Classy.Interfaces.Search;
using Classy.Models;
using Classy.Models.Search;
using Funq;
using MongoDB.Driver;
using Nest;

namespace Classy.UtilRunner.Utilities
{
    public class ListingIndexer : IUtility
    {
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

            var toIndex = new List<ListingIndexDto>();

            var cursor = _listings.FindAll().SetLimit(100);
            foreach (var listing in cursor)
            {
                toIndex.Add(new ListingIndexDto
                {
                    AddToCollectionCount = listing.AddToCollectionCount,
                    BookingCount = listing.BookingCount,
                    ClickCount = listing.ClickCount,
                    CommentCount = listing.CommentCount,
                    Content = listing.Content,
                    FavoriteCount = listing.FavoriteCount,
                    FlagCount = listing.FlagCount,
                    Keywords = listing.SearchableKeywords != null ? listing.SearchableKeywords.ToArray() : new string[0],
                    ListingType = listing.ListingType,
                    Metadata = (Dictionary<string, string>)listing.Metadata,
                    PurchaseCount = listing.PurchaseCount,
                    Title = listing.Title,
                    ViewCount = listing.ViewCount
                });
            }

            client.IndexMany(toIndex.ToArray());

            return StatusCode.Success;
        }
    }
}