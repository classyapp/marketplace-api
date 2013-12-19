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
        public static string PROXY_METADATA_KEY = "ProxyProfile";

        public Profile()
        {
            ContactInfo = new ContactInfo();
            FolloweeUsernames = new List<string>();
            Metadata = new List<CustomAttribute>();
            Created = DateTime.UtcNow;
        }

        public ContactInfo ContactInfo { get; set; }
        public string UserName { get; set; }
        public string TwitterUserId { get; set; }
        public string TwitterScreenName { get; set; }
        public string TwitterName { get; set; }
        public string FacebookName { get; set; }
        public string FacebookFirstName { get; set; }
        public string FacebookLastName { get; set; }
        public string FacebookUserId { get; set; }
        public string FacebookUserName { get; set; }
        public string FacebookEmail { get; set; }
        public string ImageUrl { get; set; }
        public int Rank { get; set; }
        public int FollowerCount { get; set; }
        public int FollowingCount { get; set; }
        public int ListingCount { get; set; }
        public int CommentCount { get; set; }
        public int ViewCount { get; set; }
        public int ReviewCount { get; set; }
        public decimal ReviewAverageScore { get; set; }
        public IList<string> FolloweeUsernames { get; set; }
        public IList<CustomAttribute> Metadata { get; set; }
        public Seller SellerInfo { get; set; }

        //
        public bool IsSeller {
            get {
                return this.SellerInfo != null;
            }
        }

        //
        public bool IsVerifiedSeller
        {
            get
            {
                return
                    this.IsSeller &&
                    this.ReviewAverageScore >= 2 &&
                    this.SellerInfo.PaymentDetails != null;
            }
        }

        //
        public bool IsProxy
        {
            get
            {
                return
                    this.Metadata != null && this.Metadata.ContainsKey(PROXY_METADATA_KEY);
            }
        }
    }
}