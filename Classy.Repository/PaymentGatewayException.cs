using Classy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Classy.Repository
{
    public class PaymentGatewayException : Exception
    {
        public PaymentGatewayResponse GatewayResponse;

        public PaymentGatewayException(PaymentGatewayResponse gatewayResponse) : 
            base(string.Format("gateway returned code {0} with reason {1}", gatewayResponse.StatusCode, gatewayResponse.StatusReason))
        {
            GatewayResponse = gatewayResponse;
        }
    }
}
