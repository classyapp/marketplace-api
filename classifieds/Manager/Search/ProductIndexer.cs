using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using classy.Extentions;
using Classy.Interfaces.Search;
using Classy.Models;
using Classy.Models.Search;

namespace classy.Manager.Search
{
    public class ProductIndexer : IIndexer<Listing>
    {
        private readonly ISearchClientFactory _searchClientFactory;
        private readonly IAppManager _appManager;

        public ProductIndexer(ISearchClientFactory searchClientFactory, IAppManager appManager)
        {
            _searchClientFactory = searchClientFactory;
            _appManager = appManager;
        }

        public void RemoveFromIndex(Listing entity, string appId)
        {
            var client = _searchClientFactory.GetClient("products", appId);
            client.Delete<ListingIndexDto>(x => x.Id(entity.Id));
        }

        public void Index(Listing[] entities, string appId)
        {
            var client = _searchClientFactory.GetClient("products", appId);
            var indexingInfo = _appManager.GetAppById(appId).IndexingInfo ?? new IndexingInfo();

            var listingsToIndex = new List<ListingIndexDto>();
            foreach (var entity in entities)
            {
                if (!indexingInfo.ListingTypes.Contains(entity.ListingType))
                    continue;

                listingsToIndex.Add(new ListingIndexDto {
                    AddToCollectionCount = entity.AddToCollectionCount,
                    BookingCount = entity.BookingCount,
                    ClickCount = entity.ClickCount,
                    CommentCount = entity.CommentCount,
                    Content = entity.Content,
                    FavoriteCount = entity.FavoriteCount,
                    FlagCount = entity.FlagCount,
                    EditorRank = entity.EditorsRank,
                    Id = entity.Id,
                    ImageUrl = entity.ExternalMedia.IsNullOrEmpty() ? entity.ExternalMedia[0].Url : null,
                    Keywords = entity.SearchableKeywords.EmptyIfNull().ToArray(),
                    ListingType = entity.ListingType,
                    PurchaseCount = entity.PurchaseCount,
                    Title = entity.Title,
                    ViewCount = entity.ViewCount,
                    Metadata = entity.Metadata
                        .Where(x => indexingInfo.MetadataPerListing[entity.ListingType].Contains(x.Key))
                        .Select(x => x.Value).ToArray(),
                    Categories = entity.Categories.ToArray(),
                    AnalyzedCategories = entity.Categories.ToArray(),
                    Price = entity.PricingInfo != null && entity.PricingInfo.BaseOption != null ?
                        entity.PricingInfo.BaseOption.Price : 0m
                });
            }

            if (!listingsToIndex.IsNullOrEmpty())
                client.IndexMany(listingsToIndex);
        }

        public void Increment<T>(string id, string appId, Expression<Func<Listing, T>> property, int amount = 1)
        {
            var propertyName = ((MemberExpression) property.Body).Member.Name;
            var elasticPropertyName = Char.ToLowerInvariant(propertyName[0]) + propertyName.Substring(1);

            var script = string.Format("ctx._source.{0} += {1}", elasticPropertyName, amount);

            var client = _searchClientFactory.GetClient("products", appId);
            client.Update<ListingIndexDto>(d => d
                .Id(id)
                .Script(script));
        }

        public void Increment<T>(string[] ids, string appId, Expression<Func<Listing, T>> property, int amount = 1)
        {
            ids.ForEach(x => Increment(x, appId, property, amount));
        }
    }
}
