using Moq;
using NUnit.Framework;
using ServiceStack.ServiceClient.Web;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface.Auth;

namespace Classy.Tests
{
    //[TestFixture]
    public class CridentialsProviderUnitTests
    {
      
        [SetUp]
        public void TestStart()
        {
        }

        [Test]
        public void TestLogin()
        {
            try
            {
                var mockCtx = new Mock<IRequestContext>();
                mockCtx.SetupGet(f => f.AbsoluteUri).Returns("localhost:1337/auth");


                //AuthService service = new AuthService() { RequestContext = mockCtx.Object};
                //AuthService.AuthProviders = new IAuthProvider[] { new MockCridentialsAuthProvider() };
                //service.Post(new Auth.Auth { UserName = "papai", Password = "mamai" });
            }
            catch (WebServiceException ex)
            {
            }
        }
    }
}
