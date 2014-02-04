using ServiceStack.FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Classy.Models.Request
{
    public class SetResourceListValues : BaseRequestDto
    {
        public string Key { get; set; }
        public IList<ListItem> ListItems{ get; set; }
    }
}