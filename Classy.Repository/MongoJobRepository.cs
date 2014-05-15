using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Classy.Models;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace Classy.Repository
{
    public class MongoJobRepository : IJobRepository
    {
        private MongoCollection<Job> JobsCollection;

        public MongoJobRepository(MongoDatabase db)
        {
            JobsCollection = db.GetCollection<Job>("jobs");
        }

        public Job GetById(string appId, string jobId)
        {
            return JobsCollection.FindOne(Query.And(Query.EQ("AppId", appId), Query.EQ("_id", jobId)));
        }

        public void Save(Models.Job job)
        {
            JobsCollection.Save(job);
        }
    }
}
