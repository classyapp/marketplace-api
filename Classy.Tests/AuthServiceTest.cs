using Classy.Auth;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceStack.ServiceClient.Web;
using ServiceStack.ServiceInterface.Testing;
using ServiceStack.WebHost.Endpoints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classy.Tests
{
    [TestClass]
    public class AuthServiceTest
    {
        private AppHostHttpListenerBase appHost;
        private JsonServiceClient client;

        [TestInitialize]
        public void TestStart()
        {
            appHost = new TestAppHost();
            appHost.Init();
            appHost.Start("http://localhost:8080/api/");

            client = new JsonServiceClient("http://localhost:8080/api");
        }

        [TestMethod]
        public void TestLogin()
        {
            try
            {
                client.Headers.Add("X-Classy-Env", new { AppId = "v1.0", CultureCode = "he", CountryCode = "IL", CurrencyCode = "ILS" }.ToJson());
                var response = client.Post<AuthResponse>("/auth",
                    new Auth.Auth { UserName = "juvaly1", Password = "333444" });
                Assert.AreEqual(response.UserName, "test1");
            }
            catch (WebServiceException ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [TestCleanup]
        public void TestEnd()
        {
            appHost.Dispose();
        }


    }
}
