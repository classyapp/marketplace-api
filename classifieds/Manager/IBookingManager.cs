using Classy.Models;
using Classy.Models.Response;
using Classy.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace classy.Manager
{
    public interface IBookingManager
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="listingId"></param>
        /// <param name="profileId"></param>
        /// <param name="dateRange"></param>
        /// <param name="includeCancelled"></param>
        /// <returns></returns>
        IList<BookedTimeslot> GetBookingsForListing(
            string appId, 
            string listingId,  
            string profileId,
            DateRange dateRange,
            bool includeCancelled);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="profileId"></param>
        /// <param name="dateRange"></param>
        /// <param name="includeCancelled"></param>
        /// <returns></returns>
        IList<BookedTimeslot> GetBookingsForProfileListings(
            string appId,
            string profileId,
            DateRange dateRange,
            bool includeCancelled);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="profileId"></param>
        /// <param name="dateRange"></param>
        /// <param name="includeCancelled"></param>
        /// <returns></returns>
        IList<BookedTimeslot> GetBookingsForProfile(
            string appId,
            string profileId, 
            DateRange dateRange,
            bool includeCancelled);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="listingId"></param>
        /// <param name="profileId"></param>
        /// <param name="paymentMethod"></param>
        /// <param name="timeslot"></param>
        /// <param name="comment"></param>
        /// <param name="maxDoubleBookings"></param>
        /// <returns></returns>
        BookedTimeslot BookListing(
            string appId,
            string listingId, 
            string profileId, 
            PaymentMethod paymentMethod, 
            DateRange timeslot, 
            string comment, 
            int maxDoubleBookings);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="bookingId"></param>
        /// <param name="profileId"></param>
        /// <param name="timeslot"></param>
        /// <param name="comment"></param>
        /// <param name="maxDoubleBookings"></param>
        /// <returns></returns>
        BookedTimeslot UpdateBooking(
            string appId,
            string bookingId,
            string profileId,
            DateRange timeslot,
            string comment,
            int maxDoubleBookings);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="bookingId"></param>
        /// <param name="profileId"></param>
        /// <param name="doRefund"></param>
        CancelBookingResponse CancelBooking(
            string appId,
            string bookingId,
            string profileId,
            bool doRefund);
    }
}
