using Classy.Repository.Infrastructure;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver.Builders;
using Classy.Models;

namespace Classy.Repository
{
    public class MongoCommentRepository : ICommentRepository
    {
        private MongoCollection<Comment> CommentsCollection;

        public MongoCommentRepository(MongoDatabaseProvider db)
        {
            CommentsCollection = db.GetCollection<Comment>();
        }

        public string Save(Comment comment)
        {
            try
            {
                CommentsCollection.Save(comment);
                return comment.Id;
            }
            catch (MongoException mex)
            {
                throw;
            }
        }

        public IList<Comment> GetByListingId(string listingId, bool formatAsHtml)
        {
            // get listing
            var query = Query<Comment>.EQ(x => x.ObjectId, listingId);
            var sort = SortBy<Comment>.Descending(x => x.Created);

            var results = CommentsCollection.Find(query).SetSortOrder(sort);
            if (results == null) return new List<Comment>();

            var comments = results.ToList();
            if (comments != null && formatAsHtml)
            {
                comments.ForEach(delegate(Comment c) { c.Content = c.Content.FormatAsHtml(); });
            }

            // return
            return comments;
        }

        public IList<Comment> GetByListingIds(IEnumerable<string> listingIds, bool formatAsHtml)
        {
            // get listing
            var query = Query.In("ListingId", new BsonArray(listingIds));
            var sort = SortBy<Comment>.Ascending(x => x.ObjectId).Descending(x => x.Created);

            var results = CommentsCollection.Find(query).SetSortOrder(sort);
            if (results == null) return new List<Comment>();

            var comments = results.ToList();
            if (comments != null && formatAsHtml)
            {
                comments.ForEach(delegate(Comment c) { c.Content = c.Content.FormatAsHtml(); });
            }

            // return
            return comments;
        }

        public IList<Comment> GetByCollectionId(string collectionId, bool formatCommentsAsHtml)
        {
            return GetByListingId(collectionId, formatCommentsAsHtml);
        }
    }
}