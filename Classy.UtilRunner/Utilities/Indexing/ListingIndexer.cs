using System;
using System.Collections.Generic;
using System.Linq;
using classy.Extentions;
using Classy.Interfaces.Search;
using Classy.Models;
using Classy.Models.Search;
using Funq;
using MongoDB.Bson.Serialization;
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
            BsonClassMap.RegisterClassMap<Listing>(c => c.AutoMap());
            _listings = mongoDatabase.GetCollection<Listing>("classifieds");
        }
        
        public StatusCode Run(string[] args)
        {
            var indexName = "listings" + "_v1.0";

            //var client = _searchClientFactory.GetClient(null, null);
            //client.DeleteIndex(x => x.Index<ListingIndexDto>().Index(indexName));

            // We currently need to create the index with the settings manually,
            // since it doesn't work fluently.
            // For this we need to use this json :
            //  {"settings":
            //{
            //        "analysis": {
            //            "filter": {
            //                "suggest_filter": {
            //                    "type":     "ngram",
            //                    "min_gram": 2,
            //                    "max_gram": 20,
            //                    "side": "front"
            //                }
            //            },
            //          "analyzer": {
            //            "suggest_analyzer": {
            //              "type":"custom",
            //              "tokenizer":"standard",
            //              "filter": ["lowercase","standard","asciifolding","suggest_filter"] 
            //            }
            //          }
            //        }
            //    }
            //}
            //client.CreateIndex("listings_v1.0",
            //    x => x.Settings(s => s
            //        .Add("merge.policy.merge_factor", "10")));
            //        // adding the token filter
            //        //.Add("index.analysis.filter.suggest_filter.type", "ngram")
            //        //.Add("index.analysis.filter.suggest_filter.min_gram", "1")
            //        //.Add("index.analysis.filter.suggest_filter.max_gram", "20")
            //        //.Add("index.analysis.filter.suggest_filter.side", "front")
            //        //// adding the analyzer that uses the token filter
            //        //.Add("index.analysis.analyzer.suggest_analyzer.type", "custom")
            //        //.Add("index.analysis.analyzer.suggest_analyzer.tokenizer", "standard")
            //        //.Add("index.analysis.analyzer.suggest_analyzer.filter", "[\"lowercase\", \"standard\", \"asciifolding\", \"suggest_filter\"]")));

            //client.CreateIndex("listings_v1.0", x => x.Settings(c => new IndexSettings()));

            var client = _searchClientFactory.GetClient("listings", "v1.0");
            client.Map<ListingIndexDto>(m => m.MapFromAttributes());
                //.Properties(p => p.String(s => s.Name(n => n.Title).IndexAnalyzer("suggest_analyzer"))));

            // This is for using the elasticsearch suggest endpoint
            //client = _searchClientFactory.GetClient("listings", "v1.0");
            //client.Map<ListingIndexDto>(
            //    m => m.MapFromAttributes().Properties(
            //        p => p.Completion(
            //            c => c.Name(n => n.Title).IndexAnalyzer("simple").SearchAnalyzer("simple")
            //                .Payloads(true).MaxInputLength(20))
            //            .Completion(
            //            c => c.Name(n => n.Metadata).IndexAnalyzer("simple").SearchAnalyzer("simple")
            //                .Payloads(true).MaxInputLength(20))));

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
                            listing.SearchableKeywords != null ? listing.SearchableKeywords.Union(listing.Hashtags).ToArray() : new string[0],
                        ImageUrl = 
                            !listing.ExternalMedia.IsNullOrEmpty() ? listing.ExternalMedia[0].Url : null,
                        ListingType = listing.ListingType,
                        Metadata = metadata,
                        PurchaseCount = listing.PurchaseCount,
                        Title = listing.Title,
                        AnalyzedTitle = listing.Title,
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