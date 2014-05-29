using Classy.Models;

namespace classy.DTO.Request.LogActivity
{
    public class LogActivityRequest : BaseRequestDto
    {
        public string SubjectId { get; set; }
        public string Predicate { get; set; }
        public string ObjectId { get; set; }
    }
}