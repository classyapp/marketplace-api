﻿using Classy.Models;
using Classy.Models.Request;
using Classy.Models.Response;
using System.Collections.Generic;

namespace classy.Manager
{
    public interface IProfileManager : IManager
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="requestedByProfileId"></param>
        /// <param name="batchId"></param>
        /// <param name="ProfessionalInfo"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        ProfileView CreateProfileProxy(
            string appId,
            string requestedByProfileId,
            string batchId,
            ProfessionalInfo ProfessionalInfo,
            IDictionary<string, string> metadata);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="profileId"></param>
        /// <param name="proxyProfileId"></param>
        /// <param name="ProfessionalInfo"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        ProxyClaimView SubmitProxyClaim(
            string appId,
            string profileId,
            string proxyProfileId,
            ProfessionalInfo ProfessionalInfo,
            IDictionary<string, string> metadata,
            string defaultCulture);
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="claimId"></param>
        /// <returns></returns>
        ProxyClaimView ApproveProxyClaim(
            string appId, 
            string claimId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="claimId"></param>
        /// <returns></returns>
        ProxyClaimView RejectProxyClaim(
            string appId, 
            string claimId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="partialUserName"></param>
        /// <param name="category"></param>
        /// <param name="location"></param>
        /// <param name="metadata"></param>
        /// <param name="professionalsOnly"></param>
        /// <returns></returns>
        SearchResultsView<object> SearchProfiles(
            string appId,
            string searchQuery,
            string category,
            Location location,
            IDictionary<string, string> metadata,
            bool professionalsOnly,
            bool ignoreLocation,
            int page,
            int pageSize,
            string culture);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="profileId"></param>
        /// <param name="requestedByProfileId"></param>
        /// <param name="includeFollowedByProfiles"></param>
        /// <param name="includeFollowingProfiles"></param>
        /// <param name="includeReviews"></param>
        /// <param name="includeListings"></param>
        /// <param name="includeCollections"></param>
        /// <param name="includeFavorites"></param>
        /// <param name="logImpression"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        ProfileView GetProfileById(
            string appId,
            string profileId,
            string requestedByProfileId,
            bool includeFollowedByProfiles,
            bool includeFollowingProfiles,
            bool includeReviews,
            bool includeListings,
            bool includeCollections,
            bool includeFavorites,
            bool logImpression,
            string culture);

        List<ProfileView> GetProfilesByIds(string[] profileIds, string appId, string culture);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="profileId"></param>
        /// <param name="followeeProfileId"></param>
        void FollowProfile(
            string appId,
            string profileId,
            string followeeProfileId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="profileId"></param>
        /// <param name="followeeProfileId"></param>
        void UnfollowProfile(
            string appId,
            string profileId,
            string followeeProfileId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="profileId"></param>
        /// <param name="contactInfo"></param>
        /// <param name="professionalInfo"></param>
        /// <param name="metadata"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        ProfileView UpdateProfile(
            string appId,
            string profileId,
            ContactInfo contactInfo,
            ProfessionalInfo professionalInfo,
            IDictionary<string, string> metadata,
            ProfileUpdateFields fields,
            byte[] profileImage,
            string profileImagContentType,
            string defaultCulture,
            IList<string> coverPhotos);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="profileId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        IList<SocialPhotoAlbumView> GetFacebookAlbums(
            string appId,
            string profileId,
            string token);

		/// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="profileId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        IEnumerable<EmailContact> GetGoogleContacts(string appId, string profileId, string token);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="profileId"></param>
        /// <param name="profileTranslation"></param>
        void SetTranslation(string appId, string profileId, ProfileTranslation profileTranslation);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="profileId"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        ProfileTranslationView GetTranslation(string appId, string profileId, string culture);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="profileId"></param>
        /// <param name="culture"></param>
        void DeleteTranslation(string appId, string profileId, string culture);

        VerifyEmailResponse VerifyEmailByHash(string appId, string hash);
    }
}
