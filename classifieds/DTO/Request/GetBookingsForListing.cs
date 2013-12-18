﻿using ServiceStack.FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Classy.Models.Request
{
    public class GetBookingsForListing : BaseRequestDto
    {
        public string ListingId { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public bool InculdeCancelled { get; set; }
    }
}