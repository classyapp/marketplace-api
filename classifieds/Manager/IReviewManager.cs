using Classy.Models;
using Classy.Models.Response;
using Classy.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace classy.Manager
{
    public interface IReviewManager
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="reviewerProfileId"></param>
        /// <param name="listingId"></param>
        /// <param name="content"></param>
        /// <param name="score"></param>
        /// <returns></returns>
        ReviewView PostReviewForListing(
            string appId,
            string reviewerProfileId,
            string listingId,
            string content,
            int score);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="reviewerProfileId"></param>
        /// <param name="revieweeProfileId"></param>
        /// <param name="content"></param>
        /// <param name="score"></param>
        /// <param name="contactInfo"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        ReviewView PostReviewForProfile(
            string appId,
            string reviewerProfileId,
            string revieweeProfileId,
            string content,
            int score,
            ContactInfo contactInfo,
            IList<CustomAttribute> metadata);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="reviewId"></param>
        /// <param name="profileId"></param>
        /// <returns></returns>
        ReviewView PublishReview(
            string appId,
            string reviewId,
            string profileId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="reviewId"></param>
        /// <param name="profileId"></param>
        /// <returns></returns>
        ReviewView DeleteReview(
            string appId,
            string reviewId,
            string profileId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="merchantProfileId"></param>
        /// <param name="includeDrafts"></param>
        /// <param name="includeOnlyDrafts"></param>
        /// <returns></returns>
        IList<ReviewView> GetReviews(
            string appId,
            string merchantProfileId,
            bool includeDrafts,
            bool includeOnlyDrafts);
    }
}
