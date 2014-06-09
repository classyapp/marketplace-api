using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Classy.Repository;

namespace Classy.Repository
{
    public class StubCurrencyRepository : ICurrencyRepository
    {
        public double? GetExchangeRate(string from, string to)
        {
            if (from == "ILS" && to == "EUR")
                return 0.2;
            if (from == "EUR" && to == "ILS")
                return 5;

            if (from == "ILS" && to == "USD")
                return 1.0 / 3.5;
            if (from == "USD" && to == "ILS")
                return 3.5;

            return 1;
        }
    }
}