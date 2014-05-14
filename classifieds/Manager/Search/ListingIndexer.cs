using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

        public void RemoveFromIndex(Listing entity)
        {
            _client.Delete(new ListingIndexDto {
                Id = entity.Id
            });
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

        public void Increment<T>(string id, Expression<Func<Listing, T>> property, int amount = 1)
        {
            var propertyName = ((MemberExpression) property.Body).Member.Name;
            var elasticPropertyName = Char.ToLowerInvariant(propertyName[0]) + propertyName.Substring(1);

            var script = string.Format("ctx._source.{0} += {1}", elasticPropertyName, amount);

            _client.Update<ListingIndexDto>(d => d
                .Id(id)
                .Script(script));
        }

        public void Increment<T>(string[] ids, Expression<Func<Listing, T>> property, int amount = 1)
        {
            ids.ForEach(x => Increment(x, property, amount));
        }
    }
}