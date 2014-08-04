using Classy.Models;

namespace Classy.Repository
{
    public interface IShippingCalculator
    {
        decimal GetShippingPrice(Profile merchantProfile, Location origin, PhysicalAddress destination);
    }
}
