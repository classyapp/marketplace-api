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
        IList<LocalizationResourceView> GetResourcesForApp(string appId);
        LocalizationResourceView GetResourceByKey(string appId, string key, bool processMarkdown);
        LocalizationResourceView SetResourceValues(string appId, string key, IDictionary<string, string> values);
        LocalizationResourceView CreateResource(string appId, string key, IDictionary<string, string> values, string description);
        LocalizationListResourceView GetListResourceByKey(string appId, string key);
        LocalizationListResourceView SetListResourceValues(string appId, string key, IList<ListItem> listItems);
        IList<string> GetCitiesByCountry(string appId, string countryCode);
    }
}
