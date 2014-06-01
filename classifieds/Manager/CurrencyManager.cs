using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Classy.Interfaces.Managers;
using Classy.Repository;

namespace classy.Manager
{
    public class CurrencyManager : ICurrencyManager
    {
        private ICurrencyRepository _currencyRepository;

        public CurrencyManager(ICurrencyRepository currencyRepository)
        {
            _currencyRepository = currencyRepository;
        }

        public double GetRate(string fromCurrency, string toCurrency, double adjustPercentage)
        {
            if (fromCurrency == toCurrency)
                return 1.0;

            double? rate = _currencyRepository.GetExchangeRate(fromCurrency, toCurrency);
            if (rate.HasValue)
                return rate.Value * (1 + adjustPercentage);

            return 1.0;
        }
    }
}