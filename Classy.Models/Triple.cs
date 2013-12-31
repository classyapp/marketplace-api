using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Classy.Models
{
    public static class ActivityPredicate 
    {
        public static string VIEW_LISTING = "view-listing";
        public static string VIEW_PROFILE = "view-profile";
        public static string VIEW_COLLECTION = "view-collection";
        public static string FAVORITE_LISTING = "fav-listing";
        public static string FAVORITE_COMMENT = "fav-comment";
        public static string FOLLOW_PROFILE = "follow-profile";
        public static string FOLLOW_LISTING = "follow-listing";
        public static string FLAG_LISTING = "flag-listing";
        public static string BOOK_LISTING = "book-listing";
        public static string PURCHASE_LISTING = "purchase-listing";
        public static string MENTION_PROFILE = "mention-profile";
        public static string COMMENT_ON_LISTING = "comment-listing";
        public static string POST_LISTING = "post-listing";
        public static string REVIEW_PROFILE = "review-profile";
        public static string CONTACT_PROFILE = "contact-profile";
        public static string ADD_LISTING_TO_COLLECTION = "add-listing-to-collection";
    }

    public class Triple : BaseObject
    {
        public string ObjectId { get; set; }
        public string Predicate { get; set; }
        public string SubjectId { get; set; }
    }
}