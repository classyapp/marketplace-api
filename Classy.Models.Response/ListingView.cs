using System.Collections.Generic;

namespace Classy.Models.Response
{
    public class ListingView
    {
        public ListingView() { }
        //
        public string Id { get; set; }
        public string ProfileId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public IList<string> Categories { get; set; }
        public IList<string> FeaturedCategories { get; set; } 
        public string ListingType { get; set; }
        public IList<MediaFileView> ExternalMedia { get; set; }
        public LocationView Location { get; set; }
        public int CommentCount { get; set; }
        public int FavoriteCount { get; set; }
        public int ViewCount { get; set; }
        public int ClickCount { get; set; }
        public int BookingCount { get; set; }
        public int PurchaseCount { get; set; }
        public int AddToCollectionCount { get; set; }
        public int DisplayOrder { get; set; }
        public int EditorsRank { get; set; }
        public bool IsPublished { get; set; }
        //
        public IList<CommentView> Comments { get; set; }
        //
        public bool HasPricingInfo { get; set; }
        public PricingInfoView PricingInfo { get; set; }
        //
        public bool HasContactInfo { get; set; }
        public ContactInfoView ContactInfo { get; set; }
        // 
        public bool HasShippingInfo { get; set; }
        public int? DomesticRadius { get; set; }
        public decimal? DomesticShippingPrice { get; set; }
        public decimal? InternationalShippingPrice { get; set; }
        //
        public bool HasInventoryInfo { get; set; }
        public int? Quantity { get; set; }
        //
        public IList<string> Hashtags { get; set; }
        public IDictionary<string, IList<string>> TranslatedKeywords { get; set; }
        public IList<string> SearchableKeywords { get; set; }
        //
        public ProfileView Profile { get; set; }
        public IList<ProfileView> FavoritedBy { get; set; }
        //
        public IDictionary<string, string> Metadata { get; set; }
        //
        public bool HasSchedulingInfo { get; set; }
        public TimeslotScheduleView SchedulingTemplate { get; set; }
        public IList<BookedTimeslotView> BookedTimeslots { get; set; }
        public string DefaultCulture { get; set; }
    }

    
}