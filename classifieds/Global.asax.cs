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
using ServiceStack.Text;
using MongoDB.Driver;
using System.Configuration;

namespace classy
{
    public class Global : System.Web.HttpApplication
    {
        public class ListingServiceHost : AppHostBase
        {
            public ListingServiceHost() : base("Listing Service Endpoint, Hello", typeof(Services.ListingService).Assembly) 
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
                container.Register<MongoDatabase>(c =>
                {
                    var connectionString = ConfigurationManager.ConnectionStrings["MongoDB"].ConnectionString;
                    var client = new MongoClient(connectionString);
                    var databaseName = MongoUrl.Create(connectionString).DatabaseName;
                    var server = client.GetServer();
                    var db = server.GetDatabase(databaseName);
                    return db;
                });
                container.Register<ITripleStore>(c => new MongoTripleStore(c.Resolve<MongoDatabase>()));
                container.Register<IListingRepository>(c => new MongoListingRepository(c.Resolve<MongoDatabase>()));
                container.Register<ICommentRepository>(c => new MongoCommentRepository(c.Resolve<MongoDatabase>()));
                container.Register<IReviewRepository>(c => new MongoReviewRepository(c.Resolve<MongoDatabase>()));
                container.Register<Amazon.S3.IAmazonS3>(c => {
                    var s3Client = Amazon.AWSClientFactory.CreateAmazonS3Client(
                        ConfigurationManager.AppSettings["S3AccessKey"],
                        ConfigurationManager.AppSettings["S3SecretKey"]
                    );
                    return s3Client;
                });
                container.Register<IStorageRepository>(c => new AmazonS3StorageRepository(c.Resolve<Amazon.S3.AmazonS3Client>(),  ConfigurationManager.AppSettings["S3BucketName"]));
                container.Register<IProfileRepository>(c => new MongoProfileRepository(c.Resolve<MongoDatabase>()));
                container.Register<IBookingRepository>(c => new MongoBookingRepository(c.Resolve<MongoDatabase>()));
                container.Register<ITransactionRepository>(c => new MongoTransactionRepository(c.Resolve<MongoDatabase>()));
                container.Register<IOrderRepository>(c => new MongoOrderRepository(c.Resolve<MongoDatabase>()));
                container.Register<ICollectionRepository>(c => new MongoCollectionRepository(c.Resolve<MongoDatabase>()));
                container.Register<ILocalizationRepository>(c => new MongoLocalizationProvider(c.Resolve<MongoDatabase>()));
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
                        c.TryResolve<ICollectionRepository>(),
                        c.TryResolve<ITripleStore>(),
                        c.TryResolve<IStorageRepository>()));
                container.Register<IProfileManager>(c =>
                    new DefaultProfileManager(
                        c.TryResolve<IAppManager>(),
                        c.TryResolve<IProfileRepository>(),
                        c.TryResolve<IListingRepository>(),
                        c.TryResolve<IReviewRepository>(),
                        c.TryResolve<ICollectionRepository>(),
                        c.TryResolve<ITripleStore>()));
                container.Register<IReviewManager>(c =>
                    new DefaultProfileManager(
                        c.TryResolve<IAppManager>(),
                        c.TryResolve<IProfileRepository>(),
                        c.TryResolve<IListingRepository>(),
                        c.TryResolve<IReviewRepository>(),
                        c.TryResolve<ICollectionRepository>(),
                        c.TryResolve<ITripleStore>()));
                container.Register<ICollectionManager>(c =>
                    new DefaultListingManager(
                        c.TryResolve<IListingRepository>(),
                        c.TryResolve<ICommentRepository>(),
                        c.TryResolve<IProfileRepository>(),
                        c.TryResolve<ICollectionRepository>(),
                        c.TryResolve<ITripleStore>(),
                        c.TryResolve<IStorageRepository>()));
                container.Register<IAnalyticsManager>(c =>
                    new DefaultAnalyticsManager(
                        c.TryResolve<ITripleStore>()));
                container.Register<ILocalizationManager>(c =>
                    new DefaultLocalizationManager(
                        c.TryResolve<ILocalizationRepository>()));

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
                container.Register<Classy.Auth.IUserAuthRepository>(c => new Classy.Auth.MongoDBAuthRepository(c.Resolve<MongoDatabase>(), true)); 

                //logging feature
#if DEBUG
                Plugins.Add(new RequestLogsFeature());
#endif
            }

            private void ConfigureServiceRoutes()
            {
                Routes
                    // Listings
                    .Add<GetListingById>("/listing/{ListingId}", "GET") // get listing by id, update listing
                    .Add<PostListing>("/listing/new", "POST") // post new listing
                    .Add<AddExternalMedia>("/listing/{ListingId}/media", "POST") // add media files and associate with listing
                    .Add<DeleteExternalMedia>("/listing/{ListingId}/media", "DELETE")
                    .Add<PublishListing>("/listing/{ListingId}/publish", "POST") // publish a post to the public
                    .Add<PostListing>("/listing/{ListingId}", "PUT") // update listing
                    .Add<SearchListings>("/listing/search", ApplyTo.Get | ApplyTo.Post) // search listings by tag and/or metadata
                    .Add<SearchListings>("/tags/{tag}", "GET") // search with a nicer url for tag
                    .Add<GetListingsByUsername>("/profile/{Username}/listing/list", "GET") // get list of listing for profile

                    // Collections
                    .Add<CreateCollection>("/collection/new", "POST") // create a new collection
                    .Add<AddListingsToCollection>("/collection/{CollectionId}/listing/new", "POST") // add listings to collection
                    //.Add<RemoveListingsFromCollection>("/collection/{CollectionId}/listing", "DELETE") // remove listings to collection
                    //.Add<AddCollaboratorsToCollection>("/collection/{CollectionId}/collaborator", "POST") // add collaborators to collection
                    //.Add<RemoveCollaboratorsFromCollection>("/collection/{CollectionId}/collaborator", "DELETE") // remove collaborators to collection
                    //.Add<AddPermittedViewersToCollection>("/collection/{CollectionId}/viewer", "POST") // add view premissions to profiles
                    //.Add<RemovePermittedViewersFromCollection>("/collection/{CollectionId}/viewer", "DELETE") // remove view permissions
                    //.Add<UpdateCollection>("/collection/{CollectionId}", "PUT") // update collection details
                    .Add<GetCollectionById>("/collection/{CollectionId}", "GET") // get a collection by id
                    .Add<GetCollectionByProfileId>("/profile/{ProfileId}/collection/list", "GET") // get a collection by id

                    // Comments
                    .Add<PostComment>("/listing/{ListingId}/comment/new", "POST") // post new comment
                    //.Add<PublishComment>("/listing/{ListingId}}/comment/{CommentId}/publish", "POST")
                    //.Add<DeleteComment>("/listing/{ListingId}/comment/{CommentId}", "DELETE")

                    // Social Actions
                    .Add<FavoriteListing>("/listing/{ListingId}/favorite", "POST") // favorite
                    //.Add<FavoriteListing>("/listing/{ListingId}/favorite", "DELETE") // un-favorite
                    .Add<FollowProfile>("/profile/{FolloweeUsername}/follow", "POST") // follow
                    //.Add<FollowProfile>("/profile/{FolloweeUsername/follow", "DELETE") // un-follow
                    .Add<FlagListing>("/listing/{ListingId}/flag", "POST") // flag a listing

                    // Scheduling and Booking
                    .Add<BookListing>("/listing/{ListingId}/schedule/book", "POST")
                    .Add<GetBookingsForListing>("/listing/{ListingId}/schedule", "GET")
                    .Add<GetBookingsForProfileListings>("/profile/schedule", "GET") // get all the entire calendar for the profile's listing portfolio
                    .Add<GetBookingsForProfile>("/profile/schedule/my", "GET") // get all bookings made by the user
                    .Add<UpdateBooking>("/profile/schedule/booking/{BookingId}", "PUT")
                    .Add<CancelBooking>("/profile/schedule/booking/{BookingId}", "DELETE")

                    // Purchasing
                    .Add<PurchaseSingleItem>("/listing/{ListingId}/buy", "POST")
                    .Add<GetOrdersForListing>("/listing/{ListingId}/orders", "GET")
                    .Add<GetOrdersForProfile>("/profile/me/purchases", "GET")
                    .Add<GetOrdersForProfileListings>("/profile/me/orders", "GET")
                    .Add<UpdateSingleItemOrder>("/profile/orders/{OrderId}", "PUT")
                    .Add<CancelSingleItemOrder>("/profile/orders/{OrderId}", "DELETE")

                    // Profiles
                    .Add<GetAutenticatedProfile>("/profile", "GET")
                    .Add<GetProfileById>("/profile/{ProfileId}", "GET")
                    .Add<UpdateProfile>("/profile/{ProfileId}", "PUT")
                    .Add<ClaimProxyProfile>("/profile/{ProxyProfileId}/claim", "POST")
                    .Add<ApproveProxyClaim>("/profile/{ClaimId}/approve", "POST")
                    .Add<RejectProxyClaim>("/profile/{ClaimId}/reject", "POST")
                    .Add<CreateProfileProxy>("/profile/new", "POST")
                    .Add<SearchProfiles>("/profile/search", ApplyTo.Get | ApplyTo.Post)

                    // Reviews
                    .Add<PostReviewForListing>("/listing/{ListingId}/reviews/new", "POST")
                    .Add<PostReviewForProfile>("/profile/{RevieweeProfileId}/reviews/new", "POST")
                    .Add<GetReviewsByProfileId>("/profile/{ProfileId}/reviews", "GET")
                    .Add<PublishOrDeleteReview>("/profile/reviews/{ReviewId}/publish", "POST")
                    .Add<PublishOrDeleteReview>("/profile/reviews/{ReviewId}", "DELETE")

                    // Analytics
                    .Add<LogActivity>("/stats/push", "POST")

                    // Localization
                    .Add<GetResourceByKey>("/resource/{Key}", "GET")
                    .Add<SetResourceValues>("/resource/{Key}", "POST")
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