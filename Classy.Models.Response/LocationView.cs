﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Classy.Models.Response
{
    public class LocationView
    {
        public CoordsView Coords { get; set; }
        public PhysicalAddressView Address { get; set; }
    }
}
