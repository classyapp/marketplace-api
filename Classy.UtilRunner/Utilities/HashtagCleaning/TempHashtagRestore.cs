using System;
using System.IO;
using System.Linq;
using Classy.Models;
using Funq;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using ServiceStack.Common;

namespace Classy.UtilRunner.Utilities.HashtagCleaning
{
    public class TempHashtagRestore : IUtility
    {
        private readonly MongoCollection<Listing> _listings;

        public TempHashtagRestore(Container container)
        {
            var mongoDatabase = container.Resolve<MongoDatabase>();
            _listings = mongoDatabase.GetCollection<Listing>("classifieds");
        }

        public StatusCode Run(string[] args)
        {
            var fileLines = File.ReadAllLines("C:\\temp\\hashtags.txt");
            var cleanLines = fileLines.Where(x => !x.Trim().IsNullOrEmpty()).ToArray();

            var i = 0;
            foreach (var line in cleanLines)
            {
                var parts = line.Split(new[] {"::"}, StringSplitOptions.None).Select(x => x.Trim()).ToArray();

                if (parts[0].IsNullOrEmpty() || parts[1].Split(',').Count() < 5)
                    continue;

                i++;

                var newKeywords = parts[1].Split(',').Select(x => x.Trim()).ToList();

                _listings.FindAndModify(Query<Listing>.EQ(x => x.Id, parts[0]), null,
                    new UpdateBuilder<Listing>().Set(x => x.SearchableKeywords, newKeywords));

                if (i % 10 == 0)
                    Console.WriteLine("Updated " + i + " documents");
            }
            Console.WriteLine("Done updating " + i + " documents");

            return StatusCode.Success;
        }
    }
}