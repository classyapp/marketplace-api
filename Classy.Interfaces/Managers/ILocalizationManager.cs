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
        IList<string> GetResourceKeysForApp(string appId);
        LocalizationResourceView GetResourceByKey(string appId, string key, bool processMarkdown);
        LocalizationResourceView SetResourceValues(string appId, string key, IDictionary<string, string> values);
        IEnumerable<LocalizationResourceView> GetAllResources(string appId);
        LocalizationListResourceView GetListResourceByKey(string appId, string key);
        LocalizationListResourceView SetListResourceValues(string appId, string key, IList<ListItem> listItems);
        IList<string> GetCitiesByCountry(string appId, string countryCode);
    }
}
