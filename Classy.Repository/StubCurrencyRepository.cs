namespace Classy.Repository
{
    public class StubCurrencyRepository : ICurrencyRepository
    {
        public decimal? GetExchangeRate(string from, string to)
        {
            if (from == "ILS" && to == "EUR")
                return (decimal?) 0.2;
            if (from == "EUR" && to == "ILS")
                return 5;

            if (from == "ILS" && to == "USD")
                return (decimal?) (1.0 / 3.5);
            if (from == "USD" && to == "ILS")
                return (decimal?) 3.5;

            return 1;
        }
    }
}