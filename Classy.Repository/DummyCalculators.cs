using Classy.Models;

namespace Classy.Repository
{
    public class DummyTaxCalculator : ITaxCalculator
    {
        public decimal CalculateTax(Profile merchantProfile, decimal amount, PhysicalAddress address)
        {
            return 0;
        }
    }

    public class DummyShippingCalculator : IShippingCalculator
    {
        public decimal GetShippingPrice(Profile merchantProfile, Location origin, PhysicalAddress destination)
        {
            return 0;
        }
    }
}