using Classy.Auth;
using ServiceStack.Common.Web;
using ServiceStack.Configuration;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Validation;
using ServiceStack.WebHost.Endpoints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack.Common;
using classy.Extentions;
using MongoDB.Driver;

namespace Classy.Tests
{
    public class TestAppHost : AppHostHttpListenerBase
    {
        public TestAppHost()
            : base("Auth Service Test Host", typeof(Auth.AuthService).Assembly)
        {
            // request filter to verify api key
            RequestFilters.Add(CustomAuthenticateAttribute.SetEnvironment);

            SetConfig(new EndpointHostConfig()
            {
                DefaultContentType = ContentType.Json,
                DebugMode = false,
                EnableFeatures = Feature.All
                    .Remove(Feature.Metadata)
                    .Remove(Feature.Html)
            });
        }

        public override void Configure(Funq.Container container)
        {
            //Enable Authentication and Registration
            ConfigureAuth(container);

            // Validation
            Plugins.Add(new ValidationFeature());

            classy.Extensions.FunqExtensions.WireUp(container);
            //container.WireUp();

            // configure service routes
            ConfigureServiceRoutes();
        }

        private void ConfigureServiceRoutes()
        {
        }

        private void ConfigureAuth(Funq.Container container)
        {
            var appSettings = new AppSettings();

            //Register all Authentication methods you want to enable for this web app. 
            Plugins.Add(new Classy.Auth.AuthFeature(
                () => new CustomUserSession(), // Use your own typed Custom UserSession type
                    new Classy.Auth.IAuthProvider[] {
                        new CredentialsAuthProvider()
                }));


            //Provide service for new users to register so they can login with supplied credentials.
            Plugins.Add(new Classy.Auth.RegistrationFeature());

            //Store User Data into the referenced MongoDB database
            //Store User Data into the referenced MongoDB database
            container.Register<IUserAuthRepository>(c => new MongoDBAuthRepositoryTests(c.Resolve<MongoDatabase>(), true)); 

        }
    }
}
