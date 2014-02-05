using Classy.Models;
using Classy.Models.Response;
using Classy.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceStack.Common;
using Classy.Auth;

namespace classy.Manager
{
    public class DefaultProfileManager : IProfileManager, IReviewManager
    {
        private IAppManager AppManager;
        private IProfileRepository ProfileRepository;
        private IListingRepository ListingRepository;
        private IReviewRepository ReviewRepository;
        private ICollectionRepository CollectionRepository;
        private ITripleStore TripleStore;

        public DefaultProfileManager(
            IAppManager appManager,
            IProfileRepository profileRepository,
            IListingRepository listingRepository,
            IReviewRepository reviewRepository,
            ICollectionRepository collectionRepository,
            ITripleStore tripleStore)
        {
            AppManager = appManager;
            ProfileRepository = profileRepository;
            ListingRepository = listingRepository;
            ReviewRepository = reviewRepository;
            CollectionRepository = collectionRepository;
            TripleStore = tripleStore;
        }

        public ProfileView CreateProfileProxy(
            string appId,
            ProfessionalInfo professionalInfo,
            IDictionary<string, string> metadata)
        {
            if (metadata == null)
            {
                metadata = new Dictionary<string, string>();
            }
            professionalInfo.IsProxy = true;
            var profile = new Profile
            {
                AppId = appId,
                ProfessionalInfo = professionalInfo,
                Metadata = metadata
            };

            ProfileRepository.Save(profile);

            return profile.ToProfileView();
        }

        public IList<ProfileView> SearchProfiles(
            string appId,
            string partialUserName,
            string category,
            Location location,
            IDictionary<string, string> metadata,
            bool professionalsOnly)
        {
            var profileList = ProfileRepository.Search(appId, partialUserName, category, location, metadata, professionalsOnly);
            IList<ProfileView> results = new List<ProfileView>();
            foreach(var profile in profileList)
            {
                results.Add(profile.ToProfileView());
            }
            return results;
        }

        public void FollowProfile(
            string appId,
            string profileId,
            string followeeProfileId)
        {
            var follower = ProfileRepository.GetById(appId, profileId, false);
            var followee = ProfileRepository.GetById(appId, followeeProfileId, false);
            if (followee == null) throw new KeyNotFoundException("invalid username");

            // save triple
            bool tripleExists = false;
            TripleStore.LogActivity(appId, follower.Id, ActivityPredicate.FOLLOW_PROFILE, followee.Id, ref tripleExists);
            if (!tripleExists)
            {
                // increase follower count
                ProfileRepository.IncreaseCounter(appId, followee.Id, ProfileCounters.Followers | ProfileCounters.Rank, 1);

                // add followee and save follower profile
                follower.FolloweeProfileIds.Add(followeeProfileId);
                follower.FollowingCount++;
                ProfileRepository.Save(follower);
            }
        }

        public ProfileView GetProfileById(
            string appId,
            string profileId,
            string requestedByProfileId,
            bool includeFollowedByProfiles,
            bool includeFollowingProfiles,
            bool includeReviews,
            bool includeListings,
            bool includeCollections,
            bool logImpression)
        {
            var profile = ProfileRepository.GetById(appId, profileId, logImpression);
            if (profile == null) throw new KeyNotFoundException("invalid profile id");

            var profileView = profile.ToProfileView();

            if (includeFollowedByProfiles) 
            {
                var profileIds = TripleStore.GetActivitySubjectList(appId, ActivityPredicate.FOLLOW_PROFILE, profileId);
                var followedby = ProfileRepository.GetByIds(appId, profileIds.ToArray());
                profileView.FollowedBy = followedby.ToProfileViewList();
            }

            if (includeFollowingProfiles)
            {
                var profileIds = TripleStore.GetActivityObjectList(appId, ActivityPredicate.FOLLOW_PROFILE, profileId);
                var following = ProfileRepository.GetByIds(appId, profile.FolloweeProfileIds.ToArray());
                profileView.Following = following.ToProfileViewList();
            }

            if (includeReviews)
            {
                var reviews = ReviewRepository.GetByRevieweeProfileId(appId, profileId, false, false);
                profileView.Reviews = reviews.ToReviewViewList();
                var reviewers = ProfileRepository.GetByIds(appId, reviews.Select(x => x.ProfileId).ToArray());
                foreach (var r in profileView.Reviews)
                {
                    r.ReviewerUsername = reviewers.Single(x => x.Id == r.ProfileId).UserName;
                    r.ReviewerThumbnailUrl = reviewers.Single(x => x.Id == r.ProfileId).ThumbnailUrl;
                }
            }

            if (includeListings)
            {
                var listings = ListingRepository.GetByProfileId(appId, profileId, false);
                profileView.Listings = listings.ToListingViewList();
            }

            if (includeCollections)
            {
                var collections = CollectionRepository.GetByProfileId(appId, profileId);
                profileView.Collections = collections.ToCollectionViewList();
                foreach(var c in profileView.Collections)
                {
                    if (c.IncludedListings != null)
                    {
                        c.Listings = ListingRepository.GetById(c.IncludedListings.Select(l => l.ListingId).ToArray(), appId, false).ToListingViewList();
                    }
                    else
                    {
                        c.Listings = new List<ListingView>();
                    }
                }
            }

            if (logImpression)
            {
                var exists = false;
                TripleStore.LogActivity(appId, requestedByProfileId.IsNullOrEmpty() ? "guest" : requestedByProfileId, ActivityPredicate.VIEW_PROFILE, profileId, ref exists);
            }

            // TODO: if requested by someone other than the profile owner, remove all non-public data!!

            return profileView;
        }

        public ProfileView UpdateProfile(
            string appId,
            string profileId,
            ProfessionalInfo ProfessionalInfo,
            IDictionary<string, string> metadata)
        {
            var profile = GetVerifiedProfile(appId, profileId);

            // copy seller info
            profile.ProfessionalInfo = ProfessionalInfo;
            // copy metadata 
            if (profile.Metadata != null)
            {
                if (metadata != null)
                {
                    foreach (var attribute in metadata)
                    {
                        profile.Metadata[attribute.Key] = attribute.Value;
                    }
                }
            }
            else profile.Metadata = metadata;

            ProfileRepository.Save(profile);
            return profile.ToProfileView();
        }

        public ProxyClaimView SubmitProxyClaim(
            string appId,
            string profileId,
            string proxyProfileId,
            ProfessionalInfo ProfessionalInfo,
            IDictionary<string, string> metadata)
        {
            var proxyProfile = GetVerifiedProfile(appId, proxyProfileId);
            if (!proxyProfile.IsProxy) throw new ApplicationException("can't claim. not a proxy.");

            var claim = new ProxyClaim {
                AppId = appId,
                ProfileId = profileId,
                ProxyProfileId = proxyProfileId,
                ProfessionalInfo = ProfessionalInfo,
                Metadata = metadata
            };
            ProfileRepository.SaveProxyClaim(claim);

            var app = AppManager.GetAppById(appId);
            if (!app.ProxyClaimNeedsVerification)
            {
                ApproveProxyClaim(claim.AppId, claim.Id);
            }

            return claim.ToProxyClaimView();
        }

        // TODO: make sure only admins can do this
        public ProxyClaimView ApproveProxyClaim(
            string appId, 
            string claimId)
        {
            var claim = ProfileRepository.GetProxyClaimById(appId, claimId);
            if (claim == null) throw new KeyNotFoundException("invalid proxy claim");
            var profile = GetVerifiedProfile(appId, claim.ProfileId);
            var proxyProfile = GetVerifiedProfile(appId, claim.ProxyProfileId);

            // copy seller info from proxy to profile
            profile.ProfessionalInfo = claim.ProfessionalInfo;
            // copy metadata from proxy to profile
            if (profile.Metadata != null) {
                if (claim.Metadata != null)
                {
                    foreach (var attribute in claim.Metadata)
                    {
                        profile.Metadata[attribute.Key] = attribute.Value;
                    }
                }
            }
            else profile.Metadata = claim.Metadata;
            // update reviews for proxy to be associated with profile
            // TODO
            // update listings associated with proxy to be associated with profile
            // TODO

            // save the new profile and delete the proxy
            ProfileRepository.Save(profile);
            ProfileRepository.Delete(proxyProfile.Id);

            // update claim status
            claim.Status = ProxyClaimStatus.Approved;
            ProfileRepository.SaveProxyClaim(claim);
            return claim.TranslateTo<ProxyClaimView>();
        }

        // TODO: make sure only admins can do this
        public ProxyClaimView RejectProxyClaim(
            string appId, 
            string claimId)
        {
            var claim = ProfileRepository.GetProxyClaimById(appId, claimId);
            if (claim == null) throw new KeyNotFoundException("invalid proxy claim");
            claim.Status = ProxyClaimStatus.Rejected;
            ProfileRepository.SaveProxyClaim(claim);
            return claim.TranslateTo<ProxyClaimView>();
        }

        public ReviewView PostReviewForListing(
            string appId,
            string reviewerProfileId,
            string listingId,
            string content,
            decimal score,
            IDictionary<string, decimal> subCriteria)
        {
            // get the listing
            var listing = GetVerifiedListing(appId, listingId);

            // get the merchant profile
            var revieweeProfile = GetVerifiedProfile(appId, listing.ProfileId);

            // get app info
            var app = AppManager.GetAppById(appId);

            // save the review
            var review = new Review
            {
                AppId = appId,
                ProfileId = reviewerProfileId,
                RevieweeProfileId = revieweeProfile.Id,
                ListingId = listingId,
                Content = content,
                Score = score,
                SubCriteria = subCriteria,
                IsPublished = app.AllowUnmoderatedReviews
            };
            ReviewRepository.Save(review);

            // log activity
            var exists = false;
            TripleStore.LogActivity(appId, reviewerProfileId, ActivityPredicate.REVIEW_PROFILE, listing.Id, ref exists);
            if (exists) throw new ApplicationException("this user already submitted a review for this listing");

            // increase the review count for the merchant, and the average score + avg score for all sub criteria
            // TODO : does this eve belog here? what about sub criteria for listings? for each listing type? aaaarghhh!!!
            revieweeProfile.ReviewAverageScore =
                ((revieweeProfile.ReviewAverageScore * revieweeProfile.ReviewCount) + score) / (++revieweeProfile.ReviewCount);
           
            ProfileRepository.Save(revieweeProfile);

            // return
            return review.TranslateTo<ReviewView>();
        }

        public ReviewView PostReviewForProfile(
            string appId,
            string reviewerProfileId,
            string revieweeProfileId,
            string content,
            decimal score,
            IDictionary<string, decimal> subCriteria,
            IDictionary<string, string> metadata,
            ContactInfo newProfessionalContactInfo,
            IDictionary<string, string> newProfessionalMetadata)
        {
            // get the merchant profile
            var revieweeProfile = GetVerifiedProfile(appId, revieweeProfileId);
            if (!revieweeProfile.IsProfessional && !revieweeProfile.IsProxy)
                throw new ArgumentException("ProfileNotProfessionalOrProxy");

            // get app info
            var app = AppManager.GetAppById(appId);

            // log activity
            var exists = false;
            TripleStore.LogActivity(appId, reviewerProfileId, ActivityPredicate.REVIEW_PROFILE, revieweeProfileId, ref exists);
            if (exists) throw new ArgumentException("AlreadyReviewed");

            // save the review
            var review = new Review
            {
                AppId = appId,
                ProfileId = reviewerProfileId,
                RevieweeProfileId = revieweeProfile.Id,
                ListingId = null,
                Content = content,
                Score = score,
                SubCriteria = subCriteria,
                Metadata = metadata,
                IsPublished = app.AllowUnmoderatedReviews
            };
            ReviewRepository.Save(review);

            // TODO: create a new proxy profile using the NewProfessionalMetadata and NewProfessionalContactInfo properties

            // increase the review count for the merchant, and the average score + avg score for all sub criteria
            revieweeProfile.ReviewAverageScore =
                ((revieweeProfile.ReviewAverageScore * revieweeProfile.ReviewCount) + score) / (++revieweeProfile.ReviewCount);
            if (revieweeProfile.ReviewAverageSubCriteria == null)
                revieweeProfile.ReviewAverageSubCriteria = new Dictionary<string, decimal>();
            foreach (var k in subCriteria.Keys)
            {
                decimal subScore = subCriteria[k];
                if (revieweeProfile.ReviewAverageSubCriteria.ContainsKey(k))
                {
                    revieweeProfile.ReviewAverageSubCriteria[k] = ((revieweeProfile.ReviewAverageSubCriteria[k] * revieweeProfile.ReviewCount) + subScore) / (revieweeProfile.ReviewCount);
                }
                else
                {
                    revieweeProfile.ReviewAverageSubCriteria.Add(k, subScore);
                }
            }
            ProfileRepository.Save(revieweeProfile);

            // return
            return review.TranslateTo<ReviewView>();
        }

        public ReviewView PublishReview(
            string appId,
            string reviewId,
            string profileId)
        {
            var review = GetVerifiedReview(appId, reviewId, profileId);
            ReviewRepository.Publish(appId, reviewId);
            review.IsPublished = true;
            return review.TranslateTo<ReviewView>();
        }

        public ReviewView DeleteReview(
            string appId,
            string reviewId,
            string profileId)
        {
            var review = GetVerifiedReview(appId, reviewId, profileId);
            ReviewRepository.Delete(appId, reviewId);
            review.IsDeleted = true;
            return review.TranslateTo<ReviewView>();
        }

        public IList<ReviewView> GetReviews(
            string appId,
            string revieweeProfileId,
            bool includeDrafts,
            bool includeOnlyDrafts)
        {
            var revieweeProfile = GetVerifiedProfile(appId, revieweeProfileId);
            var reviews = ReviewRepository.GetByRevieweeProfileId(appId, revieweeProfileId, includeDrafts, includeOnlyDrafts);
            return reviews.TranslateTo<List<ReviewView>>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="profileId"></param>
        /// <returns></returns>
        private Profile GetVerifiedProfile(string appId, string profileId)
        {
            var profile = ProfileRepository.GetById(appId, profileId, false);
            if (profile == null) throw new KeyNotFoundException("invalid profile");
            return profile;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="listingId"></param>
        /// <returns></returns>
        private Listing GetVerifiedListing(string appId, string listingId)
        {
            var listing = ListingRepository.GetById(listingId, appId, false);
            if (listing == null) throw new KeyNotFoundException("invalid listing");
            return listing;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="reviewId"></param>
        /// <param name="profileId"></param>
        /// <returns></returns>
        private Review GetVerifiedReview(string appId, string reviewId, string profileId)
        {
            var review = ReviewRepository.GetById(appId, reviewId);
            if (review == null) throw new KeyNotFoundException("invalid review");
            if (review.RevieweeProfileId != profileId) throw new UnauthorizedAccessException("unauthorized");
            return review;
        }
    }
}