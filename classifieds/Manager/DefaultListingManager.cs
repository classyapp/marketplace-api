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
using ServiceStack.Messaging;
using classy.Operations;

namespace classy.Manager
{
    public class DefaultListingManager : IListingManager, ICollectionManager
    {
        private IMessageQueueClient _messageQueueClient;
        private IListingRepository ListingRepository;
        private ICommentRepository CommentRepository;
        private IProfileRepository ProfileRepository;
        private ICollectionRepository CollectionRepository;
        private ITripleStore TripleStore;
        private IStorageRepository StorageRepository;

        public DefaultListingManager(
            IMessageQueueClient messageQueueClient,
            IListingRepository listingRepository,
            ICommentRepository commentRepository,
            IProfileRepository profileRepository,
            ICollectionRepository collectionRepository,
            ITripleStore tripleStore,
            IStorageRepository storageRepository)
        {
            _messageQueueClient = messageQueueClient;
            ListingRepository = listingRepository;
            CommentRepository = commentRepository;
            ProfileRepository = profileRepository;
            CollectionRepository = collectionRepository;
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
            var listing = GetVerifiedListing(appId, listingId);
            var listingView = listing.ToListingView();

            if (logImpression)
            {
                ListingRepository.IncreaseCounter(listingId, appId, ListingCounters.Views, 1);
            }

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
                var favProfileIds = TripleStore.GetActivitySubjectList(appId, ActivityPredicate.FAVORITE_LISTING, listing.Id).ToArray();
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
                TripleStore.LogActivity(appId, requestedByProfileId.IsNullOrEmpty() ? "guest" : requestedByProfileId, ActivityPredicate.VIEW_LISTING, listing.Id, ref exists);
            }
            return listingView;
        }

        public IList<ListingView> GetListingsByProfileId(
            string appId, 
            string profileId, 
            bool includeComments, 
            bool formatCommentsAsHtml)
        {
            var profile = GetVerifiedProfile(appId, profileId);

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
            IDictionary<string, string> metadata, 
            double? priceMin, 
            double? priceMax, 
            Location location, 
            bool includeComments, 
            bool formatCommentsAsHtml)
        {
            // TODO: cache listings
            tag = string.IsNullOrEmpty(tag) ? null : string.Concat("#", tag.TrimStart(new char[] { '#' }));
            var listings = ListingRepository.Search(tag, listingType, metadata, priceMin, priceMax, location, appId, false, false);
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
            var listing = GetVerifiedListing(appId, listingId, profileId, true);
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
                            var mediaFile = new MediaFile
                            {
                                Type = MediaFileType.Image,
                                ContentType = file.ContentType,
                                Url = StorageRepository.KeyToUrl(key),
                                Key = key
                            };
                            mediaFiles.Add(mediaFile);
                            _messageQueueClient.Publish<CreateThumbnailsRequest>(new CreateThumbnailsRequest(listingId, appId, key));
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
            var listing = GetVerifiedListing(appId, listingId, profileId, true);

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
            var listing = GetVerifiedListing(appId, listingId, profileId, true);
            var profile = ProfileRepository.GetById(appId, listing.ProfileId, false);

            // can't publish a purchasable listing if 
            if ((listing.PricingInfo != null || listing.SchedulingTemplate != null) && !profile.IsVendor)
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
            IDictionary<string, string> customAttributes)
        {
            Listing listing;
            bool isNewListing = listingId.IsNullOrEmpty();
            if (!isNewListing) {
                listing = GetVerifiedListing(appId, listingId, true);
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
            if (pricingInfo != null) listing.PricingInfo = pricingInfo;
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
            TripleStore.LogActivity(appId, profileId, ActivityPredicate.COMMENT_ON_LISTING, listingId, ref exists);

            // save mentions
            foreach (var mentionedUsername in comment.Content.ExtractUsernames())
            {
                var mentionedProfile = ProfileRepository.GetByUsername(appId, mentionedUsername.TrimStart('@'), false);
                TripleStore.LogActivity(appId, profileId, ActivityPredicate.MENTION_PROFILE, mentionedProfile.Id, ref exists);

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
            TripleStore.LogActivity(appId, profileId, ActivityPredicate.FAVORITE_LISTING, listingId, ref tripleExists);
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
                    TripleStore.LogActivity(appId, profileId, ActivityPredicate.FLAG_LISTING, listingId, ref tripleExists);
                    break;
            }
        }

        public CollectionView CreateCollection(
            string appId,
            string profileId,
            string title,
            string content,
            bool isPublic,
            IList<Classy.Models.IncludedListing> includedListings,
            IList<string> collaborators,
            IList<string> permittedViewers)
        {
            try
            {
                // create and save new collection
                var collection = new Collection
                {
                    AppId = appId,
                    ProfileId = profileId,
                    Title = title,
                    Content = content,
                    IsPublic = isPublic,
                    IncludedListings = includedListings,
                    Collaborators = collaborators,
                    PermittedViewers = permittedViewers
                };
                CollectionRepository.Insert(collection);

                // log an activity, and increase the counter for the listings that were included
                var exists = false;
                foreach (var listing in includedListings)
                {
                    TripleStore.LogActivity(appId, profileId, ActivityPredicate.ADD_LISTING_TO_COLLECTION, listing.ListingId, ref exists);
                    if (!exists) ListingRepository.IncreaseCounter(listing.ListingId, appId, ListingCounters.AddToCollection, 1);
                }

                // return
                return collection.ToCollectionView();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public CollectionView GetCollectionById(
            string appId,
            string collectionId,
            string profileId,
            bool includeProfile,
            bool includeDrafts,
            bool includeListings,
            bool increaseViewCounter,
            bool increaseViewCounterOnListings)
        {
            try
            {
                var collection = GetVerifiedCollection(appId, collectionId);
                var collectionView = collection.ToCollectionView();
                if (includeProfile)
                {
                    collectionView.Profile = ProfileRepository.GetById(appId, collection.ProfileId, false).ToProfileView();
                }
                if (includeListings)
                {
                    collectionView.Listings = ListingRepository.GetById(collection.IncludedListings.Select(l => l.ListingId).ToArray(), appId, includeDrafts).ToListingViewList();
                }
                if (increaseViewCounter)
                {
                    var exists = false;
                    TripleStore.LogActivity(appId, profileId, ActivityPredicate.VIEW_COLLECTION, collectionId, ref exists);
                    //CollectionRepository.IncreaseCounter(appId, collectionId);
                    if (increaseViewCounterOnListings)
                    {
                        ListingRepository.IncreaseCounter(collection.IncludedListings.Select(l => l.ListingId).ToArray(), appId, ListingCounters.Views, 1);
                    }
                //    var update = Update<Listing>.Inc(x => x.ViewCount, 1);
                //    ListingsCollection.Update(query, update, new MongoUpdateOptions { Flags = UpdateFlags.Multi });
                }
                return collectionView;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IList<CollectionView> GetCollectionsByProfileId(
            string appId,
            string profileId)
        {
            try
            {
                var profile = GetVerifiedProfile(appId, profileId);
                var collections = CollectionRepository.GetByProfileId(appId, profileId);
                return collections.ToCollectionViewList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public CollectionView AddListingsToCollection(
            string appId,
            string profileId,
            string collectionId,
            string[] listingIds)
        {
            try
            {
                var collection = GetVerifiedCollection(appId, collectionId);
                if (collection.ProfileId != profileId) throw new UnauthorizedAccessException();
                // TODO: verify all listings exist
                if (collection.IncludedListings == null) collection.IncludedListings = new List<Classy.Models.IncludedListing>();
                // log an activity, and increase the counter for the listings that were included
                var exists = false;
                foreach (var listingId in listingIds)
                {
                    TripleStore.LogActivity(appId, profileId, ActivityPredicate.ADD_LISTING_TO_COLLECTION, listingId, ref exists);
                    if (!exists)
                    {
                        collection.IncludedListings.Add(new Classy.Models.IncludedListing { ListingId = listingId, Comments = string.Empty });
                        ListingRepository.IncreaseCounter(listingId, appId, ListingCounters.AddToCollection, 1);
                    }
                }
                CollectionRepository.Update(collection);
                return collection.ToCollectionView();
            }
            catch(Exception)
            {
                throw;
            }
        }

        public CollectionView UpdateCollection(
            string appId,
            string profileId,
            string collectionId,
            string title,
            string content,
            IList<Classy.Models.IncludedListing> listings)
        {
            try
            {
                var collection = GetVerifiedCollection(appId, collectionId);
                if (collection.ProfileId != profileId) throw new UnauthorizedAccessException();
                // TODO: verify all listings exist
                if (collection.IncludedListings == null) collection.IncludedListings = new List<Classy.Models.IncludedListing>();
                // log an activity, and increase the counter for the listings that were included
                collection.Title = title;
                collection.Content = content;
                collection.IncludedListings = listings;

                CollectionRepository.Update(collection);
                var collectionView = collection.ToCollectionView();
                collectionView.Listings = ListingRepository.GetById(collection.IncludedListings.Select(l => l.ListingId).ToArray(), appId, false).ToListingViewList();
                return collectionView;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private Collection GetVerifiedCollection(string appId, string collectionId)
        {
            var collection = CollectionRepository.GetById(appId, collectionId);
            if (collection == null) throw new KeyNotFoundException("invalid collection");
            return collection;
        }

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
        /// <param name="profileId"></param>
        /// <param name="logImpression"></param>
        /// <returns></returns>
        private Listing GetVerifiedListing(string appId, string listingId, string profileId, bool includeDrafts)
        {
            Listing listing;
            listing = GetVerifiedListing(appId, listingId, includeDrafts);
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
        private Listing GetVerifiedListing(string appId, string listingId, bool includeDrafts)
        {
            var listing = ListingRepository.GetById(listingId, appId, includeDrafts);
            if (listing == null) throw new KeyNotFoundException("invalid listing id");
            return listing;
        }

        private Listing GetVerifiedListing(string appId, string listingId)
        {
            return GetVerifiedListing(appId, listingId, false);
        }
    }
}