using Classy.Auth;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceStack.ServiceClient.Web;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface.Testing;
using ServiceStack.WebHost.Endpoints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classy.Tests
{
    //[TestClass]
    public class CridentialsProviderUnitTests
    {
      
        [TestInitialize]
        public void TestStart()
        {
        }

        [TestMethod]
        public void TestLogin()
        {
            try
            {
                var mockCtx = new Mock<IRequestContext>();
                mockCtx.SetupGet(f => f.AbsoluteUri).Returns("localhost:1337/auth");


                AuthService service = new AuthService() { RequestContext = mockCtx.Object};
                AuthService.AuthProviders = new IAuthProvider[] { new MockCridentialsAuthProvider() };
                service.Post(new Auth.Auth { UserName = "papai", Password = "mamai" });
            }
            catch (WebServiceException ex)
            {
            }
        }

        [TestCleanup]
        public void TestEnd()
        {
        }


    }
}
