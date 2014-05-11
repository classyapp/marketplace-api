using System.Net;
using Amazon.AWSSupport.Model;
using ServiceStack.Common.Web;

namespace classy.Services
{
    public class SearchService : Service
    {
        public object Get(SearchListingsRequest searchRequest)
        {
            return new HttpResult("OK", HttpStatusCode.OK);
        }
    }

    public class SearchListingsRequest
    {
        public string q { get; set; }
    }
}