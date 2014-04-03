using Classy.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using ServiceStack.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalizationMigrator
{
    public class LocalizationCollection
    {
        private MongoDatabase _db;
        private MongoCollection<LocalizationMigration> _migrationCollection;
        private MongoCollection<LocalizationResource> _resourceCollection;
        private MongoCollection<LocalizationListResource> _resourceListCollection;
        private Dictionary<string, bool> _currentMigrations;
        public LocalizationCollection(MongoDatabase db)
        {
            _migrationCollection = db.GetCollection<LocalizationMigration>("LocalizationMigrations");
            _resourceCollection = db.GetCollection<LocalizationResource>("resources");
            _resourceListCollection = db.GetCollection<LocalizationListResource>("listresourcess");
            loadCurrentMigrations();
        }

        public void Migrate(string path)
        {
            Migrate(new DirectoryInfo(path));
        }
        public void Migrate(DirectoryInfo migrationDirectory)
        {
            var migrations = migrationDirectory.GetFiles("*.json").Where(x => !_currentMigrations.ContainsKey(x.Name));
            foreach (var file in migrations.OrderBy(x => x.Name))
            {
                var version = file.Name;
                var contents = File.ReadAllText(file.FullName);
                saveResourcesFromJson(getSupportedCultures(), contents);
                _migrationCollection.Insert(new LocalizationMigration()
                {
                    Version = version
                });
            }
        }

        private void saveResourcesFromJson(IEnumerable<string> supportedCultures, string jsonString)
        {
            var resources = JsonObject.Parse(jsonString);
            foreach (var word in resources.Keys)
            {
                var props = JsonObject.Parse(resources.Child(word));
                var appId = props.Child("appId");
                var description = props.Child("description");
                if (props.Keys.Contains("items", StringComparer.CurrentCultureIgnoreCase))
                {
                    var resourceListDoc = new LocalizationListResource()
                    {
                        AppId = appId,
                        Key = word,
                        ListItems = new List<ListItem>()
                    };
                    var items = JsonSerializer.DeserializeFromString<List<string>>(props.Child("items"));
                    foreach (var i in items)
                    {
                        var li = new ListItem() { Value = i, Text = new Dictionary<string,string>() };
                        addSupportedCulturesToResource(li.Text, word, supportedCultures);
                        resourceListDoc.ListItems.Add(li);
                    }
                    _resourceListCollection.Insert(resourceListDoc);
                }
                else
                {
                    var resourceDoc = new LocalizationResource()
                    {
                        AppId = appId,
                        Key = word,
                        Description = description,
                        Values = new Dictionary<string, string>()
                    };
                    addSupportedCulturesToResource(resourceDoc.Values, word, supportedCultures);
                    _resourceCollection.Insert(resourceDoc);
                }
            }
        }

        private void addSupportedCulturesToResource(IDictionary<string,string> item, string word, IEnumerable<string> cultures)
        {
            foreach(var c in cultures) 
            {
                item.Add(c, "#" + word + "#");
            }
        }
        private IEnumerable<string> getSupportedCultures()
        {
            return _resourceListCollection.AsQueryable<LocalizationListResource>().First(x => x.Key == "supported-cultures").ListItems.Select(x => x.Value);
        }
        private void loadCurrentMigrations()
        {
            _currentMigrations = new Dictionary<string, bool>();
            foreach (LocalizationMigration migration in _migrationCollection.FindAll())
            {
                var version = migration.Version;
                _currentMigrations[version] = true;
            }
        }
    }
}
