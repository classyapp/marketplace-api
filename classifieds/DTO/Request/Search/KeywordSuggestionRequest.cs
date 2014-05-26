using Classy.Models;

namespace classy.DTO.Request.Search
{
    public class KeywordSuggestionRequest : BaseRequestDto
    {
        public string q { get; set; }

        public string Lang { get; set; }
    }
}