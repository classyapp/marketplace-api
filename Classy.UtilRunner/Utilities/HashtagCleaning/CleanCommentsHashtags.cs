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
    public class CleanCommentsHashtags : IUtility
    {
        private const int BatchSize = 100;
        private readonly MongoCollection<Comment> _comments;
        private readonly MongoCollection<Listing> _listings;
        
        public CleanCommentsHashtags(Container container)
        {
            var mongoDatabase = container.Resolve<MongoDatabase>();
            _comments = mongoDatabase.GetCollection<Comment>("comments");
            _listings = mongoDatabase.GetCollection<Listing>("classifieds");
        }

        public StatusCode Run(string[] args)
        {
            var startTime = DateTime.Now;

            using (var stream = File.OpenWrite("C:\\comments.txt"))
            {
                int i = 0, count = 0;
                var allComments = _comments.Find(Query<Comment>.EQ(x => x.ProfileId, "1182")).SetBatchSize(BatchSize);
                foreach (var comment in allComments)
                {
                    i++;

                    if (comment.Content.IsNullOrEmpty())
                        continue;

                    var hashtags = comment.Content.ExtractHashtags();
                    if (!hashtags.IsNullOrEmpty())
                    {
                        count++;

                        var newKeywords = hashtags.Select(x => x.Replace("#", "").Replace("-", " ")).ToArray();

                        // get listing and update
                        var listing = _listings.FindOne(Query<Listing>.EQ(x => x.Id, comment.ObjectId));
                        _listings.FindAndModify(
                            Query<Listing>.EQ(x => x.Id, listing.Id), null,
                            new UpdateBuilder<Listing>().Unset(x => x.Hashtags)
                                .Set(x => x.SearchableKeywords, newKeywords));

                        // remove comment
                        _comments.Remove(Query<Comment>.EQ(x => x.Id, comment.Id));

                        var buffer = Encoding.UTF8.GetBytes(comment.Id + " :: " + String.Join(", ", newKeywords) + Environment.NewLine + Environment.NewLine);
                        stream.Write(buffer, 0, buffer.Length);
                    }

                    if (i % 100 == 0)
                        Console.WriteLine("Scanned " + i + " documents in " + (DateTime.Now-startTime).TotalSeconds + " sec");
                }
                Console.WriteLine("Found " + count);
            }

            return StatusCode.Success;
        }
    }
}