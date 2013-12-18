using Classy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classy.Repository
{
    public interface IOrderRepository
    {
        string Save(Order order);
        void Cancel(string appId, string orderId);
        Order GetById(string appId, string orderId, bool includeCancelled);
        IList<Order> GetByListingId(string appId, string listingId, bool includeCancelled);
        IList<Order> GetByListingId(string appId, string[] listingIds, bool includeCancelled);
        IList<Order> GetByProfileId(string appId, string profileId, bool includeCancelled);
    }
}
