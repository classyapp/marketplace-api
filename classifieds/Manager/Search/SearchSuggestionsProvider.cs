using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Amazon.OpsWorks.Model;
using classy.DTO.Response;
using Classy.Interfaces.Search;
using Classy.Models.Search;
using Nest;

namespace classy.Manager.Search
{
    public interface ISearchSuggestionsProvider
    {
        List<SearchSuggestion> GetSearchSuggestions(string q, string appId);
    }

    public class SearchSuggestionsProvider : ISearchSuggestionsProvider
    {
        private readonly ISearchClientFactory _searchClientFactory;
        private const string ListingsIndexName = "listings";

        public SearchSuggestionsProvider(ISearchClientFactory searchClientFactory)
        {
            _searchClientFactory = searchClientFactory;
        }

        public List<SearchSuggestion> GetSearchSuggestions(string q, string appId)
        {
            // We're using a regular search here with an ngram tokenizer
            // since using the elasticsearch 'suggest' endpoint will always
            // return suggestions from all documents, without filtering.
            // In version 1.2.0 of elasticsearch we will be able to filter suggestions
            // using the 'context_suggest' feature.
            // http://www.elasticsearch.org/guide/en/elasticsearch/reference/current/suggester-context.html

            var client = _searchClientFactory.GetClient(ListingsIndexName, appId);

            return null;


            //var descriptor = new SuggestDescriptor<ListingIndexDto>()
            //    .Completion("listing-suggest", c => c.OnField(f => f.Keywords).Text(q));

            //var response = client.Suggest<ListingIndexDto>(
            //    d => d.Completion("listing-suggest", c => c.OnField(f => f.Title).Text(q)));

            //var parsed = new List<SearchSuggestion>();
            //foreach (var suggestion in response.Suggestions)
            //{
            //    parsed.Add(new SearchSuggestion {
            //        Key = suggestion.Key,
            //        Value = suggestion.Key
            //    });
            //}

            //return parsed;
        }
    }
}