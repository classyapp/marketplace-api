using System;
using System.Configuration;
using Nest;

namespace Classy.Interfaces.Search
{
    public interface ISearchClientFactory
    {
        ElasticClient GetClient(string indexName);
    }

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