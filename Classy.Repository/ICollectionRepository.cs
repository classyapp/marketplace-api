using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Classy.Models;

namespace Classy.Repository
{
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
        Collection GetById(string appId, string collectionId);
        /// <summary>
        /// get a a list of the user's <see cref="Collection"/>s
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="profileId"></param>
        /// <returns></returns>
        IList<Collection> GetByProfileId(string appId, string profileId);
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
    }
}
