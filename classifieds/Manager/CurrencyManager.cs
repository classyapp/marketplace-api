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

        public decimal GetRate(string fromCurrency, string toCurrency, decimal adjustPercentage)
        {
            if (fromCurrency == toCurrency)
                return (decimal)1.0;

            decimal? rate = _currencyRepository.GetExchangeRate(fromCurrency, toCurrency);
            if (rate.HasValue)
                return rate.Value * (1 + adjustPercentage);

            return (decimal) 1.0;
        }
    }
}