using System;
using System.Linq;
using Classy.Interfaces.Search;
using Classy.Models.Search;
using Nest;

namespace classy.Manager.Search
{
    public class ListingSearchProvider : IListingSearchProvider
    {
        private const string IndexName = "listings";
        private readonly ISearchClientFactory _searchClientFactory;

        public ListingSearchProvider(ISearchClientFactory searchClientFactory)
        {
            _searchClientFactory = searchClientFactory;
        }

        public void Index(ListingIndexDto[] listingDtos, string appId)
        {
            var client = _searchClientFactory.GetClient(IndexName, appId);
            client.IndexMany(listingDtos);
        }

        public SearchResults<ListingIndexDto> Search(string query, string appId, int amount = 25, int page = 1)
        {
            // This elasticsearch query currently uses 'script_score' in the 'function_score' method
            // When NEST starts supporting 'field_value_factor', then we should convert this query
            // to use that, since it should be much faster in performance.
            // http://www.elasticsearch.org/guide/en/elasticsearch/reference/current/mapping-boost-field.html
            // https://github.com/elasticsearch/elasticsearch/pull/5519

            var client = _searchClientFactory.GetClient(IndexName, appId);

            // parameter factors should be from global settings (and able to change easily)
            var script = "_score + (doc['favoriteCount'].value / 10)" +
                         " + (doc['viewCount'].value / 100)" +
                         " - (doc['flagCount'].value / 3)";

            var descriptor = new SearchDescriptor<ListingIndexDto>()
                .Query(q => q.FunctionScore(
                    fs => fs.Query(
                        qq => q.QueryString(
                            qs => qs.OnFields(f => f.Metadata, f => f.Title, f => f.Content, f => f.Keywords)
                                .Query(query)))
                        .Functions(
                            ff => ff.ScriptScore(
                                ss => ss.Script(script)
                                )
                        )
                    ));

            descriptor
                .Size(amount)
                .From(amount * (page - 1));
            
            // find a better way (than precompiler flags) to log if we have problems...
            var request = client.Serializer.Serialize(descriptor);

            var response = client.Search<ListingIndexDto>(_ => descriptor);

            return new SearchResults<ListingIndexDto> {
                Results = response.Documents.ToList(),
                TotalResults = Convert.ToInt32(response.Total)
            };
        }
    }
}