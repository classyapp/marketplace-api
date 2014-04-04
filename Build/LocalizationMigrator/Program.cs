using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalizationMigrator
{
    class Program
    { 
        static void Main(string[] args)
        {
            var configPath = args[0];
            Debug.WriteLine("config path: " + configPath);
            var config = ConfigurationManager.OpenMappedExeConfiguration(new ExeConfigurationFileMap()
            {
                ExeConfigFilename = configPath
            }, ConfigurationUserLevel.None);

            var mongoUri = getMongoUri(config);
            var db = getDatabase(mongoUri);

            LocalizationCollection localizations = new LocalizationCollection(db);

            var path = Path.GetDirectoryName(configPath);
            DirectoryInfo migrationDirectory = new DirectoryInfo(Path.Combine(path, "migrations", "localizations"));
            Debug.WriteLine("path to migrations", migrationDirectory.FullName);

            localizations.Migrate(migrationDirectory);
        }

        private static MongoDB.Driver.MongoDatabase getDatabase(string mongoUri) {
            MongoClient mongoClient = new MongoClient(mongoUri);
            var databaseName = MongoUrl.Create(mongoUri).DatabaseName;
            var server = mongoClient.GetServer();
            var db = server.GetDatabase(databaseName);
            return db;
        }
        private static string getMongoUri(Configuration config)
        {
            foreach (KeyValueConfigurationElement element in config.AppSettings.Settings)
            {
                if (element.Key == "MONGO_URI")
                    return element.Value;
            }

            foreach (ConnectionStringSettings element in config.ConnectionStrings.ConnectionStrings)
            {
                if (element.Name == "MONGO")
                    return element.ConnectionString;
            }

            throw new Exception("mongo config setting not found");
        }
    }
}
