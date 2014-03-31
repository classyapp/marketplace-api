using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;

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
            var migrations = migrationDirectory.GetFiles("*.yml").Where(x => !_currentMigrations.ContainsKey(x.Name));
            foreach (var file in migrations.OrderBy(x => x.Name))
            {
                var version = file.Name;
                var contents = File.ReadAllText(file.FullName);
                saveResourcesFromYaml(contents);
                _migrationCollection.Insert(new LocalizationMigration()
                {
                    Version = version
                });
            }
        }

        private void saveResourcesFromYaml(string yamlContents)
        {
            using (var reader = new StringReader(yamlContents))
            {
                var yaml = new YamlStream();
                yaml.Load(reader);

                var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;
                foreach (var element in mapping.Children) // e.g. "window"
                {
                    var word = ((YamlScalarNode)element.Key).Value;
                    if (element.Value is YamlSequenceNode)
                    {
                        saveResourceList(word, (YamlSequenceNode)element.Value);
                    }
                    else
                    {
                        saveResource(word, (YamlMappingNode)element.Value);
                    }
                }
            }
        }

        private void saveResourceList(string word, YamlSequenceNode listItems)
        {
            var resourceListDoc = new LocalizationListResource()
            {
                Key = word,
                AppId = "v1.0",
                ListItems = new List<ListItem>() { }
            };

            foreach (var node in listItems.Children)
            {
                var wrap = (YamlMappingNode)node;
                var listItem = new ListItem() {
                    Value = ((YamlScalarNode)wrap.Children.First().Key).Value,
                    Text = new Dictionary<string, string>()
                };
                foreach(var child in wrap.Children.Skip(1))
                {
                    var resourceName = ((YamlScalarNode)child.Key).Value;
                    var resourceValue = ((YamlScalarNode)child.Value).Value;
                    listItem.Text.Add(resourceName, resourceValue);
                }
                resourceListDoc.ListItems.Add(listItem);
            }

            _resourceListCollection.Insert(resourceListDoc);
        }
        private void saveResource(string word, YamlMappingNode languages)
        {
            var resourceDoc = new LocalizationResource()
            {
                Key = word,
                AppId = "v1.0",
                Values = new Dictionary<string, string>()
            };
            foreach (var resource in languages)
            {
                var resourceName = ((YamlScalarNode)resource.Key).Value;
                var resourceValue = ((YamlScalarNode)resource.Value).Value;
                resourceDoc.Values.Add(resourceName, resourceValue);
            }
            _resourceCollection.Insert(resourceDoc);
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
