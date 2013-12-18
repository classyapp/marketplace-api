using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Classy.Models.Response
{
    public class PostReviewResponse : BaseDeleteResponse
    {
        public ReviewView Review { get; set; }
        public ProfileView RevieweeProfile { get; set; }
        public ProfileView ReviewerProfile { get; set; }
    }
}