using Classy.Models;

namespace classy.DTO.Request.Images
{
    public class GetCollageRequest : BaseRequestDto
    {
        public string[] ImageKeys { get; set; }
    }
}