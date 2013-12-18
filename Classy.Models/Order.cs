using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Classy.Models
{
    public class ShippingStatus
    {
        public DateTime? ShippingDate { get; set; }
        public bool IsShipped { get { return ShippingDate.HasValue; } }
        public IList<string> TrackingRefs { get; set; }
    }

    public class Payout
    {
        public double Amount { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class SettlementStatus
    {
        public SettlementStatus()
        {
            Payouts = new List<Payout>();
        }

        public IList<Payout> Payouts { get; set; }
    }

    public class OrderItem
    {
        public string SKU { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }
        public double Shipping { get; set; }
        public double Tax { get; set; }
    }

    public class Order : BaseObject
    {
        public Order()
        {
            ShippingStatus = new ShippingStatus();
            SettlementStatus = new SettlementStatus();
        }

        public string ListingId { get; set; }
        public string ProfileId { get; set; }
        public PhysicalAddress ShippingAddress { get; set; }
        public IList<OrderItem> OrderItems { get; set; }
        public string TransactionId { get; set; }
        public double TotalItemPrice { get; set; }
        public double TotalShipping { get; set; }
        public double TotalTax { get; set; }
        public double TotalAmount { get; set; }
        public bool IsCancelled { get; set; }
        public ShippingStatus ShippingStatus { get; set; }
        public SettlementStatus SettlementStatus { get; set; }
    }
}