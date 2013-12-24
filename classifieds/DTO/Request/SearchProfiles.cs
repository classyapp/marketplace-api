using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;
using ServiceStack.FluentValidation;

namespace Classy.Models.Request
{
    public class SearchProfiles : BaseRequestDto
    {
        public string DisplayName { get; set; }
        public IDictionary<string, string> Metadata { get; set; }
        public Location Location { get; set; }
        public string Category { get; set; }
        public bool ProfessionalsOnly { get; set; }
    }
}