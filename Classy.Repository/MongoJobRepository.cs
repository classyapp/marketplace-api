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
            JobsCollection.Save(job);
        }
    }
}