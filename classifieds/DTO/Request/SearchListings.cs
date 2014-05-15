using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;
using ServiceStack.FluentValidation;

namespace Classy.Models.Request
{
    public class SearchListings : BaseRequestDto
    {
        public string[] Tags { get; set; }
        public string[] ListingTypes { get; set; }
        public IDictionary<string, string[]> Metadata { get; set; }
        public double? PriceMin { get; set; }
        public double? PriceMax { get; set; }
        public Location Location { get; set; }
        public bool IncludeComments { get; set; }
        public bool FormatCommentsAsHtml { get; set; }
        public int Page { get; set; }
        public int? PageSize { get; set; }
    }
}