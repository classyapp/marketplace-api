using System;
using System.Collections.Generic;
using Classy.Models.Attributes;

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
        public decimal Price { get; set; }
        public decimal Shipping { get; set; }
        public decimal Tax { get; set; }
    }

    [MongoCollection(Name = "orders")]
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
        public decimal TotalItemPrice { get; set; }
        public decimal TotalShipping { get; set; }
        public decimal TotalTax { get; set; }
        public decimal TotalAmount { get; set; }
        public bool IsCancelled { get; set; }
        public ShippingStatus ShippingStatus { get; set; }
        public SettlementStatus SettlementStatus { get; set; }
    }
}