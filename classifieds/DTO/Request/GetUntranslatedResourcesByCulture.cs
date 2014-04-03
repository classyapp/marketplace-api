using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Classy.Models.Request
{
    public class GetUntranslatedResourcesByCulture : BaseRequestDto
    {
        public string Culture { get; set; }
    }
}