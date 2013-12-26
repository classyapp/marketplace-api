using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;
using ServiceStack.FluentValidation;

namespace Classy.Models.Request
{
    public class GetCollectionById : BaseRequestDto
    {
        public string CollectionId { get; set; }
        public bool IncludeListings { get; set; }
        public bool IncludeCollaboratorProfiles { get; set; }
        public bool IncludePermittedViewersProfiles { get; set; }
    }
}