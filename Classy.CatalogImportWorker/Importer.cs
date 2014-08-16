using System;
using System.Linq;
<<<<<<< HEAD
using classy.Manager;
using Classy.Interfaces.Managers;
using Classy.Models;
using Classy.Repository;
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
                    System.Diagnostics.Trace.WriteLine("Checking for pending jobs.");
                    // get next job
                    var job = _jobsCollection.Find(Query.And(Query<Job>.EQ(j => j.AppId, "v1.0"), Query<Job>.EQ(j => j.Status, "Not Started"))).OrderBy(j => j.Created)
                        .FirstOrDefault();

                    // Import Code goes here
                    if (job != null)
                    {
                        System.Diagnostics.Trace.WriteLine(string.Format("Processing job: {0}.", job.Id));
                        new CatalogImportProcessor(
                            _container.Resolve<IStorageRepository>(),
                            _container.Resolve<IListingRepository>(),
                            _container.Resolve<IJobRepository>(),
                            _container.Resolve<ICurrencyManager>(),
                            _container.Resolve<IProfileRepository>(),
                            _container.Resolve<ILocalizationRepository>(),
                            _container.Resolve<IAppManager>(),
                            _container.Resolve<IIndexer<Listing>>()
                            ).Process(job);
                    }
                    else
                    {
                        System.Diagnostics.Trace.WriteLine("No pending jobs found.");
                        System.Threading.Thread.Sleep(5000);
                    }
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
=======
using classy.Manager;
using Classy.Interfaces.Managers;
using Classy.Models;
using Classy.Repository;
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
                    System.Diagnostics.Trace.WriteLine("Checking for pending jobs.");
                    // get next job
                    var job = _jobsCollection.Find(Query.And(Query<Job>.EQ(j => j.AppId, "v1.0"), Query<Job>.EQ(j => j.Status, "Not Started"))).OrderBy(j => j.Created)
                        .FirstOrDefault();

                    // Import Code goes here
                    if (job != null)
                    {
                        System.Diagnostics.Trace.WriteLine(string.Format("Processing job: {0}.", job.Id));
                        new CatalogImportProcessor(
                            _container.Resolve<IStorageRepository>(),
                            _container.Resolve<IListingRepository>(),
                            _container.Resolve<IJobRepository>(),
                            _container.Resolve<ICurrencyManager>(),
                            _container.Resolve<IProfileRepository>(),
                            _container.Resolve<ILocalizationRepository>(),
                            _container.Resolve<IAppManager>()
                            ).Process(job);
                    }
                    else
                    {
                        System.Diagnostics.Trace.WriteLine("No pending jobs found.");
                        System.Threading.Thread.Sleep(5000);
                    }
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
>>>>>>> origin/master
