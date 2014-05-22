using System.Collections.Generic;
using System.Linq;
using System.Net;
using classy.DTO.Request.Search;
using Classy.Interfaces.Search;
using classy.Manager;
using Classy.Models;
using Classy.Models.Request;
using Classy.Models.Response;
using Classy.Models.Response.Search;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;

namespace classy.Services
{
    public class SearchService : Service
    {
        public IAppManager AppManager { get; set; }
        public IProfileManager ProfileManager { get; set; }
        public IListingSearchProvider ListingSearchProvider { get; set; }
        public IListingManager ListingManager { get; set; }

        public object Post(SearchListingsRequest searchRequest)
        {
            var searchResults = ListingSearchProvider.Search(
                searchRequest.Q, searchRequest.Environment.AppId,
                searchRequest.Amount, searchRequest.Page);

            var listingsFromDb = ListingManager.GetListingsByIds(
                searchResults.Results.Select(x => x.Id).ToArray(),
                searchRequest.Environment.AppId,
                false,
                searchRequest.Environment.CultureCode);

            var orderedListings = new List<ListingView>();
            foreach (var dbResult in searchResults.Results)
                orderedListings.Add(listingsFromDb.First(x => x.Id == dbResult.Id));

            var response = new SearchResultsResponse<ListingView>(orderedListings, searchResults.TotalResults);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public object Get(SearchListings request)
        {
            return Post(request);
        }

        public object Post(SearchListings request)
        {
            ListingManager.Environment = request.Environment;
            var listingViews = ListingManager.SearchListings(
                request.Environment.AppId,
                request.Tags,
                request.ListingTypes,
                request.Metadata,
                request.PriceMin,
                request.PriceMax,
                request.Location ?? request.Environment.GetDefaultLocation(AppManager.GetAppById(request.Environment.AppId).DefaultCountry),
                request.IncludeComments,
                request.FormatCommentsAsHtml,
                request.Page,
                AppManager.GetAppById(request.Environment.AppId).PageSize,
                request.Environment.CultureCode);

            return new HttpResult(listingViews, HttpStatusCode.OK);
        }

        // search profiles
        public object Get(SearchProfiles request)
        {
            return Post(request);
        }

        public object Post(SearchProfiles request)
        {
            var profiles = ProfileManager.SearchProfiles(
                request.Environment.AppId,
                request.DisplayName,
                request.Category,
                request.Location ?? request.Environment.GetDefaultLocation(AppManager.GetAppById(request.Environment.AppId).DefaultCountry),
                request.Metadata,
                request.ProfessionalsOnly,
                request.IgnoreLocation,
                request.Page,
                AppManager.GetAppById(request.Environment.AppId).PageSize,
                request.Environment.CultureCode);

            return new HttpResult(profiles, HttpStatusCode.OK);
        }
    }
}