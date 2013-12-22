using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using ServiceStack.Common;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Auth;
using ServiceStack.Text;
using System.Linq;
using Classy.Repository;
using Classy.Models;
using System.Net;

namespace Classy.Auth
{
    /// <summary>
    /// Create your own strong-typed Custom AuthUserSession where you can add additional AuthUserSession 
    /// fields required for your application. The base class is automatically populated with 
    /// User Data as and when they authenticate with your application. 
    /// </summary>
    public class CustomUserSession : AuthUserSession
    {
        public string AppId { get; set; }

        public override bool IsAuthorized(string provider)
        {
            var tokens = ProviderOAuthAccess.FirstOrDefault(x => x.Provider == provider);
            return AuthService.GetAuthProvider(provider).IsAuthorizedSafe(this, tokens);
        }

        public override void OnAuthenticated(IServiceBase authService, IAuthSession session, IOAuthTokens tokens, Dictionary<string, string> authInfo)
        {
            base.OnAuthenticated(authService, session, tokens, authInfo);

            var isNew = false;
            // TODO: does this get called on eveyr call? if so find a different way.
            var repo = authService.TryResolve<IProfileRepository>();
            var storage = authService.TryResolve<IStorageRepository>();
            var profile = repo.GetById(AppId, session.UserAuthId, false);
            if (profile == null)
            {
                isNew = true;
                profile = (session as CustomUserSession).TranslateTo<Profile>();
                profile.ContactInfo.Name = session.DisplayName;
                profile.ContactInfo.FirstName = session.FirstName;
                profile.ContactInfo.LastName = session.LastName;
                profile.ContactInfo.Email = session.Email;
            }

            foreach (var authToken in session.ProviderOAuthAccess)
            {
                if (authToken.Provider == FacebookAuthProvider.Name)
                {
                    profile.FacebookName = authToken.DisplayName;
                    profile.FacebookFirstName = authToken.FirstName;
                    profile.FacebookLastName = authToken.LastName;
                    profile.FacebookEmail = authToken.Email;
                    if (isNew)
                    {
                        profile.ImageUrl = SaveFileFromUrl(storage, session.UserAuthId,  
                            string.Format("http://graph.facebook.com/{0}/picture?type=large", authToken.UserName));
                    }
                }
                else if (authToken.Provider == TwitterAuthProvider.Name)
                {
                    profile.TwitterName = authToken.DisplayName;
                }
            }

            // still no profile pic?
            if (isNew && string.IsNullOrEmpty(profile.ImageUrl))
            {
                try
                {
                    profile.ImageUrl = SaveFileFromUrl(storage, session.UserAuthId, "http://www.gravatar.com/avatar/?f=y&d=mm");
                }
                catch(WebException)
                {
                    // sink
                }
            }

            //
            repo.Save(profile);
        }

        public string SaveFileFromUrl(IStorageRepository storage, string profileId, string url)
        {
            string key = string.Concat("profile_img_", profileId);
            storage.SaveFileFromUrl(key, url, "image/jpeg");
            return storage.KeyToUrl(key);
        }
    }

    public static class IAuthSessionExtensions {
        public static string GetAppId (this IAuthSession session)
        {
            var custom = session as CustomUserSession;
            if (custom == null) return null;
            else return custom.AppId;
        }

        public static void SetAppId(this IAuthSession session, string appId)
        {
            var custom = session as CustomUserSession;
            if (custom != null) custom.AppId = appId;
        }
    }
}
