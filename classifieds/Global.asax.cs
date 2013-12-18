using Classy.Auth;
using classy.Manager;
using Classy.Models;
using Classy.Models.Request;
using Classy.Repository;
using ServiceStack.Common;
using ServiceStack.Common.Web;
using ServiceStack.Configuration;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Admin;
using ServiceStack.ServiceInterface.Auth;
using ServiceStack.ServiceInterface.Cors;
using ServiceStack.ServiceInterface.Validation;
using ServiceStack.WebHost.Endpoints;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

namespace classy
{
    public class Global : System.Web.HttpApplication
    {
        public class ListingServiceHost : AppHostBase
        {
            public ListingServiceHost() : base("Listing Service Endpoint, Hello", typeof(Services.ListingService).Assembly) 
            {
                // request filter to verify api key
                RequestFilters.Add(CustomAuthenticateAttribute.ApiKeyFilter);

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
                // CORS
                //Plugins.Add(new CorsFeature(
                //    allowedOrigins: "http://www.thisisclassy.com",
                //    allowedMethods: "GET, POST, PUT, DELETE, OPTIONS",
                //    allowedHeaders: "Content-Type, X-ApiKey, Control-Type, Accept, Origin",
                //    allowCredentials: true
                //));

                //Enable Authentication and Registration
                ConfigureAuth(container);

                // Validation
                Plugins.Add(new ValidationFeature());
                container.RegisterValidators(typeof(PostListing).Assembly);

                // register mongodb repositories
                container.Register<ITripleStore>(new MongoTripleStore());
                container.Register<IListingRepository>(new MongoListingRepository());
                container.Register<ICommentRepository>(new MongoCommentRepository());
                container.Register<IReviewRepository>(new MongoReviewRepository());
                container.Register<IStorageRepository>(new AmazonS3StorageRepository());
                container.Register<IProfileRepository>(new MongoProfileRepository());
                container.Register<IBookingRepository>(new MongoBookingRepository());
                container.Register<ITransactionRepository>(new MongoTransactionRepository());
                container.Register<IOrderRepository>(new MongoOrderRepository());
                container.Register<IAppManager>(c =>
                    new DefaultAppManager());
                container.Register<IPaymentGateway>(c => 
                    new TranzilaPaymentGateway(
                        c.TryResolve<ITransactionRepository>()));
                container.Register<IBookingManager>(c =>
                    new DefaultBookingManager(
                        c.TryResolve<IListingRepository>(),
                        c.TryResolve<IBookingRepository>(),
                        c.TryResolve<IPaymentGateway>(),
                        c.TryResolve<ITripleStore>()));
                container.Register<IOrderManager>(c =>
                    new DefaultOrderManager(
                        c.TryResolve<IListingRepository>(),
                        c.TryResolve<IProfileRepository>(),
                        c.TryResolve<IOrderRepository>(),
                        c.TryResolve<ITransactionRepository>(),
                        c.TryResolve<IPaymentGateway>(),
                        c.TryResolve<ITripleStore>(),
                        c.TryResolve<ITaxCalculator>(),
                        c.TryResolve<IShippingCalculator>()));
                container.Register<IListingManager>(c =>
                    new DefaultListingManager(
                        c.TryResolve<IListingRepository>(),
                        c.TryResolve<ICommentRepository>(),
                        c.TryResolve<IProfileRepository>(),
                        c.TryResolve<ITripleStore>(),
                        c.TryResolve<IStorageRepository>()));
                container.Register<IProfileManager>(c =>
                    new DefaultProfileManager(
                        c.TryResolve<IAppManager>(),
                        c.TryResolve<IProfileRepository>(),
                        c.TryResolve<IListingRepository>(),
                        c.TryResolve<IReviewRepository>(),
                        c.TryResolve<ITripleStore>()));
                container.Register<IReviewManager>(c =>
                    new DefaultProfileManager(
                        c.TryResolve<IAppManager>(),
                        c.TryResolve<IProfileRepository>(),
                        c.TryResolve<IListingRepository>(),
                        c.TryResolve<IReviewRepository>(),
                        c.TryResolve<ITripleStore>()));
                container.Register<IAnalyticsManager>(c =>
                    new DefaultAnalyticsManager(
                        c.TryResolve<ITripleStore>()));

                // configure service routes
                ConfigureServiceRoutes();
            }

            private void ConfigureAuth(Funq.Container container)
            {
                var appSettings = new AppSettings(); 
                
                //Register all Authentication methods you want to enable for this web app. 
                Plugins.Add(new Classy.Auth.AuthFeature(
                    () => new CustomUserSession(), // Use your own typed Custom UserSession type
                        new Classy.Auth.IAuthProvider[] {
                        new Classy.Auth.CredentialsAuthProvider(),              //HTML Form post of UserName/Password credentials
                        new CustomFacebookAuthProvider(appSettings),    //Sign-in with Facebook
                        //new DigestAuthProvider(appSettings),        //Sign-in with Digest Auth
                        new Classy.Auth.BasicAuthProvider()
                }));


                //Provide service for new users to register so they can login with supplied credentials.
                Plugins.Add(new Classy.Auth.RegistrationFeature());

                //override the default registration validation with your own custom implementation
                //container.RegisterAs<CustomRegistrationValidator, IValidator<Registration>>();

                //Store User Data into the referenced MongoDB database
                container.Register<Classy.Auth.IUserAuthRepository>(c =>
                    new Classy.Auth.MongoDBAuthRepository(true)); 

                //logging feature
#if DEBUG
                Plugins.Add(new RequestLogsFeature());
#endif
            }

            private void ConfigureServiceRoutes()
            {
                Routes
                    // Listings
                    .Add<GetListingById>("/listings/{ListingId}", "GET") // get listing by id, update listing
                    .Add<PostListing>("/listings/new", "POST") // post new listing
                    .Add<AddExternalMedia>("/listings/{ListingId}/media", "POST") // add media files and associate with listing
                    .Add<DeleteExternalMedia>("/listings/{ListingId}/media", "DELETE")
                    .Add<PublishListing>("/listings/{ListingId}/publish", "POST") // publish a post to the public
                    .Add<PostListing>("/listings/{ListingId}", "PUT") // update listing
                    .Add<SearchListings>("/listings/search", "GET") // search listings by tag and/or metadata
                    .Add<SearchListings>("/tags/{tag}", "GET") // search with a nicer url for tag
                    .Add<GetListingsByUsername>("/profile/{Username}/listings", "GET") // get list of listing for profile

                    // Comments
                    .Add<PostComment>("/listings/{ListingId}/comments/new", "POST") // post new comment
                    //.Add<PublishComment>("/listings/{ListingId}}/comments/{CommentId}/publish", "POST")
                    //.Add<DeleteComment>("/listings/{ListingId}/comment/{CommentId}", "DELETE")

                    // Social Actions
                    .Add<FavoriteListing>("/listings/{ListingId}/favorite", "POST") // favorite
                    //.Add<FavoriteListing>("/listings/{ListingId}/favorite", "DELETE") // un-favorite
                    .Add<FollowProfile>("/profile/{FolloweeUsername}/follow", "POST") // follow
                    //.Add<FollowProfile>("/profile/{FolloweeUsername/follow", "DELETE") // un-follow
                    .Add<FlagListing>("/listings/{ListingId}/flag", "POST") // flag a listing

                    // Scheduling and Booking
                    .Add<BookListing>("/listings/{ListingId}/schedule/book", "POST")
                    .Add<GetBookingsForListing>("/listings/{ListingId}/schedule", "GET")
                    .Add<GetBookingsForProfileListings>("/profile/schedule", "GET") // get all the entire calendar for the profile's listing portfolio
                    .Add<GetBookingsForProfile>("/profile/schedule/my", "GET") // get all bookings made by the user
                    .Add<UpdateBooking>("/profile/schedule/booking/{BookingId}", "PUT")
                    .Add<CancelBooking>("/profile/schedule/booking/{BookingId}", "DELETE")

                    // Purchasing
                    .Add<PurchaseSingleItem>("/listings/{ListingId}/buy", "POST")
                    .Add<GetOrdersForListing>("/listings/{ListingId}/orders", "GET")
                    .Add<GetOrdersForProfile>("/profile/me/purchases", "GET")
                    .Add<GetOrdersForProfileListings>("/profile/me/orders", "GET")
                    .Add<UpdateSingleItemOrder>("/profile/orders/{OrderId}", "PUT")
                    .Add<CancelSingleItemOrder>("/profile/orders/{OrderId}", "DELETE")

                    // Profiles
                    .Add<GetProfileById>("/profile/{ProfileId}", "GET")
                    .Add<UpdateProfile>("/profile/{ProfileId}", "PUT")
                    .Add<ClaimProxyProfile>("/profile/{ProxyProfileId}/claim", "POST")
                    .Add<ApproveProxyClaim>("/profile/{ClaimId}/approve", "POST")
                    .Add<RejectProxyClaim>("/profile/{ClaimId}/reject", "POST")
                    .Add<CreateProfileProxy>("/profile/new", "POST")
                    .Add<SearchProfiles>("/profile/search", "GET")

                    // Reviews
                    .Add<PostReviewForListing>("/listings/{ListingId}/reviews/new", "POST")
                    .Add<PostReviewForProfile>("/profile/{RevieweeProfileId}/reviews/new", "POST")
                    .Add<GetReviewsByProfileId>("/profile/{ProfileId}/reviews", "GET")
                    .Add<PublishOrDeleteReview>("/profile/reviews/{ReviewId}/publish", "POST")
                    .Add<PublishOrDeleteReview>("/profile/reviews/{ReviewId}", "DELETE")

                    // Analytics
                    .Add<LogActivity>("/stats/push", "POST")
                ;
            }
        }

        protected void Application_Start(object sender, EventArgs e)
        {
            var host = new ListingServiceHost();
            host.Init();
        }
    }
}