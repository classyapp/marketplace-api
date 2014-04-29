using Classy.Auth;
using ServiceStack.Common.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classy.Tests
{
    class MockCridentialsAuthProvider : AuthProvider
    {
        public override bool IsAuthorized(ServiceStack.ServiceInterface.Auth.IAuthSession session, 
            ServiceStack.ServiceInterface.Auth.IOAuthTokens tokens, Auth.Auth request = null)
        {
            return false;

        }

        public override object Authenticate(ServiceStack.ServiceInterface.IServiceBase authService, 
            ServiceStack.ServiceInterface.Auth.IAuthSession session, Auth.Auth request)
        {
            if (request.Environment.AppId != "Test")
                throw new InvalidOperationException("Invalid AppID!");
            
            if (String.IsNullOrEmpty(request.UserName))
                throw new InvalidOperationException("Missing username!");

            if (String.IsNullOrEmpty(request.Password))
                throw new InvalidOperationException("Missing Password!");

            if (request.UserName == "Test1" && request.Password=="Test1")
                return new AuthResponse
                {
                    UserName = "Test1",
                    SessionId = "UnitTest",
                    ReferrerUrl = request.Continue
                };
            throw HttpError.Unauthorized("Invalid UserName or Password");
        }
    }
}
