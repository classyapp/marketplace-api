using ServiceStack.FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Classy.Models.Request
{
    public class UpdateProfile : BaseRequestDto
    {
        public string ProfileId { get; set; }
        public IList<CustomAttribute> Metadata { get; set; }
        public Seller SellerInfo { get; set; }
    }
}