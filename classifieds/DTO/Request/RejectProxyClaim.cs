using ServiceStack.FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Classy.Models.Request
{
    public class RejectProxyClaim : BaseRequestDto
    {
        public string ClaimId { get; set; }
    }
}