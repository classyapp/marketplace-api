using System.Collections.Generic;
using System.Linq;
using System.Net;
using classy.DTO.Request.Search;
using classy.DTO.Response;
using Classy.Interfaces.Search;
using classy.Manager;
using classy.Manager.Search;
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
        public IProfileSearchProvider ProfileSearchProvider { get; set; }
        public IListingManager ListingManager { get; set; }
        public ISearchSuggestionsProvider SearchSuggestionsProvider { get; set; }

        public object Post(FreeSearchRequest freeSearchRequest)
        {
            // get and organize listings
            var searchListingsResults = ListingSearchProvider.Search(
                freeSearchRequest.Q, freeSearchRequest.Environment.AppId,
                freeSearchRequest.Amount, freeSearchRequest.Page);

            var listingsFromDb = ListingManager.GetListingsByIds(
                searchListingsResults.Results.Select(x => x.Id).ToArray(),
                freeSearchRequest.Environment.AppId,
                false,
                freeSearchRequest.Environment.CultureCode);

            var orderedListings = new List<ListingView>();
            foreach (var dbResult in searchListingsResults.Results)
            {
                var listingFromDb = listingsFromDb.FirstOrDefault(x => x.Id == dbResult.Id);
                if (listingFromDb != null)
                    orderedListings.Add(listingFromDb);
                // TODO: else - LOG about this, since it shouldn't happen!
            }

            // get and organize profiles
            var searchProfilesResults = ProfileSearchProvider.Search(
                freeSearchRequest.Q, freeSearchRequest.Environment.CountryCode,
                freeSearchRequest.Environment.AppId, freeSearchRequest.Amount, freeSearchRequest.Page);

            var profilesFromDb = ProfileManager.GetProfilesByIds(
                searchProfilesResults.Results.Select(x => x.Id).ToArray(),
                freeSearchRequest.Environment.AppId,
                freeSearchRequest.Environment.CultureCode);

            var orderedProfiles = new List<ProfileView>();
            foreach (var dbResult in searchProfilesResults.Results)
                orderedProfiles.Add(profilesFromDb.First(x => x.Id == dbResult.Id));

            // build model from results
            var listingsResponse = new SearchResultsResponse<ListingView>(orderedListings, searchListingsResults.TotalResults);
            var profilesResponse = new SearchResultsResponse<ProfileView>(orderedProfiles, searchProfilesResults.TotalResults);

            return new HttpResult(new FreeSearchResultsResponse() {
                ListingsResults = listingsResponse,
                ProfilesResults = profilesResponse
            }, HttpStatusCode.OK);
        }

        public object Get(SearchListings request)
        {
            return Post(request);
        }

        public object Post(SearchListings request)
        {
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

        public object Get(SearchSuggestionsRequest request)
        {
            var suggestions = new List<SearchSuggestion>();
            if (request.EntityType == "listing")
                suggestions = SearchSuggestionsProvider.GetListingsSuggestions(request.q, request.Environment.AppId);
            else if (request.EntityType == "profile")
                suggestions = SearchSuggestionsProvider.GetProfilesSuggestions(request.q, request.Environment.AppId);

            return new HttpResult(suggestions, HttpStatusCode.OK);
        }

        public object Get(KeywordSuggestionRequest request)
        {
            var suggestions = SearchSuggestionsProvider.KeywordSuggestions(request.q, request.Lang,
                request.Environment.AppId);

            return new HttpResult(suggestions, HttpStatusCode.OK);
        }
    }
}