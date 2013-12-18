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
    public class Listing : BaseObject
    {
        public Listing()
        {
            ExternalMedia = new List<MediaFile>();
            Metadata = new List<CustomAttribute>();
            Hashtags = new List<string>();
            ContactInfo = new ContactInfo();
        }
        //
        public string ProfileId { get; set; }
        public bool IsPublished { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string ListingType { get; set; }
        public IList<string> Hashtags { get; set; }
        public IList<MediaFile> ExternalMedia { get; set; }
        public int CommentCount { get; set; }
        public int FavoriteCount { get; set; }
        public int FlagCount { get; set; }
        public int ViewCount { get; set; }
        public int ClickCount { get; set; }
        public int PurchaseCount { get; set; }
        public int BookingCount { get; set; }

        // contact info
        public ContactInfo ContactInfo { get; set; }

        // pricing info
        public PricingInfo Pricing { get; set; }

        // booking and scheduling
        public TimeslotSchedule SchedulingTemplate { get; set; }
        public IList<BookedTimeslot> BookedTimeslots { get; set; }

        // custom metadata
        public IList<CustomAttribute> Metadata { get; set; }
    }
}