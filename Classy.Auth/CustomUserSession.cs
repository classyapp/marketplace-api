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
using System;
using Classy.Interfaces.Managers;
using classy.Manager;

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
            var profile = repo.GetById(Environment.AppId, session.UserAuthId, false, Environment.CultureCode);
            if (profile == null)
            {
                isNew = true;
                profile = (session as CustomUserSession).TranslateTo<Profile>();
                profile.AppId = Environment.AppId;
                profile.ContactInfo.FirstName = session.FirstName;
                profile.ContactInfo.LastName = session.LastName;
                profile.ContactInfo.Email = session.Email;
                profile.DefaultCulture = Environment.CultureCode;
                profile.IsEmailVerified = false;
                profile.Metadata = new Dictionary<string, string>();
                profile.Metadata.Add(Profile.EmailHashMetadata, ComputeHash(profile.ContactInfo.Email).SafeSubstring(16)); 
            }

            foreach (var authToken in session.ProviderOAuthAccess)
            {
                if (authToken.Provider == FacebookAuthProvider.Name)
                {
                    profile.FacebookUserId = authToken.UserId;
                    profile.FacebookUserName = authToken.UserName;
                    if (isNew)
                    {
                        profile.ContactInfo.FirstName = authToken.FirstName;
                        profile.ContactInfo.LastName = authToken.LastName;
                        profile.ContactInfo.Email = authToken.Email;
                        profile.UserName = authToken.UserName;
                        profile.Avatar = CreateAvatar(storage, string.Format("http://graph.facebook.com/{0}/picture?type=large", authToken.UserName), session.UserAuthId);
                    }
                }
                else if (authToken.Provider == GoogleOAuth2Provider.Name)
                {
                    profile.GoogleUserName = authToken.UserName;
                    if (isNew)
                    {
                        profile.ContactInfo.FirstName = authToken.FirstName;
                        profile.ContactInfo.LastName = authToken.LastName;
                        profile.ContactInfo.Email = authToken.Email;
                        profile.UserName = authToken.UserName;
                        //profile.Avatar = CreateAvatar(storage, string.Format("https://plus.google.com/s2/photos/profile/{0}?sz=220", authToken.UserId), session.UserAuthId);
                    }
                }
                else if (authToken.Provider == TwitterAuthProvider.Name)
                {

                }
            }

            // still no profile pic?
            if (profile.Avatar == null)
            {
                try
                {
                    profile.Avatar = CreateAvatar(storage, "http://www.gravatar.com/avatar/?f=y&d=mm&s=261", session.UserAuthId); 
                }
                catch(WebException)
                {
                    // sink
                }
            }

            //
            profile.Permissions = session.Permissions;
            repo.Save(profile);
        }

        private string ComputeHash(string input)
        {
            byte[] data = Encoding.UTF8.GetBytes(input);
            byte[] result;
            SHA512 shaM = new SHA512Managed();
            result = shaM.ComputeHash(data);

            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data  
            // and format each one as a hexadecimal string. 
            for (int i = 0; i < result.Length; i++)
            {
                sBuilder.Append(result[i].ToString("x2"));
            }

            // Return the hexadecimal string. 
            return sBuilder.ToString();
        }

        public string SaveFileFromUrl(IStorageRepository storage, string key, string url)
        {
            storage.SaveFileFromUrl(key, url, "image/jpeg");
            return storage.KeyToUrl(key);
        }

        private MediaFile CreateAvatar(IStorageRepository storage, string url, string userAuthId)
        {
            var avatarKey = string.Concat("profile_img_", userAuthId, "_", Guid.NewGuid().ToString());
            var avatarUrl = SaveFileFromUrl(storage, avatarKey, url);

            var avatar = new MediaFile
            {
                Type = MediaFileType.Image,
                ContentType = "image/jpeg",
                Url = avatarUrl,
                Key = avatarKey
            };
            return avatar;
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
