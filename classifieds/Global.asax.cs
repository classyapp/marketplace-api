using System;
using System.Linq;
using System.Web;
using classy.DTO.Request;
using classy.DTO.Request.Images;
using classy.DTO.Request.LogActivity;
using classy.DTO.Request.Search;
using classy.Extensions;
using classy.Services;
using Classy.Auth;
using Classy.Models.Request;
using MongoDB.Driver;
using ServiceStack;
using ServiceStack.Common;
using ServiceStack.Common.Web;
using ServiceStack.Configuration;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Admin;
using ServiceStack.ServiceInterface.Validation;
using ServiceStack.WebHost.Endpoints;

namespace classy
{
    public class Global : HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            var host = new ListingServiceHost();
            host.Init();
        }
    }

    public class ListingServiceHost : AppHostBase
    {
        public ListingServiceHost()
            : base("Listing Service Endpoint, Hello", typeof(ListingService).Assembly)
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
            container.RegisterValidators(typeof(PostListing).Assembly);

            PreRequestFilters.Add((httpReq, httpRes) =>
            {
                //Handles Request and closes Responses after emitting global HTTP Headers
                var originWhitelist = new[] { "http://local.homelab:8080", "http://myhome-3.apphb.com", "https://myhome-3.apphb.com" };

                httpRes.AddHeader(HttpHeaders.AllowMethods, "GET, POST, PUT, DELETE, OPTIONS");
                httpRes.AddHeader(HttpHeaders.AllowHeaders, "accept, x-classy-env, content-type");

                var origin = httpReq.Headers.Get("Origin");
                if (originWhitelist.Contains(origin))
                    httpRes.AddHeader(HttpHeaders.AllowOrigin, origin);

                if (httpReq.HttpMethod == "OPTIONS") {
                    httpRes.EndRequest(); //add a 'using ServiceStack;'
                }
            });
            
            container.WireUp();

            // configure service routes
            ConfigureServiceRoutes();

            ConfigureOperators(container);
        }

        private void ConfigureOperators(Funq.Container container)
        {
            //var mqServer = container.TryResolve<ServiceStack.Messaging.IMessageService>();

            //// example operator registration
            //// -----------------------------
            //container.Register<CreateThumbnailsOperator>(c => new CreateThumbnailsOperator(c.TryResolve<IStorageRepository>(), c.TryResolve<IListingRepository>(), c.TryResolve<IAppManager>()));
            //mqServer.RegisterHandler<CreateThumbnailsRequest>(m =>
            //{
            //    var operation = container.TryResolve<CreateThumbnailsOperator>();
            //    operation.PerformOperation(m.GetBody());
            //    return true;
            //});

            //mqServer.Start();
        }

        private void ConfigureAuth(Funq.Container container)
        {
            var appSettings = new AppSettings();

            //Register all Authentication methods you want to enable for this web app. 
            Plugins.Add(new Classy.Auth.AuthFeature(
                () => new CustomUserSession(), // Use your own typed Custom UserSession type
                    new Classy.Auth.IAuthProvider[] {
                        new Classy.Auth.CredentialsAuthProvider(),      //HTML Form post of UserName/Password credentials
                        new CustomFacebookAuthProvider(appSettings),    //Sign-in with Facebook
                        new GoogleOAuth2Provider(appSettings),
                        //new DigestAuthProvider(appSettings),        //Sign-in with Digest Auth
                        new Classy.Auth.BasicAuthProvider()
                }));


            //Provide service for new users to register so they can login with supplied credentials.
            Plugins.Add(new Classy.Auth.RegistrationFeature());

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
                .Add<FreeSearchRequest>("/free_search", "POST")
                .Add<KeywordSuggestionRequest>("/search/keywords/suggest", "GET")
                .Add<SearchSuggestionsRequest>("/search/{EntityType}/suggest", "GET")

                // App settings
                .Add<GetAppSettings>("/app/settings", "GET")

                // ResetPassword
                .Add<ForgotPasswordRequest>("/auth/forgot", "POST")
                .Add<VerifyPasswordResetRequest>("/auth/reset", "GET")
                .Add<PasswordResetRequest>("/auth/reset", "POST")

                // Thumbnails
                .Add<GetCollageRequest>("/collage", "GET")
                .Add<GetThumbnail>("/thumbnail/{ImageKey}", "GET")

                // Listings
                .Add<EditMultipleListings>("/listings/edit-multiple", "POST")
                .Add<GetListingById>("/listing/{ListingId}", ApplyTo.Get | ApplyTo.Options) // get listing by id, update listing
                .Add<GetListingsById>("/listing/get-multiple", "POST")
                .Add<DeleteListing>("/listing/{ListingId}", "DELETE") // delete listing by id, update listing
                .Add<PostListing>("/listing/new", "POST") // post new listing
                .Add<AddExternalMedia>("/listing/{ListingId}/media", "POST") // add media files and associate with listing
                .Add<DeleteExternalMedia>("/listing/{ListingId}/media", "DELETE")
                .Add<PublishListing>("/listing/{ListingId}/publish", "POST") // publish a post to the public
                .Add<UpdateListing>("/listing/{ListingId}", "PUT") // update listing
                .Add<SearchListings>("/listing/search", ApplyTo.Get | ApplyTo.Post) // search listings by tag and/or metadata
                .Add<SearchListings>("/tags/{tag}", "GET") // search with a nicer url for tag
                .Add<GetListingsByProfileId>("/profile/{ProfileId}/listing/list", "GET") // get list of listing for profile
                .Add<SetListingTranslation>("/listing/{ListingId}/translation", "POST")
                .Add<SetListingTranslation>("/listing/{ListingId}/translation/{CultureCode}", "POST")
                .Add<GetListingTranslation>("/listing/{ListingId}/translation/{CultureCode}", "GET")
                .Add<DeleteListingTranslation>("/listing/{ListingId}/translation/{CultureCode}", "DELETE")
                .Add<GetListingMoreInfo>("/listing/{ListingId}/more", "POST")
                .Add<CheckListingDuplicateSKUs>("/listing/sku/check", "POST")

                // Collections
                .Add<CreateCollection>("/collection/new", "POST") // create a new collection
                .Add<AddListingsToCollection>("/collection/{CollectionId}/listing/new", "POST") // add listings to collection
                .Add<RemoveListingFromCollection>("/collection/{CollectionId}/remove", "POST")
                .Add<UpdateCollection>("/collection/{CollectionId}", "PUT") //update listings comments, and collection title and content
                .Add<DeleteCollection>("/collection/{CollectionId}", "DELETE") //delete collection
                .Add<SubmitCollectionForEditorialApproval>("/collection/{CollectionId}/submit", "POST")
                .Add<GetApprovedCollections>("/collection/list/approved", "GET")
                .Add<SetCollectionCoverPhotos>("/collection/{CollectionId}/cover", "POST") //set collection cover photos
                //.Add<RemoveListingsFromCollection>("/collection/{CollectionId}/listing", "DELETE") // remove listings to collection
                //.Add<AddCollaboratorsToCollection>("/collection/{CollectionId}/collaborator", "POST") // add collaborators to collection
                //.Add<RemoveCollaboratorsFromCollection>("/collection/{CollectionId}/collaborator", "DELETE") // remove collaborators to collection
                //.Add<AddPermittedViewersToCollection>("/collection/{CollectionId}/viewer", "POST") // add view premissions to profiles
                //.Add<RemovePermittedViewersFromCollection>("/collection/{CollectionId}/viewer", "DELETE") // remove view permissions
                //.Add<UpdateCollection>("/collection/{CollectionId}", "PUT") // update collection details
                .Add<GetCollectionById>("/collection/{CollectionId}", ApplyTo.Get | ApplyTo.Options) // get a collection by id
                .Add<GetCollectionByProfileId>("/profile/{ProfileId}/collection/list/{CollectionType}", "GET") // get a collection by id
                .Add<GetCollectionTranslation>("/collection/{CollectionID}/translation/{CultureCode}", "GET")
                .Add<SetCollectionTranslation>("/collection/{CollectionId}/translation/{CultureCode}", "POST")
                .Add<DeleteCollectionTranslation>("/collection/{CollectionID}/translation/{CultureCode}", "DELETE")

                // Comments
                .Add<PostCommentForListing>("/listing/{ListingId}/comment/new", "POST") // post new comment
                .Add<PostCommentForCollection>("/collection/{CollectionId}/comment/new", "POST") // post new comment
                //.Add<PublishComment>("/listing/{ListingId}}/comment/{CommentId}/publish", "POST")
                //.Add<DeleteComment>("/listing/{ListingId}/comment/{CommentId}", "DELETE")

                // Social Action
                .Add<FavoriteListing>("/listing/{ListingId}/favorite", ApplyTo.Post | ApplyTo.Delete) // favorite / unfavorite
                .Add<FollowProfile>("/profile/{FolloweeProfileId}/follow", ApplyTo.Post | ApplyTo.Delete) // follow / unfollow
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
                .Add<GetProfileById>("/profile/{ProfileId}", ApplyTo.Get | ApplyTo.Options)
                .Add<UpdateProfile>("/profile/{ProfileId}", "PUT")
                .Add<ClaimProxyProfile>("/profile/{ProxyProfileId}/claim", "POST")
                .Add<ApproveProxyClaim>("/profile/{ClaimId}/approve", "POST")
                .Add<RejectProxyClaim>("/profile/{ClaimId}/reject", "POST")
                .Add<CreateProfileProxy>("/profile/new", "POST")
                .Add<SearchProfiles>("/profile/search", ApplyTo.Get | ApplyTo.Post)
                .Add<GetFacebookAlbums>("/profile/social/facebook/albums", ApplyTo.Get)
                .Add<GetGoogleContacts>("/profile/social/google/contacts", ApplyTo.Get)
                .Add<SetProfileTranslation>("/profile/{ProfileID}/translation/{CultureCode}", "POST")
                .Add<GetProfileTranslation>("/profile/{ProfileID}/translation/{CultureCode}", "GET")
                .Add<DeleteProfileTranslation>("/profile/{ProfileID}/translation/{CultureCode}", "DELETE")
                .Add<VerifyEmailRequest>("/profile/verify/{hash}", "GET")

                // Products
                .Add<ImportPorductCatalogRequest>("/product/uploadcatalog", "POST")

                // Job
                .Add<JobsStatusRequest>("/jobs/{ProfileId}", "GET")
                .Add<JobErrorsRequest>("/job/{JobId}/errors", "GET")

                // Reviews
                .Add<PostReviewForListing>("/listing/{ListingId}/reviews/new", "POST")
                .Add<PostReviewForProfile>("/profile/{RevieweeProfileId}/reviews/new", "POST")
                .Add<GetReviewsByProfileId>("/profile/{ProfileId}/reviews", "GET")
                .Add<PublishOrDeleteReview>("/profile/reviews/{ReviewId}/publish", "POST")
                .Add<PublishOrDeleteReview>("/profile/reviews/{ReviewId}", "DELETE")

                // Analytics
                .Add<LogActivity>("/stats/push", "POST")

                // Localization
                .Add<GetListResourceByKey>("/resource/list/{Key}", "GET")
                .Add<GetResourceByKey>("/resource/{Key}",ApplyTo.Get | ApplyTo.Options)
                .Add<GetResourcesForApp>("/resource/all", "GET")
                .Add<CreateNewResource>("/resource", "POST")
                .Add<SetResourceValues>("/resource/{Key}", "POST")
                .Add<SetResourceListValues>("/resource/list/{Key}", "POST")
                .Add<GetCitiesByCountry>("/resource/list/cities/{countryCode}", "GET")

                // Email
                .Add<SendEmailRequest>("/email", "POST")

                // Log Activity
                .Add<LogActivityRequest>("/log-activity/log", "POST")
                .Add<GetLogActivityRequest>("/log-activity/log", "GET")

                // Media Files
                .Add<SaveTempMediaRequest>("/media", "POST")
                .Add<DeleteTempMediaRequest>("/media", "DELETE")
            ;
        }
    }
}