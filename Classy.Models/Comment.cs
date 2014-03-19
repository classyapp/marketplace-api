using ServiceStack.FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Classy.Models
{
    public class Comment : BaseObject
    {
        public string ObjectId { get; set; }
        public string ProfileId { get; set; }
        public string Content { get; set; }
    }
}