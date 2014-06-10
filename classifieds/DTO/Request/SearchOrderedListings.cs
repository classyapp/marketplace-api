using Classy.Models;

namespace classy.DTO.Request
{
    public class SearchOrderedListings : BaseRequestDto
    {
        public string[] ListingTypes { get; set; }
        public int Page { get; set; }
        public string Date { get; set; }
        public int? PageSize { get; set; }
    }
}