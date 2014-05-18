using Amazon.OpsWorks.Model;
using classy.Cache;
using Classy.Interfaces.Search;
using classy.Manager;
using classy.Manager.Search;
using Classy.Models;
using Classy.Repository;
using MongoDB.Driver;
using ServiceStack.Messaging;
using ServiceStack.Redis;
using ServiceStack.Redis.Messaging;
using System;
using System.Configuration;
using Classy.Interfaces.Managers;

namespace classy.Extensions
{
    public static class FunqExtensions
    {
        private static string GetConnectionString(string key) {
            // try app settings
            string connectionString = ConfigurationManager.AppSettings.Get(key + "_URI");
            if (string.IsNullOrEmpty(connectionString))
            {
                // fall back on connection strings
                connectionString = ConfigurationManager.ConnectionStrings[key].ConnectionString;
            }
            return connectionString;
        }
        public static void WireUp(this Funq.Container container)
        {
            container.Register<ICache<Classy.Models.App>>(r => new DefaultCache<Classy.Models.App>());
            container.Register<ISearchClientFactory>(_ => new SearchClientFactory());
            container.Register<IListingSearchProvider>(
                c => new ListingSearchProvider(c.TryResolve<ISearchClientFactory>()));

            container.Register<IIndexer<Listing>>(x =>
                new ListingIndexer(x.TryResolve<ISearchClientFactory>(), x.TryResolve<IAppManager>()));
            container.Register<IIndexer<Profile>>(x =>
                new ProfileIndexer(x.TryResolve<ISearchClientFactory>(), x.TryResolve<IAppManager>()));

            container.Register<IRedisClientsManager>(c =>
            {
                var connectionString = GetConnectionString("REDIS");
                return new PooledRedisClientManager(new string[] { connectionString });
            });
            container.Register<IMessageService>(c => 
            {
                return new RedisMqServer(c.Resolve<IRedisClientsManager>());
            });
            container.Register<IMessageQueueClient>(c =>
            {
                return new RedisMessageQueueClient(c.Resolve<IRedisClientsManager>());
            });

            // register mongodb repositories
            container.Register<MongoDatabase>(c =>
            {
                var connectionString = GetConnectionString("MONGO");
                var client = new MongoClient(connectionString);
                var databaseName = MongoUrl.Create(connectionString).DatabaseName;
                var server = client.GetServer();
                var db = server.GetDatabase(databaseName);
                return db;
            });
            container.Register<ITripleStore>(c => new MongoTripleStore(c.Resolve<MongoDatabase>()));
            container.Register<IListingRepository>(c => new MongoListingRepository(c.Resolve<MongoDatabase>()));
            container.Register<ICommentRepository>(c => new MongoCommentRepository(c.Resolve<MongoDatabase>()));
            container.Register<IReviewRepository>(c => new MongoReviewRepository(c.Resolve<MongoDatabase>()));
            container.Register<Amazon.S3.IAmazonS3>(c =>
            {
                var config = new Amazon.S3.AmazonS3Config()
                {
                    ServiceURL = "s3.amazonaws.com",
                    RegionEndpoint = Amazon.RegionEndpoint.USEast1,
                    Timeout = new TimeSpan(0, 5, 0)
                };
                var s3Client = Amazon.AWSClientFactory.CreateAmazonS3Client(config);
                return s3Client;
            });
            container.Register<IStorageRepository>(c => new AmazonS3StorageRepository(c.Resolve<Amazon.S3.IAmazonS3>(), ConfigurationManager.AppSettings["S3BucketName"]));
            container.Register<IProfileRepository>(c => new MongoProfileRepository(c.Resolve<MongoDatabase>()));
            container.Register<IBookingRepository>(c => new MongoBookingRepository(c.Resolve<MongoDatabase>()));
            container.Register<ITransactionRepository>(c => new MongoTransactionRepository(c.Resolve<MongoDatabase>()));
            container.Register<IOrderRepository>(c => new MongoOrderRepository(c.Resolve<MongoDatabase>()));
            container.Register<ICollectionRepository>(c => new MongoCollectionRepository(c.Resolve<MongoDatabase>()));
            container.Register<ILocalizationRepository>(c => new MongoLocalizationProvider(c.Resolve<MongoDatabase>()));
            container.Register<IAppManager>(c =>
                new DefaultAppManager(c.TryResolve<MongoDatabase>(), c.TryResolve<ICache<Classy.Models.App>>()));
            container.Register<IEmailManager>(c =>
                new MandrillEmailManager(c.TryResolve<IAppManager>()));
            container.Register<IPaymentGateway>(c =>
                new TranzilaPaymentGateway(
                    c.TryResolve<ITransactionRepository>()));
            container.Register<IBookingManager>(c =>
                new DefaultBookingManager(
                    c.TryResolve<IListingRepository>(),
                    c.TryResolve<IBookingRepository>(),
                    c.TryResolve<IPaymentGateway>(),
                    c.TryResolve<ITripleStore>()));
            container.Register<IOrderManager>(c =>
                new DefaultOrderManager(
                    c.TryResolve<IListingRepository>(),
                    c.TryResolve<IProfileRepository>(),
                    c.TryResolve<IOrderRepository>(),
                    c.TryResolve<ITransactionRepository>(),
                    c.TryResolve<IPaymentGateway>(),
                    c.TryResolve<ITripleStore>(),
                    c.TryResolve<ITaxCalculator>(),
                    c.TryResolve<IShippingCalculator>()));
            container.Register<IListingManager>(c =>
                new DefaultListingManager(
                    c.TryResolve<IAppManager>(),
                    c.TryResolve<IListingRepository>(),
                    c.TryResolve<ICommentRepository>(),
                    c.TryResolve<IProfileRepository>(),
                    c.TryResolve<ICollectionRepository>(),
                    c.TryResolve<ITripleStore>(),
                    c.TryResolve<IStorageRepository>(),
                    c.TryResolve<IIndexer<Listing>>(),
                    c.TryResolve<IIndexer<Profile>>()));
            container.Register<IProfileManager>(c =>
                new DefaultProfileManager(
                    c.TryResolve<IAppManager>(),
                    c.TryResolve<ILocalizationManager>(),
                    c.TryResolve<IProfileRepository>(),
                    c.TryResolve<IListingRepository>(),
                    c.TryResolve<IReviewRepository>(),
                    c.TryResolve<ICollectionRepository>(),
                    c.TryResolve<ITripleStore>(),
                    c.TryResolve<IStorageRepository>(),
                    c.TryResolve<IIndexer<Profile>>()));
            container.Register<IReviewManager>(c =>
                new DefaultProfileManager(
                    c.TryResolve<IAppManager>(),
                    c.TryResolve<ILocalizationManager>(),
                    c.TryResolve<IProfileRepository>(),
                    c.TryResolve<IListingRepository>(),
                    c.TryResolve<IReviewRepository>(),
                    c.TryResolve<ICollectionRepository>(),
                    c.TryResolve<ITripleStore>(),
                    c.TryResolve<IStorageRepository>(),
                    c.TryResolve<IIndexer<Profile>>()));
            container.Register<ICollectionManager>(c =>
                new DefaultListingManager(
                    c.TryResolve<IAppManager>(),
                    c.TryResolve<IListingRepository>(),
                    c.TryResolve<ICommentRepository>(),
                    c.TryResolve<IProfileRepository>(),
                    c.TryResolve<ICollectionRepository>(),
                    c.TryResolve<ITripleStore>(),
                    c.TryResolve<IStorageRepository>(),
                    c.TryResolve<IIndexer<Listing>>(),
                    c.TryResolve<IIndexer<Profile>>()));
            container.Register<IAnalyticsManager>(c =>
                new DefaultAnalyticsManager(
                    c.TryResolve<ITripleStore>()));
            container.Register<ILocalizationManager>(c =>
                new DefaultLocalizationManager(
                    c.TryResolve<ILocalizationRepository>(),
                    c.TryResolve<IProfileRepository>()));
            container.Register<IThumbnailManager>(c =>
                new DefaultThumbnailManager(
                    c.TryResolve<IStorageRepository>()));
        }
    }
}