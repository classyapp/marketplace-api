﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OAuth2;
using ServiceStack.Common.Web;
using ServiceStack.Configuration;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Auth;
using ServiceStack.WebHost.Endpoints;

namespace Classy.Auth
{
    public abstract class OAuth2Provider : AuthProvider
    {
        protected OAuth2Provider(IResourceManager appSettings, string realm, string provider)
            : base(appSettings, realm, provider)
        {
            this.ConsumerKey = appSettings.GetString(string.Format("oauth.{0}.ConsumerKey", provider))
                ?? FallbackConfig(appSettings.GetString("oauth.ConsumerKey"));
            this.ConsumerSecret = appSettings.GetString(string.Format("oauth.{0}.ConsumerSecret", provider))
                ?? FallbackConfig(appSettings.GetString("oauth.ConsumerSecret"));
            var scopes = appSettings.GetString(string.Format("oauth.{0}.Scopes", provider))
                ?? FallbackConfig(appSettings.GetString("oauth.Scopes")) ?? "";
            this.Scopes = scopes.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);

            this.RequestTokenUrl = appSettings.GetString(string.Format("oauth.{0}.RequestTokenUrl", provider))
                ?? FallbackConfig(appSettings.GetString("oauth.RequestTokenUrl"));
            this.AuthorizeUrl = appSettings.GetString(string.Format("oauth.{0}.AuthorizeUrl", provider))
                ?? FallbackConfig(appSettings.GetString("oauth.AuthorizeUrl"));
            this.AccessTokenUrl = appSettings.GetString(string.Format("oauth.{0}.AccessTokenUrl", provider))
                ?? FallbackConfig(appSettings.GetString("oauth.AccessTokenUrl"));
            this.UserProfileUrl = appSettings.GetString(string.Format("oauth.{0}.UserProfileUrl", provider))
                ?? FallbackConfig(appSettings.GetString("oauth.UserProfileUrl"));
        }

        public string AccessTokenUrl { get; set; }

        public IAuthHttpGateway AuthHttpGateway { get; set; }

        public string AuthorizeUrl { get; set; }

        public string ConsumerKey { get; set; }

        public string ConsumerSecret { get; set; }

        public string RequestTokenUrl { get; set; }

        public string UserProfileUrl { get; set; }

        protected string[] Scopes { get; set; }

        public override object Authenticate(IServiceBase authService, IAuthSession session, Auth request)
        {
            var tokens = this.Init(authService, ref session, request);

            var authServer = new AuthorizationServerDescription { AuthorizationEndpoint = new Uri(this.AuthorizeUrl), TokenEndpoint = new Uri(this.AccessTokenUrl) };
            var authClient = new WebServerClient(authServer, this.ConsumerKey)
            {
                ClientCredentialApplicator = ClientCredentialApplicator.PostParameter(this.ConsumerSecret),
            };

            var authState = authClient.ProcessUserAuthorization();
            if (authState == null)
            {
                try
                {
                    var authReq = authClient.PrepareRequestUserAuthorization(this.Scopes, new Uri(this.CallbackUrl));
                    var authContentType = authReq.Headers[HttpHeaders.ContentType];
                    var httpResult = new HttpResult(authReq.ResponseStream, authContentType) { StatusCode = authReq.Status, StatusDescription = "Moved Temporarily" };
                    foreach (string header in authReq.Headers)
                    {
                        httpResult.Headers[header] = authReq.Headers[header];
                    }

                    foreach (string name in authReq.Cookies)
                    {
                        var cookie = authReq.Cookies[name];

                        if (cookie != null)
                        {
                            httpResult.SetSessionCookie(name, cookie.Value, cookie.Path);
                        }
                    }

                    authService.SaveSession(session, this.SessionExpiry);
                    return httpResult;
                }
                catch (ProtocolException ex)
                {
                    Log.Error(string.Format("Failed to login to {0}", this.Provider), ex);
                    return authService.Redirect(session.ReferrerUrl.AddHashParam("f", "Unknown"));
                }
            }

            var accessToken = authState.AccessToken;
            if (accessToken != null)
            {
                try
                {
                    tokens.AccessToken = accessToken;
                    tokens.RefreshToken = authState.RefreshToken;
                    tokens.RefreshTokenExpiry = authState.AccessTokenExpirationUtc;
                    session.IsAuthenticated = true;
                    var authInfo = this.CreateAuthInfo(accessToken);
                    this.OnAuthenticated(authService, session, tokens, authInfo);
                    return authService.Redirect(session.ReferrerUrl.AddHashParam("s", "1"));
                }
                catch (WebException we)
                {
                    var statusCode = ((HttpWebResponse)we.Response).StatusCode;
                    if (statusCode == HttpStatusCode.BadRequest)
                    {
                        return authService.Redirect(session.ReferrerUrl.AddHashParam("f", "AccessTokenFailed"));
                    }
                }
            }

            return authService.Redirect(session.ReferrerUrl.AddHashParam("f", "RequestTokenFailed"));
        }

        public override bool IsAuthorized(IAuthSession session, IOAuthTokens tokens, Auth request = null)
        {
            if (request != null)
            {
                if (!LoginMatchesSession(session, request.UserName))
                {
                    return false;
                }
            }

            return tokens != null && !string.IsNullOrEmpty(tokens.UserId);
        }

        public void LoadUserOAuthProvider(IAuthSession authSession, IOAuthTokens tokens)
        {
            var userSession = authSession as AuthUserSession;
            if (userSession == null)
            {
                return;
            }

            userSession.UserName = tokens.UserName ?? userSession.UserName;
            userSession.DisplayName = tokens.DisplayName ?? userSession.DisplayName;
            userSession.FirstName = tokens.FirstName ?? userSession.FirstName;
            userSession.LastName = tokens.LastName ?? userSession.LastName;
            userSession.PrimaryEmail = tokens.Email ?? userSession.PrimaryEmail ?? userSession.Email;
            userSession.Email = tokens.Email ?? userSession.PrimaryEmail ?? userSession.Email;
        }

        protected abstract Dictionary<string, string> CreateAuthInfo(string accessToken);

        protected override void LoadUserAuthInfo(IAuthSession userSession, IOAuthTokens tokens, Dictionary<string, string> authInfo)
        {
            try
            {
                tokens.UserId = authInfo["user_id"];
                tokens.UserName = authInfo["username"];
                tokens.DisplayName = authInfo["name"];
                tokens.FirstName = authInfo["first_name"];
                tokens.LastName = authInfo["last_name"];
                tokens.Email = authInfo["email"];
                userSession.UserAuthName = tokens.Email;

                this.LoadUserOAuthProvider(userSession, tokens);
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("Could not retrieve Profile info for '{0}'", tokens.DisplayName), ex);
            }
        }

        protected IOAuthTokens Init(IServiceBase authService, ref IAuthSession session, Auth request)
        {
            var requestUri = authService.RequestContext.AbsoluteUri;
            if (string.IsNullOrEmpty(this.CallbackUrl))
            {
                this.CallbackUrl = requestUri;
            }

            if (string.IsNullOrEmpty(session.ReferrerUrl))
            {
                session.ReferrerUrl = (request != null ? request.Continue : null) ?? authService.RequestContext.GetHeader("Referer");
            }

            if (string.IsNullOrEmpty(session.ReferrerUrl) || session.ReferrerUrl.IndexOf("/auth", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                session.ReferrerUrl = this.RedirectUrl
                                      ?? ServiceStackHttpHandlerFactory.GetBaseUrl()
                                      ?? requestUri.Substring(0, requestUri.IndexOf("/", "https://".Length + 1, StringComparison.Ordinal));
            }

            var tokens = session.ProviderOAuthAccess.FirstOrDefault(x => x.Provider == this.Provider);
            if (tokens == null)
            {
                session.ProviderOAuthAccess.Add(tokens = new OAuthTokens { Provider = this.Provider });
            }

            return tokens;
        }
    }
}
