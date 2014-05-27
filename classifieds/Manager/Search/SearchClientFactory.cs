using System;
using System.Configuration;
using classy.Extentions;
using Classy.Interfaces.Search;
using Nest;

namespace classy.Manager.Search
{
    public class SearchClientFactory : ISearchClientFactory
    {
        public static string ElasticConnectionString = ConfigurationManager.AppSettings["SEARCHBOX_URL"];

        public ElasticClient GetClient(string indexName, string appId)
        {
            var settings = new ConnectionSettings(new Uri(ElasticConnectionString)).ExposeRawResponse();

            if (indexName != null)
                settings.SetDefaultIndex("{0}_{1}".With(indexName, appId));

            return new ElasticClient(settings);
        }
    }
}