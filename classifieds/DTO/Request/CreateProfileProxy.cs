using ServiceStack.FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Classy.Models.Request
{
    public class CreateProfileProxy : BaseRequestDto
    {
        public IDictionary<string, string> Metadata { get; set; }
        public Seller SellerInfo { get; set; }
    }
}