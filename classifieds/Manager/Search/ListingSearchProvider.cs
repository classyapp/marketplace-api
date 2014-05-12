using System.Linq;
using Classy.Interfaces.Search;
using Classy.Models.Search;
using Nest;

namespace classy.Manager.Search
{
    public class ListingSearchProvider : IListingSearchProvider
    {
        private readonly IElasticClient _client;

        public ListingSearchProvider(ISearchClientFactory searchClientFactory)
        {
            _client = searchClientFactory.GetClient("listings");
        }

        public void Index(ListingIndexDto[] listingDtos)
        {
            _client.IndexMany(listingDtos);
        }

        public SearchResults<ListingIndexDto> Search(string query)
        {
            //var searchDescriptor = new SearchDescriptor<ListingIndexDto>()
            //    .Query(q => q.Bool(b =>
            //        b.Should(s => s.QueryString(x => x.OnFields(f => f.Title, f => f.Content, f => f.Keywords).Query(query)),
            //        s => s.Nested(n => 
            //            n.Path(p => p.Metadata)
            //            .Query(nq => nq.Term(t => t.Metadata, query))))));

            var searchDescriptor = new SearchDescriptor<ListingIndexDto>()
                .Query(q => q.QueryString(x =>
                    x.OnFields(f => f.Metadata, f => f.Title, f => f.Content, f => f.Keywords).Query(query)));

            var request = _client.Serializer.Serialize(searchDescriptor);

            var response = _client.Search(searchDescriptor);

            //queryDescriptor.Filtered(q => q.Filter(f => f.Range(t => t.Greater(0))));

            return new SearchResults<ListingIndexDto> {
                Results = response.Documents.ToList(),
                TotalResults = response.Total
            };
        }
    }
}