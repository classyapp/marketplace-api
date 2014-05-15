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
        public IListingSearchProvider ListingSearchProvider { get; set; }

        
    }

    public class SearchListingsRequest : BaseRequestDto
    {
        public string Q { get; set; }
        public int Amount { get; set; }
        public int Page { get; set; }
    }
}