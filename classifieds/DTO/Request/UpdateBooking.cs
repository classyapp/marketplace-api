using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Classy.Models.Request
{
    public class UpdateBooking : BaseRequestDto
    {
        public string BookingId { get; set; }
        public DateRange DateRange { get; set; }
        public int MaxDoubleBookings { get; set; }
        public string Comment { get; set; }
        // TODO: what if the price for the new timeslot is cheaper or more expensive? need refund or re-charge...
        // TODO: what if the original booking has been fulfilled? or its time is in the past?
    }
}