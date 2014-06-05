namespace Classy.Models.Request
{
    public abstract class PostComment : BaseRequestDto
    {
        public string Content { get; set; }
        public bool FormatAsHtml { get; set; }
    }
}