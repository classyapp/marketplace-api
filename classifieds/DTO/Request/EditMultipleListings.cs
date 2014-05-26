using System.Collections.Generic;
using Classy.Models;

namespace classy.DTO.Request
{
    public class EditMultipleListings : BaseRequestDto
    {
        public string[] ListingIds { get; set; }
        public int EditorsRank { get; set; }
        public Dictionary<string, string> Metadata { get; set; } 
    }
}