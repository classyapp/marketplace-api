using System;
using System.Collections;
using System.Collections.Generic;
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
            var queryDescriptor = new QueryDescriptor<ListingIndexDto>();
            queryDescriptor.QueryString(
                q => q.OnFields(f => f.Title, f => f.Content).Query(query));
            queryDescriptor.Filtered(q => q.Filter(f => f.Term(t => t.FlagCount, 0)));

            var searchDescriptor = new SearchDescriptor<ListingIndexDto>();
            searchDescriptor.Query(queryDescriptor);
            
            var response = _client.Search(searchDescriptor);

            return new SearchResults<ListingIndexDto> {
                Results = response.Documents.ToList(),
                TotalResults = response.Total
            };
        }
    }
}