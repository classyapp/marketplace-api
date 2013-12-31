using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Classy.Models.Response
{
    public class TripleView
    {
        public string ObjectId { get; set; }
        public string Predicate { get; set; }
        public string SubjectId { get; set; }
    }
}