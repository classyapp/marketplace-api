using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Classy.Models.Request
{
    public class UpdateSingleItemOrder : BaseRequestDto
    {
        public string OrderId { get; set; }
        public string SKU { get; set; }
        public int Quantity { get; set; }
        // TODO: what if the price for the new quantity is cheaper or more expensive? need refund or re-charge...
        // TODO: what if the order has been shipped?
    }
}