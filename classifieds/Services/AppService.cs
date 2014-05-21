using System.Net;
using classy.Manager;
using Classy.Models.Request;
using Classy.Models.Response;
using ServiceStack.Common;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;

namespace classy.Services
{
    public class AppService : Service
    {
        public IAppManager AppManager { get; set; }
        public object Get(GetAppSettings request)
        {
            var app = AppManager.GetAppById(request.Environment.AppId);
            return new HttpResult(app.Translate(request.Environment.CultureCode).ToAppView(), HttpStatusCode.OK);
        }
    }
}