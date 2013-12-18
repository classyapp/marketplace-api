﻿using ServiceStack.FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Classy.Models.Request
{
    public class PublishListing : BaseRequestDto
    {
        public string ListingId { get; set; }
    }
}