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
        /// <param name="profileId"></param>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <param name="isPublic"></param>
        /// <param name="includedListings"></param>
        /// <param name="collaborators"></param>
        /// <param name="permittedViewers"></param>
        /// <returns>the created <see cref="Collection"/></returns>
        Collection CreateCollection(
            string profileId,
            string title,
            string content,
            bool isPublic,
            IList<string> includedListings,
            IList<string> collaborators,
            IList<string> permittedViewers);
 
    }
}
