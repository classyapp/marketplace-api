using System;
using System.Collections.Generic;
using System.Linq;
using classy.Extentions;
using Classy.Interfaces.Search;
using Classy.Models;
using Classy.Models.Keywords;
using Funq;
using MongoDB.Driver;

namespace Classy.UtilRunner.Utilities.Indexing
{
    public class KeywordsIndexer : IUtility
    {
        private const int BatchSize = 300;
        private readonly MongoCollection<Listing> _listings;
        private readonly MongoCollection<Keyword> _keywords;
        
        public KeywordsIndexer(Container container)
        {
            var mongoDatabase = container.Resolve<MongoDatabase>();
            _listings = mongoDatabase.GetCollection<Listing>("classifieds");
            _keywords = mongoDatabase.GetCollection<Keyword>("keywords");
        }

        public StatusCode Run(string[] args)
        {
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
                _keywords.Insert(new Keyword {
                    Name = keyword.Key.Substring(0, keyword.Key.IndexOf("__")),
                    Language = keyword.Key.Substring(keyword.Key.IndexOf("__") + 2),
                    Count = keyword.Value
                });
            }

            return StatusCode.Success;
        }
    }
}