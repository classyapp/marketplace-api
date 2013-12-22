using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Classy.Models;
using Classy.Models.Response;
using Classy.Repository;
using Classy.Auth;
using ServiceStack.Common;
using ServiceStack.ServiceHost;
using System.IO;

namespace classy.Manager
{
    public class DefaultListingManager : IListingManager
    {
        private IListingRepository ListingRepository;
        private ICommentRepository CommentRepository;
        private IProfileRepository ProfileRepository;
        private ITripleStore TripleStore;
        private IStorageRepository StorageRepository;

        public DefaultListingManager(
            IListingRepository listingRepository,
            ICommentRepository commentRepository,
            IProfileRepository profileRepository,
            ITripleStore tripleStore,
            IStorageRepository storageRepository)
        {
            ListingRepository = listingRepository;
            CommentRepository = commentRepository;
            ProfileRepository = profileRepository;
            TripleStore = tripleStore;
            StorageRepository = storageRepository;
        }

        public ListingView GetListingById(
            string appId, 
            string listingId, 
            string requestedByProfileId,
            bool logImpression, 
            bool includeDrafts, 
            bool includeComments,
            bool formatCommentsAsHtml,
            bool includeCommenterProfiles,
            bool includeProfile,
            bool includeFavoritedByProfiles)
        {
            // TODO: cache listings
            var listing = GetVerifiedListing(appId, listingId, logImpression);
            var listingView = listing.ToListingView();

            if (includeComments)
            {
                var comments = CommentRepository.GetByListingId(listingId, formatCommentsAsHtml);
                IList<Profile> commenterProfiles = null;
                if (includeCommenterProfiles) commenterProfiles = ProfileRepository.GetByIds(appId, (from c in comments select c.ProfileId).ToArray());
                listingView.Comments = new List<CommentView>();
                foreach (var c in comments)
                {
                    var comment = c.TranslateTo<CommentView>();
                    if (commenterProfiles != null)
                    {
                        comment.Profile = (from p in commenterProfiles where p.Id == comment.ProfileId select p).Single().ToProfileView();
                    }
                    listingView.Comments.Add(comment.TranslateTo<CommentView>());
                }
            }
            if (includeProfile)
            {
                listingView.Profile = ProfileRepository.GetById(appId, listing.ProfileId, false).ToProfileView();
            }
            if (includeFavoritedByProfiles)
            {
                var favProfileIds = TripleStore.GetActivitySubjectList(appId, Classy.Models.ActivityPredicate.Like, listing.Id).ToArray();
                var favProfiles = ProfileRepository.GetByIds(appId, favProfileIds);
                listingView.FavoritedBy = new List<ProfileView>();
                foreach (var p in favProfiles)
                {
                    listingView.FavoritedBy.Add(p.ToProfileView());
                }
            }
            if (logImpression)
            {
                var exists = false;
                TripleStore.LogActivity(appId, requestedByProfileId.IsNullOrEmpty() ? "guest" : requestedByProfileId, Classy.Models.ActivityPredicate.View, listing.Id, ref exists);
            }
            return listingView;
        }

        public IList<ListingView> GetListingsByUsername(
            string appId, 
            string username, 
            bool includeComments, 
            bool formatCommentsAsHtml)
        {
            var profile = ProfileRepository.GetByUsername(appId, username, false);
            if (profile == null) throw new KeyNotFoundException("invalid username");

            // TODO: cache listings
            var listings = ListingRepository.GetByProfileId(profile.Id, appId, false);
            var comments = includeComments ?
                CommentRepository.GetByListingIds(listings.Select(x => x.Id).AsEnumerable(), formatCommentsAsHtml) : null;
            var listingViews = new List<ListingView>();
            foreach (var c in listings)
            {
                var view = c.ToListingView();
                if (includeComments)
                {
                    view.Comments = comments.Where(x => x.ListingId == view.Id).ToCommentViewList();
                }
                listingViews.Add(view);
            }
            return listingViews;
        }

        public IList<ListingView> SearchListings(
            string appId,
            string tag, 
            string listingType,
            IList<CustomAttribute> metadata, 
            double? priceMin, 
            double? priceMax, 
            Location location, 
            bool includeComments, 
            bool formatCommentsAsHtml)
        {
            // TODO: cache listings
            tag = string.IsNullOrEmpty(tag) ? null : string.Concat("#", tag.TrimEnd(new char[] { '#' }));
            var listings = ListingRepository.Search(tag, listingType, metadata, priceMin, priceMax, location, appId, false, true);
            var comments = includeComments ?
                CommentRepository.GetByListingIds(listings.Select(x => x.Id).AsEnumerable(), formatCommentsAsHtml) : null;
            var listingViews = new List<ListingView>();
            foreach (var c in listings)
            {
                var view = c.ToListingView();
                if (includeComments)
                {
                    view.Comments = comments.Where(x => x.ListingId == view.Id).ToCommentViewList();
                }
                listingViews.Add(view);
            }
            return listingViews;
        }

        public ListingView AddExternalMediaToListing(
            string appId, 
            string listingId, 
            string profileId, 
            IFile[] files)
        {
            var listing = GetVerifiedListing(appId, listingId, profileId);
            var mediaFiles = new List<MediaFile>();

            if (files != null && files.Count() > 0)
            {
                foreach (IFile file in files)
                {
                    if (file.ContentType.Contains("image")) // only images for now
                    {
                        using (var reader = new BinaryReader(file.InputStream))
                        {
                            var key = Guid.NewGuid().ToString();
                            var content = reader.ReadBytes((int)file.ContentLength);
                            StorageRepository.SaveFile(key, content, file.ContentType);
                            mediaFiles.Add(new MediaFile
                            {
                                ContentType = file.ContentType,
                                Url = StorageRepository.KeyToUrl(key)
                            });
                        }
                    }
                }
                ListingRepository.AddExternalMedia(listingId, appId, mediaFiles);
            }
            listing.ExternalMedia.Union(mediaFiles);

            return listing.ToListingView();
        }

        public ListingView DeleteExternalMediaFromListing(
            string appId, 
            string listingId, 
            string profileId, 
            string url)
        {
            var listing = GetVerifiedListing(appId, listingId, profileId);

            ListingRepository.DeleteExternalMedia(listingId, appId, url);
            StorageRepository.DeleteFile(url);

            listing.ExternalMedia.Remove(listing.ExternalMedia.SingleOrDefault(x => x.Url == url));

            return listing.ToListingView();
        }

        public ListingView PublishListing(
            string appId,
            string listingId,
            string profileId)
        {
            var listing = GetVerifiedListing(appId, listingId, profileId);
            var profile = ProfileRepository.GetById(appId, listing.ProfileId, false);

            // can't publish a purchasable listing if 
            if ((listing.Pricing != null || listing.SchedulingTemplate != null) && !profile.IsVerifiedSeller)
                throw new ApplicationException("a listing with pricing or scheduling information can only be published by a merchant profile");
            
            // publish
            ListingRepository.Publish(listingId, appId);
            listing.IsPublished = true;
            return listing.ToListingView();
        }

        public ListingView SaveListing(
            string appId,
            string listingId,
            string profileId,
            string title,
            string content,
            string listingType,
            PricingInfo pricingInfo,
            ContactInfo contactInfo,
            TimeslotSchedule timeslotSchedule,
            IList<CustomAttribute> customAttributes)
        {
            Listing listing;
            bool isNewListing = listingId.IsNullOrEmpty();
            if (!isNewListing) {
                listing = GetVerifiedListing(appId, listingId);
            }
            else {
                listing = new Listing
                {
                    ProfileId = profileId,
                    AppId = appId
                };
            }
            
            // include basic listing info
            if (!title.IsNullOrEmpty()) listing.Title = title;
            if (!listingType.IsNullOrEmpty()) listing.ListingType = listingType;
            if (!content.IsNullOrEmpty()) 
            {
                listing.Content = content;
                listing.Hashtags = content.ExtractHashtags();
            }
            if (pricingInfo != null) listing.Pricing = pricingInfo;
            if (contactInfo != null) listing.ContactInfo = contactInfo;
            if (timeslotSchedule != null) listing.SchedulingTemplate = timeslotSchedule;
            if (customAttributes != null)
            {
                foreach (var c in customAttributes)
                {
                    if (listing.Metadata.ContainsKey(c.Key))
                    {
                        listing.Metadata.Remove(listing.Metadata.SingleOrDefault(x => x.Key == c.Key));
                    }
                    listing.Metadata.Add(c);
                }
            }
            if (isNewListing)
            {
                listing.Id = ListingRepository.Insert(listing);
            }
            else
            {
                ListingRepository.Update(listing);
            }
            
            // return
            return listing.ToListingView();
        }

        public CommentView AddCommentToListing(
            string appId,
            string listingId,
            string profileId,
            string content,
            bool formatAsHHtml)
        {
            var listing = GetVerifiedListing(appId, listingId);

            // save to repository
            var comment = new Comment
            {
                AppId = appId,
                ProfileId = profileId,
                ListingId = listingId,
                Content = content
            };
            
            comment.Id = CommentRepository.Save(comment);

            // increase comment count for listing
            ListingRepository.IncreaseCounter(listingId, appId, ListingCounters.Comments, 1);

            // increase comment count for profile of commenter
            ProfileRepository.IncreaseCounter(appId, profileId, ProfileCounters.Comments, 1);

            // increase rank of listing owner
            ProfileRepository.IncreaseCounter(appId, listing.ProfileId, ProfileCounters.Rank, 1);

            // add hashtags to listing if the comment is by the listing owner
            if (profileId == listing.ProfileId)
            {
                ListingRepository.AddHashtags(listingId, appId, content.ExtractHashtags());
            }

            // log a comment activity
            var exists = false;
            TripleStore.LogActivity(appId, profileId, Classy.Models.ActivityPredicate.Comment, listingId, ref exists);

            // save mentions
            foreach (var mentionedUsername in comment.Content.ExtractUsernames())
            {
                var mentionedProfile = ProfileRepository.GetByUsername(appId, mentionedUsername.TrimStart('@'), false);
                TripleStore.LogActivity(appId, profileId, Classy.Models.ActivityPredicate.Mention, mentionedProfile.Id, ref exists);

                // increase rank of mentioned profile
                ProfileRepository.IncreaseCounter(appId, mentionedProfile.Id, ProfileCounters.Rank, 1);
            }

            // format as html
            if (formatAsHHtml) comment.Content = comment.Content.FormatAsHtml();

            return comment.TranslateTo<CommentView>();
        }

        public void FavoriteListing(
            string appId, 
            string listingId, 
            string profileId)
        {
            var listing = GetVerifiedListing(appId, listingId);

            bool tripleExists = false;
            TripleStore.LogActivity(appId, profileId, Classy.Models.ActivityPredicate.Like, listingId, ref tripleExists);
            if (!tripleExists)
            {
                ListingRepository.IncreaseCounter(listingId, appId, ListingCounters.Favorites, 1);
                ProfileRepository.IncreaseCounter(appId, listing.ProfileId, ProfileCounters.Rank, 1);
            }
        }

        public void FlagListing(
            string appId,
            string listingId,
            string profileId,
            FlagReason FlagReason)
        {
            var listing = GetVerifiedListing(appId, listingId);

            switch (FlagReason)
            {
                case FlagReason.Inapropriate:
                    ListingRepository.IncreaseCounter(listingId, appId, ListingCounters.Flags, 1);
                    ProfileRepository.IncreaseCounter(appId, profileId, ProfileCounters.Rank, -3);
                    break;
                case FlagReason.Dislike:
                default:
                    bool tripleExists = false;
                    TripleStore.LogActivity(appId, profileId, Classy.Models.ActivityPredicate.Flag, listingId, ref tripleExists);
                    break;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="listingId"></param>
        /// <param name="profileId"></param>
        /// <param name="logImpression"></param>
        /// <returns></returns>
        private Listing GetVerifiedListing(string appId, string listingId, string profileId, bool increaseViewCounter)
        {
            Listing listing;
            try
            {
                listing = GetVerifiedListing(appId, listingId, increaseViewCounter);
            }
            catch (KeyNotFoundException kex)
            {
                throw;
            }
            if (listing.ProfileId != profileId) throw new UnauthorizedAccessException("not authorized");
            return listing;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="listingId"></param>
        /// <param name="profileId"></param>
        /// <returns></returns>
        private Listing GetVerifiedListing(string appId, string listingId, string profileId)
        {
            return GetVerifiedListing(appId, listingId, profileId, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="listingId"></param>
        /// <returns></returns>
        private Listing GetVerifiedListing(string appId, string listingId)
        {
            return GetVerifiedListing(appId, listingId, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="listingId"></param>
        /// <param name="logImpression"></param>
        /// <returns></returns>
        private Listing GetVerifiedListing(string appId, string listingId, bool increaseViewCounter)
        {
            var listing = ListingRepository.GetById(listingId, appId, true, increaseViewCounter);
            if (listing == null) throw new KeyNotFoundException("invalid listing");
            return listing;
        }
    }
}