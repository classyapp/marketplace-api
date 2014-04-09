using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Configuration;
using System.IO;
using System.Reflection;
using MongoDB.Driver;
using LocalizationMigrator;

namespace Classy.AppHarbor.Deploy
{
    [TestClass]
    public class DeploymentTasks
    {
        [TestMethod]
        public void AddLocalizations()
        {
            var config = GetConfig();
            var db = GetDatabase(config);

            LocalizationCollection localizations = new LocalizationCollection(db);

            var migrationsPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "migrations", "localizations");

            localizations.Migrate(migrationsPath);
        }

        private Configuration GetConfig()
        {
            string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var config = ConfigurationManager.OpenMappedExeConfiguration(new ExeConfigurationFileMap()
            {
                ExeConfigFilename = Path.Combine(assemblyFolder, "_PublishedWebsites", "classy", "Web.AppHarbor.config")
            }, ConfigurationUserLevel.None);
            return config;
        }

        private MongoDB.Driver.MongoDatabase GetDatabase(Configuration config)
        {
            string connectionString = null;
            foreach (KeyValueConfigurationElement element in config.AppSettings.Settings)
            {
                if (element.Key == "MONGO_URI")
                    connectionString = element.Value;
            }

            foreach (ConnectionStringSettings element in config.ConnectionStrings.ConnectionStrings)
            {
                if (element.Name == "MONGO")
                    connectionString = element.ConnectionString;
            }

            if (connectionString == null)
            {
                throw new Exception("mongo config setting not found");
            }

            MongoClient mongoClient = new MongoClient(connectionString);
            var databaseName = MongoUrl.Create(connectionString).DatabaseName;
            var server = mongoClient.GetServer();
            var db = server.GetDatabase(databaseName);
            return db;
        }
    }
}
