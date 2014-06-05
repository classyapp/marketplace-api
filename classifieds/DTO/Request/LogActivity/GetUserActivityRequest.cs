using Classy.Models;

namespace classy.DTO.Request.LogActivity
{
    public class GetUserActivityRequest : BaseRequestDto
    {
        public string UserId { get; set; }
        public string Predicate { get; set; }
    }
}