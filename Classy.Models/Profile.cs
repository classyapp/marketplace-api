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
    public class Profile : BaseObject, ITranslatable<Profile>
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
        public string GoogleUserName { get; set; }
        public MediaFile Avatar { get; set; }
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
        public string DefaultCulture { get; set; }

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

        // Translations
        public IDictionary<string, ProfileTranslation> Translations { get; set; }

        public Profile Translate(string culture) 
        {
            if (culture != this.DefaultCulture)
            {
                if (Translations != null && !string.IsNullOrEmpty(culture))
                {
                    ProfileTranslation translation = null;
                    if (Translations.TryGetValue(culture, out translation))
                    {
                        if (this.IsProfessional && !string.IsNullOrEmpty(translation.CompanyName))
                        {
                            this.ProfessionalInfo.CompanyName = translation.CompanyName;
                        }
                        if (translation.Metadata != null)
                        {
                            foreach (var key in translation.Metadata.Keys)
                            {
                                Metadata[key] = translation.Metadata[key];
                            }
                        }
                    }
                }
            }

            return this;
        }
    }
}