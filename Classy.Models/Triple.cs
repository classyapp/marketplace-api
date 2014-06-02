using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Classy.Models.Attributes;

namespace Classy.Models
{
    public static class ActivityPredicate 
    {
        public const string VIEW_LISTING = "view-listing";
        public const string VIEW_PROFILE = "view-profile";
        public const string VIEW_COLLECTION = "view-collection";
        public const string EDIT_COLLECTION = "edit-collection";
        public const string FAVORITE_LISTING = "fav-listing";
        public const string FAVORITE_COMMENT = "fav-comment";
        public const string FOLLOW_PROFILE = "follow-profile";
        public const string FOLLOW_LISTING = "follow-listing";
        public const string FLAG_LISTING = "flag-listing";
        public const string BOOK_LISTING = "book-listing";
        public const string PURCHASE_LISTING = "purchase-listing";
        public const string MENTION_PROFILE = "mention-profile";
        public const string COMMENT_ON_LISTING = "comment-listing";
        public const string POST_LISTING = "post-listing";
        public const string REVIEW_PROFILE = "review-profile";
        public const string CONTACT_PROFILE = "contact-profile";
        public const string ADD_LISTING_TO_COLLECTION = "add-listing-to-collection";
        public const string EXTERNAL_WEBSITE_CLICK = "click-external-website";
        public const string COMMENT_ON_COLLECTION = "comment-collection";
    }

    [MongoCollection(Name = "triples")]
    public class Triple : BaseObject
    {
        public string ObjectId { get; set; }
        public string Predicate { get; set; }
        public string SubjectId { get; set; }
        public Dictionary<string, string> Metadata { get; set; }
        public int Count { get; set; }
    }
}