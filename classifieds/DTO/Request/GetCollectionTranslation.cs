using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Classy.Models.Request 
{
    public class GetCollectionTranslation : BaseRequestDto
    {
        public string CollectionId { get; set; }
        public string CultureCode { get; set; }
    }
}