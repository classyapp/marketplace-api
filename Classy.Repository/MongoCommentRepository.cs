﻿using MongoDB.Bson;
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
    public class MongoCommentRepository : ICommentRepository
    {
        private MongoCollection<Comment> CommentsCollection;

        public MongoCommentRepository(MongoDatabase db)
        {
            CommentsCollection = db.GetCollection<Comment>("comments");
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
            var query = Query<Comment>.EQ(x => x.ListingId, listingId);

            var results = CommentsCollection.Find(query);
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

            var results = CommentsCollection.Find(query);
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