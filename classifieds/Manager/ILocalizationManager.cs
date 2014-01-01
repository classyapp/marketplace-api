using Classy.Models;
using Classy.Models.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace classy.Manager
{
    public interface ILocalizationManager
    {
        LocalizationResourceView GetResourceByKey(string appId, string key);
        LocalizationResourceView SetResourceValues(string appId, string key, IDictionary<string, string> values);
    }
}
