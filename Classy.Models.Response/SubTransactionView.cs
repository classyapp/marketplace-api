using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Classy.Models.Response
{
    public class SubTransactionView
    {
        public DateTime Created { get; set; }
        public double Amount { get; set; }
        public string GatewayRefId { get; set; }
    }
}