﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Classy.Models.Request
{
    public class VerifyEmailRequest : BaseRequestDto
    {
        public string Hash { get; set; }
    }
}