using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Driver.Builders;
using Classy.Models;
using System.IO;

namespace Classy.Repository
{
    public class MongoReviewRepository : IReviewRepository
    {
        private MongoCollection<Review> ReviewsCollection;

        public MongoReviewRepository(MongoDatabase db)
        {
            ReviewsCollection = db.GetCollection<Review>("reviews");
        }

        public string Save(Review review)
        {
            try
            {
                ReviewsCollection.Save(review);
                return review.Id;
            }
            catch (MongoException mex)
            {
                throw;
            }
        }

        public void Publish(string appId, string reviewId)
        {
            try
            {
                var query = Query<Review>.Where(x =>
                    x.AppId == appId &&
                    x.Id == reviewId);
                var update = Update<Review>.Set(x => x.IsPublished, true);

                ReviewsCollection.Update(query, update);
            }
            catch (MongoException mex)
            {
                throw;
            }
        }

        public void Delete(string appId, string reviewId)
        {
            try
            {
                var query = Query<Review>.Where(x =>
                    x.AppId == appId &&
                    x.Id == reviewId);
                var update = Update<Review>.Set(x => x.IsDeleted, true);

                ReviewsCollection.Update(query, update);
            }
            catch (MongoException mex)
            {
                throw;
            }
        }

        public Review GetById(string appId, string reviewId)
        {
            // setup query
            var query = Query<Review>.Where(x =>
                x.AppId == appId &&
                x.Id == reviewId);

            // get review
            var review = ReviewsCollection.FindOne(query);

            // return
            return review;
        }

        public IList<Review> GetByRevieweeProfileId(string appId, string revieweeProfileId, bool includeDrafts, bool includeOnlyDrafts)
        {
            // setup query
            var query = Query<Review>.Where(x => 
                x.AppId == appId &&
                x.RevieweeProfileId == revieweeProfileId &&
                !x.IsDeleted);
            if (!includeDrafts)
            {
                query = Query.And(query, Query<Review>.Where(x => x.IsPublished == true));
            }
            if (includeOnlyDrafts)
            {
                query = Query.And(query, Query<Review>.Where(x => x.IsPublished == false));
            }
            
            // get reviews
            var results = ReviewsCollection.Find(query);
            if (results == null) return new List<Review>();
            var reviews = results.ToList();

            // return
            return reviews;
        }
    }
}