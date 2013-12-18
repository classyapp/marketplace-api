using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Classy.Repository
{
    public class DummyTaxCalculator : ITaxCalculator
    {
        public double CalculateTax(Models.Profile merchantProfile, double amount, Models.PhysicalAddress address)
        {
            return 0;
        }
    }

    public class DummyShippingCalculator : IShippingCalculator
    {
        public double GetShippingPrice(Models.Profile merchantProfile, Models.Location origin, Models.PhysicalAddress destination)
        {
            return 0;
        }
    }
}