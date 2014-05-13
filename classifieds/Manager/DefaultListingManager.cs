using System;
using System.Collections.Generic;
using System.Linq;
using Classy.Models;
using Classy.Models.Response;
using Classy.Repository;
using ServiceStack.Common;
using ServiceStack.ServiceHost;
using System.IO;
using ServiceStack.Messaging;
using Classy.Models.Request;
using classy.Extentions;


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
        private IAppManager AppManager;

        public DefaultListingManager(
            IAppManager appManager,
            IMessageQueueClient messageQueueClient,
            IListingRepository listingRepository,
            ICommentRepository commentRepository,
            IProfileRepository profileRepository,
            ICollectionRepository collectionRepository,
            ITripleStore tripleStore,
            IStorageRepository storageRepository)
        {
            _messageQueueClient = messageQueueClient;
            AppManager = appManager;
            ListingRepository = listingRepository;
            CommentRepository = commentRepository;
            ProfileRepository = profileRepository;
            CollectionRepository = collectionRepository;
            TripleStore = tripleStore;
            StorageRepository = storageRepository;
        }

        public ManagerSecurityContext SecurityContext { get; set; }

        public IList<ListingView> GetListingsByIds(string[] listingIds, string appId, bool includeDrafts, string culture)
        {
            var listings = ListingRepository.GetById(listingIds, appId, includeDrafts, null);
            foreach (var listing in listings)
                listing.Translate(culture);

            return listings.Select(x => x.ToListingView()).ToList();
        }

        public ListingView GetListingById(
            string appId,
            string listingId,
            bool logImpression,
            bool includeDrafts,
            bool includeComments,
            bool formatCommentsAsHtml,
            bool includeCommenterProfiles,
            bool includeProfile,
            bool includeFavoritedByProfiles,
            string culture)
        {
            // TODO: cache listings
            var listing = GetVerifiedListing(appId, listingId);
            listing.Translate(culture);
            var listingView = listing.ToListingView();

            if (logImpression)
            {
                ListingRepository.IncreaseCounter(listingId, appId, ListingCounters.Views, 1);
            }

            if (includeComments)
            {
                var comments = CommentRepository.GetByListingId(listingId, formatCommentsAsHtml);
                IList<Profile> commenterProfiles = null;
                if (includeCommenterProfiles) commenterProfiles = ProfileRepository.GetByIds(appId, (from c in comments select c.ProfileId).ToArray(), culture);
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
                listingView.Profile = ProfileRepository.GetById(appId, listing.ProfileId, false, culture).ToProfileView();
            }
            if (includeFavoritedByProfiles)
            {
                var favProfileIds = TripleStore.GetActivitySubjectList(appId, ActivityPredicate.FAVORITE_LISTING, listing.Id).ToArray();
                var favProfiles = ProfileRepository.GetByIds(appId, favProfileIds, culture);
                listingView.FavoritedBy = new List<ProfileView>();
                foreach (var p in favProfiles)
                {
                    listingView.FavoritedBy.Add(p.ToProfileView());
                }
            }

            // i don't think we should be logging impressions here... 
            // it should be in the UI level since only there we really know what the user saw
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
            bool includeDrafts,
            string culture)
        {
            var profile = GetVerifiedProfile(appId, profileId, culture);

            // TODO: cache listings
            var listings = ListingRepository.GetByProfileId(appId, profile.Id, includeDrafts, culture);
            var comments = includeComments ?
                CommentRepository.GetByListingIds(listings.Select(x => x.Id).AsEnumerable(), formatCommentsAsHtml) : null;
            var listingViews = new List<ListingView>();
            foreach (var c in listings)
            {
                var view = c.Translate(culture).ToListingView();
                if (includeComments)
                {
                    view.Comments = comments.Where(x => x.ObjectId == view.Id).ToCommentViewList();
                }
                listingViews.Add(view);
            }
            return listingViews;
        }

        public SearchResultsView<ListingView> SearchListings(
            string appId,
            string[] tags,
            string[] listingTypes,
            IDictionary<string, string[]> metadata,
            double? priceMin,
            double? priceMax,
            Location location,
            bool includeComments,
            bool formatCommentsAsHtml,
            int page,
            int pageSize,
            string culture)
        {
            long count = 0;

            // TODO: cache listings
            var listings = ListingRepository.Search(tags, listingTypes, metadata, priceMin, priceMax, location, appId, false, false, page, pageSize, ref count, culture);
            var comments = includeComments ?
                CommentRepository.GetByListingIds(listings.Select(x => x.Id).AsEnumerable(), formatCommentsAsHtml) : null;
            var listingViews = new List<ListingView>();
            foreach (var c in listings)
            {
                var view = c.Translate(culture).ToListingView();
                if (includeComments)
                {
                    view.Comments = comments.Where(x => x.ObjectId == view.Id).ToCommentViewList();
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
                            byte[] content = reader.ReadBytes((int)file.ContentLength);
                            byte[] reducedContent = content.Rescale(AppManager.GetAppById(appId).ImageReducedSize);
                            StorageRepository.SaveFile(key, content, file.ContentType, false, ListingRepository);
                            StorageRepository.SaveFile(key + "_reduced", reducedContent, file.ContentType, true, ListingRepository);
                            var mediaFile = new MediaFile
                            {
                                Type = MediaFileType.Image,
                                ContentType = file.ContentType,
                                Url = StorageRepository.KeyToUrl(key),
                                Key = key
                            };
                            mediaFiles.Add(mediaFile);
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
            var profile = ProfileRepository.GetById(appId, listing.ProfileId, false, null);

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
                var profile = GetVerifiedProfile(appId, SecurityContext.AuthenticatedProfileId, null);
                listing.DefaultCulture = profile.DefaultCulture;
                listing.Id = ListingRepository.Insert(listing);
            }
            else
            {
                ListingRepository.Update(listing);
            }

            // return
            return listing.ToListingView();
        }

        public ListingView UpdateListing(
            string appId,
            string listingId,
            string title,
            string content,
            PricingInfo pricingInfo,
            ContactInfo contactInfo,
            TimeslotSchedule timeslotSchedule,
            IDictionary<string, string> metadata,
            IList<string> hashtags,
            IDictionary<string, IList<string>> editorKeywords,
            ListingUpdateFields fields)
        {
            var listing = GetVerifiedListing(appId, listingId, true, true);

            // include basic listing info
            if (fields.HasFlag(ListingUpdateFields.Title)) listing.Title = title;
            listing.Hashtags = hashtags;
            if (fields.HasFlag(ListingUpdateFields.Content))
            {
                listing.Content = content;
                var newTags = string.IsNullOrEmpty(content) ? new string[0] : content.ExtractHashtags();
                listing.Hashtags = hashtags.EmptyIfNull().Union(newTags).ToList();
            }
            if (fields.HasFlag(ListingUpdateFields.Pricing)) listing.PricingInfo = pricingInfo;
            if (fields.HasFlag(ListingUpdateFields.ContactInfo)) listing.ContactInfo = contactInfo;
            if (fields.HasFlag(ListingUpdateFields.SchedulingTemplate)) listing.SchedulingTemplate = timeslotSchedule;
            if (fields.HasFlag(ListingUpdateFields.Metadata))
            {
                foreach (var c in metadata)
                {
                    if (listing.Metadata.ContainsKey(c.Key))
                    {
                        listing.Metadata.Remove(listing.Metadata.SingleOrDefault(x => x.Key == c.Key));
                    }
                    listing.Metadata.Add(c);
                }
            }
            listing.TranslatedKeywords = editorKeywords;
            ListingRepository.Update(listing);

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
                ObjectId = listingId,
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
            if (SecurityContext.AuthenticatedProfileId == listing.ProfileId || SecurityContext.IsAdmin)
            {
                ListingRepository.AddHashtags(listingId, appId, content.ExtractHashtags());
            }

            // log a comment activity
            int count = 0;
            TripleStore.LogActivity(appId, SecurityContext.AuthenticatedProfileId, ActivityPredicate.COMMENT_ON_LISTING, listingId, ref count);

            // save mentions
            foreach (var mentionedUsername in comment.Content.ExtractUsernames())
            {
                var mentionedProfile = ProfileRepository.GetByUsername(appId, mentionedUsername.TrimStart('@'), false, null);
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
                var profile = GetVerifiedProfile(appId, string.IsNullOrEmpty(profileId) ? SecurityContext.AuthenticatedProfileId : profileId, null);

                // create and save new collection
                var collection = new Collection
                {
                    AppId = appId,
                    ProfileId = string.IsNullOrEmpty(profileId) ? SecurityContext.AuthenticatedProfileId : profileId,
                    Type = type,
                    Title = title,
                    Content = content,
                    IsPublic = isPublic,
                    IncludedListings = includedListings,
                    Collaborators = collaborators,
                    PermittedViewers = permittedViewers,
                    DefaultCulture = profile.DefaultCulture
                };

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
            bool includeCommenterProfiles,
            string culture)
        {
            try
            {
                var collection = GetVerifiedCollection(appId, collectionId, culture);
                var collectionView = collection.Translate(culture).ToCollectionView();
                if (includeProfile)
                {
                    collectionView.Profile = ProfileRepository.GetById(appId, collection.ProfileId, false, culture).ToProfileView();
                }
                if (includeListings)
                {
                    collectionView.Listings = ListingRepository.GetById(collection.IncludedListings.Select(l => l.Id).ToArray(), appId, includeDrafts, culture).ToListingViewList(culture);
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
                    if (includeCommenterProfiles) commenterProfiles = ProfileRepository.GetByIds(appId, (from c in comments select c.ProfileId).ToArray(), culture);
                    collectionView.Comments = new List<CommentView>();
                    foreach (var c in comments)
                    {
                        var comment = c.TranslateTo<CommentView>();
                        if (commenterProfiles != null)
                        {
                            comment.Profile = (from p in commenterProfiles where p.Id == comment.ProfileId select p).Single().Translate(culture).ToProfileView();
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
            string collectionType,
            string culture)
        {
            try
            {
                var profile = GetVerifiedProfile(appId, profileId, culture);
                var collections = CollectionRepository.GetByProfileId(appId, profileId, collectionType, culture);

                foreach (var collection in collections)
                {
                    if (collection.CoverPhotos == null || collection.CoverPhotos.Count == 0)
                    {
                        collection.CoverPhotos = ListingRepository.GetById(collection.IncludedListings.Select(l => l.Id).Skip(Math.Max(0, collection.IncludedListings.Count - 4)).ToArray(), appId, false, null).Select(l => l.ExternalMedia[0].Key).ToArray();
                    }
                }

                return collections.ToCollectionViewList(culture);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public CollectionView AddListingsToCollection(
            string appId,
            string collectionId,
            IList<IncludedListing> includedListings,
            string culture)
        {
            try
            {
                var collection = GetVerifiedCollection(appId, collectionId, culture);
                if (collection.ProfileId != SecurityContext.AuthenticatedProfileId && collection.Type != CollectionType.WebPhotos) throw new UnauthorizedAccessException();
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
                        TripleStore.LogActivity(appId, SecurityContext.AuthenticatedProfileId, ActivityPredicate.ADD_LISTING_TO_COLLECTION, listing.Id, ref count);
                        collection.IncludedListings.Add(new Classy.Models.IncludedListing { Id = listing.Id, Comments = listing.Comments, ListingType = listing.ListingType });
                        ListingRepository.IncreaseCounter(listing.Id, appId, ListingCounters.AddToCollection, 1);
                    }
                }
                if (changed)
                {
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
                var collection = GetVerifiedCollection(appId, collectionId, null);
                if (collection.ProfileId != profileId) throw new UnauthorizedAccessException();

                foreach (string listingId in listingIds)
                {
                    IncludedListing listing = collection.IncludedListings.FirstOrDefault(l => l.Id == listingId);
                    if (listing != null)
                    {
                        collection.IncludedListings.Remove(listing);

                        int count = 0;
                        TripleStore.DeleteActivity(appId, profileId, ActivityPredicate.ADD_LISTING_TO_COLLECTION, listing.Id, ref count);

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
            string collectionId,
            string title,
            string content,
            IList<IncludedListing> listings,
            string culture)
        {
            try
            {
                var collection = GetVerifiedCollection(appId, collectionId, culture);
                if (collection.ProfileId != SecurityContext.AuthenticatedProfileId && !SecurityContext.IsAdmin) throw new UnauthorizedAccessException();
                // TODO: verify all listings exist
                if (collection.IncludedListings == null) collection.IncludedListings = new List<Classy.Models.IncludedListing>();
                // log an activity, and increase the counter for the listings that were included
                collection.Title = title;
                collection.Content = content;
                collection.IncludedListings = listings;

                CollectionRepository.Update(collection);
                var collectionView = collection.ToCollectionView();
                collectionView.Listings = ListingRepository.GetById(collection.IncludedListings.Select(l => l.Id).ToArray(), appId, false, culture).ToListingViewList(culture);
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
                var collection = GetVerifiedCollection(appId, collectionId, null);
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
            var collection = GetVerifiedCollection(appId, collectionId, null);

            // save to repository
            var comment = new Comment
            {
                AppId = appId,
                ProfileId = SecurityContext.AuthenticatedProfileId,
                ObjectId = collectionId,
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
            if (SecurityContext.AuthenticatedProfileId == collection.ProfileId || SecurityContext.IsAdmin)
            {
                CollectionRepository.AddHashtags(collectionId, appId, content.ExtractHashtags());
            }

            // log a comment activity
            int count = 0;
            TripleStore.LogActivity(appId, SecurityContext.AuthenticatedProfileId, ActivityPredicate.COMMENT_ON_COLLECTION, collectionId, ref count);

            // save mentions
            foreach (var mentionedUsername in comment.Content.ExtractUsernames())
            {
                var mentionedProfile = ProfileRepository.GetByUsername(appId, mentionedUsername.TrimStart('@'), false, null);
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
            IList<string> photoKeys,
            string culture)
        {
            try
            {
                var collection = GetVerifiedCollection(appId, collectionId, null);
                if (collection.ProfileId != SecurityContext.AuthenticatedProfileId && !SecurityContext.IsAdmin) throw new UnauthorizedAccessException();
                collection.CoverPhotos = photoKeys;

                CollectionRepository.Update(collection);
                var collectionView = collection.Translate(culture).ToCollectionView();
                collectionView.Listings = ListingRepository.GetById(collection.IncludedListings.Select(l => l.Id).ToArray(), appId, false, culture).ToListingViewList(culture);
                return collectionView;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public CollectionView SubmitCollectionForEditorialApproval(string appId, string collectionId)
        {
            var collection = GetVerifiedCollection(appId, collectionId, null);
            collection.SumittedForEditorialApproval = true;
            CollectionRepository.SubmitForEditorialApproval(appId, collectionId);
            return collection.ToCollectionView();
        }

        public IList<CollectionView> GetApprovedCollections(
            string appId,
            string[] categories,
            int maxCollections,
            string culture)
        {
            var collections = CollectionRepository.GetApprovedCollections(appId, categories, maxCollections, culture);
            var profiles = ProfileRepository.GetByIds(appId, collections.Select(x => x.ProfileId).ToArray(), culture);
            var view = collections.ToCollectionViewList(culture);
            foreach (var c in view)
            {
                c.Profile = profiles.SingleOrDefault(p => p.Id == c.ProfileId).ToProfileView();
            }
            return view;
        }

        public ListingTranslationView GetTranslation(string appId, string listingId, string culture)
        {
            Listing listing = GetVerifiedListing(appId, listingId);
            ListingTranslation translation = null;
            if (listing.Translations == null || !listing.Translations.TryGetValue(culture, out translation))
            {
                if (culture == listing.DefaultCulture || string.IsNullOrEmpty(culture))
                {
                    return new ListingTranslationView { CultureCode = culture, Title = listing.Title, Content = listing.Content };
                }
                return new ListingTranslationView { CultureCode = culture, Title = string.Empty, Content = string.Empty };
            }

            return new ListingTranslationView { CultureCode = culture, Title = translation.Title, Content = translation.Content };
        }

        public void SetCollectionTranslation(string appId, string collectionId, CollectionTranslation collectionTranslation)
        {
            Collection collection = GetVerifiedCollection(appId, collectionId, null);

            if (SecurityContext.IsAdmin || SecurityContext.AuthenticatedProfileId == collection.ProfileId)
            {
                if (collection.DefaultCulture == collectionTranslation.Culture)
                    throw new InvalidOperationException("Cannot translate default culture values");
                if (collection.Translations == null)
                {
                    collection.Translations = new Dictionary<string, CollectionTranslation>();
                }
                collection.Translations[collectionTranslation.Culture] = collectionTranslation;
                CollectionRepository.Update(collection);
            }
        }

        public void SetTranslation(string appId, string listingId, ListingTranslation listingTranslation)
        {
            Listing listing = GetVerifiedListing(appId, listingId);
            if (SecurityContext.IsAdmin || SecurityContext.AuthenticatedProfileId == listing.ProfileId)
            {
                if (listing.DefaultCulture == listingTranslation.Culture)
                    throw new InvalidOperationException("Cannot translate default culture values");
                if (listing.Translations == null)
                {
                    listing.Translations = new Dictionary<string, ListingTranslation>();
                }
                listing.Translations[listingTranslation.Culture] = listingTranslation;
                ListingRepository.Update(listing);
            }
        }

        public void DeleteTranslation(string appId, string listingId, string culture)
        {
            Listing listing = GetVerifiedListing(appId, listingId);
            if (SecurityContext.IsAdmin || SecurityContext.AuthenticatedProfileId == listing.ProfileId)
            {
                if (listing.Translations != null)
                {
                    listing.Translations.Remove(culture);
                    ListingRepository.Update(listing);
                }
            }
        }

        public void DeleteCollectionTranslation(string appId, string collectionId, string culture)
        {
            Collection collection = GetVerifiedCollection(appId, collectionId, null);
            if (SecurityContext.IsAdmin || SecurityContext.AuthenticatedProfileId == collection.ProfileId)
            {
                if (collection.Translations != null)
                {
                    collection.Translations.Remove(culture);
                    CollectionRepository.Update(collection);
                }
            }
        }

        public CollectionTranslationView GetCollectionTranslation(string appId, string collectionId, string culture)
        {
            Collection collection = GetVerifiedCollection(appId, collectionId, null);
            if (collection.Translations == null)
            {
                return new CollectionTranslationView
                {
                    CultureCode = culture,
                    Title = string.Empty,
                    Content = string.Empty
                };
            }
            else
            {
                return new CollectionTranslationView
                {
                    CultureCode = culture,
                    Title = collection.Translations.ContainsKey(culture) ? collection.Translations[culture].Title : string.Empty,
                    Content = collection.Translations.ContainsKey(culture) ? collection.Translations[culture].Content : string.Empty
                };
            }
        }

        private Collection GetVerifiedCollection(string appId, string collectionId, string culture)
        {
            var collection = CollectionRepository.GetById(appId, collectionId, culture);
            if (collection == null) throw new KeyNotFoundException("invalid collection");
            return collection;
        }

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
            listing = ListingRepository.GetById(listingId, appId, includeDrafts, null);
            if (listing == null) throw new KeyNotFoundException("invalid listing");
            return listing;
        }

        private Listing GetVerifiedListing(string appId, string listingId)
        {
            return GetVerifiedListing(appId, listingId, false);
        }


        public ListingMoreInfoView GetListingMoreInfo(string appId, string listingId, Dictionary<string, string[]> metadata, Location location, string culture)
        {
            ListingMoreInfoView data = new ListingMoreInfoView();

            Listing listing = GetVerifiedListing(appId, listingId);

            // Get original collection
            Collection originalCollection = CollectionRepository.GetOriginalCollection(listing);
            if (originalCollection != null)
            {
                data.CollectionType = originalCollection.Type.ToLowerInvariant();
                data.CollectionLisitngs = ListingRepository.GetById(originalCollection.IncludedListings.Select(l => l.Id).ToArray(), appId, false, culture).ToListingViewList(culture);
            }

            // Search by metadata provided
            if (metadata != null)
            {
                long count = 0;
                data.SearchResults = ListingRepository.Search(null, new string[] { listing.ListingType }, 
                    metadata, 
                    null, null, location, appId, false, false, 0, 0, ref count, culture).ToListingViewList(culture);
                if (data.SearchResults != null)
                {
                    data.SearchResults.Remove(data.SearchResults.First(wl => wl.Id == listingId));
                }
            }
            
            // Get more collections including this listing
            IList<CollectionView> collections = GetCollectionsByListingId(appId, listing.Id, culture);
            CollectionView currenctCollection = collections.FirstOrDefault(c => c.Id == originalCollection.Id);
            if (currenctCollection != null)
                collections.Remove(currenctCollection);

            data.Collections = collections;

            return data;
        }

        private IList<CollectionView> GetCollectionsByListingId(string appId, string listingId, string culture)
        {
            try
            {
                var collections = CollectionRepository.GetByListingId(appId, listingId, culture);

                foreach (var collection in collections)
                {
                    if (collection.CoverPhotos == null || collection.CoverPhotos.Count == 0)
                    {
                        collection.CoverPhotos = ListingRepository.GetById(collection.IncludedListings.Select(l => l.Id).Skip(Math.Max(0, collection.IncludedListings.Count - 4)).ToArray(), appId, false, null).Select(l => l.ExternalMedia[0].Key).ToArray();
                    }
                }

                return collections.ToCollectionViewList(culture);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}