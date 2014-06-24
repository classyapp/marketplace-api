using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using Amazon.DataPipeline.Model;
using Classy.Models;
using Funq;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Linq;

namespace Classy.UtilRunner.Utilities.Poll
{
    public class PollBatchCreator : IUtility
    {
        private readonly MongoCollection<Collection> _collections;
        private readonly MongoCollection<Listing> _listings;
        private readonly MongoCollection<Profile> _profiles;

        public PollBatchCreator(Container container)
        {
            var mongoDatabase = container.Resolve<MongoDatabase>();
            _collections = mongoDatabase.GetCollection<Collection>("collections");
            _listings = mongoDatabase.GetCollection<Listing>("classifieds");
            _profiles = mongoDatabase.GetCollection<Profile>("profiles");
        }

        public StatusCode Run(string[] args)
        {
            //var polls = _listings.Find(Query<Listing>.EQ(x => x.ListingType, "Poll"));

            //foreach (var poll in polls)
            //{
            //    var imageKeys = new List<string>(3);
            //    var j = 0;
            //    while (poll.Metadata.ContainsKey("Listing_" + j))
            //    {
            //        var listing = _listings.FindOne(Query<Listing>.EQ(x => x.Id, poll.Metadata["Listing_" + j]));
            //        imageKeys.Add(listing.ExternalMedia[0].Key);
            //        j++;
            //    }

            //    var url = "http://" + ConfigurationManager.AppSettings["CloudFrontDistributionUrl"]
            //        + "/collage?ImageKeys=" + string.Join(",", imageKeys) + "&format=json";

            //    using (var client = new WebClient())
            //    {
            //        try
            //        {
            //            var response = client.DownloadData(new Uri(url));
            //        }
            //        catch (Exception ex)
            //        {
                        
            //        }
            //    }
            //}

            //return StatusCode.Success;

            var rand = new Random(88);

            var j = 1;
            using (var reader = new StreamReader("C:\\Dev\\polls.csv"))
            {
                using (var writer = new StreamWriter("C:\\Dev\\polls_output.csv"))
                {
                    var poll = new Listing();
                    
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var data = line.Split(',');

                        var collectionLink = data[0];
                        var collectionId = collectionLink.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries)[3];
                        var collection = _collections.FindOne(Query<Collection>.EQ(x => x.Id, collectionId));

                        var translations = new Dictionary<string, ListingTranslation> {
                            {"nl", new ListingTranslation {Culture = "nl", Title = data[7], Content = data[11]}},
                            {"fr", new ListingTranslation {Culture = "fr", Title = data[6], Content = data[10]}},
                            {"he", new ListingTranslation {Culture = "he", Title = data[8], Content = data[12]}},
                            {"en", new ListingTranslation {Culture = "en", Title = data[5], Content = data[9]}}
                        };

                        var numVotes = rand.Next(20, 80);
                        var metadata = new Dictionary<string, string> {
                            {"Batch", "2"}
                        };
                        var i = 0;
                        var prosInfo = new List<string>();
                        foreach (var listing in collection.IncludedListings)
                        {
                            metadata.Add("Listing_" + i, listing.Id);
                            metadata.Add("Vote_" + i, (numVotes + rand.Next(6)).ToString());

                            i++;

                            var listingData = _listings.FindOne(Query<Listing>.EQ(x => x.Id, listing.Id));
                            var profileData = _profiles.FindOne(Query<Profile>.EQ(x => x.Id, listingData.ProfileId));
                            var titleInDefaultCulture = string.IsNullOrEmpty(profileData.DefaultCulture) ? "" : translations[profileData.DefaultCulture].Title;
                            prosInfo.Add(profileData.UserName + ", " + profileData.ContactInfo.Email + ", " + profileData.DefaultCulture + ", " + titleInDefaultCulture);
                        }

                        poll = new Listing {
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

                        var outputData = new[] {
                            "http://www.homelab.com/poll/" + poll.Id + "--show",
                            string.Join(",", prosInfo)
                        };

                        writer.Write(string.Join(",", outputData) + Environment.NewLine);

                        Console.WriteLine("Completed: " + j);
                        j++;
                    }
                }
            }

            return StatusCode.Success;
        }
    }
}