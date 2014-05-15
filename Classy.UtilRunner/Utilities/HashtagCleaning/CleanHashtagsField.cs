using System;
using System.IO;
using System.Linq;
using System.Text;
using classy.Extentions;
using Classy.Models;
using Funq;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace Classy.UtilRunner.Utilities.HashtagCleaning
{
    public class CleanHashtagsField : IUtility
    {
        private const int BatchSize = 100;
        private readonly MongoCollection<Listing> _listings;
        
        public CleanHashtagsField(Container container)
        {
            var mongoDatabase = container.Resolve<MongoDatabase>();
            _listings = mongoDatabase.GetCollection<Listing>("classifieds");
        }

        public StatusCode Run(string[] args)
        {
            var startTime = DateTime.Now;

            using (var stream = File.OpenWrite("C:\\hashtags.txt"))
            {
                int i = 0, count = 0;
                var allListings = _listings.FindAll().SetBatchSize(BatchSize);
                foreach (var listing in allListings)
                {
                    i++;

                    if (listing.Hashtags.IsNullOrEmpty() || listing.Hashtags.Count < 5)
                        continue;

                    count++;

                    _listings.FindAndModify(Query<Listing>.EQ(x => x.Id, listing.Id), null,
                        new UpdateBuilder<Listing>().Unset(x => x.Hashtags));

                    //var newKeywords = listing.Hashtags.Select(x => x.Replace("#", "").Replace("-", " ")).ToArray();

                    //var buffer = Encoding.UTF8.GetBytes(listing.Id + " :: " + String.Join(", ", newKeywords) + Environment.NewLine + Environment.NewLine);
                    //stream.Write(buffer, 0, buffer.Length);

                    if (i % 100 == 0)
                        Console.WriteLine("Scanned " + i + " documents in " + (DateTime.Now-startTime).TotalSeconds + " sec");
                }
                Console.WriteLine("Found " + count);
            }

            return StatusCode.Success;
        }
    }
}