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
            var repo = authService.TryResolve<IProfileRepository>();
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

            // Resolve the user repository from the IOC and persist the user info
            // Populate all matching fields from this session to your own custom User table
            // TODO: does this get called on eveyr call? if so find a different way.
            profile.GravatarImageUrl64 = !session.Email.IsNullOrEmpty()
                ? CreateGravatarUrl(session.Email, 64)
                : null;

            foreach (var authToken in session.ProviderOAuthAccess)
            {
                if (authToken.Provider == FacebookAuthProvider.Name)
                {
                    profile.FacebookName = authToken.DisplayName;
                    profile.FacebookFirstName = authToken.FirstName;
                    profile.FacebookLastName = authToken.LastName;
                    profile.FacebookEmail = authToken.Email;
                }
                else if (authToken.Provider == TwitterAuthProvider.Name)
                {
                    profile.TwitterName = authToken.DisplayName;
                }
            }
            //
            repo.Save(profile);
        }

        private static string CreateGravatarUrl(string email, int size = 64)
        {
            var md5 = MD5.Create();
            var md5HadhBytes = md5.ComputeHash(email.ToUtf8Bytes());

            var sb = new StringBuilder();
            for (var i = 0; i < md5HadhBytes.Length; i++)
                sb.Append(md5HadhBytes[i].ToString("x2"));

            string gravatarUrl = "http://www.gravatar.com/avatar/{0}?d=mm&s={1}".Fmt(sb, size);
            return gravatarUrl;
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
