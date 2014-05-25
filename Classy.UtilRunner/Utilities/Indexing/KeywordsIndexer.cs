using System;
using System.Collections.Generic;
using System.Linq;
using classy.Extentions;
using Classy.Interfaces.Search;
using Classy.Models;
using Classy.Models.Keywords;
using Funq;
using MongoDB.Driver;
using ServiceStack.ServiceModel.Serialization;

namespace Classy.UtilRunner.Utilities.Indexing
{
    public class KeywordsIndexer : IUtility
    {
        private const int BatchSize = 300;
        private readonly MongoCollection<Listing> _listings;
        private readonly ISearchClientFactory _searchClientFactory;
        
        public KeywordsIndexer(Container container)
        {
            _searchClientFactory = container.Resolve<ISearchClientFactory>();
            var mongoDatabase = container.Resolve<MongoDatabase>();
            _listings = mongoDatabase.GetCollection<Listing>("classifieds");
        }

        public StatusCode Run(string[] args)
        {
            var client = _searchClientFactory.GetClient(null, null);
            client.CreateIndex("keywords_v1.0");
            client = _searchClientFactory.GetClient("keywords", "v1.0");
            client.Map<KeywordIndexDto>(m => m.MapFromAttributes());
            
            var keywords = new Dictionary<string, int>();

            var i = 0;
            var cursor = _listings.FindAll().SetBatchSize(BatchSize);
            foreach (var listingsBulk in cursor.Bulks(BatchSize))
            {
                foreach (var listing in listingsBulk)
                {
                    if (listing.TranslatedKeywords == null || !listing.TranslatedKeywords.Any())
                        continue;

                    foreach (var language in listing.TranslatedKeywords)
                    {
                        foreach (var word in listing.TranslatedKeywords[language.Key])
                        {
                            var key = word + "__" + language.Key;
                            if (!keywords.ContainsKey(key))
                                keywords.Add(key, 0);

                            keywords[key] = keywords[key] + 1;
                        }
                    }
                }

                Console.WriteLine("Indexed {0} documents", ++i * BatchSize);
            }

            foreach (var keyword in keywords)
            {
                client.Index(new KeywordIndexDto
                {
                    Keyword = keyword.Key.Substring(0, keyword.Key.IndexOf("__")),
                    Language = keyword.Key.Substring(keyword.Key.IndexOf("__") + 2),
                    Count = keyword.Value
                });
            }

            return StatusCode.Success;
        }
    }
}