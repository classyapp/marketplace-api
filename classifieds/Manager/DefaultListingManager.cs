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
using ServiceStack.CacheAccess;

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

        public ManagerSecurityContext SecurityContext { get; set; }

        public ListingView GetListingById(
            string appId,
            string listingId,
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
                int count = 1;
                TripleStore.LogActivity(appId, SecurityContext.IsAuthenticated ? SecurityContext.AuthenticatedProfileId : "guest", ActivityPredicate.VIEW_LISTING, listing.Id, ref count);
            }
            return listingView;
        }

        public IList<ListingView> GetListingsByProfileId(
            string appId,
            string profileId,
            bool includeComments,
            bool formatCommentsAsHtml,
            bool includeDrafts)
        {
            var profile = GetVerifiedProfile(appId, profileId);

            // TODO: cache listings
            var listings = ListingRepository.GetByProfileId(appId, profile.Id, includeDrafts);
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

        public SearchResultsView<ListingView> SearchListings(
            string appId,
            string tag,
            string listingType,
            IDictionary<string, string> metadata,
            double? priceMin,
            double? priceMax,
            Location location,
            bool includeComments,
            bool formatCommentsAsHtml,
            int page,
            int pageSize)
        {
            long count = 0;

            // TODO: cache listings
            tag = string.IsNullOrEmpty(tag) ? null : string.Concat("#", tag.TrimStart(new char[] { '#' }));
            var listings = ListingRepository.Search(tag, listingType, metadata, priceMin, priceMax, location, appId, false, false, page, pageSize, ref count);
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
            return new SearchResultsView<ListingView> { Results = listingViews, Count = count };
        }

        public ListingView AddExternalMediaToListing(
            string appId,
            string listingId,
            IFile[] files)
        {
            var listing = GetVerifiedListing(appId, listingId, true, true);
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
                            StorageRepository.SaveFile(key, content, file.ContentType, true);
                            var mediaFile = new MediaFile
                            {
                                Type = MediaFileType.Image,
                                ContentType = file.ContentType,
                                Url = StorageRepository.KeyToUrl(key),
                                Key = key
                            };
                            mediaFiles.Add(mediaFile);

                            //_messageQueueClient.Publish<CreateThumbnailsRequest>(new CreateThumbnailsRequest(appId, listingId, key, file.ContentType, content));
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
            string url)
        {
            var listing = GetVerifiedListing(appId, listingId, true, false);

            MediaFile file = listing.ExternalMedia.SingleOrDefault(e => e.Url == url);
            if (file != null)
            {
                foreach (var thumb in file.Thumbnails)
                {
                    StorageRepository.DeleteFile(thumb.Key);
                }
                ListingRepository.DeleteExternalMedia(listingId, appId, url);
                StorageRepository.DeleteFile(url);

                listing.ExternalMedia.Remove(file);
            }
            return listing.ToListingView();
        }

        public ListingView PublishListing(
            string appId,
            string listingId,
            string profileId)
        {
            var listing = GetVerifiedListing(appId, listingId, true, true);
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
            if (!isNewListing)
            {
                listing = GetVerifiedListing(appId, listingId, true, true);
            }
            else
            {
                listing = new Listing
                {
                    ProfileId = SecurityContext.AuthenticatedProfileId,
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

        public string DeleteListing(string appId, string listingId)
        {
            var listing = GetVerifiedListing(appId, listingId, true, true);

            CollectionRepository.RemoveListingById(appId, listingId);
            // TODO: reverse all add-to-collection activities for this listing


            foreach (var file in listing.ExternalMedia)
            {
                foreach (var thumb in file.Thumbnails)
                {
                    StorageRepository.DeleteFile(thumb.Key);
                }
                StorageRepository.DeleteFile(file.Key);
            }
            ListingRepository.Delete(listingId, appId);

            return listingId;
        }

        public CommentView AddCommentToListing(
            string appId,
            string listingId,
            string content,
            bool formatAsHHtml)
        {
            var listing = GetVerifiedListing(appId, listingId);

            // save to repository
            var comment = new Comment
            {
                AppId = appId,
                ProfileId = SecurityContext.AuthenticatedProfileId,
                ListingId = listingId,
                Content = content
            };

            comment.Id = CommentRepository.Save(comment);

            // increase comment count for listing
            ListingRepository.IncreaseCounter(listingId, appId, ListingCounters.Comments, 1);

            // increase comment count for profile of commenter
            ProfileRepository.IncreaseCounter(appId, SecurityContext.AuthenticatedProfileId, ProfileCounters.Comments, 1);

            // increase rank of listing owner
            ProfileRepository.IncreaseCounter(appId, listing.ProfileId, ProfileCounters.Rank, 1);

            // add hashtags to listing if the comment is by the listing owner
            if (SecurityContext.AuthenticatedProfileId == listing.ProfileId)
            {
                ListingRepository.AddHashtags(listingId, appId, content.ExtractHashtags());
            }

            // log a comment activity
            int count = 0;
            TripleStore.LogActivity(appId, SecurityContext.AuthenticatedProfileId, ActivityPredicate.COMMENT_ON_LISTING, listingId, ref count);

            // save mentions
            foreach (var mentionedUsername in comment.Content.ExtractUsernames())
            {
                var mentionedProfile = ProfileRepository.GetByUsername(appId, mentionedUsername.TrimStart('@'), false);
                TripleStore.LogActivity(appId, SecurityContext.AuthenticatedProfileId, ActivityPredicate.MENTION_PROFILE, mentionedProfile.Id, ref count);

                // increase rank of mentioned profile
                ProfileRepository.IncreaseCounter(appId, mentionedProfile.Id, ProfileCounters.Rank, 1);
            }

            // format as html
            if (formatAsHHtml) comment.Content = comment.Content.FormatAsHtml();

            return comment.TranslateTo<CommentView>();
        }

        public void FavoriteListing(
            string appId,
            string listingId)
        {
            var listing = GetVerifiedListing(appId, listingId);

            int count = 0;
            TripleStore.LogActivity(appId, SecurityContext.AuthenticatedProfileId, ActivityPredicate.FAVORITE_LISTING, listingId, ref count);
            if (count == 1)
            {
                var listingCounters = ListingCounters.Favorites;
                if (SecurityContext.IsAdmin) listingCounters |= ListingCounters.DisplayOrder;
                ListingRepository.IncreaseCounter(listingId, appId, listingCounters, 1);
                ProfileRepository.IncreaseCounter(appId, listing.ProfileId, ProfileCounters.Rank, 1);
            }
        }

        public void UnfavoriteListing(
            string appId,
            string listingId)
        {
            var listing = GetVerifiedListing(appId, listingId);

            int count = 0;
            TripleStore.ResetActivity(appId, SecurityContext.AuthenticatedProfileId, ActivityPredicate.FAVORITE_LISTING, listingId);
            if (count == 0)
            {
                ListingRepository.IncreaseCounter(listingId, appId, ListingCounters.Favorites, -1);
                ProfileRepository.IncreaseCounter(appId, listing.ProfileId, ProfileCounters.Rank, -1);
            }
        }

        public void FlagListing(
            string appId,
            string listingId,
            FlagReason FlagReason)
        {
            var listing = GetVerifiedListing(appId, listingId);

            switch (FlagReason)
            {
                case FlagReason.Inapropriate:
                    ListingRepository.IncreaseCounter(listingId, appId, ListingCounters.Flags, 1);
                    ProfileRepository.IncreaseCounter(appId, SecurityContext.AuthenticatedProfileId, ProfileCounters.Rank, -3);
                    break;
                case FlagReason.Dislike:
                default:
                    int count = 0;
                    TripleStore.LogActivity(appId, SecurityContext.AuthenticatedProfileId, ActivityPredicate.FLAG_LISTING, listingId, ref count);
                    break;
            }
        }

        public CollectionView CreateCollection(
            string appId,
            string profileId,
            string type,
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
                    Type = type,
                    Title = title,
                    Content = content,
                    IsPublic = isPublic,
                    IncludedListings = includedListings,
                    Collaborators = collaborators,
                    PermittedViewers = permittedViewers
                };

                // TODO: thumbnails should be created async and show a grid of recent items in the collection
                if (includedListings.Count > 0)
                {
                    var last = GetVerifiedListing(appId, includedListings.Last().Id);
                    collection.Thumbnails = last.ExternalMedia[0].Thumbnails;
                }
                CollectionRepository.Insert(collection);

                // log an activity, and increase the counter for the listings that were included
                int count = 0;
                foreach (var listing in includedListings)
                {
                    TripleStore.LogActivity(appId, profileId, ActivityPredicate.ADD_LISTING_TO_COLLECTION, listing.Id, ref count);
                    if (count == 1)
                    {
                        ListingRepository.IncreaseCounter(listing.Id, appId, ListingCounters.AddToCollection, 1);
                        ProfileRepository.IncreaseCounter(appId, profileId, ProfileCounters.Rank, 1);
                    }
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
            bool includeProfile,
            bool includeListings,
            bool includeDrafts,
            bool increaseViewCounter,
            bool increaseViewCounterOnListings,
            bool includeComments,
            bool formatCommentsAsHtml,
            bool includeCommenterProfiles)
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
                    collectionView.Listings = ListingRepository.GetById(collection.IncludedListings.Select(l => l.Id).ToArray(), appId, includeDrafts).ToListingViewList();
                }
                if (increaseViewCounter)
                {
                    int count = 0;
                    TripleStore.LogActivity(appId, SecurityContext.AuthenticatedProfileId, ActivityPredicate.VIEW_COLLECTION, collectionId, ref count);
                    //CollectionRepository.IncreaseCounter(appId, collectionId);
                    if (increaseViewCounterOnListings)
                    {
                        ListingRepository.IncreaseCounter(collection.IncludedListings.Select(l => l.Id).ToArray(), appId, ListingCounters.Views, 1);
                    }
                    //    var update = Update<Listing>.Inc(x => x.ViewCount, 1);
                    //    ListingsCollection.Update(query, update, new MongoUpdateOptions { Flags = UpdateFlags.Multi });
                }

                if (includeComments)
                {
                    var comments = CommentRepository.GetByCollectionId(collectionId, formatCommentsAsHtml);
                    IList<Profile> commenterProfiles = null;
                    if (includeCommenterProfiles) commenterProfiles = ProfileRepository.GetByIds(appId, (from c in comments select c.ProfileId).ToArray());
                    collectionView.Comments = new List<CommentView>();
                    foreach (var c in comments)
                    {
                        var comment = c.TranslateTo<CommentView>();
                        if (commenterProfiles != null)
                        {
                            comment.Profile = (from p in commenterProfiles where p.Id == comment.ProfileId select p).Single().ToProfileView();
                        }
                        collectionView.Comments.Add(comment.TranslateTo<CommentView>());
                    }
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
            string profileId,
            string collectionType)
        {
            try
            {
                var profile = GetVerifiedProfile(appId, profileId);
                var collections = CollectionRepository.GetByProfileId(appId, profileId, collectionType);

                foreach (var collection in collections)
                {
                    if (collection.CoverPhotos == null || collection.CoverPhotos.Count == 0)
                    {
                        collection.CoverPhotos = ListingRepository.GetById(collection.IncludedListings.Select(l => l.Id).Skip(Math.Max(0, collection.IncludedListings.Count - 4)).ToArray(), appId, false).Select(l => l.ExternalMedia[0].Key).ToArray();
                    }
                }

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
            IList<IncludedListing> includedListings)
        {
            try
            {
                var collection = GetVerifiedCollection(appId, collectionId);
                if (collection.ProfileId != profileId) throw new UnauthorizedAccessException();
                // TODO: verify all listings exist
                if (collection.IncludedListings == null) collection.IncludedListings = new List<Classy.Models.IncludedListing>();
                // log an activity, and increase the counter for the listings that were included
                int count = 0;
                bool changed = false;
                foreach (var listing in includedListings)
                {
                    if (!collection.IncludedListings.Any(i => i.Id == listing.Id))
                    {
                        changed = true;
                        TripleStore.LogActivity(appId, profileId, ActivityPredicate.ADD_LISTING_TO_COLLECTION, listing.Id, ref count);
                        collection.IncludedListings.Add(new Classy.Models.IncludedListing { Id = listing.Id, Comments = listing.Comments, ListingType = listing.ListingType });
                        ListingRepository.IncreaseCounter(listing.Id, appId, ListingCounters.AddToCollection, 1);
                    }
                }
                if (changed)
                {
                    // TODO: thumbnails should be created async and show a grid of recent items in the collection
                    var last = GetVerifiedListing(appId, includedListings.Last().Id);
                    collection.Thumbnails = last.ExternalMedia[0].Thumbnails;
                    // save
                    CollectionRepository.Update(collection);
                }
                return collection.ToCollectionView();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void RemoveListingsFromCollection(
            string appId,
            string profileId,
            string collectionId,
            string[] listingIds)
        {
            try
            {
                var collection = GetVerifiedCollection(appId, collectionId);
                if (collection.ProfileId != profileId) throw new UnauthorizedAccessException();

                foreach (string listingId in listingIds)
                {
                    IncludedListing listing = collection.IncludedListings.FirstOrDefault(l => l.Id == listingId);
                    if (listing != null)
                    {
                        collection.IncludedListings.Remove(listing);

                        int count = 0;
                        TripleStore.DeleteActivity(appId, profileId, ActivityPredicate.ADD_LISTING_TO_COLLECTION, listing.Id, ref count);

                        if (collection.IncludedListings.Count > 0)
                        {
                            var last = GetVerifiedListing(appId, collection.IncludedListings.Last().Id);
                            collection.Thumbnails = last.ExternalMedia[0].Thumbnails;
                        }
                        else
                        {
                            collection.Thumbnails = new MediaThumbnail[0];
                        }

                        // save
                        CollectionRepository.Update(collection);
                    }
                }
            }
            catch (Exception)
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
            IList<IncludedListing> listings)
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
                collectionView.Listings = ListingRepository.GetById(collection.IncludedListings.Select(l => l.Id).ToArray(), appId, false).ToListingViewList();
                return collectionView;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void DeleteCollection(
            string appId,
            string collectionId)
        {
            try
            {
                var collection = GetVerifiedCollection(appId, collectionId);
                if (collection.ProfileId != SecurityContext.AuthenticatedProfileId && !SecurityContext.IsAdmin) throw new UnauthorizedAccessException();
                
                CollectionRepository.Delete(appId, collectionId);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public CommentView AddCommentToCollection(string appId, string collectionId, string content, bool formatAsHtml)
        {
            var collection = GetVerifiedCollection(appId, collectionId);

            // save to repository
            var comment = new Comment
            {
                AppId = appId,
                ProfileId = SecurityContext.AuthenticatedProfileId,
                ListingId = collectionId,
                Content = content
            };

            comment.Id = CommentRepository.Save(comment);

            // increase comment count for listing
            CollectionRepository.IncreaseCounter(collectionId, appId, CollectionCounters.Comments, 1);

            // increase comment count for profile of commenter
            ProfileRepository.IncreaseCounter(appId, SecurityContext.AuthenticatedProfileId, ProfileCounters.Comments, 1);

            // increase rank of listing owner
            ProfileRepository.IncreaseCounter(appId, collection.ProfileId, ProfileCounters.Rank, 1);

            // add hashtags to listing if the comment is by the listing owner
            if (SecurityContext.AuthenticatedProfileId == collection.ProfileId)
            {
                CollectionRepository.AddHashtags(collectionId, appId, content.ExtractHashtags());
            }

            // log a comment activity
            int count = 0;
            TripleStore.LogActivity(appId, SecurityContext.AuthenticatedProfileId, ActivityPredicate.COMMENT_ON_COLLECTION, collectionId, ref count);

            // save mentions
            foreach (var mentionedUsername in comment.Content.ExtractUsernames())
            {
                var mentionedProfile = ProfileRepository.GetByUsername(appId, mentionedUsername.TrimStart('@'), false);
                TripleStore.LogActivity(appId, SecurityContext.AuthenticatedProfileId, ActivityPredicate.MENTION_PROFILE, mentionedProfile.Id, ref count);

                // increase rank of mentioned profile
                ProfileRepository.IncreaseCounter(appId, mentionedProfile.Id, ProfileCounters.Rank, 1);
            }

            // format as html
            if (formatAsHtml) comment.Content = comment.Content.FormatAsHtml();

            return comment.TranslateTo<CommentView>();
        }

        public CollectionView UpdateCollectionCover(
            string appId,
            string collectionId,
            IList<string> photoKeys)
        {
            try
            {
                var collection = GetVerifiedCollection(appId, collectionId);
                if (collection.ProfileId != SecurityContext.AuthenticatedProfileId) throw new UnauthorizedAccessException();
                collection.CoverPhotos = photoKeys;

                CollectionRepository.Update(collection);
                var collectionView = collection.ToCollectionView();
                collectionView.Listings = ListingRepository.GetById(collection.IncludedListings.Select(l => l.Id).ToArray(), appId, false).ToListingViewList();
                return collectionView;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public CollectionView SubmitCollectionForEditorialApproval(string appId, string collectionId)
        {
            var collection = GetVerifiedCollection(appId, collectionId);
            collection.SumittedForEditorialApproval = true;
            CollectionRepository.SubmitForEditorialApproval(appId, collectionId);
            return collection.ToCollectionView();
        }

        public IList<CollectionView> GetApprovedCollections(
            string appId,
            string[] categories,
            int maxCollections)
        {
            var collections = CollectionRepository.GetApprovedCollections(appId, categories, maxCollections);
            var profiles = ProfileRepository.GetByIds(appId, collections.Select(x => x.ProfileId).ToArray());
            var view = collections.ToCollectionViewList();
            foreach (var c in view)
            {
                c.Profile = profiles.SingleOrDefault(p => p.Id == c.ProfileId).ToProfileView();
            }
            return view;
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
        private Listing GetVerifiedListing(string appId, string listingId, bool includeDrafts, bool verifyOwnership)
        {
            Listing listing;
            listing = GetVerifiedListing(appId, listingId, includeDrafts);
            if (SecurityContext.IsAuthenticated && listing.ProfileId != SecurityContext.AuthenticatedProfileId && !SecurityContext.IsAdmin) throw new UnauthorizedAccessException("not authorized");
            return listing;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="listingId"></param>
        /// <param name="profileId"></param>
        /// <returns></returns>
        private Listing GetVerifiedListing(string appId, string listingId, bool includeDrafts)
        {
            Listing listing;
            listing = ListingRepository.GetById(listingId, appId, includeDrafts);
            if (listing == null) throw new KeyNotFoundException("invalid listing");
            return listing;
        }

        private Listing GetVerifiedListing(string appId, string listingId)
        {
            return GetVerifiedListing(appId, listingId, false);
        }
    }
}