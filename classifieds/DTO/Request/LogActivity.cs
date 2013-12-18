using Classy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Classy.Models.Request
{
    public class LogActivity : BaseRequestDto
    {
        public string SubjectId { get; set; }
        public ActivityPredicate Predicate { get; set; }
        public string ObjectId { get; set; }
    }
}