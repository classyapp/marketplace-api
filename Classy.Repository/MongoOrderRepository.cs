using Classy.Models;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Classy.Repository
{
    public class MongoOrderRepository : IOrderRepository
    {
        private MongoCollection<Order> OrdersCollection;

        public MongoOrderRepository(MongoDatabase db)
        {
            OrdersCollection = db.GetCollection<Order>("orders");
        }

        public string Save(Order order)
        {
            try
            {
                OrdersCollection.Save(order);
                return order.Id;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public void Cancel(string appId, string orderId)
        {
            try
            {
                var query = Query<Order>.Where(x =>
                    x.AppId == appId &&
                    x.Id == orderId);

                var update = Update<Order>.Set(x => x.IsCancelled, true);

                OrdersCollection.Update(query, update);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public Order GetById(string appId, string orderId, bool includeCancelled)
        {
            try
            {
                var query = Query<Order>.Where(x =>
                    x.AppId == appId &&
                    x.Id == orderId &&
                    x.IsCancelled == includeCancelled);

                var order = OrdersCollection.FindOne(query);
                return order;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public IList<Order> GetByListingId(string appId, string listingId, bool includeCancelled)
        {
            try
            {
                var query = Query<Order>.Where(x => 
                    x.AppId == appId && 
                    x.ListingId == listingId && 
                    x.IsCancelled == includeCancelled);

                return OrdersCollection.Find(query).ToList();
            }
            catch(MongoException)
            {
                throw;
            }
        }

        public IList<Order> GetByListingId(string appId, string[] listingIds, bool includeCancelled)
        {
            try
            {
                var query = Query.And(
                    Query<Order>.Where(x =>
                        x.AppId == appId &&
                        x.IsCancelled == includeCancelled),
                    Query<Order>.In(x => x.ListingId, listingIds));

                return OrdersCollection.Find(query).ToList();
            }
            catch (MongoException)
            {
                throw;
            }
        }

        public IList<Order> GetByProfileId(string appId, string profileId, bool includeCancelled)
        {
            try
            {
                var query = Query<Order>.Where(x =>
                    x.AppId == appId &&
                    x.ProfileId == profileId &&
                    x.IsCancelled == includeCancelled);

                return OrdersCollection.Find(query).ToList();
            }
            catch (MongoException)
            {
                throw;
            }
        }
    }
}