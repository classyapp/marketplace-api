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
    //[TestClass]
    public class AuthServiceUnitTestWithHost
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
        public void TestLoginWrongUserPassword()
        {
            try
            {
                client.Headers.Add("X-Classy-Env", new { AppId = "Test", CultureCode = "he", CountryCode = "IL", CurrencyCode = "ILS" }.ToJson());
                
                var response = client.Post<AuthResponse>("/auth",
                    new Auth.Auth { UserName = "juvaly1", Password = "333444" });
                // Assert.AreEqual(response.UserName, "test1");
            }
            catch (WebServiceException ex)
            {
                Assert.AreEqual(ex.ErrorMessage, "Invalid UserName or Password");
            }
        }


        [TestMethod]
        public void TestLoginWrongHeader()
        {
            try
            {
                client.Headers.Add("X-Class-Env", new { AppId = "Test", CultureCode = "he", CountryCode = "IL", CurrencyCode = "ILS" }.ToJson());

                var response = client.Post<AuthResponse>("/auth",
                    new Auth.Auth { UserName = "juvaly1", Password = "333444" });
            }
            catch (WebServiceException ex)
            {
                Assert.AreEqual(ex.ErrorMessage, "Invalid API Key");
            }
        }


        [TestMethod]
        public void TestLoginWrongHeaderData()
        {
            try
            {
                client.Headers.Add("X-Classy-Env", new { AppId = "Testttt", CultureCode = "Dummy", CountryCode = "Dummy", CurrencyCode = "I" }.ToJson());

                var response = client.Post<AuthResponse>("/auth",
                    new Auth.Auth { UserName = "juvaly1", Password = "333444" });
            }
            catch (WebServiceException ex)
            {
                Assert.AreEqual(ex.ErrorMessage, "Invalid API Key");
            }
        }

        [TestMethod]
        public void TestCorrectLogin()
        {
            try
            {
                client.Headers.Add("X-Classy-Env", new { AppId = "Test", CultureCode = "he", CountryCode = "IL", CurrencyCode = "ILS" }.ToJson());

                var response = client.Post<AuthResponse>("/auth",
                    new Auth.Auth { UserName = "Test1", Password = "Test1" });
                Assert.AreEqual(response.UserName, "Test1");
            }
            catch (WebServiceException ex)
            {
                Assert.Fail(ex.ErrorMessage);
            }
        }


        [TestCleanup]
        public void TestEnd()
        {
            appHost.Dispose();
        }


    }
}
