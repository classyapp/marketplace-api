using System;
using System.Collections.Generic;
using System.IO;
using Amazon.DataPipeline.Model;
using Classy.Models;
using Funq;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace Classy.UtilRunner.Utilities.Poll
{
    public class PollBatchCreator : IUtility
    {
        private readonly MongoCollection<Collection> _collections;
        private readonly MongoCollection<Listing> _listings;

        public PollBatchCreator(Container container)
        {
            var mongoDatabase = container.Resolve<MongoDatabase>();
            _collections = mongoDatabase.GetCollection<Collection>("collections");
            _listings = mongoDatabase.GetCollection<Listing>("classifieds");
        }

        public StatusCode Run(string[] args)
        {
            var rand = new Random(88);

            using (var reader = new StreamReader("C:\\Users\\Gilly\\Downloads\\polls.csv"))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var data = line.Split(',');

                    var collectionId = data[0];
                    var collection = _collections.FindOne(Query<Collection>.EQ(x => x.Id, collectionId));

                    var translations = new Dictionary<string, ListingTranslation> {
                        {"nl", new ListingTranslation {Culture = "nl", Title = data[5], Content = data[6]}},
                        {"fr", new ListingTranslation {Culture = "fr", Title = data[7], Content = data[8]}},
                        {"he", new ListingTranslation {Culture = "he", Title = data[9], Content = data[10]}},
                        {"en", new ListingTranslation {Culture = "en", Title = data[11], Content = data[12]}}
                    };

                    var numVotes = rand.Next(20, 80);
                    var metadata = new Dictionary<string, string> {
                        { "Batch", "1" }
                    };
                    var i = 0;
                    foreach (var listing in collection.IncludedListings)
                    {
                        metadata.Add("Listing_" + i, listing.Id);
                        metadata.Add("Vote_" + i, (numVotes + rand.Next(6)).ToString());

                        i++;
                    }

                    var poll = new Listing {
                        AppId = "v1.0",
                        Created = DateTime.Now,
                        IsPublished = true,
                        ListingType = "Poll",
                        Translations = translations,
                        Metadata = metadata,
                        Title = data[11],
                        Content = data[12],
                        ProfileId = collection.ProfileId,
                        ViewCount = numVotes + rand.Next(50),
                        FavoriteCount = rand.Next(20),
                        ContactInfo = new ContactInfo {
                            Email = "",
                            Location = new Location {
                                Address = new PhysicalAddress {
                                    Country = "FR"
                                },
                                Coords = new Coords {
                                    Latitude = 0,
                                    Longitude = 0
                                }
                            }
                        },
                        ExternalMedia = new MediaFile[0]
                    };

                    _listings.Insert(poll);
                }
            }

            return StatusCode.Success;
        }
    }
}