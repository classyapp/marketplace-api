using Classy.Models;
using Classy.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace classy.Manager
{
    public class DefaultCollectionManager : ICollectionManager
    {
        #region // fields and ctors

        private ICollectionRepository CollectionRepository;
        private IListingRepository ListingRepository;
        private IProfileRepository ProfileRepository;
        private ITripleStore TripleStore;

        public DefaultCollectionManager(
            ICollectionRepository collectionRepository,
            IListingRepository listingRepository,
            IProfileRepository profileRepository,
            ITripleStore tripleStore)
        {
            CollectionRepository = collectionRepository;
            ListingRepository = listingRepository;
            ProfileRepository = profileRepository;
            TripleStore = tripleStore;
        }

        #endregion

        public Collection CreateCollection(
            string appId,
            string profileId, 
            string title, 
            string content, 
            bool isPublic, 
            IList<string> includedListings, 
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
                foreach (var listingId in includedListings)
                {
                    TripleStore.LogActivity(appId, profileId, ActivityPredicate.SaveToCollection, listingId, ref exists);
                    if (!exists) ListingRepository.IncreaseCounter(listingId, appId, ListingCounters.AddToCollection, 1);
                }

                // return
                return collection;
            }
            catch(Exception)
            {
                throw;
            }
        }

        public Collection GetCollectionById(
            string appId, 
            string collectionId)
        {
            try
            {
                var collection = GetVerifiedCollection(appId, collectionId);
                return collection;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IList<Collection> GetCollectionsByProfileId(
            string appId, 
            string profileId)
        {
            try
            {
                var profile = GetVerifiedProfile(appId, profileId);
                var collections = CollectionRepository.GetByProfileId(appId, profileId);
                return collections;
            }
            catch (Exception )
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
    }
}