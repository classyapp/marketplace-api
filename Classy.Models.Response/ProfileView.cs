using System.Collections.Generic;

namespace Classy.Models.Response
{
    public class ProfileView
    {
        public ProfileView()
        {
            Metadata = new Dictionary<string, string>();
            FolloweeProfileIds = new List<string>();
        }

        public string Id { get; set; }
        public string UserName { get; set; }
        public ExtendedContactInfoView ContactInfo { get; set; }
        public MediaFileView Avatar{ get; set; }
        public int FollowerCount { get; set; }
        public int FollowingCount { get; set; }
        public int ListingCount { get; set; }
        public int CommentCount { get; set; }
        public int ViewCount { get; set; }
        public int ReviewCount { get; set; }
        public decimal ReviewAverageScore { get; set; }
        public IDictionary<string, decimal> ReviewAverageSubCriteria { get; set; }
        public bool IsVerifiedProfessional { get; set; }
        public bool IsProfessional { get; set; }
        public bool IsEditor { get; set; }
        public bool IsCmsUser { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsVendor { get; set; }
        public bool IsProxy { get; set; }
        public bool IsFacebookConnected { get; set; }
        public bool IsGoogleConnected { get; set; }
        public ProfessionalInfoView ProfessionalInfo { get; set; }
        public int Rank { get; set; }
        public IList<string> FolloweeProfileIds { get; set; }
        public IList<ProfileView> Following { get; set; }
        public IList<ProfileView> FollowedBy { get; set; }
        public IList<ReviewView> Reviews { get; set; }
        public IList<ListingView> Listings { get; set; }
        public IList<CollectionView> Collections { get; set; }
        public IList<string> FavoriteListingIds { get; set; }
        public IDictionary<string, string> Metadata { get; set; }
        public IList<string> Permissions { get; set; }
        public string DefaultCulture { get; set; }
        public IList<string> CoverPhotos { get; set; }
        public bool IsEmailVerified { get; set; }
    }
}