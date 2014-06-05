namespace Classy.Models.Request
{
    public class LogActivity : BaseRequestDto
    {
        public string SubjectId { get; set; }
        public string Predicate { get; set; }
        public string ObjectId { get; set; }
    }
}