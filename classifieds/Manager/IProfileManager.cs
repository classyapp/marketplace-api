using Classy.Auth;
using Classy.Models;
using Classy.Models.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace classy.Manager
{
    public interface IProfileManager
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="sellerInfo"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        ProfileView CreateProfileProxy(
            string appId,
            Seller sellerInfo,
            IDictionary<string, string> metadata);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="profileId"></param>
        /// <param name="proxyProfileId"></param>
        /// <param name="sellerInfo"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        ProxyClaimView SubmitProxyClaim(
            string appId,
            string profileId,
            string proxyProfileId,
            Seller sellerInfo,
            IDictionary<string, string> metadata);
        
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
        /// <returns></returns>
        IList<ProfileView> SearchProfiles(
            string appId,
            string partialUserName,
            string category,
            Location location,
            IDictionary<string, string> metadata);

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
        /// <param name="logImpression"></param>
        /// <returns></returns>
        ProfileView GetProfileById(
            string appId,
            string profileId,
            string requestedByProfileId,
            bool includeFollowedByProfiles,
            bool includeFollowingProfiles,
            bool includeReviews,
            bool includeListings,
            bool logImpression);
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="profileId"></param>
        /// <param name="followeeUsername"></param>
        void FollowProfile(
            string appId,
            string profileId,
            string followeeUsername);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="profileId"></param>
        /// <param name="sellerInfo"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        ProfileView UpdateProfile(
            string appId,
            string profileId,
            Seller sellerInfo,
            IDictionary<string, string> metadata);
    }
}
