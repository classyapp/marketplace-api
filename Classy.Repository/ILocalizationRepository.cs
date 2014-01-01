using Classy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classy.Repository
{
    public interface ILocalizationRepository
    {
        LocalizationResource GetResourceByKey(string appId, string key);
        string SetResource(LocalizationResource resources);
    }
}
