using Classy.Models;
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
        Collection CreateCollection(
            string appId,
            string profileId,
            string title,
            string content,
            bool isPublic,
            IList<string> includedListings,
            IList<string> collaborators,
            IList<string> permittedViewers);

        /// <summary>
        /// get a specific <see cref="Collection"/> by id
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="collectionId"></param>
        /// <returns>a <see cref="Collection"/> object</returns>
        Collection GetCollectionById(
            string appId,
            string collectionId);

        /// <summary>
        /// get a list of a user's <see cref="Collection"/>s
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="profileId"></param>
        /// <returns>a list of <see cref="Collection"/> objects</returns>
        IList<Collection> GetCollectionsByProfileId(
            string appId,
            string profileId);
    }
}
