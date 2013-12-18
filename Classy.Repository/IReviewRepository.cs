using Classy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classy.Repository
{
    public interface IReviewRepository
    {
        string Save(Review review);
        void Delete(string appId, string reviewId);
        void Publish(string appId, string reviewId);
        Review GetById(string appId, string reviewId);
        IList<Review> GetByRevieweeProfileId(string appId, string revieweeProfileId, bool includeDrafts, bool includeOnlyDrafts);
    }
}
