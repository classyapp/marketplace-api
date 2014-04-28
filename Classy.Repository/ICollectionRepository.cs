using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Classy.Models;

namespace Classy.Repository
{
    [Flags]
    public enum CollectionCounters
    {
        None = 0,
        Comments = 1
    }

    /// <summary>
    /// an interface for handling a <see cref="Collection"/> repository
    /// </summary>
    public interface ICollectionRepository
    {
        /// <summary>
        /// gets a <see cref="Collection"/> by id
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="collectionId"></param>
        /// <returns></returns>
        Collection GetById(string appId, string collectionId, string culture);
        /// <summary>
        /// get a a list of the user's <see cref="Collection"/>s
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="profileId"></param>
        /// <param name="collectionType"></param>
        /// <returns></returns>
        IList<Collection> GetByProfileId(string appId, string profileId, string collectionType, string culture);
        /// <summary>
        /// get a a list of the user's <see cref="Collection"/>s
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="profileId"></param>
        /// <returns></returns>
        IList<Collection> GetByProfileId(string appId, string profileId, string culture);
        /// <summary>
        /// insert a new <see cref="Collection"/>
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        string Insert(Collection collection);
        /// <summary>
        /// update an existing <see cref="Collection"/>
        /// </summary>
        /// <param name="collection"></param>
        void Update(Collection collection);
        /// <summary>
        /// deletes an existing collection
        /// </summary>
        /// <param name="collection"></param>
        void Delete(string appId, string collectionId);
        /// <summary>
        /// remove a listing from the collection
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="listingId"></param>
        void RemoveListingById(string appId, string listingId);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="categories"></param>
        /// <returns></returns>
        IList<Collection> GetApprovedCollections(string appId, string[] categories, int maxCollections, string culture);

        void IncreaseCounter(string collectionId, string appId, CollectionCounters counters, int value);

        void AddHashtags(string collectionId, string appId, string[] hashtags);
    }
}
