using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Classy.Auth;
using Classy.Models;
using classy.Manager;

namespace classy
{
    public static class UserSessionExtensions
    {
        public static ContactInfo GetDefaultContactInfo(this CustomUserSession session)
        {
            return new ContactInfo { FirstName = session.FirstName, LastName = session.LastName, Email = session.Email, Location = session.Environment.GetDefaultLocation() };
        }

        public static ManagerSecurityContext ToSecurityContext(this CustomUserSession session)
        {
            var securityContext = new ManagerSecurityContext();
            securityContext.IsAdmin = (session != null && session.Permissions != null) ? session.Permissions.Contains("admin") : false;
            securityContext.IsAuthenticated = session.IsAuthenticated;
            securityContext.AuthenticatedProfileId = session.UserAuthId;
            return securityContext;
        }
    }
}