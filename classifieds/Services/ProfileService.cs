using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Classy.Auth;
using classy.Manager;
using Classy.Models;
using Classy.Models.Request;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;

namespace classy.Services
{
    public class ProfileService : Service
    {
        public IProfileManager ProfileManager { get; set; }
        public IAppManager AppManager { get; set; }
        public IUserAuthRepository UserAuthRepository { get; set; }

        [CustomAuthenticate]
        public object Post(CreateProfileProxy request)
        {
            var session = SessionAs<CustomUserSession>();
            var profile = ProfileManager.CreateProfileProxy(
                request.Environment.AppId,
                session.UserAuthId,
                request.BatchId,
                request.ProfessionalInfo,
                request.Metadata);

            return new HttpResult(profile, HttpStatusCode.OK);
        }

        // get profile form session
        [CustomAuthenticate]
        public object Get(GetAutenticatedProfile request)
        {
            var session = SessionAs<CustomUserSession>();
            if (!session.IsAuthenticated) return new HttpError(HttpStatusCode.Unauthorized, "No Session");
            else
            {
                var profile = ProfileManager.GetProfileById(
                    request.Environment.AppId,
                    session.UserAuthId,
                    session.UserAuthId,
                    true,
                    true,
                    false,
                    false,
                    false,
                    true,
                    false,
                    request.Environment.CultureCode);
                return new HttpResult(profile);
            }
        }

        // get profile
        public object Get(GetProfileById request)
        {
            try
            {
                var session = SessionAs<CustomUserSession>();

                var profile = ProfileManager.GetProfileById(
                    request.Environment.AppId,
                    request.ProfileId,
                    session.UserAuthId,
                    request.IncludeFollowedByProfiles,
                    request.IncludeFollowingProfiles,
                    request.IncludeReviews,
                    request.IncludeListings,
                    request.IncludeCollections,
                    request.IncludeFavorites,
                    request.LogImpression,
                    request.Environment.CultureCode);

                return new HttpResult(profile, HttpStatusCode.OK);
            }
            catch (KeyNotFoundException kex)
            {
                return new HttpError(HttpStatusCode.NotFound, kex.Message);
            }
        }

        [CustomAuthenticate]
        public object Put(UpdateProfile request)
        {
            try
            {
                var session = SessionAs<CustomUserSession>();

                if (session.UserAuthId != request.ProfileId &&
                    !session.Permissions.Contains("admin")) throw new UnauthorizedAccessException("not yours to update");

                if (request.Fields.HasFlag(ProfileUpdateFields.SetPassword))
                {
                    IUserAuthRepository authRepository = GetAppHost().TryResolve<IUserAuthRepository>();
                    UserAuth userAuth = authRepository.GetUserAuth(request.Environment.AppId, request.ProfileId);
                    authRepository.UpdateUserAuth(userAuth, userAuth, request.Password);
                }

                byte[] imageData = null;
                string imageContentType = null;
                if (request.Fields.HasFlag(ProfileUpdateFields.ProfileImage) && Request.Files.Length == 1)
                {
                    var reader = new BinaryReader(Request.Files[0].InputStream);
                    imageData = new byte[Request.Files[0].InputStream.Length];
                    reader.Read(imageData, 0, imageData.Length);
                    imageContentType = Request.Files[0].ContentType;
                }

                var profile = ProfileManager.UpdateProfile(
                    request.Environment.AppId,
                    request.ProfileId,
                    request.ContactInfo ?? session.GetDefaultContactInfo(AppManager.GetAppById(request.Environment.AppId).DefaultCountry),
                    request.ProfessionalInfo,
                    request.Metadata,
                    request.Fields,
                    imageData,
                    imageContentType,
                    string.IsNullOrEmpty(request.DefaultCulture) ? request.Environment.CultureCode : request.DefaultCulture,
                    request.CoverPhotos);

                // update email on user auth if needed
                if (((profile.IsProfessional && request.Fields.HasFlag(ProfileUpdateFields.ProfessionalInfo) && (session.Email != profile.ProfessionalInfo.CompanyContactInfo.Email)) ||
                    (!profile.IsProfessional && request.Fields.HasFlag(ProfileUpdateFields.ContactInfo) && (session.Email != profile.ContactInfo.Email))))
                {
                    var email = profile.IsProfessional
                        ? profile.ProfessionalInfo.CompanyContactInfo.Email
                        : profile.ContactInfo.Email;
                    UserAuthRepository.UpdateUserEmail(request.Environment.AppId, profile.Id, email);
                }

                return new HttpResult(profile, HttpStatusCode.OK);
            }
            catch (KeyNotFoundException kex)
            {
                return new HttpError(HttpStatusCode.NotFound, kex.Message);
            }
        }

        [CustomAuthenticate]
        public object Post(SetProfileTranslation request)
        {
            try
            {
                var session = SessionAs<CustomUserSession>();
                ProfileManager.SecurityContext = session.ToSecurityContext();
                ProfileManager.SetTranslation(
                    request.Environment.AppId,
                    request.ProfileId,
                    new ProfileTranslation
                    {
                        Culture = request.CultureCode,
                        CompanyName = request.CompanyName,
                        Metadata = request.Metadata
                    });

                return new HttpResult(new { ObjectId = request.ProfileId, Culture = request.CultureCode }, HttpStatusCode.OK);
            }
            catch (KeyNotFoundException kex)
            {
                return new HttpError(HttpStatusCode.NotFound, kex.Message);
            }
        }

        [CustomAuthenticate]
        public object Delete(DeleteProfileTranslation request)
        {
            try
            {
                var session = SessionAs<CustomUserSession>();
                ProfileManager.SecurityContext = session.ToSecurityContext();
                ProfileManager.DeleteTranslation(
                    request.Environment.AppId,
                    request.ProfileId,
                    request.CultureCode);

                return new HttpResult(new { ObjectId = request.ProfileId, Culture = request.CultureCode }, HttpStatusCode.OK);
            }
            catch (KeyNotFoundException kex)
            {
                return new HttpError(HttpStatusCode.NotFound, kex.Message);
            }
        }

        [CustomAuthenticate]
        public object Get(GetProfileTranslation request)
        {
            return new HttpResult(ProfileManager.GetTranslation(request.Environment.AppId, request.ProfileId, request.CultureCode), HttpStatusCode.OK);
        }

        // follow a profile
        [CustomAuthenticate]
        public object Post(FollowProfile request)
        {
            try
            {
                var session = SessionAs<CustomUserSession>();

                ProfileManager.FollowProfile(
                    request.Environment.AppId,
                    session.UserAuthId,
                    request.FolloweeProfileId);

                return new HttpResult(HttpStatusCode.OK);
            }
            catch (KeyNotFoundException kex)
            {
                return new HttpError(HttpStatusCode.NotFound, kex.Message);
            }
        }

        // unfollow a profile
        [CustomAuthenticate]
        public object Delete(FollowProfile request)
        {
            try
            {
                var session = SessionAs<CustomUserSession>();

                ProfileManager.UnfollowProfile(
                    request.Environment.AppId,
                    session.UserAuthId,
                    request.FolloweeProfileId);

                return new HttpResult(HttpStatusCode.OK);
            }
            catch (KeyNotFoundException kex)
            {
                return new HttpError(HttpStatusCode.NotFound, kex.Message);
            }
        }

        // submit proxy claim
        [CustomAuthenticate]
        public object Post(ClaimProxyProfile request)
        {
            try
            {
                var session = SessionAs<CustomUserSession>();

                var claim = ProfileManager.SubmitProxyClaim(
                    request.Environment.AppId,
                    session.UserAuthId,
                    request.ProxyProfileId,
                    request.ProfessionalInfo,
                    request.Metadata,
                    request.DefaultCulture);

                return new HttpResult(claim, HttpStatusCode.OK);
            }
            catch (KeyNotFoundException kex)
            {
                return new HttpError(HttpStatusCode.NotFound, kex.Message);
            }
        }

        // approve proxy claim
        [CustomAuthenticate]
        public object Post(ApproveProxyClaim request)
        {
            try
            {
                var claim = ProfileManager.ApproveProxyClaim(
                    request.Environment.AppId,
                    request.ClaimId);

                return new HttpResult(claim, HttpStatusCode.OK);
            }
            catch (KeyNotFoundException kex)
            {
                return new HttpError(HttpStatusCode.NotFound, kex.Message);
            }
        }

        // reject proxy claim
        [CustomAuthenticate]
        public object Post(RejectProxyClaim request)
        {
            try
            {
                var claim = ProfileManager.RejectProxyClaim(
                    request.Environment.AppId,
                    request.ClaimId);

                return new HttpResult(claim, HttpStatusCode.OK);
            }
            catch (KeyNotFoundException kex)
            {
                return new HttpError(HttpStatusCode.NotFound, kex.Message);
            }
        }
    }
}