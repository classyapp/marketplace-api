using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Classy.Auth;
using Classy.Interfaces.Managers;
using classy.Manager;
using Classy.Models.Request;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;
 
namespace classy.Services
{
    public class PasswordService : Service
    {
        public ILocalizationManager LocalizationManager { get; set; }
        public IEmailManager EmailManager { get; set; }
        public IAppManager AppManager { get; set; }

        public object Post(ForgotPasswordRequest request)
        {
            var authRepo = ResolveService<IUserAuthRepository>();
            var userAuth = authRepo.GetUserAuthByUserName(request.Environment.AppId, request.Email);

            if (userAuth != null)
            {
                // create hash
                if (userAuth.Meta == null)
                {
                    userAuth.Meta = new Dictionary<string, string>();
                }

                var md5 = MD5.Create();
                byte[] inputBytes = Encoding.ASCII.GetBytes(Guid.NewGuid().ToString());
                byte[] hash = md5.ComputeHash(inputBytes);
                var sb = new StringBuilder();
                for (int i = 0; i < hash.Length; i++)
                {
                    sb.Append(hash[i].ToString("X2"));
                }
                userAuth.Meta["ResetPasswordHash"] = sb.ToString();
                authRepo.SaveUserAuth(userAuth);

                // Send Email
                string subject = "ForgotPassword_ResetEmailSubject";
                string body = "ForgotPassword_ResetEmailBody";
                var subjectRes = LocalizationManager.GetResourceByKey(request.Environment.AppId, "ForgotPassword_ResetEmailSubject", true);
                var bodyRes = LocalizationManager.GetResourceByKey(request.Environment.AppId, "ForgotPassword_ResetEmailBody", true);
                EmailManager.SendHtmlMessage(
                    AppManager.GetAppById(request.Environment.AppId).MandrilAPIKey,
                    null, new string[] { userAuth.Email },
                    subjectRes == null ? subject : subjectRes.Values[request.Environment.CultureCode],
                    bodyRes == null ? body : string.Format(bodyRes.Values[request.Environment.CultureCode], string.Format("http://{0}/reset/{1}", AppManager.GetAppById(request.Environment.AppId).Hostname, sb.ToString())),
                    "reset_password_template",
                    null
                    );
                return new HttpResult(new { }, HttpStatusCode.OK);

            }
            return new HttpError("Email not found");
        }

        public object Get(VerifyPasswordResetRequest request)
        {
            IUserAuthRepository authRepo = ResolveService<IUserAuthRepository>();
            UserAuth userAuth = authRepo.GetUserAuthByResetHash(request.Environment.AppId, request.Hash);

            if (userAuth != null)
            {
                return new HttpResult(new { }, HttpStatusCode.OK);
            }
            return new HttpError("Invalid hash");
        }

        public object Post(PasswordResetRequest request)
        {
            IUserAuthRepository authRepo = ResolveService<IUserAuthRepository>();
            UserAuth userAuth = authRepo.GetUserAuthByResetHash(request.Environment.AppId, request.Hash);

            if (userAuth != null)
            {
                authRepo.ResetUserPassword(userAuth, request.Password);

                return new HttpResult(new { }, HttpStatusCode.OK);
            }
            return new HttpError("Invalid hash");
        }
    }
}