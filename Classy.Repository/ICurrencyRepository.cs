using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classy.Repository
{
    public interface ICurrencyRepository
    {
        double? GetExchangeRate(string from, string to);
    }
}
