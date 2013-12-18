using Classy.Models;
using Classy.Models.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace classy.Manager
{
    public interface IOrderManager
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="listingId"></param>
        /// <param name="profileId"></param>
        /// <param name="paymentMethod"></param>
        /// <param name="SKU"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        Order PlaceSingleItemOrder(
            string appId,
            string listingId,
            string profileId, 
            PaymentMethod paymentMethod, 
            PhysicalAddress shippingAddress,
            string SKU, 
            int quantity);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="orderId"></param>
        /// <param name="profileId"></param>
        /// <param name="SKU"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        Order UpdateSingleItemOrder(
            string appId,
            string orderId,
            string profileId,
            string SKU,
            int quantity);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="orderId"></param>
        /// <param name="profileId"></param>
        /// <param name="DoRefund"></param>
        /// <returns></returns>
        CancelOrderResponse CancelSingleItemOrder(
            string appId,
            string orderId,
            string profileId,
            bool doRefund);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="listingId"></param>
        /// <param name="profileId"></param>
        /// <param name="includeCancelled"></param>
        /// <returns></returns>
        IList<Order> GetOrdersForListing(
            string appId,
            string listingId,
            string profileId,
            bool includeCancelled);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="profileId"></param>
        /// <param name="includeCancelled"></param>
        /// <returns></returns>
        IList<Order> GetOrdersForProfile(
            string appId,
            string profileId,
            bool includeCancelled);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="profileId"></param>
        /// <param name="includeCancelled"></param>
        /// <returns></returns>
        IList<Order> GetOrdersForProfileListings(
            string appId,
            string profileId,
            bool includeCancelled);

        // Order CancelOrder(string orderId, string appId, string profileId);
        // Order UpdateItemQuantity(string orderId, string appId, string profileId, string SKU, int quantity);
    }
}
