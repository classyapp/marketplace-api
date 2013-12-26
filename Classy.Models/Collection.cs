using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classy.Models
{
    /// <summary>
    /// represents a collection of listings curated by a user, with the ability to collaborate with other users
    /// </summary>
    public class Collection : BaseObject
    {
        /// <summary>
        /// the owner
        /// </summary>
        public string ProfileId { get; set; }
        /// <summary>
        /// the title of the collection
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// the content of the collection - free text 
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// when set to true, the collection will be publicly visible
        /// </summary>
        public bool IsPublic { get; set; }
        /// <summary>
        /// the ids of the listings included in this collection
        /// </summary>
        public IList<string> IncludedListings { get; set; }
        /// <summary>
        /// a list of profile ids of collaborators - these profiles will be able to edit this collection
        /// </summary>
        public IList<string> Collaborators { get; set; }
        /// <summary>
        /// a list of profile ids of permitted viewers - when the collection is private, these profiles will be able to view this collection
        /// when the collection is public, this list is irrelevant
        /// </summary>
        public IList<string> PermittedViewers { get; set; }
    }
}
