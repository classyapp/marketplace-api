using ServiceStack.FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceStack.Common.Web;

namespace Classy.Models.Request
{
    public class PostComment : BaseRequestDto
    {
        public string Content { get; set; }
        public bool FormatAsHtml { get; set; }
    }
}