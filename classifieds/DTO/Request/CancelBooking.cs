using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Classy.Models.Request
{
    public class CancelBooking : BaseRequestDto
    {
        public string BookingId { get; set; }
        public bool DoRefund { get; set; }
    }
}