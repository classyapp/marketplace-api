﻿using Classy.Models;
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
        /// <param name="subCriteria"></param>
        /// <returns></returns>
        ReviewView PostReviewForListing(
            string appId,
            string reviewerProfileId,
            string listingId,
            string content,
            decimal score,
            IDictionary<string, decimal> subCriteria);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="reviewerProfileId"></param>
        /// <param name="revieweeProfileId"></param>
        /// <param name="content"></param>
        /// <param name="score"></param>
        /// <param name="subCriteria"></param>
        /// <param name="metadata"></param>
        /// <param name="newProfessionalContactInfo"></param>
        /// <param name="newProfessionalMetadata"></param>
        /// <returns></returns>
        ReviewView PostReviewForProfile(
            string appId,
            string reviewerProfileId,
            string revieweeProfileId,
            string content,
            decimal score,
            IDictionary<string, decimal> subCriteria,
            IDictionary<string, string> metadata,
            ContactInfo newProfessionalContactInfo,
            IDictionary<string, string> newProfessionalMetadata);

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
