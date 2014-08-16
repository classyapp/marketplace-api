using System;
using System.Collections.Generic;
using System.Linq;
using Classy.Interfaces.Search;
using Classy.Models;
using Classy.Repository;
using Classy.Models.Response;
using ServiceStack.Common;

namespace classy.Manager
{
    public class DefaultOrderManager : IOrderManager
    {
        private IListingRepository ListingRepository;
        private IProfileRepository ProfileRepository;
        private IOrderRepository OrderRepository;
        private IIndexer<Listing> ListingIndexer;
        private IPaymentGateway PaymentGateway;
        private ITripleStore TripleStore;
        public ITaxCalculator TaxCalculator;
        public IShippingCalculator ShippingCalculator;

        public DefaultOrderManager(
            IListingRepository listingRepository,
            IProfileRepository profileRepository,
            IOrderRepository orderRepository,
            IPaymentGateway paymentGateway,
            ITripleStore tripleStore,
            ITaxCalculator taxCalculator,
            IShippingCalculator shippingCalculator,
            IIndexer<Listing> listingIndexer)
        {
            ListingRepository = listingRepository;
            OrderRepository = orderRepository;
            PaymentGateway = paymentGateway;
            TripleStore = tripleStore;
            TaxCalculator = taxCalculator;
            ShippingCalculator = shippingCalculator;
            ProfileRepository = profileRepository;
            ListingIndexer = listingIndexer;
        }

        public Order PlaceSingleItemOrder(
            string appId, 
            string listingId, 
            string profileId, 
            PaymentMethod paymentMethod,
            PhysicalAddress shippingAddress,
            string sku, 
            int quantity)
        {
            // get the listing
            var listing = GetVerifiedListing(appId, listingId);
            if (listing.PricingInfo == null) throw new ApplicationException("this listing cannot be purchased");
            var profile = ProfileRepository.GetById(appId, listing.ProfileId, false, null);

            // calculate the price of the sku
            var price = listing.PricingInfo.GetPriceForSKU(sku);
            var shipping = ShippingCalculator.GetShippingPrice(profile, listing.ContactInfo.Location, shippingAddress);
            var tax = TaxCalculator.CalculateTax(profile, price, shippingAddress);
            var orderTotal = (price * quantity) + shipping + tax;

            // charge via gateway
            try
            {
                var transaction = PaymentGateway.Charge(appId, orderTotal, "USD", paymentMethod, true);

                // save the order
                var order = new Order
                {
                    AppId = appId,
                    ListingId = listingId,
                    ProfileId = profileId,
                    OrderItems = new List<OrderItem> 
                    { 
                        new OrderItem {
                            SKU = sku,
                            Quantity = quantity,
                            Price = price,
                            Shipping = 0,
                            Tax = 0,
                        }
                    },
                    TransactionId = transaction.Id,
                    TotalItemPrice = price,
                    TotalShipping = shipping,
                    TotalTax = tax,
                    TotalAmount = orderTotal
                };
                OrderRepository.Save(order);

                // log the purchase activity
                int count = 0;
                TripleStore.LogActivity(appId, profileId, ActivityPredicate.PURCHASE_LISTING, listingId, null, ref count);

                // increase purchase counter
                ListingRepository.IncreaseCounter(listingId, appId, ListingCounters.Purchases, quantity);
                ListingIndexer.Increment(listingId, appId, x => x.PurchaseCount, quantity);

                // return the order
                return order;
            }
            catch (PaymentGatewayException)
            {
                throw;
            }
        }

        public Order UpdateSingleItemOrder(
            string appId,
            string orderId,
            string profileId,
            string SKU,
            int quantity)
        {
            var order = GetVerifiedOrder(appId, orderId, profileId);
            var item = order.OrderItems.Where(x => x.SKU == SKU).SingleOrDefault();
            if (item == null) throw new ArgumentOutOfRangeException("sku not found in order");
            if (item.Quantity < quantity) throw new ArgumentOutOfRangeException("can only decrease quantity");
            item.Quantity = quantity;
            OrderRepository.Save(order);
            return order;
        }

        public CancelOrderResponse CancelSingleItemOrder(
            string appId,
            string orderId,
            string profileId,
            bool doRefund)
        {
            var order = GetVerifiedOrder(appId, orderId, profileId);
            var response = new CancelOrderResponse
            {
                Id = orderId
            };

            // throw if refunded already
            if (doRefund)
            {
                try
                {
                    var transaction = PaymentGateway.Refund(appId, order.TransactionId, order.TotalAmount);
                    response.RefundTransaction = transaction.Refunds[0].TranslateTo<SubTransactionView>();
                }
                catch (PaymentGatewayException)
                {
                    throw;
                }
            }

            // cancel the order
            OrderRepository.Cancel(appId, orderId);

            return response;
        }

        public IList<Order> GetOrdersForListing(
            string appId,
            string listingId,
            string profileId,
            bool includeCancelled)
        {
            var listing = GetVerifiedListing(appId, listingId, profileId);
            var orders = OrderRepository.GetByListingId(appId, listingId, includeCancelled);
            return orders;
        }

        public IList<Order> GetOrdersForProfile(
            string appId,
            string profileId,
            bool includeCancelled)
        {
            return OrderRepository.GetByProfileId(appId, profileId, includeCancelled);
        }

        public IList<Order> GetOrdersForProfileListings(
            string appId,
            string profileId,
            bool includeCancelled)
        {
            var listings = ListingRepository.GetByProfileId(profileId, appId, false, null);
            var listingIds = listings.Select(x => x.Id).ToArray();
            return OrderRepository.GetByListingId(appId, listingIds, includeCancelled);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="listingId"></param>
        /// <param name="profileId"></param>
        /// <returns></returns>
        private Listing GetVerifiedListing(string appId, string listingId, string profileId)
        {
            var listing = GetVerifiedListing(appId, listingId);
            if (listing.ProfileId != profileId) throw new UnauthorizedAccessException("not authorized");
            return listing;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="listingId"></param>
        /// <returns></returns>
        private Listing GetVerifiedListing(string appId, string listingId)
        {
            var listing = ListingRepository.GetById(listingId, appId, false, null);
            if (listing == null) throw new KeyNotFoundException("invalid listing");
            return listing;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="orderId"></param>
        /// <param name="profileId"></param>
        /// <returns></returns>
        private Order GetVerifiedOrder(string appId, string orderId, string profileId)
        {
            var order = GetVerifiedOrder(appId, orderId);
            if (order.ProfileId != profileId) throw new UnauthorizedAccessException("not authorized");
            return order;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="orderId"></param>
        /// <returns></returns>
        private Order GetVerifiedOrder(string appId, string orderId)
        {
            var order = OrderRepository.GetById(appId, orderId, false);
            if (order == null) throw new KeyNotFoundException("invalid order");
            return order;
        }
    }
}