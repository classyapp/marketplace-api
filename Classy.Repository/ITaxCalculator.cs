using Classy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classy.Repository
{
    public interface ITaxCalculator
    {
        double CalculateTax(
            Profile merchantProfile, 
            double amount, 
            PhysicalAddress address);
    }
}
