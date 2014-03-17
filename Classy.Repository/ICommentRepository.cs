using Classy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classy.Repository
{
    public interface ICommentRepository
    {
        string Save(Comment comment);
        IList<Comment> GetByListingId(string listingId, bool formatAsHtml);
        IList<Comment> GetByListingIds(IEnumerable<string> listingIds, bool formatAsHtml);
        IList<Comment> GetByCollectionId(string collectionId, bool formatCommentsAsHtml);
    }
}
