using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Classy.Models;
using ServiceStack.Common;

namespace Classy.Models
{
    /// <summary>
    /// Custom User DataModel for harvesting UserAuth info into your own DB table
    /// </summary>   
    public class Profile : BaseObject
    {
        public Profile()
        {
            ContactInfo = new ContactInfo();
            FolloweeProfileIds = new List<string>();
            Metadata = new Dictionary<string, string>();
            Created = DateTime.UtcNow;
        }

        public ContactInfo ContactInfo { get; set; }
        public string UserName { get; set; }
        public string TwitterUserId { get; set; }
        public string TwitterScreenName { get; set; }
        public string FacebookUserId { get; set; }
        public string FacebookUserName { get; set; }
        public string ImageUrl { get; set; }
        public string ThumbnailUrl { get; set; }
        public int Rank { get; set; }
        public int FollowerCount { get; set; }
        public int FollowingCount { get; set; }
        public int ListingCount { get; set; }
        public int CommentCount { get; set; }
        public int ViewCount { get; set; }
        public int ReviewCount { get; set; }
        public decimal ReviewAverageScore { get; set; }
        public IDictionary<string, decimal> ReviewAverageSubCriteria { get; set; }
        public IList<string> FolloweeProfileIds { get; set; }
        public IDictionary<string, string> Metadata { get; set; }
        public ProfessionalInfo ProfessionalInfo { get; set; }
        public IList<string> Permissions { get; set; }

        //
        public bool IsProfessional {
            get {
                return this.ProfessionalInfo != null;
            }
        }

        //
        public bool IsVendor
        {
            get
            {
                return this.ProfessionalInfo != null &&
                    this.ProfessionalInfo.PaymentDetails != null;
            }
        }

        //
        public bool IsVerifiedProfessional
        {
            get
            {
                return
                    this.IsProfessional &&
                    this.ReviewAverageScore >= 2;
            }
        }

        //
        public bool IsProxy
        {
            get
            {
                return this.IsProfessional && this.ProfessionalInfo.IsProxy;
            }
        }
    }
}