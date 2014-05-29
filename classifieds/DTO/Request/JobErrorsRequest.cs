using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Classy.Models.Request
{
    public class JobErrorsRequest : BaseRequestDto
    {
        public string JobId { get; set; }
    }
}