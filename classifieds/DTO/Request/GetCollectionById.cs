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
        public bool IncludeProfile { get; set; }
        public bool IncludeListings { get; set; }
        public bool IncludeCollaboratorProfiles { get; set; }
        public bool IncludePermittedViewersProfiles { get; set; }
        public bool IncludeDrafts { get; set; }
        public bool IncreaseViewCounter { get; set; }
        public bool IncreaseViewCounterOnListings { get; set; }
        public bool IncludeComments { get; set; }
        public bool FormatCommentsAsHtml { get; set; }
        public bool IncludeCommenterProfiles { get; set; }
    }
}