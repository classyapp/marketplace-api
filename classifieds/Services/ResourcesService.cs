using System.Net;
using classy.Manager;
using Classy.Models.Request;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;

namespace classy.Services
{
    public class ResourcesService : Service
    {
        public ILocalizationManager LocalizationManager { get; set; }

        //
        // GET: /resource/{Key}
        // get resource by key
        public object Get(GetResourceByKey request)
        {
            var resource = LocalizationManager.GetResourceByKey(request.Environment.AppId, request.Key, request.ProcessMarkdown);
            return new HttpResult(resource, HttpStatusCode.OK);
        }

        //
        // GET: /resource/all
        // get all available resources for an app
        [CustomAuthenticate]
        [CustomRequiredPermission("cms")]
        public object Get(GetResourcesForApp request)
        {
            var resourceKeys = LocalizationManager.GetResourcesForApp(request.Environment.AppId);
            return new HttpResult(resourceKeys, HttpStatusCode.OK);
        }

        //
        // GET: /resource/list/{Key}
        // get resource by key
        public object Get(GetListResourceByKey request)
        {
            var resource = LocalizationManager.GetListResourceByKey(request.Environment.AppId, request.Key);
            return new HttpResult(resource, HttpStatusCode.OK);
        }

        //
        // POST: /resource/{Key}
        // set resource values
        [CustomAuthenticate]
        [CustomRequiredPermission("cms")]
        public object Post(SetResourceValues request)
        {
            var resource = LocalizationManager.SetResourceValues(request.Environment.AppId, request.Key, request.Values);
            return new HttpResult(resource, HttpStatusCode.OK);
        }

        //
        // POST: /resource
        // create new resource
        [CustomAuthenticate]
        [CustomRequiredPermission("cms")]
        public object Post(CreateNewResource request)
        {
            var resource = LocalizationManager.CreateResource(request.Environment.AppId, request.Key, request.Values, request.Description);
            return new HttpResult(resource, HttpStatusCode.OK);
        }

        //
        // POST: /resource/list/{Key}
        // set resource values
        [CustomAuthenticate]
        [CustomRequiredPermission("cms")]
        public object Post(SetResourceListValues request)
        {
            var listResource = LocalizationManager.SetListResourceValues(request.Environment.AppId, request.Key, request.ListItems);
            return new HttpResult(listResource, HttpStatusCode.OK);
        }
    }
}