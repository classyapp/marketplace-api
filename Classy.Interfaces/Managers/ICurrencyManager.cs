using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classy.Interfaces.Managers
{
    public interface ICurrencyManager
    {
        double GetRate(string fromCurrency, string toCode, double adjustPercentage);
    }
}
