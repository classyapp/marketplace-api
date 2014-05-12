using System;
using System.Configuration;
using Classy.Interfaces.Search;
using Nest;

namespace classy.Manager.Search
{
    public class SearchClientFactory : ISearchClientFactory
    {
        public static string ElasticConnectionString = ConfigurationManager.AppSettings["SEARCHBOX_URL"];

        public ElasticClient GetClient(string indexName)
        {
            var settings = new ConnectionSettings(new Uri(ElasticConnectionString));

            if (indexName != null)
                settings.SetDefaultIndex(indexName);

            return new ElasticClient(settings);
        }
    }
}