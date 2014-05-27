using classy.Manager;
using classy.Operations;
using Classy.Auth;
using Classy.Repository;
using MongoDB.Driver;
using NUnit.Framework;
using ServiceStack.ServiceClient.Web;
using ServiceStack.ServiceInterface.Testing;
using ServiceStack.WebHost.Endpoints;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classy.Tests
{
    //[TestFixture]
    public class CatalogImportTests
    {

        public CatalogImportTests()
        {

        }
       
       // [Test]
        public void TestImport()
        {
            try
            {
                var config = new Amazon.S3.AmazonS3Config()
                {
                    ServiceURL = "s3.amazonaws.com",
                    RegionEndpoint = Amazon.RegionEndpoint.USEast1,
                    Timeout = new TimeSpan(0, 5, 0)
                };
                var s3Client = Amazon.AWSClientFactory.CreateAmazonS3Client(config);

                var mongoDB = getDB();
                


                ProductCatalogImportOperator catalogImport =
                    new ProductCatalogImportOperator(new AmazonS3StorageRepository(s3Client, ConfigurationManager.AppSettings["S3BucketName"]),
                    new MongoListingRepository(new Repository.Infrastructure.MongoDatabaseProvider(mongoDB)),
                    new MongoJobRepository(new Repository.Infrastructure.MongoDatabaseProvider(mongoDB)),
                    new CurrencyManager(new StubCurrencyRepository()),
                    new MongoProfileRepository(new Repository.Infrastructure.MongoDatabaseProvider(mongoDB)));

                catalogImport.PerformOperation(new ImportProductCatalogJob("5373ab038e65962b7cd8eac9", "v1.0"));
                
            }
            catch (WebServiceException ex)
            {
                Assert.Fail(ex.Message);
            }
        }

      private MongoDB.Driver.MongoDatabase getDB()
      {
          var connectionString = GetConnectionString("MONGO");
          var client = new MongoClient(connectionString);
          var databaseName = MongoUrl.Create(connectionString).DatabaseName;
          var server = client.GetServer();
          var db = server.GetDatabase(databaseName);

          return db;
      }


      private string GetConnectionString(string key)
      {
          // try app settings
          string connectionString = ConfigurationManager.AppSettings.Get(key + "_URI");
          if (string.IsNullOrEmpty(connectionString))
          {
              // fall back on connection strings
              connectionString = ConfigurationManager.ConnectionStrings[key].ConnectionString;
          }
          return connectionString;
      }

    }
}
