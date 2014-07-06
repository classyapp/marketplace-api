namespace Classy.Repository
{
    public interface ICurrencyRepository
    {
        decimal? GetExchangeRate(string from, string to);
    }
}
