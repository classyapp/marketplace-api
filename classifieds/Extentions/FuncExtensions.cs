using classy.Manager;
using Classy.Repository;
using MongoDB.Driver;
using ServiceStack.CacheAccess;
using ServiceStack.Messaging;
using ServiceStack.Redis;
using ServiceStack.Redis.Messaging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace classy.Extentions
{
    public static class FunqExtensions
    {
        public static void WireUp(this Funq.Container container)
        {
            container.Register<IRedisClientsManager>(c =>
            {
                var connectionString = ConfigurationManager.ConnectionStrings["RedisPubSub"].ConnectionString;
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
                var connectionString = ConfigurationManager.ConnectionStrings["MongoDB"].ConnectionString;
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
                new DefaultAppManager());
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
                    c.TryResolve<IMessageQueueClient>(),
                    c.TryResolve<IListingRepository>(),
                    c.TryResolve<ICommentRepository>(),
                    c.TryResolve<IProfileRepository>(),
                    c.TryResolve<ICollectionRepository>(),
                    c.TryResolve<ITripleStore>(),
                    c.TryResolve<IStorageRepository>()));
            container.Register<IProfileManager>(c =>
                new DefaultProfileManager(
                    c.TryResolve<IAppManager>(),
                    c.TryResolve<IProfileRepository>(),
                    c.TryResolve<IListingRepository>(),
                    c.TryResolve<IReviewRepository>(),
                    c.TryResolve<ICollectionRepository>(),
                    c.TryResolve<ITripleStore>(),
                    c.TryResolve<IStorageRepository>()));
            container.Register<IReviewManager>(c =>
                new DefaultProfileManager(
                    c.TryResolve<IAppManager>(),
                    c.TryResolve<IProfileRepository>(),
                    c.TryResolve<IListingRepository>(),
                    c.TryResolve<IReviewRepository>(),
                    c.TryResolve<ICollectionRepository>(),
                    c.TryResolve<ITripleStore>(),
                    c.TryResolve<IStorageRepository>()));
            container.Register<ICollectionManager>(c =>
                new DefaultListingManager(
                    c.TryResolve<IMessageQueueClient>(),
                    c.TryResolve<IListingRepository>(),
                    c.TryResolve<ICommentRepository>(),
                    c.TryResolve<IProfileRepository>(),
                    c.TryResolve<ICollectionRepository>(),
                    c.TryResolve<ITripleStore>(),
                    c.TryResolve<IStorageRepository>()));
            container.Register<IAnalyticsManager>(c =>
                new DefaultAnalyticsManager(
                    c.TryResolve<ITripleStore>()));
            container.Register<ILocalizationManager>(c =>
                new DefaultLocalizationManager(
                    c.TryResolve<ILocalizationRepository>()));
        }
    }
}