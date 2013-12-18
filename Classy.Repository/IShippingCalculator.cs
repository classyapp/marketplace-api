using Classy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classy.Repository
{
    public interface IShippingCalculator
    {
        double GetShippingPrice(
            Profile merchantProfile, 
            Location origin, 
            PhysicalAddress destination);
    }
}
