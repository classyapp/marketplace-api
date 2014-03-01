using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Classy.Auth;
using Classy.Models;

namespace classy
{
    public static class UserSessionExtensions
    {
        public static ContactInfo GetDefaultContactInfo(this CustomUserSession session)
        {
            return new ContactInfo { FirstName = session.FirstName, LastName = session.LastName, Email = session.Email, Location = session.Environment.GetDefaultLocation() };
        }
    }
}