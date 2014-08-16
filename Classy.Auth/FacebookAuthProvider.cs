<<<<<<< HEAD
using ServiceStack.Configuration;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Auth;
using ServiceStack.Text;
using ServiceStack.WebHost.Endpoints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using ServiceStack.Common;
using System.IO;
using System.Diagnostics;

namespace Classy.Auth
{
    public class CustomFacebookAuthProvider : OAuthProvider
    {
        public const string Name = "facebook";
        public static string Realm = "https://graph.facebook.com/";
        public static string PreAuthUrl = "https://www.facebook.com/dialog/oauth";

        public string AppId { get; set; }
        public string AppSecret { get; set; }
        public string[] Permissions { get; set; }

        public CustomFacebookAuthProvider(IResourceManager appSettings)
            : base(appSettings, Realm, Name, "AppId", "AppSecret")
        {
            this.AppId = appSettings.GetString("oauth.facebook.AppId");
            this.AppSecret = appSettings.GetString("oauth.facebook.AppSecret");
        }

        public override object Authenticate(IServiceBase authService, IAuthSession session, Auth request)
        {
            var tokens = Init(authService, ref session, request);

            if (request != null)
                Trace.TraceInformation("FacebookAuthProvider.Authenticate: request.oauth_token -> " + request.oauth_token);

            try
            {
                if (string.IsNullOrEmpty(request.oauth_token))
                    throw new Exception();

                tokens.AccessToken = request.oauth_token;
                session.IsAuthenticated = true;
                session.SetEnvironment(request.Environment);

                var json = AuthHttpGateway.DownloadFacebookUserInfo(request.oauth_token);
                var authInfo = JsonSerializer.DeserializeFromString<Dictionary<string, string>>(json);

                // save the email so that the userauth session gets merged
                session.UserAuthName = authInfo["email"];

                authService.SaveSession(session, SessionExpiry);
                OnAuthenticated(authService, session, tokens, authInfo);

                return new AuthResponse
                {
                    UserName = session.UserName,
                    SessionId = session.Id,
                    ReferrerUrl = session.ReferrerUrl
                };
            }
            catch (WebException ex)
            {
                Trace.TraceError(ex.ToString());
                throw;
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
                throw;
            }
        }

        protected override void LoadUserAuthInfo(IAuthSession userSession, IOAuthTokens tokens, Dictionary<string, string> authInfo)
        {
            if (authInfo.ContainsKey("id"))
                tokens.UserId = authInfo.Get("id");
            if (authInfo.ContainsKey("name"))
                tokens.DisplayName = authInfo.Get("name");
            if (authInfo.ContainsKey("username"))
                tokens.UserName = authInfo.Get("username");
            if (authInfo.ContainsKey("first_name"))
                tokens.FirstName = authInfo.Get("first_name");
            if (authInfo.ContainsKey("last_name"))
                tokens.LastName = authInfo.Get("last_name");
            if (authInfo.ContainsKey("email"))
                tokens.Email = authInfo.Get("email");
            if (authInfo.ContainsKey("gender"))
                tokens.Gender = authInfo.Get("gender");
            if (authInfo.ContainsKey("timezone"))
                tokens.TimeZone = authInfo.Get("timezone");

            LoadUserOAuthProvider(userSession, tokens);
        }

        public override void LoadUserOAuthProvider(IAuthSession authSession, IOAuthTokens tokens)
        {
            var userSession = authSession as CustomUserSession;
            if (userSession == null) return;

            userSession.FacebookUserId = tokens.UserId ?? userSession.FacebookUserId;
            userSession.FacebookUserName = tokens.UserName ?? userSession.FacebookUserName;
            userSession.DisplayName = tokens.DisplayName ?? userSession.DisplayName;
            userSession.FirstName = tokens.FirstName ?? userSession.FirstName;
            userSession.LastName = tokens.LastName ?? userSession.LastName;
            userSession.PrimaryEmail = tokens.Email ?? userSession.PrimaryEmail ?? userSession.Email;
            userSession.Email = tokens.Email ?? userSession.PrimaryEmail ?? userSession.Email;
        }
    }

=======
using ServiceStack.Configuration;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Auth;
using ServiceStack.Text;
using ServiceStack.WebHost.Endpoints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using ServiceStack.Common;
using System.IO;
using System.Diagnostics;

namespace Classy.Auth
{
    public class CustomFacebookAuthProvider : OAuthProvider
    {
        public const string Name = "facebook";
        public static string Realm = "https://graph.facebook.com/";
        public static string PreAuthUrl = "https://www.facebook.com/dialog/oauth";

        public string AppId { get; set; }
        public string AppSecret { get; set; }
        public string[] Permissions { get; set; }

        public CustomFacebookAuthProvider(IResourceManager appSettings)
            : base(appSettings, Realm, Name, "AppId", "AppSecret")
        {
            this.AppId = appSettings.GetString("oauth.facebook.AppId");
            this.AppSecret = appSettings.GetString("oauth.facebook.AppSecret");
        }

        public override object Authenticate(IServiceBase authService, IAuthSession session, Auth request)
        {
            var tokens = Init(authService, ref session, request);

            if (request != null)
                Trace.TraceInformation("FacebookAuthProvider.Authenticate: request.oauth_token -> " + request.oauth_token);

            try
            {
                if (string.IsNullOrEmpty(request.oauth_token))
                    throw new Exception();

                tokens.AccessToken = request.oauth_token;
                session.IsAuthenticated = true;
                session.SetEnvironment(request.Environment);

                var json = AuthHttpGateway.DownloadFacebookUserInfo(request.oauth_token);
                var authInfo = JsonSerializer.DeserializeFromString<Dictionary<string, string>>(json);

                // save the email so that the userauth session gets merged
                session.UserAuthName = authInfo["email"];

                authService.SaveSession(session, SessionExpiry);
                OnAuthenticated(authService, session, tokens, authInfo);

                return new AuthResponse
                {
                    UserName = session.UserName,
                    SessionId = session.Id,
                    ReferrerUrl = session.ReferrerUrl
                };
            }
            catch (WebException ex)
            {
                Trace.TraceError(ex.ToString());
                throw;
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
                throw;
            }
        }

        protected override void LoadUserAuthInfo(IAuthSession userSession, IOAuthTokens tokens, Dictionary<string, string> authInfo)
        {
            if (authInfo.ContainsKey("id"))
                tokens.UserId = authInfo.Get("id");
            if (authInfo.ContainsKey("name"))
                tokens.DisplayName = authInfo.Get("name");
            if (authInfo.ContainsKey("username"))
                tokens.UserName = authInfo.Get("username");
            if (authInfo.ContainsKey("first_name"))
                tokens.FirstName = authInfo.Get("first_name");
            if (authInfo.ContainsKey("last_name"))
                tokens.LastName = authInfo.Get("last_name");
            if (authInfo.ContainsKey("email"))
                tokens.Email = authInfo.Get("email");
            if (authInfo.ContainsKey("gender"))
                tokens.Gender = authInfo.Get("gender");
            if (authInfo.ContainsKey("timezone"))
                tokens.TimeZone = authInfo.Get("timezone");

            LoadUserOAuthProvider(userSession, tokens);
        }

        public override void LoadUserOAuthProvider(IAuthSession authSession, IOAuthTokens tokens)
        {
            var userSession = authSession as CustomUserSession;
            if (userSession == null) return;

            userSession.FacebookUserId = tokens.UserId ?? userSession.FacebookUserId;
            userSession.FacebookUserName = tokens.UserName ?? userSession.FacebookUserName;
            userSession.DisplayName = tokens.DisplayName ?? userSession.DisplayName;
            userSession.FirstName = tokens.FirstName ?? userSession.FirstName;
            userSession.LastName = tokens.LastName ?? userSession.LastName;
            userSession.PrimaryEmail = tokens.Email ?? userSession.PrimaryEmail ?? userSession.Email;
            userSession.Email = tokens.Email ?? userSession.PrimaryEmail ?? userSession.Email;
        }
    }

>>>>>>> origin/master
}