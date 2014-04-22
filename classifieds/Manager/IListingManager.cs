using Classy.Models;
using Classy.Models.Request;
using Classy.Models.Response;
using ServiceStack.ServiceHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace classy.Manager
{
    public interface IListingManager : IManager
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="listingId"></param>
        /// <param name="logImpression"></param>
        /// <param name="includeDrafts"></param>
        /// <param name="includeComments"></param>
        /// <param name="formatCommentsAsHtml"></param>
        /// <param name="includeCommenterProfiles"></param>
        /// <param name="includeProfile"></param>
        /// <param name="includeFavoritedByProfiles"></param>
        /// <returns></returns>
        ListingView GetListingById(
            string appId, 
            string listingId, 
            bool logImpression, 
            bool includeDrafts, 
            bool includeComments, 
            bool formatCommentsAsHtml, 
            bool includeCommenterProfiles,
            bool includeProfile,
            bool includeFavoritedByProfiles,
            string culture);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="profileId"></param>
        /// <param name="includeComments"></param>
        /// <param name="formatCommentsAsHtml"></param>
        /// <returns></returns>
        IList<ListingView> GetListingsByProfileId(
            string appId,
            string profileId,
            bool includeComments,
            bool formatCommentsAsHtml,
            bool includeDrafts,
            string culture);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="tag"></param>
        /// <param name="listingType"></param>
        /// <param name="metadata"></param>
        /// <param name="priceMin"></param>
        /// <param name="priceMax"></param>
        /// <param name="location"></param>
        /// <param name="includeComments"></param>
        /// <param name="formatCommentsAsHtml"></param>
        /// <returns></returns>
        SearchResultsView<ListingView> SearchListings(
            string appId,
            string[] tags,
            string[] listingTypes,
            IDictionary<string, string[]> metadata,
            double? priceMin,
            double? priceMax,
            Location location,
            bool includeComments,
            bool formatCommentsAsHtml,
            int page,
            int pageSize,
            string culture);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="listingId"></param>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <param name="listingType"></param>
        /// <param name="pricingInfo"></param>
        /// <param name="contactInfo"></param>
        /// <param name="timeslotSchedule"></param>
        /// <param name="customAttributes"></param>
        /// <returns></returns>
        ListingView SaveListing(
            string appId,
            string listingId,
            string title,
            string content,
            string listingType,
            PricingInfo pricingInfo,
            ContactInfo contactInfo,
            TimeslotSchedule timeslotSchedule,
            IDictionary<string, string> customAttributes);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="listingId"></param>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <param name="pricingInfo"></param>
        /// <param name="contactInfo"></param>
        /// <param name="timeslotSchedule"></param>
        /// <param name="metadata"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        ListingView UpdateListing(
            string appId,
            string listingId,
            string title,
            string content,
            PricingInfo pricingInfo,
            ContactInfo contactInfo,
            TimeslotSchedule timeslotSchedule,
            IDictionary<string, string> metadata,
            ListingUpdateFields fields);

        /// <summary>
        /// delete listing and update relevant collections
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="listingId"></param>
        /// <returns></returns>
        string DeleteListing(string appId, string listingId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="listingId"></param>
        /// <param name="mediaFiles"></param>
        /// <returns></returns>
        ListingView AddExternalMediaToListing(
            string appId,
            string listingId,
            IFile[] files);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="listingId"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        ListingView DeleteExternalMediaFromListing(
            string appId,
            string listingId,
            string url);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="listingId"></param>
        /// <param name="profileId"></param>
        /// <returns></returns>
        ListingView PublishListing(
            string appId,
            string listingId,
            string profileId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="listingId"></param>
        /// <param name="profileId"></param>
        /// <param name="comment"></param>
        /// <returns></returns>
        CommentView AddCommentToListing(
            string appId,
            string listingId,
            string content,
            bool formatAsHtml);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="listingId"></param>
        void FavoriteListing(
            string appId,
            string listingId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="listingId"></param>
        void UnfavoriteListing(
            string appId,
            string listingId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="listingId"></param>
        /// <param name="profileId"></param>
        /// <param name="FlagReason"></param>
        void FlagListing(
            string appId,
            string listingId,
            FlagReason FlagReason);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="listingId"></param>
        /// <param name="cultureCode"></param>
        /// <returns></returns>
        ListingTranslationView GetTranslation(string appId, string listingId, string cultureCode);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="listingId"></param>
        /// <param name="translation"></param>
        void SetTranslation(string appId, string listingId, ListingTranslation translation);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="listingId"></param>
        /// <param name="cultureCode"></param>
        void DeleteTranslation(string appId, string listingId, string cultureCode);
    }
}
