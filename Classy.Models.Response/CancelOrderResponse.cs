﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Classy.Models.Response
{
    public class CancelOrderResponse : BaseDeleteResponse
    {
        public SubTransactionView RefundTransaction { get; set; }
    }
}