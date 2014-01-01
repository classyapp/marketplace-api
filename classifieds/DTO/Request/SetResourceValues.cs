using ServiceStack.FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Classy.Models.Request
{
    public class SetResourceValues : BaseRequestDto
    {
        public string Key { get; set; }
        public IDictionary<string, string> Values { get; set; }
    }
}