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
            var client = _searchClientFactory.GetClient(ListingsIndexName, appId);

            var descriptor = new SuggestDescriptor<ListingIndexDto>()
                .Completion("suggest", c => c.OnField(f => f.Keywords).Text(q));

            var response = client.Suggest<ListingIndexDto>(_ => descriptor);

            var parsed = new List<SearchSuggestion>();
            foreach (var suggestion in response.Suggestions)
            {
                parsed.Add(new SearchSuggestion {
                    Key = suggestion.Key,
                    Value = suggestion.Key
                });
            }

            return parsed;
        }
    }
}