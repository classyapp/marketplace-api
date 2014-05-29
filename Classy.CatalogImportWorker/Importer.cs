using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Classy.Models;
using Classy.Repository.Infrastructure;
using MongoDB.Driver.Builders;

namespace Classy.CatalogImportWorker
{
    public class Importer
    {
        private Funq.Container _container = null;
        private MongoDB.Driver.MongoCollection<Job> _jobsCollection = null;

        public Importer(Funq.Container container)
        {
            _container = container;
            var mongoDB = _container.Resolve<MongoDatabaseProvider>();
            _jobsCollection = mongoDB.GetCollection<Job>();
        }

        public void Run()
        {
            while (true)
            {
                try
                {
                    // get next job
                    Job job = _jobsCollection.Find(Query.And(Query<Job>.EQ(j => j.AppId, "v1.0"), Query<Job>.EQ(j => j.Status, "Not Started"))).OrderBy(j => j.Created)
                        .FirstOrDefault();

                    // Import Code goes here
                    System.Diagnostics.Trace.WriteLine(string.Format("Running import for Profile {0} - Job {1}", job.ProfileId, job.Id));

                    if (job == null)
                        System.Threading.Thread.Sleep(5000);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine(ex.ToString());
                    throw;
                }
            }
        }
    }
}
