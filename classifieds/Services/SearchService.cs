using System.Diagnostics;
using System.Net;
using Amazon.AWSSupport.Model;
using Classy.Interfaces.Search;
using Classy.Models;
using ServiceStack.Common.Web;

namespace classy.Services
{
    public class SearchService : Service
    {
        //public IListingSearchProvider ListingSearchProvider { get; set; }

        public object Post(SearchListingsRequest searchRequest)
        {
            Debugger.Launch();

            //var searchResults = ListingSearchProvider.Search(
            //    searchRequest.Q, searchRequest.Amount, searchRequest.Page);

            return new HttpResult("OK", HttpStatusCode.OK);
        }
    }

    public class SearchListingsRequest : BaseRequestDto
    {
        public string Q { get; set; }
        public int Amount { get; set; }
        public int Page { get; set; }
    }
}