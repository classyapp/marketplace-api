﻿using System.Security.Cryptography.X509Certificates;
using Classy.Interfaces.Search;
using Classy.Models;
using Classy.Models.Response;
using Classy.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver.Linq;
using ServiceStack.Common;

namespace classy.Manager
{
    public class DefaultBookingManager : IBookingManager
    {
        private IListingRepository ListingRepository;
        private IBookingRepository BookingRepository;
        private IPaymentGateway PaymentGateway;
        private ITripleStore TripleStore;
        private IIndexer<Listing> ListingIndexer; 

        public DefaultBookingManager(
            IListingRepository listingRepository,
            IBookingRepository bookingRepository,
            IPaymentGateway paymentGateway,
            ITripleStore tripleStore,
            IIndexer<Listing> listingIndexer)
        {
            ListingRepository = listingRepository;
            BookingRepository = bookingRepository;
            PaymentGateway = paymentGateway;
            TripleStore = tripleStore;
            ListingIndexer = listingIndexer;
        }

        public BookedTimeslot BookListing(
            string appId,
            string listingId, 
            string profileId, 
            PaymentMethod paymentMethod, 
            DateRange timeslot, 
            string comment, 
            int maxDoubleBookings)
        {
            // get the listing
            var listing = GetVerifiedListing(appId, listingId);
            if (listing.SchedulingTemplate == null) throw new Exception("no scheduling template found for listing");

            // calculate the price of the timeslot
            var price = listing.SchedulingTemplate.GetTimeslotPrice(timeslot);
            
            // charge via gateway
            try
            {
                // save a transaction
                var transaction = PaymentGateway.Charge(appId, price, "USD", paymentMethod, true);

                // save the timeslot
                var bookedTimeslot = new BookedTimeslot
                {
                    AppId = appId,
                    ListingId = listingId,
                    ProfileId = profileId,
                    DateRange = timeslot,
                    Price = price,
                    Comment = comment,
                    TransactionId = transaction.Id
                };
                BookingRepository.Save(bookedTimeslot, maxDoubleBookings);

                // increase booking counter on listing
                ListingRepository.IncreaseCounter(listingId, appId, ListingCounters.Bookings, 1);
                ListingIndexer.Increment(listingId, appId, x => x.BookingCount);

                // log the booking activity
                int count = 1;
                TripleStore.LogActivity(appId, profileId, ActivityPredicate.BOOK_LISTING, listingId, null, ref count);

                // return the transaction
                return bookedTimeslot;
            }
            catch(PaymentGatewayException pex)
            {
                throw;
            }
        }

        public IList<BookedTimeslot> GetBookingsForProfileListings(
            string appId,
            string profileId,
            DateRange dateRange,
            bool includeCancelled)
        {
            var listings = ListingRepository.GetByProfileId(profileId, appId, false, null);
            var listingIds = listings.Select(x => x.Id).ToArray();
            return BookingRepository.GetByListingId(listingIds, appId, dateRange, includeCancelled);
        }

        public IList<BookedTimeslot> GetBookingsForProfile(
            string appId,
            string profileId,
            DateRange dateRange,
            bool includeCancelled)
        {
            return BookingRepository.GetByProfileId(profileId, appId, dateRange, includeCancelled);
        }

        public IList<BookedTimeslot> GetBookingsForListing(
            string appId,
            string listingId,
            string profileId,
            DateRange dateRange,
            bool includeCancelled)
        {
            var listing = GetVerifiedListing(appId, listingId, profileId);
            return BookingRepository.GetByListingId(listingId, appId, dateRange, includeCancelled);
        }

        public BookedTimeslot UpdateBooking(
            string appId,
            string bookingId,
            string profileId,
            DateRange timeslot,
            string comment,
            int maxDoubleBookings)
        {
            var booking = GetVerifiedBooking(appId, bookingId, profileId);

            booking.DateRange = timeslot;
            booking.Comment = comment;
            BookingRepository.Save(booking, maxDoubleBookings);

            return booking;
        }

        public CancelBookingResponse CancelBooking(
            string appId, 
            string bookingId, 
            string profileId, 
            bool doRefund)
        {
            var booking = GetVerifiedBooking(appId, bookingId, profileId);
            var response = new CancelBookingResponse
            {
                Id = bookingId
            };

            // throw if refunded already
            if (doRefund)
            {
                try
                {
                    var transaction = PaymentGateway.Refund(appId, booking.TransactionId, booking.Price);
                    response.RefundTransaction = transaction.Refunds[0].TranslateTo<SubTransactionView>();
                }
                catch(PaymentGatewayException)
                {
                    throw;
                }
            }

            // cancel the booking
            BookingRepository.Cancel(appId, bookingId);

            return response;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="bookingId"></param>
        /// <param name="profileId"></param>
        /// <returns></returns>
        private BookedTimeslot GetVerifiedBooking(string appId, string bookingId, string profileId)
        {
            var booking = GetVerifiedBooking(appId, bookingId);
            if (booking.ProfileId != profileId) throw new UnauthorizedAccessException("access denied");
            return booking;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="bookingId"></param>
        /// <returns></returns>
        private BookedTimeslot GetVerifiedBooking(string appId, string bookingId)
        {
            var booking = BookingRepository.GetById(appId, bookingId, false);
            if (booking == null) throw new KeyNotFoundException("invalid booking");
            return booking;
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
            if (listing == null) throw new KeyNotFoundException("Invalid Listing");
            return listing;
        }
    }
}