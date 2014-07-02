using Classy.Models;

namespace Classy.Repository
{
    public interface ITaxCalculator
    {
        decimal CalculateTax(Profile merchantProfile, decimal amount, PhysicalAddress address);
    }
}
