using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.Configuration;
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
            var projectPath = args[0];
            var config = ConfigurationManager.OpenMappedExeConfiguration(new ExeConfigurationFileMap()
            {
                ExeConfigFilename = Path.Combine(projectPath, "Web.config")
            }, ConfigurationUserLevel.None);

            var mongoUri = getMongoUri(config);
            var db = getDatabase(mongoUri);

            LocalizationCollection localizations = new LocalizationCollection(db);

            DirectoryInfo migrationDirectory = new DirectoryInfo(Path.Combine(projectPath, "Localizations"));
            
            localizations.Migrate(Path.Combine(projectPath, "Localizations"));
        }

        private static void AddLocalizedWordsFromJson(string json, MongoDatabase db)
        {
            JsonSchema schema = JsonSchema.Parse(json);
            foreach (var p in schema.Properties)
            {
                var word = p.Key;
            }
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
