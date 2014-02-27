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
        public Classy.Models.Env Environment { get; set; }

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
            var profile = repo.GetById(Environment.AppId, session.UserAuthId, false);
            if (profile == null)
            {
                isNew = true;
                profile = (session as CustomUserSession).TranslateTo<Profile>();
                profile.AppId = Environment.AppId;
                profile.ContactInfo.FirstName = session.FirstName;
                profile.ContactInfo.LastName = session.LastName;
                profile.ContactInfo.Email = session.Email;
                profile.ContactInfo.Location = Environment.GetDefaultLocation();
            }

            foreach (var authToken in session.ProviderOAuthAccess)
            {
                if (authToken.Provider == FacebookAuthProvider.Name)
                {
                    profile.ContactInfo.FirstName = authToken.FirstName;
                    profile.ContactInfo.LastName = authToken.LastName;
                    profile.ContactInfo.Email = authToken.Email;
                    if (isNew)
                    {
                        profile.UserName = authToken.UserName;
                        profile.ImageUrl = SaveFileFromUrl(storage, string.Concat("profile_img_", session.UserAuthId),  
                            string.Format("http://graph.facebook.com/{0}/picture?type=large", authToken.UserName));
                        profile.ThumbnailUrl = SaveFileFromUrl(storage, string.Concat("profile_thumb_", session.UserAuthId),
                            string.Format("http://graph.facebook.com/{0}/picture?type=square", authToken.UserName));
                    }
                }
                else if (authToken.Provider == GoogleOAuth2Provider.Name)
                {
                    profile.ContactInfo.FirstName = authToken.FirstName;
                    profile.ContactInfo.LastName = authToken.LastName;
                    profile.ContactInfo.Email = authToken.Email;
                    if (isNew)
                    {
                        profile.UserName = authToken.UserName;
                        profile.ImageUrl = SaveFileFromUrl(storage, string.Concat("profile_img_", session.UserAuthId),
                            string.Format("https://plus.google.com/s2/photos/profile/{0}?sz=220", authToken.UserId));
                        profile.ThumbnailUrl = SaveFileFromUrl(storage, string.Concat("profile_thumb_", session.UserAuthId),
                            string.Format("https://plus.google.com/s2/photos/profile/{0}?sz=100", authToken.UserId));
                    }
                }
                else if (authToken.Provider == TwitterAuthProvider.Name)
                {

                }
            }

            // still no profile pic?
            if (isNew && string.IsNullOrEmpty(profile.ImageUrl))
            {
                try
                {
                    profile.ImageUrl = SaveFileFromUrl(storage, string.Concat("profile_img_", session.UserAuthId), "http://www.gravatar.com/avatar/?f=y&d=mm&s=261");
                    profile.ThumbnailUrl = SaveFileFromUrl(storage, string.Concat("profile_thumb_", session.UserAuthId), "http://www.gravatar.com/avatar/?f=y&d=mm&s=50");
                }
                catch(WebException)
                {
                    // sink
                }
            }

            //
            repo.Save(profile);
        }

        public string SaveFileFromUrl(IStorageRepository storage, string key, string url)
        {
            storage.SaveFileFromUrl(key, url, "image/jpeg");
            return storage.KeyToUrl(key);
        }
    }

    public static class IAuthSessionExtensions {
        public static Classy.Models.Env GetEnvironment (this IAuthSession session)
        {
            var custom = session as CustomUserSession;
            if (custom == null) return null;
            else return custom.Environment;
        }

        public static void SetEnvironment(this IAuthSession session, Classy.Models.Env env)
        {
            var custom = session as CustomUserSession;
            if (custom != null) custom.Environment = env;
        }
    }
}
