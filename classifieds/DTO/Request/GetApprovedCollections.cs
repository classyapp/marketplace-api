using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Classy.Models;

namespace Classy.Models.Request
{
    public class GetApprovedCollections : BaseRequestDto
    {
        public string[] Categories  { get; set; }
        public int MaxCollections { get; set; }
    }
}