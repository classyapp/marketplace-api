using Classy.Models;

namespace classy.DTO.Request.Search
{
    public class FreeSearchRequest : BaseRequestDto
    {
        public string Q { get; set; }
        public int Amount { get; set; }
        public int Page { get; set; }
    }
}