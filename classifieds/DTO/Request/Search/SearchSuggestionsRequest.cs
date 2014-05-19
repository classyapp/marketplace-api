using Classy.Models;

namespace classy.DTO.Request.Search
{
    public class SearchSuggestionsRequest : BaseRequestDto
    {
        public string q { get; set; }
    }
}