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
        IList<string> GetResourceKeysForApp(string appId);

        LocalizationResource GetResourceByKey(string appId, string key);
        string SetResource(LocalizationResource resource);

        LocalizationListResource GetListResourceByKey(string appId, string key);
        string SetListResource(LocalizationListResource listResource);
    }
}
