using System.Linq;
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
            //var searchDescriptor = new SearchDescriptor<ListingIndexDto>()
            //    .Query(q => q.Bool(b =>
            //        b.Should(s => s.QueryString(x => x.OnFields(f => f.Title, f => f.Content, f => f.Keywords).Query(query)),
            //        s => s.Nested(n => 
            //            n.Path(p => p.Metadata)
            //            .Query(nq => nq.Term(t => t.Metadata, query))))));

            var client = _searchClientFactory.GetClient(IndexName, appId);

            var searchDescriptor = new SearchDescriptor<ListingIndexDto>()
                .Query(q => q.CustomFiltersScore(
                    c => c.Query(cq => cq.QueryString(
                        qs => qs.OnFields(f => f.Metadata, f => f.Title, f => f.Content, f => f.Keywords).Query(query))
                    ).Filters(
                        f => f.Filter(
                            ff => ff.NumericRange(fn => fn.GreaterOrEquals(1).OnField(aa => aa.FlagCount)))
                            .Boost(0.5f),
                        f => f.Filter(
                            ff => ff.Exists(e => e.FavoriteCount))
                            .Script("1.0 + (doc['favoriteCount'].value * 2)")
                    ).ScoreMode(ScoreMode.multiply)
                )
            );

            searchDescriptor
                .Size(amount)
                .From(amount*(page - 1));
            
            // find a better way (than precompiler flags) to log if we have problems...
            var request = client.Serializer.Serialize(searchDescriptor);

            var response = client.Search(searchDescriptor);

            //queryDescriptor.Filtered(q => q.Filter(f => f.Range(t => t.Greater(0))));

            return new SearchResults<ListingIndexDto> {
                Results = response.Documents.ToList(),
                TotalResults = response.Total
            };
        }
    }
}