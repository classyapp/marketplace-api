using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Bson;
using System.Text.RegularExpressions;
using ServiceStack.FluentValidation;
using MongoDB.Bson.Serialization.IdGenerators;
using ServiceStack.ServiceInterface;
using System.ComponentModel.DataAnnotations;

namespace Classy.Models
{
    /// <summary>
    /// a <see cref="Listing"/> is the basic unit of the marketplace
    /// </summary>
    public class Listing : BaseObject
    {
        public Listing()
        {
            ExternalMedia = new List<MediaFile>();
            Metadata = new Dictionary<string, string>();
            Hashtags = new List<string>();
            ContactInfo = new ContactInfo();
        }
        /// <summary>
        /// the profile id of the listing owner
        /// </summary>
        public string ProfileId { get; set; }
        /// <summary>
        /// when false, the listing is in draft mode and is not visible in the marketplace
        /// </summary>
        public bool IsPublished { get; set; }
        /// <summary>
        /// the title of the listing
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// the free text body of the listing
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// a string representing the type the listing belongs to. used when searching for listings
        /// </summary>
        public string ListingType { get; set; }
        /// <summary>
        /// tags that identify the listing in searches
        /// </summary>
        public IList<string> Hashtags { get; set; }
        /// <summary>
        /// external media files relating to the listing (images, video, etc)
        /// </summary>
        public IList<MediaFile> ExternalMedia { get; set; }
        /// <summary>
        /// the number of comments for the listing
        /// </summary>
        public int CommentCount { get; set; }
        /// <summary>
        /// the number of times users favorited the listing
        /// </summary>
        public int FavoriteCount { get; set; }
        /// <summary>
        /// the number of times users flagged this listing
        /// </summary>
        public int FlagCount { get; set; }
        /// <summary>
        /// the numer of times users viewed this listing
        /// </summary>
        public int ViewCount { get; set; }
        /// <summary>
        /// the number of times the listing was clicked
        /// </summary>
        public int ClickCount { get; set; }
        /// <summary>
        /// the number of times the listing was purchased (if listing is a product)
        /// </summary>
        public int PurchaseCount { get; set; }
        /// <summary>
        /// the number of times the listing was booked (if listing is a bookable listing)
        /// </summary>
        public int BookingCount { get; set; }
        /// <summary>
        /// the number of times the listing has been added to a collection
        /// </summary>
        public int AddToCollectionCount { get; set; }

        /// <summary>
        /// contact info for the listing. use to override contact info of the listing owner <see cref="Profile"/>
        /// </summary>
        public ContactInfo ContactInfo { get; set; }

        /// <summary>
        /// pricing information for the listing. when present, the listing can be purchased
        /// </summary>
        public PricingInfo PricingInfo { get; set; }

        /// <summary>
        /// scheduling and booking pricing information for the listing. when present, the listing can be booked
        /// </summary>
        public TimeslotSchedule SchedulingTemplate { get; set; }
 
        /// <summary>
        /// a dictionary of app specific key-value pairs that can be used to extend the listing object, and can be used in search
        /// </summary>
        public IDictionary<string, string> Metadata { get; set; }
    }
}