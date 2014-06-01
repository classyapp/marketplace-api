using System;
using System.Collections.Generic;
using Classy.Models;
using Classy.Repository.Infrastructure;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace Classy.Repository
{
    public class MongoJobRepository : IJobRepository
    {
        private MongoCollection<Job> JobsCollection;

        public MongoJobRepository(MongoDatabaseProvider db)
        {
            JobsCollection = db.GetCollection<Job>();
        }

        public Job GetById(string appId, string jobId)
        {
            return JobsCollection.FindOne(Query.And(Query.EQ("AppId", appId), Query.EQ("_id", jobId)));
        }

        public void Save(Models.Job job)
        {
            if (!string.IsNullOrEmpty(job.Id))
            {
                job.UpdatedAt = DateTime.UtcNow;
            }
            JobsCollection.Save(job);
        }


        public IEnumerable<Job> GetByProfileId(string appId, string profileId)
        {
            return JobsCollection.Find(Query.And(Query.EQ("AppId", appId), Query.EQ("ProfileId", profileId)));
        }
    }
}