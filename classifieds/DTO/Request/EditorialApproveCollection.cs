using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Classy.Models;

namespace Classy.Models.Request
{
    public class EditorialApproveCollection : BaseRequestDto
    {
        public string CollectionId { get; set; }
        public string[] Categories { get; set; }
        public string Comments { get; set; }
    }
}