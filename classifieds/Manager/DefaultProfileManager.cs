﻿using Classy.Models;
using Classy.Models.Response;
using Classy.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceStack.Common;
using Classy.Auth;
using Classy.Models.Request;
using ServiceStack.Text;
using ServiceStack.ServiceClient.Web;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
        private IStorageRepository StorageRepository;

        public DefaultProfileManager(
            IAppManager appManager,
            IProfileRepository profileRepository,
            IListingRepository listingRepository,
            IReviewRepository reviewRepository,
            ICollectionRepository collectionRepository,
            ITripleStore tripleStore,
            IStorageRepository storageRepository)
        {
            AppManager = appManager;
            ProfileRepository = profileRepository;
            ListingRepository = listingRepository;
            ReviewRepository = reviewRepository;
            CollectionRepository = collectionRepository;
            TripleStore = tripleStore;
            StorageRepository = storageRepository;
        }

        public ManagerSecurityContext SecurityContext { get; set; }

        public ProfileView CreateProfileProxy(
            string appId,
            string requestedByProfileId,
            string batchId,
            ProfessionalInfo professionalInfo,
            IDictionary<string, string> metadata)
        {
            if (professionalInfo == null) throw new ArgumentNullException("professionalInfo cannot be null");

            if (metadata == null)
            {
                metadata = new Dictionary<string, string>();
            }
            // log the user that created this, and the batch id (so we can find all related records)
            metadata.Add("CreatedBy", requestedByProfileId);
            metadata.Add("BatchId", batchId);
            // mark as a proxy
            professionalInfo.IsProxy = true;
            // create the profile
            var app = AppManager.GetAppById(appId);
            var profile = new Profile
            {
                AppId = appId,
                ProfessionalInfo = professionalInfo,
                Metadata = metadata
            };
            // save in repo
            ProfileRepository.Save(profile);

            return profile.ToProfileView();
        }

        public SearchResultsView<ProfileView> SearchProfiles(
            string appId,
            string searchQuery,
            string category,
            Location location,
            IDictionary<string, string> metadata,
            bool professionalsOnly,
            bool ignoreLocation,
            int page,
            int pageSize,
            string culture)
        {
            long count = 0;
            var profileList = ProfileRepository.Search(appId, searchQuery, category, location, metadata, professionalsOnly, ignoreLocation, page, pageSize, ref count, culture);
            IList<ProfileView> results = new List<ProfileView>();
            foreach (var profile in profileList)
            {
                results.Add(profile.ToProfileView());
            }
            return new SearchResultsView<ProfileView> { Results = results, Count = count };
        }

        public void FollowProfile(
            string appId,
            string profileId,
            string followeeProfileId)
        {
            var follower = ProfileRepository.GetById(appId, profileId, false, null);
            var followee = ProfileRepository.GetById(appId, followeeProfileId, false, null);
            if (followee == null) throw new KeyNotFoundException("invalid username");

            // save triple
            int count = 0;
            TripleStore.LogActivity(appId, follower.Id, ActivityPredicate.FOLLOW_PROFILE, followee.Id, ref count);
            if (count == 1)
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
            bool includeFavorites,
            bool logImpression,
            string culture)
        {
            var profile = ProfileRepository.GetById(appId, profileId, logImpression, culture);
            if (profile == null) throw new KeyNotFoundException("invalid profile id");

            var profileView = profile.ToProfileView();

            if (includeFollowedByProfiles)
            {
                var profileIds = TripleStore.GetActivitySubjectList(appId, ActivityPredicate.FOLLOW_PROFILE, profileId);
                var followedby = ProfileRepository.GetByIds(appId, profileIds.ToArray(), culture);
                profileView.FollowedBy = followedby.ToProfileViewList();
            }

            if (includeFollowingProfiles)
            {
                var profileIds = TripleStore.GetActivityObjectList(appId, ActivityPredicate.FOLLOW_PROFILE, profileId);
                var following = ProfileRepository.GetByIds(appId, profile.FolloweeProfileIds.ToArray(), culture);
                profileView.Following = following.ToProfileViewList();
            }

            if (includeReviews)
            {
                var reviews = ReviewRepository.GetByRevieweeProfileId(appId, profileId, false, false);
                profileView.Reviews = reviews.ToReviewViewList();
                var reviewers = ProfileRepository.GetByIds(appId, reviews.Select(x => x.ProfileId).ToArray(), culture);
                foreach (var r in profileView.Reviews)
                {
                    r.ReviewerUsername = reviewers.Single(x => x.Id == r.ProfileId).UserName;
                    r.ReviewerThumbnailUrl = reviewers.Single(x => x.Id == r.ProfileId).Avatar.Url;
                }
            }

            if (includeListings)
            {
                var listings = ListingRepository.GetByProfileId(appId, profileId, false, culture);
                profileView.Listings = listings.ToListingViewList();
            }

            if (includeCollections)
            {
                var collections = CollectionRepository.GetByProfileId(appId, profileId, culture);
                profileView.Collections = collections.ToCollectionViewList();
                foreach (var c in profileView.Collections)
                {
                    if (c.IncludedListings != null)
                    {
                        c.Listings = ListingRepository.GetById(c.IncludedListings.Select(l => l.Id).ToArray(), appId, false, culture).ToListingViewList();
                        if (c.CoverPhotos == null || c.CoverPhotos.Count == 0)
                        {
                            c.CoverPhotos = ListingRepository.GetById(c.IncludedListings.Select(l => l.Id).Skip(Math.Max(0, c.IncludedListings.Count - 4)).ToArray(), appId, false, null).Select(l => l.ExternalMedia[0].Key).ToArray();
                        }
                    }
                    else
                    {
                        c.Listings = new List<ListingView>();
                    }
                }
            }

            if (includeFavorites)
            {
                var listingIds = TripleStore.GetActivityObjectList(appId, ActivityPredicate.FAVORITE_LISTING, profileId);
                profileView.FavoriteListingIds = listingIds;
            }

            if (logImpression)
            {
                int count = 1;
                TripleStore.LogActivity(appId, requestedByProfileId.IsNullOrEmpty() ? "guest" : requestedByProfileId, ActivityPredicate.VIEW_PROFILE, profileId, ref count);
            }

            // TODO: if requested by someone other than the profile owner, remove all non-public data!!

            return profileView;
        }

        public ProfileView UpdateProfile(
            string appId,
            string profileId,
            ContactInfo contactInfo,
            ProfessionalInfo professionalInfo,
            IDictionary<string, string> metadata,
            ProfileUpdateFields fields,
            byte[] profileImage,
            string profileImageContentType,
            string defaultCulture)
        {
            var profile = GetVerifiedProfile(appId, profileId);
            var rankInc = 0;

            // copy seller info
            if (fields.HasFlag(ProfileUpdateFields.ProfessionalInfo))
            {
                if (profile.ProfessionalInfo != null)
                {
                    // increase rank if company contact info fields have been entered for the first time
                    if (profile.ProfessionalInfo.CompanyContactInfo == null && professionalInfo.CompanyContactInfo != null) rankInc++;
                }
                profile.ProfessionalInfo = professionalInfo;
                TryGeocoding(profile.ProfessionalInfo);
            }

            // copy metadata 
            if (fields.HasFlag(ProfileUpdateFields.Metadata))
            {
                // increase rank if metadata fields have been entered for the first time
                if (profile.Metadata == null || (metadata.Count > profile.Metadata.Count)) rankInc++;

                if (profile.Metadata != null)
                {
                    foreach (var attribute in metadata)
                    {
                        profile.Metadata[attribute.Key] = attribute.Value;
                    }
                }
                else profile.Metadata = metadata;
            }

            // copy contact info
            if (fields.HasFlag(ProfileUpdateFields.ContactInfo)) profile.ContactInfo = contactInfo;

            // image 
            if (fields.HasFlag(ProfileUpdateFields.ProfileImage))
            {
                // increase rank if profile image fields is uploaded for the first time
                if (profile.Avatar == null) rankInc++;

                var avatarKey = string.Concat("profile_img_", profile.Id, "_", Guid.NewGuid().ToString());
                StorageRepository.SaveFile(avatarKey, profileImage, profileImageContentType);
                profile.Avatar = new MediaFile
                {
                    Key = avatarKey,
                    Url = StorageRepository.KeyToUrl(avatarKey),
                    ContentType = profileImageContentType,
                    Type = MediaFileType.Image
                };
            }

            // cover photos
            if (fields.HasFlag(ProfileUpdateFields.CoverPhotos))
            {
                if (profile.ProfessionalInfo.CoverPhotos == null) rankInc++;

                profile.ProfessionalInfo.CoverPhotos = professionalInfo.CoverPhotos;
            }

            if (string.IsNullOrEmpty(profile.DefaultCulture))
            {
                profile.DefaultCulture = defaultCulture;
            }

            profile.Rank += rankInc;
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

            var claim = new ProxyClaim
            {
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
            var rankInc = 0;

            // copy seller info from proxy to profile
            if (claim.ProfessionalInfo != null)
            {
                if (claim.ProfessionalInfo.CompanyContactInfo != null) rankInc++;
            }
            profile.ProfessionalInfo = claim.ProfessionalInfo;

            // copy metadata from proxy to profile
            if (profile.Metadata != null)
            {
                if (claim.Metadata != null)
                {
                    rankInc++;
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
            profile.Rank += rankInc;
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
            int count = 1;
            TripleStore.LogActivity(appId, reviewerProfileId, ActivityPredicate.REVIEW_PROFILE, listing.Id, ref count);
            if (count > 1) throw new ApplicationException("this user already submitted a review for this listing");

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
            int count = 1;
            TripleStore.LogActivity(appId, reviewerProfileId, ActivityPredicate.REVIEW_PROFILE, revieweeProfileId, ref count);
            if (count > 1) throw new ArgumentException("AlreadyReviewed");

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
            var review = GetVerifiedReview(appId, reviewId);
            if (review.Score > 3) ProfileRepository.IncreaseCounter(appId, profileId, ProfileCounters.Rank, 1);
            ReviewRepository.Publish(appId, reviewId);
            review.IsPublished = true;
            return review.TranslateTo<ReviewView>();
        }

        public ReviewView DeleteReview(
            string appId,
            string reviewId,
            string profileId)
        {
            var review = GetVerifiedReview(appId, reviewId);
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

        public IList<SocialPhotoAlbumView> GetFacebookAlbums(
            string appId,
            string profileId,
            string token)
        {
            var profile = GetVerifiedProfile(appId, profileId);
            profile.FacebookUserId.ThrowIfNullOrEmpty("this profile is not connected to facebook");

            var url = "https://graph.facebook.com/me/albums?access_token={0}&fields=id,name,count,photos".Fmt(token);
            var albums = url.GetStringFromUrl();
            var albumsObj = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(albums);
            var albumList = new List<SocialPhotoAlbumView>();
            foreach (dynamic a in albumsObj["data"])
            {
                var album = new SocialPhotoAlbumView
                {
                    Id = a["id"],
                    Name = a["name"],
                    PhotoCount = a["count"]
                };
                if (a["photos"] != null)
                {
                    album.Photos = new List<SocialPhotoView>();
                    foreach (var p in a["photos"]["data"])
                    {
                        album.Photos.Add(new SocialPhotoView
                        {
                            Id = p["id"],
                            Url = p["source"]
                        });
                    }
                }
                albumList.Add(album);
            }
            return albumList;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="profileId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public IEnumerable<EmailContact> GetGoogleContacts(string appId, string profileId, string token)
        {
            try
            {
                IList<EmailContact> contacts = new List<EmailContact>();
                IList<EmailContact> noNameContacts = new List<EmailContact>();

                var profile = GetVerifiedProfile(appId, profileId);
                var url = string.Format("https://www.google.com/m8/feeds/contacts/default/full?access_token={0}&max-results=1000&alt=json", token);

                var contactsJson = url.GetStringFromUrl();
                JObject data = (JObject)JsonConvert.DeserializeObject(contactsJson);
                foreach (var entry in data["feed"]["entry"])
                {
                    var photoLink = entry["link"].FirstOrDefault(l => l.Value<string>("rel") == "http://schemas.google.com/contacts/2008/rel#photo");
                    if (entry["gd$email"] != null)
                    {
                        if (!string.IsNullOrEmpty(entry["title"].Value<string>("$t")))
                        {
                            contacts.Add(new EmailContact
                            {
                                Email = entry["gd$email"][0].Value<string>("address"),
                                Name = entry["title"].Value<string>("$t")
                                //ImageUrl = photoLink == null ? null : string.Format("{0}?access_token={1}", photoLink.Value<string>("href"), token)
                            });
                        }
                        else
                        {
                            noNameContacts.Add(new EmailContact
                            {
                                Email = entry["gd$email"][0].Value<string>("address"),
                                Name = string.Empty
                                //ImageUrl = photoLink == null ? null : string.Format("{0}?access_token={1}", photoLink.Value<string>("href"), token)
                            });
                        }
                    }
                }


                List<EmailContact> result = new List<EmailContact>(contacts.OrderBy(c => c.Name));
                result.AddRange(noNameContacts);
                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="profileId"></param>
        /// <param name="profileTranslation"></param>
        public void SetTranslation(string appId, string profileId, ProfileTranslation profileTranslation)
        {
            if (SecurityContext.IsAdmin || SecurityContext.AuthenticatedProfileId == profileId)
            {
                Profile profile = GetVerifiedProfile(appId, profileId);
                if (profile.Translations == null)
                {
                    profile.Translations = new Dictionary<string, ProfileTranslation>();
                }
                profile.Translations[profileTranslation.Culture] = profileTranslation;
                ProfileRepository.Save(profile);
            }
        }

        public ProfileTranslationView GetTranslation(string appId, string profileId, string culture)
        {
            Profile profile = GetVerifiedProfile(appId, profileId);
            ProfileTranslation translation = null;
            if (profile.Translations == null || !profile.Translations.TryGetValue(culture, out translation))
            {
                if (culture == profile.DefaultCulture || string.IsNullOrEmpty(culture))
                {
                    return new ProfileTranslationView { CultureCode = culture, Metadata = new Dictionary<string, string>(profile.Metadata) };
                }
                return new ProfileTranslationView { CultureCode = culture, Metadata = new Dictionary<string, string>() };
            }

            return new ProfileTranslationView { CultureCode = culture, Metadata = new Dictionary<string,string>(translation.Metadata) };
        }

        public void DeleteTranslation(string appId, string profileId, string culture)
        {
            if (SecurityContext.IsAdmin || SecurityContext.AuthenticatedProfileId == profileId)
            {
                Profile profile = GetVerifiedProfile(appId, profileId);
                if (profile.Translations != null)
                {
                    profile.Translations.Remove(culture);
                    ProfileRepository.Save(profile);
                }
            }
        }

        private Profile GetVerifiedProfile(string appId, string profileId)
        {
            return GetVerifiedProfile(appId, profileId, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="profileId"></param>
        /// <returns></returns>
        private Profile GetVerifiedProfile(string appId, string profileId, string culture)
        {
            var profile = ProfileRepository.GetById(appId, profileId, false, culture);
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
            var listing = ListingRepository.GetById(listingId, appId, false, null);
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
        private Review GetVerifiedReview(string appId, string reviewId)
        {
            var review = ReviewRepository.GetById(appId, reviewId);
            if (review == null) throw new KeyNotFoundException("invalid review");
            return review;
        }

        private void TryGeocoding(ProfessionalInfo professionalInfo)
        {
            if (professionalInfo.CompanyContactInfo != null &&
                professionalInfo.CompanyContactInfo.Location != null &&
                professionalInfo.CompanyContactInfo.Location.Coords != null)
            {
                // Do geocoding!!!
                professionalInfo.CompanyContactInfo.Location.Coords.Longitude = 0;
                professionalInfo.CompanyContactInfo.Location.Coords.Latitude = 0;
            }
        }
    }
}