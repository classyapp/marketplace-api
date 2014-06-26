namespace Classy.Models.Request
{
    public class GetCollectionByProfileId : BaseRequestDto
    {
        public string ProfileId { get; set; }
        public string CollectionType { get; set; }
    }
}
