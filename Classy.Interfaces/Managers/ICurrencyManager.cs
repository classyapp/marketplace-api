namespace Classy.Interfaces.Managers
{
    public interface ICurrencyManager
    {
        decimal GetRate(string fromCurrency, string toCode, decimal adjustPercentage);
    }
}
