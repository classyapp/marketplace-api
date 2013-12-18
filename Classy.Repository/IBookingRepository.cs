using Classy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classy.Repository
{
    public interface IBookingRepository
    {
        string Save(BookedTimeslot timeslot, int doubleBookingMax);
        void Cancel(string appId, string bookingId);
        BookedTimeslot GetById(string appId, string bookingId, bool includeCancelled);
        IList<BookedTimeslot> GetByListingId(string listingId, string appId, DateRange dateRange, bool includeCancelled);
        IList<BookedTimeslot> GetByListingId(string[] listingIds, string appId, DateRange dateRange, bool includeCancelled);
        IList<BookedTimeslot> GetByProfileId(string profileId, string appId, DateRange dateRange, bool includeCancelled);    
    }
}
