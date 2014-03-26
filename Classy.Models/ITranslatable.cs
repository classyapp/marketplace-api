using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classy.Models
{
    public interface ITranslatable<T>
    {
        T Translate(string culture);
    }
}
