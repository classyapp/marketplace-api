using ServiceStack.Common.Web;
using ServiceStack.ServiceHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack.Text;

namespace Classy.Tests
{
    public class TestCustomAuthenticationAttribute
    {
        public static void SetEnvironment(IHttpRequest req, IHttpResponse res, object dto)
        {
            if (req.HttpMethod != HttpMethods.Options)
            {
                if (!(new Uri(req.AbsoluteUri).AbsolutePath.ToLower().StartsWith("/thumbnail")))
                {
                    var json = req.Headers["X-Classy-Env"];
                    var env = json.FromJson<Classy.Models.Env>();
                    if (!VerifyApiKey(env != null ? env.AppId : null))
                    {
                        throw HttpError.Unauthorized("Invalid API Key");
                    }
                    if (dto != null && dto is Classy.Models.BaseRequestDto) (dto as Classy.Models.BaseRequestDto).Environment = env;
                }
            }
        }

        private static bool VerifyApiKey(string apiKey)
        {
            return apiKey != null && apiKey == "Test";
        }
    }
}
