using Classy.Models;
using Classy.Repository.Infrastructure;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Classy.Repository
{
    public class MongoBookingRepository : IBookingRepository
    {
        private MongoCollection<BookedTimeslot> BookingsCollection;

        public MongoBookingRepository(MongoDatabaseProvider db)
        {
            BookingsCollection = db.GetCollection<BookedTimeslot>();
        }
    
        public string Save(BookedTimeslot timeslot, int maxDoubleBookings)
        {
            try
            {
                var query = Query<BookedTimeslot>.Where(x =>
                    x.AppId == timeslot.AppId &&
                    x.ListingId == timeslot.ListingId &&
                    !x.IsCancelled &&
                    x.DateRange.Start >= timeslot.DateRange.Start &&
                    x.DateRange.Start <= timeslot.DateRange.End);
                var bookings = BookingsCollection.Find(query);
                if (bookings.Count() > maxDoubleBookings) throw new ApplicationException("timeslot is fully booked");

                BookingsCollection.Save(timeslot);
                return timeslot.Id;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public IList<BookedTimeslot> GetByListingId(string listingId, string appId, DateRange dateRange, bool includeCancelled)
        {
            var query = Query<BookedTimeslot>.Where(x => 
                    x.AppId == appId &&
                    x.ListingId == listingId &&
                    x.IsCancelled == includeCancelled &&
                    x.DateRange.Start >= dateRange.Start &&
                    x.DateRange.Start <= dateRange.End);

            return BookingsCollection.Find(query).ToList();
        }

        public IList<BookedTimeslot> GetByListingId(string[] listingIds, string appId, DateRange dateRange, bool includeCancelled)
        {
            var query = Query.And(
                Query<BookedTimeslot>.Where(x =>
                    x.AppId == appId &&
                    x.IsCancelled == includeCancelled &&
                    x.DateRange.Start >= dateRange.Start &&
                    x.DateRange.Start <= dateRange.End),
                Query<BookedTimeslot>.In(x => x.ListingId, listingIds));

            return BookingsCollection.Find(query).ToList();
        }

        public IList<BookedTimeslot> GetByProfileId(string profileId, string appId, DateRange dateRange, bool includeCancelled)
        {
            var query = Query<BookedTimeslot>.Where(x =>
                    x.AppId == appId &&
                    x.IsCancelled == includeCancelled &&
                    x.ProfileId == profileId &&
                    x.DateRange.Start >= dateRange.Start &&
                    x.DateRange.Start <= dateRange.End);

            return BookingsCollection.Find(query).ToList();
        }

        public BookedTimeslot GetById(string appId, string bookingId, bool includeCancelled)
        {
            var query = Query<BookedTimeslot>.Where(x =>
                    x.AppId == appId &&
                    x.IsCancelled == includeCancelled &&
                    x.Id == bookingId);

            return BookingsCollection.FindOne(query);
        }


        public void Cancel(string appId, string bookingId)
        {
            var query = Query<BookedTimeslot>.Where(x =>
                    x.AppId == appId &&
                    x.Id == bookingId);

            var update = Update<BookedTimeslot>.Set(x => x.IsCancelled, true);

            BookingsCollection.Update(query, update);
        }
    }
}
