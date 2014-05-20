using System.Collections.Generic;
using classy.DTO.Response;
using classy.Extentions;
using Classy.Interfaces.Search;
using Classy.Models.Search;
using Nest;

namespace classy.Manager.Search
{
    public interface ISearchSuggestionsProvider
    {
        List<SearchSuggestion> GetListingsSuggestions(string q, string appId);
        List<SearchSuggestion> GetProfilesSuggestions(string q, string appId);
    }

    public class SearchSuggestionsProvider : ISearchSuggestionsProvider
    {
        private readonly ISearchClientFactory _searchClientFactory;
        private const string ListingsIndexName = "listings";
        private const string ProfilesIndexName = "profiles";

        public SearchSuggestionsProvider(ISearchClientFactory searchClientFactory)
        {
            _searchClientFactory = searchClientFactory;
        }

        public List<SearchSuggestion> GetListingsSuggestions(string q, string appId)
        {
            // We're using a regular search here with an ngram tokenizer
            // since using the elasticsearch 'suggest' endpoint will always
            // return suggestions from all documents, without filtering.
            // In version 1.2.0 of elasticsearch we will be able to filter suggestions
            // using the 'context_suggest' feature.
            // http://www.elasticsearch.org/guide/en/elasticsearch/reference/current/suggester-context.html

            var client = _searchClientFactory.GetClient(ListingsIndexName, appId);

            var searchDescriptor = new SearchDescriptor<ListingIndexDto>()
                .Query(
                    qq => qq.QueryString(
                        qs => qs.OnField(f => f.AnalyzedTitle).Query(q)));

            var response = client.Search<ListingIndexDto>(_ => searchDescriptor);

            var suggestions = new List<SearchSuggestion>();
            if (response.Documents.IsNullOrEmpty())
                return suggestions;

            foreach (var suggestion in response.Documents)
            {
                suggestions.Add(new SearchSuggestion {
                    Key = suggestion.Title,
                    Value = suggestion.Title
                });
            }

            return suggestions;
        }

        public List<SearchSuggestion> GetProfilesSuggestions(string q, string appId)
        {
            var client = _searchClientFactory.GetClient(ProfilesIndexName, appId);

            var searchDescriptor = new SearchDescriptor<ProfileIndexDto>()
                .Query(
                    qq => qq.QueryString(
                        qs => qs.OnField(f => f.AnalyzedCompanyName).Query(q)));

            var response = client.Search<ProfileIndexDto>(_ => searchDescriptor);

            var suggestions = new List<SearchSuggestion>();
            if (response.Documents.IsNullOrEmpty())
                return suggestions;

            foreach (var suggestion in response.Documents)
            {
                suggestions.Add(new SearchSuggestion
                {
                    Key = suggestion.CompanyName,
                    Value = suggestion.CompanyName
                });
            }

            return suggestions;
        }
    }
}