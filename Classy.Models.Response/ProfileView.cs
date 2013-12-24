using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Classy.Models.Response
{
    public class ProfileView
    {
        public ProfileView()
        {
            Metadata = new Dictionary<string, string>();
            FolloweeUsernames = new List<string>();
        }

        public string Id { get; set; }
        public string UserName { get; set; }
        public ExtendedContactInfoView ContactInfo { get; set; }
        public string ImageUrl { get; set; }
        public int FollowerCount { get; set; }
        public int FollowingCount { get; set; }
        public int ListingCount { get; set; }
        public int CommentCount { get; set; }
        public int ViewCount { get; set; }
        public int ReviewCount { get; set; }
        public decimal ReviewAverageScore { get; set; }
        public IDictionary<string, decimal> ReviewAverageSubCriteria { get; set; }
        public bool IsVerified { get; set; }
        public bool IsSeller { get; set; }
        public bool IsProxy { get; set; }
        public SellerView SellerInfo { get; set; }
        public int Rank { get; set; }
        public IList<string> FolloweeUsernames { get; set; }
        public IList<ProfileView> Following { get; set; }
        public IList<ProfileView> FollowedBy { get; set; }
        public IList<ReviewView> Reviews { get; set; }
        public IList<ListingView> Listings { get; set; }
        public IDictionary<string, string> Metadata { get; set; }
    }
}