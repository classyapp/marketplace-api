using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OAuth2;
using ServiceStack.Common.Web;
using ServiceStack.Configuration;
using ServiceStack.Text;

namespace Classy.Auth
{
    public class GoogleOAuth2Provider : OAuth2Provider
    {
        public const string Name = "GoogleOAuth";

        public const string Realm = "https://accounts.google.com/o/oauth2/auth";

        public GoogleOAuth2Provider(IResourceManager appSettings)
            : base(appSettings, Realm, Name)
        {
            this.AuthorizeUrl = this.AuthorizeUrl ?? Realm;
            this.AccessTokenUrl = this.AccessTokenUrl ?? "https://accounts.google.com/o/oauth2/token";
            this.UserProfileUrl = this.UserProfileUrl ?? "https://www.googleapis.com/oauth2/v1/userinfo";

            if (this.Scopes.Length == 0)
            {
                this.Scopes = new[] {
                    "https://www.googleapis.com/auth/userinfo.profile",
                    "https://www.googleapis.com/auth/userinfo.email"
                };
            }
        }

        public override object Authenticate(ServiceStack.ServiceInterface.IServiceBase authService, ServiceStack.ServiceInterface.Auth.IAuthSession session, Auth request)
        {
            var tokens = this.Init(authService, ref session, request);

            var accessToken = request.oauth_token;
            if (accessToken != null)
            {
                try
                {
                    tokens.AccessToken = accessToken;
                    session.IsAuthenticated = true;
                    session.SetEnvironment(request.Environment);
                    var authInfo = this.CreateAuthInfo(accessToken);
                    this.OnAuthenticated(authService, session, tokens, authInfo);

                    return new AuthResponse
                    {
                        UserName = session.UserName,
                        SessionId = session.Id,
                        ReferrerUrl = session.ReferrerUrl
                    };
                }
                catch (WebException ex)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw;
                }
            }

            return authService.Redirect(session.ReferrerUrl.AddHashParam("f", "RequestTokenFailed"));
        }

        protected override Dictionary<string, string> CreateAuthInfo(string accessToken)
        {
            var url = this.UserProfileUrl.AddQueryParam("access_token", accessToken);
            string json = url.GetJsonFromUrl();
            var obj = JsonObject.Parse(json);
            var authInfo = new Dictionary<string, string>
            {
                { "user_id", obj["id"] }, 
                { "username", obj["email"] }, 
                { "email", obj["email"] }, 
                { "name", obj["name"] }, 
                { "first_name", obj["given_name"] }, 
                { "last_name", obj["family_name"] },
                { "gender", obj["gender"] },
                { "birthday", obj["birthday"] },
                { "link", obj["link"] },
                { "picture", obj["picture"] },
                { "locale", obj["locale"] },
            };
            return authInfo;
        }
    }
}
