using Classy.Models;

namespace classy.DTO.Request.Search
{
    public class SearchSuggestionsRequest : BaseRequestDto
    {
        public string EntityType { get; set; } // This could be 'listing' or 'profile'
        public string q { get; set; }
    }
}