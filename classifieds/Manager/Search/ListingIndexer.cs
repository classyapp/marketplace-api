using System.Collections.Generic;
using System.Linq;
using classy.Extentions;
using Classy.Interfaces.Search;
using Classy.Models;
using Classy.Models.Search;
using Nest;

namespace classy.Manager.Search
{
    public class ListingIndexer : IIndexer<Listing>
    {
        private readonly ElasticClient _client;
        private readonly IndexingInfo _indexingInfo;

        public ListingIndexer(ISearchClientFactory searchClientFactory, IAppManager appManager)
        {
            _indexingInfo = appManager.GetIndexingInfo();
            _client = searchClientFactory.GetClient("listings");
        }

        public void Index(Listing[] entities)
        {
            var listingsToIndex = new List<ListingIndexDto>();
            foreach (var entity in entities)
            {
                if (!_indexingInfo.ListingTypes.Contains(entity.ListingType))
                    continue;

                listingsToIndex.Add(new ListingIndexDto {
                    AddToCollectionCount = entity.AddToCollectionCount,
                    BookingCount = entity.BookingCount,
                    ClickCount = entity.ClickCount,
                    CommentCount = entity.CommentCount,
                    Content = entity.Content,
                    FavoriteCount = entity.FavoriteCount,
                    FlagCount = entity.FlagCount,
                    Id = entity.Id,
                    ImageUrl = entity.ExternalMedia.IsNullOrEmpty() ? entity.ExternalMedia[0].Url : null,
                    Keywords = entity.SearchableKeywords.ToArray(),
                    ListingType = entity.ListingType,
                    PurchaseCount = entity.PurchaseCount,
                    Title = entity.Title,
                    ViewCount = entity.ViewCount,
                    Metadata = entity.Metadata
                        .Where(x => _indexingInfo.MetadataPerListing[entity.ListingType].Contains(x.Key))
                        .Select(x => x.Value).ToArray()
                });
            }
            _client.IndexMany(listingsToIndex);
        }
    }
}