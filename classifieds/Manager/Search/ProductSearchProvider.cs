using System;
using System.Linq;
using Classy.Interfaces.Search;
using Classy.Models.Search;
using Nest;

namespace classy.Manager.Search
{
    public interface IProductSearchProvider
    {
        SearchResults<ListingIndexDto> Search(string query, string appId, int amount = 25, int page = 1);
    }

    public class ProductSearchProvider : IProductSearchProvider
    {
        private const string IndexName = "listings";
        private readonly ISearchClientFactory _searchClientFactory;

        public ProductSearchProvider(ISearchClientFactory searchClientFactory)
        {
            _searchClientFactory = searchClientFactory;
        }

        public SearchResults<ListingIndexDto> Search(string query, string appId, int amount = 25, int page = 1)
        {
            var client = _searchClientFactory.GetClient(IndexName, appId);

            // parameter factors should be from global settings (and able to change easily)
            var script = "_score + ((doc['reviewAverageScore'].value - 2.5) * 2)" +
                         " + (doc['viewCount'].value / 100)" +
                         " + (doc['followerCount'].value / 3)";

            var descriptor = new SearchDescriptor<ListingIndexDto>()
                .Query(q => q.FunctionScore(
                    fs => fs.Query(
                        mq => mq.QueryString(
                            qs => qs.OnFields(f => f.AnalyzedTitle, f => f.Content, f => f.Content, f => f.Metadata).Query(query))
                        )
                        /*.Functions(
                            ff => ff.ScriptScore(
                                ss => ss.Script(script)
                                )
                        )*/
                    )
                ).Filter(f => f.Term(t => t.ListingType, "product"));

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