using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Classy.Models.Request
{
    public class GetProfileTranslation :BaseRequestDto
    {
        public string ProfileId { get; set; }
        public string CultureCode { get; set; }
    }
}