using Classy.Models;
using Classy.Models.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace classy.Manager
{
    /// <summary>
    /// manage all <see cref="Collection"/> related operations 
    /// </summary>
    public interface ICollectionManager
    {
        /// <summary>
        /// creates a new <see cref="Collection"/>
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="profileId"></param>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <param name="isPublic"></param>
        /// <param name="includedListings"></param>
        /// <param name="collaborators"></param>
        /// <param name="permittedViewers"></param>
        /// <returns>the created <see cref="Collection"/></returns>
        CollectionView CreateCollection(
            string appId,
            string profileId,
            string title,
            string content,
            bool isPublic,
            IList<Classy.Models.IncludedListing> includedListings,
            IList<string> collaborators,
            IList<string> permittedViewers);

        /// <summary>
        /// adds listings to an existing collection
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="profileId"></param>
        /// <param name="collectionId"></param>
        /// <param name="listingIds"></param>
        /// <returns>the collection with the new listings added to the IncludedListings param</returns>
        CollectionView AddListingsToCollection(
            string appId,
            string profileId,
            string collectionId,
            IList<IncludedListing> listingIds);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="profileId"></param>
        /// <param name="collectionId"></param>
        /// <param name="listingId"></param>
        void RemoveListingsFromCollection(
            string appId, 
            string profileId, 
            string collectionId, 
            string listingId);

        /// <summary>
        /// updates existing collection
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="profileId"></param>
        /// <param name="collectionId"></param>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <param name="listings"></param>
        /// <returns>the collection with the updated listings</returns>
        CollectionView UpdateCollection(
            string appId,
            string profileId,
            string collectionId,
            string title,
            string content,
            IList<Classy.Models.IncludedListing> listings);

        /// <summary>
        /// get a specific <see cref="CollectionView"/> by id
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="collectionId"></param>
        /// <param name="profileId">the profile id of the authenticated user</param>
        /// <param name="includeProfile"></param>
        /// <param name="includeListings"></param>
        /// <param name="includeDrafts"></param>
        /// <param name="increaseViewCounter"></param>
        /// <param name="increaseViewCounterOnListings"></param>
        /// <returns>a <see cref="CollectionView"/> object</returns>
        CollectionView GetCollectionById(
            string appId,
            string collectionId,
            string profileId,
            bool includeProfile,
            bool includeListings,
            bool includeDrafts,
            bool increaseViewCounter,
            bool increaseViewCounterOnListings);

        /// <summary>
        /// get a list of a user's <see cref="CollectionView"/>s
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="profileId"></param>
        /// <returns>a list of <see cref="CollectionView"/> objects</returns>
        IList<CollectionView> GetCollectionsByProfileId(
            string appId,
            string profileId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="collectionId"></param>
        /// <returns></returns>
        CollectionView SubmitCollectionForEditorialApproval(
            string appId,
            string collectionId);

        /// <summary>
        /// get a list of the most recently editorial approved collections
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="categories"></param>
        /// <param name="maxCollections"></param>
        /// <returns></returns>
        IList<CollectionView> GetApprovedCollections(
            string appId,
            string[] categories,
            int maxCollections);
    }
}
