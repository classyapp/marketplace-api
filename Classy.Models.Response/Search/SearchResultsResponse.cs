using System.Collections.Generic;

namespace Classy.Models.Response.Search
{
    public class SearchResultsResponse<T>
    {
        public IList<T> Results { get; private set; }
        public int Total { get; private set; }

        public SearchResultsResponse(IList<T> results, int total)
        {
            Results = results;
            Total = total;
        }
    }

    public class FreeSearchResultsResponse
    {
        public SearchResultsResponse<ListingView> ListingsResults { get; set; }
        public SearchResultsResponse<ProfileView> ProfilesResults { get; set; }
        public SearchResultsResponse<ListingView> ProductsResults { get; set; }
    }
}