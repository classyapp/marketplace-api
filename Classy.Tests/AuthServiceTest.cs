using NUnit.Framework;
using ServiceStack.ServiceClient.Web;
using ServiceStack.ServiceInterface.Auth;
using ServiceStack.WebHost.Endpoints;

namespace Classy.Tests
{
    [TestFixture]
    public class AuthServiceUnitTest
    {
        private AppHostHttpListenerBase appHost;
        private JsonServiceClient client;

        [SetUp]
        public void TestStart()
        {
            appHost = new TestAppHost();
            appHost.Init();
            appHost.Start("http://localhost:8080/api/");

            client = new JsonServiceClient("http://localhost:8080/api");
        }

        [Test]
        public void TestLogin()
        {
            try
            {

                // make sure there the user doesn't exist before we start the tests.
                client.Headers.Add("X-Classy-Env",
                    new {AppId = "v1.0", CultureCode = "he", CountryCode = "IL", CurrencyCode = "ILS"}.ToJson());
                var deleteResponse = client.Delete<RegistrationResponse>(new Registration {UserName = "papai"});


                // create user
                client.Headers.Add("X-Classy-Env",
                    new {AppId = "v1.0", CultureCode = "he", CountryCode = "IL", CurrencyCode = "ILS"}.ToJson());
                var response = client.Post<RegistrationResponse>("/register",
                    new Registration
                    {
                        FirstName = "papai",
                        LastName = "papaev",
                        DisplayName = "Boss",
                        Email = "theboss@papai.com",
                        UserName = "papai",
                        Password = "123456"
                    });
                Assert.IsNull(response.ResponseStatus.Errors);
                Assert.IsNotNull(response.UserId);

                // Try login
                client.Headers.Add("X-Classy-Env",
                    new {AppId = "v1.0", CultureCode = "he", CountryCode = "IL", CurrencyCode = "ILS"}.ToJson());
                var loginResponse = client.Post<AuthResponse>("/auth",
                    new Auth.Auth {UserName = "papai", Password = "123456"});
                Assert.AreEqual(loginResponse.UserName, "papai");

                // Logoff
                client.Headers.Add("X-Classy-Env",
                    new {AppId = "v1.0", CultureCode = "he", CountryCode = "IL", CurrencyCode = "ILS"}.ToJson());
                //var logoutResponse = client.Delete<AuthResponse>(new Auth.Auth {UserName = "papai", Password = "123456"});


                // delete user
                client.Headers.Add("X-Classy-Env",
                    new {AppId = "v1.0", CultureCode = "he", CountryCode = "IL", CurrencyCode = "ILS"}.ToJson());
                deleteResponse = client.Delete<RegistrationResponse>(new Registration {UserName = "papai"});
                Assert.AreEqual(deleteResponse.UserId, response.UserId);

                //try login user missing
                client.Headers.Add("X-Classy-Env",
                    new {AppId = "v1.0", CultureCode = "he", CountryCode = "IL", CurrencyCode = "ILS"}.ToJson());
                try
                {
                    loginResponse = client.Post<AuthResponse>("/auth",
                        new Auth.Auth {UserName = "papai", Password = "123456"});
                }
                catch (WebServiceException ex)
                {
                    Assert.AreEqual(ex.ErrorMessage, "Invalid UserName or Password");
                }

            }
            catch (WebServiceException ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [TearDown]
        public void TestEnd()
        {
            appHost.Dispose();
        }


    }
}
