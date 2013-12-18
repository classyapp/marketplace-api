using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Classy.Models
{
    public class PaymentGatewayResponse
    {
        public string GatewayRefId { get; set; }
        public string StatusCode { get; set; }
        public string StatusReason { get; set; }
    }
}
