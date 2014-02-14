using Classy.Models;
using Classy.Models.Response;
using ServiceStack.ServiceHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace classy.Manager
{
    public interface IListingManager
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="listingId"></param>
        /// <param name="requestedByProfileId"></param>
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
            string requestedByProfileId, 
            bool logImpression, 
            bool includeDrafts, 
            bool includeComments, 
            bool formatCommentsAsHtml, 
            bool includeCommenterProfiles,
            bool includeProfile,
            bool includeFavoritedByProfiles);

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
            int page);

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
        SearchResultsView SearchListings(
            string appId,
            string tag,
            string listingType,
            IDictionary<string, string> metadata,
            double? priceMin,
            double? priceMax,
            Location location,
            bool includeComments,
            bool formatCommentsAsHtml,
            int page);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="listingId"></param>
        /// <param name="profileId"></param>
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
            string profileId,
            string title,
            string content,
            string listingType,
            PricingInfo pricingInfo,
            ContactInfo contactInfo,
            TimeslotSchedule timeslotSchedule,
            IDictionary<string, string> customAttributes);

        /// <summary>
        /// delete listing and update relevant collections
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="listingId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        string DeleteListing(string appId, string listingId, string userId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="listingId"></param>
        /// <param name="profileId"></param>
        /// <param name="mediaFiles"></param>
        /// <returns></returns>
        ListingView AddExternalMediaToListing(
            string appId,
            string listingId,
            string profileId,
            IFile[] files
            );

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="listingId"></param>
        /// <param name="profileId"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        ListingView DeleteExternalMediaFromListing(
            string appId,
            string listingId,
            string profileId,
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
            string profileId,
            string content,
            bool formatAsHtml);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="listingId"></param>
        /// <param name="profileId"></param>
        void FavoriteListing(
            string appId,
            string listingId,
            string profileId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="listingId"></param>
        /// <param name="profileId"></param>
        void UnfavoriteListing(
            string appId,
            string listingId,
            string profileId);

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
            string profileId,
            FlagReason FlagReason);
    }
}
