using System.Collections.Generic;
using System.Net;
using Classy.Auth;
using classy.Manager;
using Classy.Models.Request;
using Classy.Models.Response;
using ServiceStack.Common;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;

namespace classy.Services
{
    public class ReviewsService : Service
    {
        public IReviewManager ReviewManager { get; set; }
        public IProfileManager ProfileManager { get; set; }

        // post a review for listing
        [CustomAuthenticate]
        public object Post(PostReviewForListing request)
        {
            try
            {
                var session = SessionAs<CustomUserSession>();
                var review = ReviewManager.PostReviewForListing(
                    request.Environment.AppId,
                    session.UserAuthId,
                    request.ListingId,
                    request.Content,
                    request.Score,
                    request.SubCriteria);
                return new HttpResult(review, HttpStatusCode.OK);
            }
            catch (KeyNotFoundException kex)
            {
                return new HttpError(HttpStatusCode.NotFound, kex.Message);
            }
        }

        // post a review for profile
        [CustomAuthenticate]
        public object Post(PostReviewForProfile request)
        {
            try
            {
                var session = SessionAs<CustomUserSession>();
                var review = ReviewManager.PostReviewForProfile(
                    request.Environment.AppId,
                    session.UserAuthId,
                    request.RevieweeProfileId,
                    request.Content,
                    request.Score,
                    request.SubCriteria,
                    request.Metadata,
                    request.NewProfessionalContactInfo,
                    request.NewProfessionalMetadata);
                var response = new PostReviewResponse
                {
                    Review = review.TranslateTo<ReviewView>()
                };
                if (request.ReturnRevieweeProfile)
                    response.RevieweeProfile = ProfileManager.GetProfileById(
                        request.Environment.AppId,
                        request.RevieweeProfileId,
                        null,
                        false,
                        false,
                        false,
                        false,
                        false,
                        false,
                        false,
                        request.Environment.CultureCode);
                if (request.ReturnReviewerProfile)
                    response.ReviewerProfile = ProfileManager.GetProfileById(
                        request.Environment.AppId,
                        session.UserAuthId,
                        null,
                        false,
                        false,
                        false,
                        false,
                        false,
                        false,
                        false,
                        request.Environment.CultureCode);
                return new HttpResult(response, HttpStatusCode.OK);
            }
            catch (KeyNotFoundException kex)
            {
                return new HttpError(HttpStatusCode.NotFound, kex.Message);
            }
        }

        // get a list of reviews for profile
        [CustomAuthenticate]
        public object Get(GetReviewsByProfileId request)
        {
            try
            {
                var reviews = ReviewManager.GetReviews(
                    request.Environment.AppId,
                    request.ProfileId,
                    request.IncludeDrafts,
                    request.IncludeOnlyDrafts);

                return new HttpResult(reviews, HttpStatusCode.OK);
            }
            catch (KeyNotFoundException kex)
            {
                return new HttpError(HttpStatusCode.NotFound, kex.Message);
            }
        }

        // publish a review
        [CustomAuthenticate]
        public object Post(PublishOrDeleteReview request)
        {
            try
            {
                var session = SessionAs<CustomUserSession>();
                var review = ReviewManager.PublishReview(
                    request.Environment.AppId,
                    request.ReviewId,
                    session.UserAuthId);

                return new HttpResult(review, HttpStatusCode.OK);
            }
            catch (KeyNotFoundException kex)
            {
                return new HttpError(HttpStatusCode.NotFound, kex.Message);
            }
        }

        // delete a review
        public object Delete(PublishOrDeleteReview request)
        {
            try
            {
                var session = SessionAs<CustomUserSession>();
                var review = ReviewManager.DeleteReview(
                    request.Environment.AppId,
                    request.ReviewId,
                    session.UserAuthId);

                return new HttpResult(review, HttpStatusCode.OK);
            }
            catch (KeyNotFoundException kex)
            {
                return new HttpError(HttpStatusCode.NotFound, kex.Message);
            }
        }
    }
}