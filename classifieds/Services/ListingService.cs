﻿using ServiceStack.Common;
using ServiceStack.ServiceInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using ServiceStack.Common.Web;
using System.Net;
using Classy.Models;
using Classy.Models.Response;
using Classy.Models.Request;
using Classy.Auth;
using classy.Manager;
using System.Security.Cryptography;
using System.Text;
using Classy.Interfaces.Managers;

namespace classy.Services
{
    public class ListingService : Service
    {
        public IBookingManager BookingManager { get; set; }
        public IOrderManager OrderManager { get; set; }
        public IListingManager ListingManager { get; set; }
        public IProfileManager ProfileManager { get; set; }
        public IReviewManager ReviewManager { get; set; }
        public IAnalyticsManager AnalyticsManager { get; set; }
        public ILocalizationManager LocalizationManager { get; set; }
        public IThumbnailManager ThumbnailManager { get; set; }
        public IEmailManager EmailManager { get; set; }
        public IAppManager AppManager { get; set; }

        public object Get(GetListingById request)
        {
            try
            {
                var session = SessionAs<CustomUserSession>();
                ListingManager.SecurityContext = session.ToSecurityContext();

                var listingView = ListingManager.GetListingById(
                    request.Environment.AppId,
                    request.ListingId,
                    request.LogImpression,
                    false,
                    request.IncludeComments,
                    request.FormatCommentsAsHtml,
                    request.IncludeCommenterProfiles,
                    request.IncludeProfile,
                    request.IncludeFavoritedByProfiles,
                    request.Environment.CultureCode);

                return new HttpResult(listingView, HttpStatusCode.OK);
            }
            catch (KeyNotFoundException kex)
            {
                return new HttpError(HttpStatusCode.NotFound, kex.Message);
            }
        }

        public object Get(GetListingsByProfileId request)
       { 
            try
            {
                var listingViews = ListingManager.GetListingsByProfileId(
                    request.Environment.AppId,
                    request.ProfileId,
                    request.IncludeComments,
                    request.FormatCommentsAsHtml,
                    request.IncludeDrafts,
                    request.Environment.CultureCode);

                return new HttpResult(listingViews, HttpStatusCode.OK);
            }
            catch (KeyNotFoundException kex)
            {
                return new HttpError(HttpStatusCode.NotFound, kex.Message);
            }
        }

        // create new listing
        [CustomAuthenticate]
        public object Post(PostListing request)
        {
            var session = SessionAs<CustomUserSession>();
            ListingManager.SecurityContext = session.ToSecurityContext();

            var listing = ListingManager.SaveListing(
                request.Environment.AppId,
                null,
                request.Title,
                request.Content,
                request.ListingType,
                request.Pricing,
                request.ContactInfo ?? session.GetDefaultContactInfo(AppManager.GetAppById(request.Environment.AppId).DefaultCountry),
                request.SchedulingTemplate,
                request.Metadata);

            return new HttpResult
            {
                StatusCode = HttpStatusCode.Created,
                Response = listing,
                Headers = {
                    { HttpHeaders.Location, string.Concat("/l/", listing.Id) }
                }
            };
        }

        // add media files
        [CustomAuthenticate]
        public object Post(AddExternalMedia request)
        {
            try
            {
                var session = SessionAs<CustomUserSession>();
                ListingManager.SecurityContext = session.ToSecurityContext();

                var listing = ListingManager.AddExternalMediaToListing(
                    request.Environment.AppId,
                    request.ListingId,
                    Request.Files);

                return new HttpResult(listing, HttpStatusCode.OK);
            }
            catch (KeyNotFoundException kex)
            {
                return new HttpError(HttpStatusCode.NotFound, kex.Message);
            }
        }

        // delete media files
        [CustomAuthenticate]
        public object Delete(DeleteExternalMedia request)
        {
            try
            {
                var session = SessionAs<CustomUserSession>();
                ListingManager.SecurityContext = session.ToSecurityContext();

                var listing = ListingManager.DeleteExternalMediaFromListing(
                    request.Environment.AppId,
                    request.ListingId,
                    request.Url);

                return new HttpResult(listing, HttpStatusCode.OK);
            }
            catch (KeyNotFoundException kex)
            {
                return new HttpError(HttpStatusCode.NotFound, kex.Message);
            }
        }

        // publish listing
        [CustomAuthenticate]
        public object Post(PublishListing request)
        {
            try
            {
                var session = SessionAs<CustomUserSession>();

                var listing = ListingManager.PublishListing(
                    request.Environment.AppId,
                    request.ListingId,
                    session.UserAuthId);

                return new HttpResult(listing, HttpStatusCode.OK);
            }
            catch (KeyNotFoundException kex)
            {
                return new HttpError(HttpStatusCode.NotFound, kex.Message);
            }
        }

        // update listing
        [CustomAuthenticate]
        public object Put(UpdateListing request)
        {
            try
            {
                var session = SessionAs<CustomUserSession>();
                ListingManager.SecurityContext = session.ToSecurityContext();

                var listing = ListingManager.UpdateListing(
                    request.Environment.AppId,
                    request.ListingId,
                    request.Title,
                    request.Content,
                    request.Pricing,
                    request.ContactInfo,
                    request.SchedulingTemplate,
                    request.Metadata,
                    request.Hashtags,
                    request.EditorKeywords,
                    request.Fields);

                return new HttpResult(listing, HttpStatusCode.OK);
            }
            catch (KeyNotFoundException kex)
            {
                return new HttpError(HttpStatusCode.NotFound, kex.Message);
            }
        }

        // delete listing
        [CustomAuthenticate]
        public object Delete(DeleteListing request)
        {
            try
            {
                var session = SessionAs<CustomUserSession>();
                ListingManager.SecurityContext = session.ToSecurityContext();

                var listing = ListingManager.DeleteListing(
                    request.Environment.AppId,
                    request.ListingId);

                return new HttpResult(listing, HttpStatusCode.OK);
            }
            catch (KeyNotFoundException kex)
            {
                return new HttpError(HttpStatusCode.NotFound, kex.Message);
            }
        }

        // favorite a listing
        [CustomAuthenticate]
        public object Post(FavoriteListing request)
        {
            try
            {
                var session = SessionAs<CustomUserSession>();
                ListingManager.SecurityContext = session.ToSecurityContext();

                ListingManager.FavoriteListing(
                    request.Environment.AppId,
                    request.ListingId);

                return new HttpResult(HttpStatusCode.OK);
            }
            catch (KeyNotFoundException kex)
            {
                return new HttpError(HttpStatusCode.NotFound, kex.Message);
            }
        }

        // un-favorite a listing
        [CustomAuthenticate]
        public object Delete(FavoriteListing request)
        {
            try
            {
                var session = SessionAs<CustomUserSession>();
                ListingManager.SecurityContext = session.ToSecurityContext();

                ListingManager.UnfavoriteListing(
                    request.Environment.AppId,
                    request.ListingId);

                return new HttpResult(HttpStatusCode.OK);
            }
            catch (KeyNotFoundException kex)
            {
                return new HttpError(HttpStatusCode.NotFound, kex.Message);
            }
        }

        // favorite a listing
        [CustomAuthenticate]
        public object Post(FlagListing request)
        {
            try
            {
                var session = SessionAs<CustomUserSession>();
                ListingManager.SecurityContext = session.ToSecurityContext();

                ListingManager.FlagListing(
                    request.Environment.AppId,
                    request.ListingId,
                    request.FlagReason);

                return new HttpResult(HttpStatusCode.OK);
            }
            catch (KeyNotFoundException kex)
            {
                return new HttpError(HttpStatusCode.NotFound, kex.Message);
            }
        }

        // book timelost within listing
        [CustomAuthenticate]
        public object Post(BookListing request)
        {
            try
            {
                var session = SessionAs<CustomUserSession>();
                var timeslot = BookingManager.BookListing(
                    request.Environment.AppId,
                    request.ListingId,
                    session.UserAuthId,
                    request.PaymentMethod,
                    request.DateRange,
                    request.Comment,
                    request.MaxDoubleBookings);

                return new HttpResult(timeslot, HttpStatusCode.OK);
            }
            catch (KeyNotFoundException kex)
            {
                return new HttpError(HttpStatusCode.NotFound, kex.Message);
            }
        }

        // get listing bookings
        [CustomAuthenticate]
        public object Get(GetBookingsForListing request)
        {
            try
            {
                var session = SessionAs<CustomUserSession>();
                var bookings = BookingManager.GetBookingsForListing(
                    request.Environment.AppId,
                    request.ListingId,
                    session.UserAuthId,
                    new DateRange { Start = request.Start, End = request.End },
                    request.InculdeCancelled);

                return new HttpResult(bookings, HttpStatusCode.OK);
            }
            catch (KeyNotFoundException kex)
            {
                return new HttpError(HttpStatusCode.NotFound, kex.Message);
            }
        }

        public object Get(GetBookingsForProfileListings request)
        {
            try
            {
                var session = SessionAs<CustomUserSession>();
                if (!session.IsAuthenticated) throw new UnauthorizedAccessException("you must be logged in");
                var bookings = BookingManager.GetBookingsForProfileListings(
                    request.Environment.AppId,
                    session.UserAuthId,
                    new DateRange { Start = request.Start, End = request.End },
                    request.InculdeCancelled);

                return new HttpResult(bookings, HttpStatusCode.OK);
            }
            catch (KeyNotFoundException kex)
            {
                return new HttpError(HttpStatusCode.NotFound, kex.Message);
            }
        }

        public object Get(GetBookingsForProfile request)
        {
            try
            {
                var session = SessionAs<CustomUserSession>();
                if (!session.IsAuthenticated) throw new UnauthorizedAccessException("you must be logged in");
                var bookings = BookingManager.GetBookingsForProfile(
                    request.Environment.AppId,
                    session.UserAuthId,
                    new DateRange { Start = request.Start, End = request.End },
                    request.InculdeCancelled);

                return new HttpResult(bookings, HttpStatusCode.OK);
            }
            catch (KeyNotFoundException kex)
            {
                return new HttpError(HttpStatusCode.NotFound, kex.Message);
            }
        }

        // update booking timeslot
        public object Put(UpdateBooking request)
        {
            try
            {
                var session = SessionAs<CustomUserSession>();

                var booking = BookingManager.UpdateBooking(
                    request.Environment.AppId,
                    request.BookingId,
                    session.UserAuthId,
                    request.DateRange,
                    request.Comment,
                    request.MaxDoubleBookings);

                return new HttpResult(booking, HttpStatusCode.OK);
            }
            catch (KeyNotFoundException kex)
            {
                return new HttpError(HttpStatusCode.NotFound, kex.Message);
            }
        }

        // cancel a booking
        [CustomAuthenticate]
        public object Delete(CancelBooking request)
        {
            try
            {
                var session = SessionAs<CustomUserSession>();
                var response = BookingManager.CancelBooking(
                    request.Environment.AppId,
                    request.BookingId,
                    session.UserAuthId,
                    request.DoRefund);
                return new HttpResult(response, HttpStatusCode.OK);
            }
            catch (KeyNotFoundException kex)
            {
                return new HttpError(HttpStatusCode.NotFound, kex.Message);
            }
        }

        // purchase item offered in a listing
        [CustomAuthenticate]
        public object Post(PurchaseSingleItem request)
        {
            try
            {
                var session = SessionAs<CustomUserSession>();

                var order = OrderManager.PlaceSingleItemOrder(
                    request.Environment.AppId,
                    request.ListingId,
                    session.UserAuthId,
                    request.PaymentMethod,
                    request.ShippingAddress,
                    request.SKU,
                    request.Quantity);

                return new HttpResult(order, HttpStatusCode.OK);
            }
            catch (KeyNotFoundException kex)
            {
                return new HttpError(HttpStatusCode.NotFound, kex.Message);
            }
        }

        // update quantity on single item order
        public object Put(UpdateSingleItemOrder request)
        {
            try
            {
                var session = SessionAs<CustomUserSession>();
                var order = OrderManager.UpdateSingleItemOrder(
                    request.Environment.AppId,
                    request.OrderId,
                    session.UserAuthId,
                    request.SKU,
                    request.Quantity);

                return new HttpResult(order, HttpStatusCode.OK);
            }
            catch (KeyNotFoundException kex)
            {
                return new HttpError(HttpStatusCode.NotFound, kex.Message);
            }
        }

        // cancel a single item order 
        [CustomAuthenticate]
        public object Delete(CancelSingleItemOrder request)
        {
            try
            {
                var session = SessionAs<CustomUserSession>();
                var response = OrderManager.CancelSingleItemOrder(
                    request.Environment.AppId,
                    request.OrderId,
                    session.UserAuthId,
                    request.DoRefund);

                return new HttpResult(response, HttpStatusCode.OK);
            }
            catch (KeyNotFoundException kex)
            {
                return new HttpError(HttpStatusCode.NotFound, kex.Message);
            }
        }

        // get orders for listing
        [CustomAuthenticate]
        public object Get(GetOrdersForListing request)
        {
            try
            {
                var session = SessionAs<CustomUserSession>();
                var orders = OrderManager.GetOrdersForListing(
                    request.Environment.AppId,
                    request.ListingId,
                    session.UserAuthId,
                    request.IncludeCancelled);

                return new HttpResult(orders, HttpStatusCode.OK);
            }
            catch (KeyNotFoundException kex)
            {
                return new HttpError(HttpStatusCode.NotFound, kex.Message);
            }
        }

        // get orders for profile
        [CustomAuthenticate]
        public object Get(GetOrdersForProfile request)
        {
            try
            {
                var session = SessionAs<CustomUserSession>();
                if (!session.IsAuthenticated) throw new UnauthorizedAccessException("you must be logged in");
                var orders = OrderManager.GetOrdersForProfile(
                    request.Environment.AppId,
                    session.UserAuthId,
                    request.IncludeCancelled);

                return new HttpResult(orders, HttpStatusCode.OK);
            }
            catch (KeyNotFoundException kex)
            {
                return new HttpError(HttpStatusCode.NotFound, kex.Message);
            }
        }

        // get orders for all of the profile's listings
        [CustomAuthenticate]
        public object Get(GetOrdersForProfileListings request)
        {
            try
            {
                var session = SessionAs<CustomUserSession>();
                if (!session.IsAuthenticated) throw new UnauthorizedAccessException("you must be logged in");
                var orders = OrderManager.GetOrdersForProfileListings(
                    request.Environment.AppId,
                    session.UserAuthId,
                    request.IncludeCancelled);

                return new HttpResult(orders, HttpStatusCode.OK);
            }
            catch (KeyNotFoundException kex)
            {
                return new HttpError(HttpStatusCode.NotFound, kex.Message);
            }
        }

        // post a review for listing
        [CustomAuthenticate]
        public object Post(PostReviewForListing request)
        {
            try
            {
                var session = SessionAs<CustomUserSession>();
                var review = ReviewManager.PostReviewForListing(
                    request.Environment.AppId,
                    session.UserAuthId,
                    request.ListingId,
                    request.Content,
                    request.Score,
                    request.SubCriteria);
                return new HttpResult(review, HttpStatusCode.OK);
            }
            catch (KeyNotFoundException kex)
            {
                return new HttpError(HttpStatusCode.NotFound, kex.Message);
            }
        }

        // post a review for profile
        [CustomAuthenticate]
        public object Post(PostReviewForProfile request)
        {
            try
            {
                var session = SessionAs<CustomUserSession>();
                var review = ReviewManager.PostReviewForProfile(
                    request.Environment.AppId,
                    session.UserAuthId,
                    request.RevieweeProfileId,
                    request.Content,
                    request.Score,
                    request.SubCriteria,
                    request.Metadata,
                    request.NewProfessionalContactInfo,
                    request.NewProfessionalMetadata);
                var response = new PostReviewResponse
                {
                    Review = review.TranslateTo<ReviewView>()
                };
                if (request.ReturnRevieweeProfile)
                    response.RevieweeProfile = ProfileManager.GetProfileById(
                        request.Environment.AppId,
                        request.RevieweeProfileId,
                        null,
                        false,
                        false,
                        false,
                        false,
                        false,
                        false,
                        false,
                        request.Environment.CultureCode);
                if (request.ReturnReviewerProfile)
                    response.ReviewerProfile = ProfileManager.GetProfileById(
                        request.Environment.AppId,
                        session.UserAuthId,
                        null,
                        false,
                        false,
                        false,
                        false,
                        false,
                        false,
                        false,
                        request.Environment.CultureCode);
                return new HttpResult(response, HttpStatusCode.OK);
            }
            catch (KeyNotFoundException kex)
            {
                return new HttpError(HttpStatusCode.NotFound, kex.Message);
            }
        }

        // get a list of reviews for profile
        [CustomAuthenticate]
        public object Get(GetReviewsByProfileId request)
        {
            try
            {
                var reviews = ReviewManager.GetReviews(
                    request.Environment.AppId,
                    request.ProfileId,
                    request.IncludeDrafts,
                    request.IncludeOnlyDrafts);

                return new HttpResult(reviews, HttpStatusCode.OK);
            }
            catch (KeyNotFoundException kex)
            {
                return new HttpError(HttpStatusCode.NotFound, kex.Message);
            }
        }

        // publish a review
        [CustomAuthenticate]
        public object Post(PublishOrDeleteReview request)
        {
            try
            {
                var session = SessionAs<CustomUserSession>();
                var review = ReviewManager.PublishReview(
                    request.Environment.AppId,
                    request.ReviewId,
                    session.UserAuthId);

                return new HttpResult(review, HttpStatusCode.OK);
            }
            catch (KeyNotFoundException kex)
            {
                return new HttpError(HttpStatusCode.NotFound, kex.Message);
            }
        }

        // delete a review
        public object Delete(PublishOrDeleteReview request)
        {
            try
            {
                var session = SessionAs<CustomUserSession>();
                var review = ReviewManager.DeleteReview(
                    request.Environment.AppId,
                    request.ReviewId,
                    session.UserAuthId);

                return new HttpResult(review, HttpStatusCode.OK);
            }
            catch (KeyNotFoundException kex)
            {
                return new HttpError(HttpStatusCode.NotFound, kex.Message);
            }
        }

        // 
        // POST: /stats/push
        // log an activity 
        public object Post(LogActivity request)
        {
            var session = SessionAs<CustomUserSession>();
            if (!session.IsAuthenticated && request.SubjectId != "guest") throw new ApplicationException("when no user logged in, SubjectId must be 'guest'");
            var tripleView = AnalyticsManager.LogActivity(request.Environment.AppId, request.SubjectId, ActivityPredicate.CONTACT_PROFILE, request.ObjectId);
            return new HttpResult(tripleView, HttpStatusCode.OK);
        }

        //
        // GET: /resource/{Key}
        // get resource by key
        public object Get(GetResourceByKey request)
        {
            var resource = LocalizationManager.GetResourceByKey(request.Environment.AppId, request.Key, request.ProcessMarkdown);
            return new HttpResult(resource, HttpStatusCode.OK);
        }

        //
        // GET: /resource/all
        // get all available resources for an app
        [CustomAuthenticate]
        [CustomRequiredPermission("cms")]
        public object Get(GetResourcesForApp request)
        {
            var resourceKeys = LocalizationManager.GetResourcesForApp(request.Environment.AppId);
            return new HttpResult(resourceKeys, HttpStatusCode.OK);
        }

        //
        // GET: /resource/list/{Key}
        // get resource by key
        public object Get(GetListResourceByKey request)
        {
            var resource = LocalizationManager.GetListResourceByKey(request.Environment.AppId, request.Key);
            return new HttpResult(resource, HttpStatusCode.OK);
        }

        //
        // POST: /resource/{Key}
        // set resource values
        [CustomAuthenticate]
        [CustomRequiredPermission("cms")]
        public object Post(SetResourceValues request)
        {
            var resource = LocalizationManager.SetResourceValues(request.Environment.AppId, request.Key, request.Values);
            return new HttpResult(resource, HttpStatusCode.OK);
        }

        //
        // POST: /resource
        // create new resource
        [CustomAuthenticate]
        [CustomRequiredPermission("cms")]
        public object Post(CreateNewResource request)
        {
            var resource = LocalizationManager.CreateResource(request.Environment.AppId, request.Key, request.Values, request.Description);
            return new HttpResult(resource, HttpStatusCode.OK);
        }

        //
        // POST: /resource/list/{Key}
        // set resource values
        [CustomAuthenticate]
        [CustomRequiredPermission("cms")]
        public object Post(SetResourceListValues request)
        {
            var listResource = LocalizationManager.SetListResourceValues(request.Environment.AppId, request.Key, request.ListItems);
            return new HttpResult(listResource, HttpStatusCode.OK);
        }

        //
        //GET: /profile/facebook/friends
        // get a list of the user's facebook friends
        [CustomAuthenticate]
        public object Get(GetFacebookAlbums request)
        {
            var session = SessionAs<CustomUserSession>();
            var token = session.ProviderOAuthAccess.SingleOrDefault(x => x.Provider == "facebook").AccessToken;
            var albums = ProfileManager.GetFacebookAlbums(request.Environment.AppId, session.UserAuthId, token);
            return new HttpResult(albums, HttpStatusCode.OK);
        }
        //GET: /profile/google/contacts
        // get a list of the user's facebook friends
        [CustomAuthenticate]
        public object Get(GetGoogleContacts request)
        {
            var session = SessionAs<CustomUserSession>();
            var token = session.ProviderOAuthAccess.SingleOrDefault(x => x.Provider == "GoogleOAuth");
            if (token != null)
            {
                var contacts = ProfileManager.GetGoogleContacts(request.Environment.AppId, session.UserAuthId, token.AccessToken);
                return new HttpResult(contacts, HttpStatusCode.OK);
            }
            return new HttpResult(null, HttpStatusCode.OK);

        }

        [AddHeader(ContentType = "image/jpeg")]
        [AddHeader(CacheControl = "max-age: 315360000")]
        public object Get(GetThumbnail request)
        {
            return new HttpResult(ThumbnailManager.CreateThumbnail(request.ImageKey, request.Width, request.Height), "image/jpeg");
        }

        [CustomAuthenticate]
        public object Get(GetListingTranslation request)
        {
            return new HttpResult(ListingManager.GetTranslation(request.Environment.AppId, request.ListingId, request.CultureCode), HttpStatusCode.OK);
        }

        [CustomAuthenticate]
        public object Post(SetListingTranslation request)
        {
            try
            {
                var session = SessionAs<CustomUserSession>();
                ListingManager.SecurityContext = session.ToSecurityContext();
                ListingManager.SetTranslation(
                    request.Environment.AppId,
                    request.ListingId,
                    new ListingTranslation { Culture = request.CultureCode, Metadata = new Dictionary<string, string>(), Title = request.Title, Content = request.Content });

                return new HttpResult(new { ObjectId = request.ListingId, Culture = request.CultureCode }, HttpStatusCode.OK);
            }
            catch (KeyNotFoundException kex)
            {
                return new HttpError(HttpStatusCode.NotFound, kex.Message);
            }
        }

        [CustomAuthenticate]
        public object Delete(DeleteListingTranslation request)
        {
            try
            {
                var session = SessionAs<CustomUserSession>();
                ListingManager.SecurityContext = session.ToSecurityContext();
                ListingManager.DeleteTranslation(
                    request.Environment.AppId,
                    request.ListingId,
                    request.CultureCode);

                return new HttpResult(new { ObjectId = request.ListingId, Culture = request.CultureCode }, HttpStatusCode.OK);
            }
            catch (KeyNotFoundException kex)
            {
                return new HttpError(HttpStatusCode.NotFound, kex.Message);
            }
        }

        [CustomAuthenticate]
        public object Post(SendEmailRequest request)
        {
            try
            {
                var session = SessionAs<CustomUserSession>();
                EmailResult result = EmailManager.SendHtmlMessage(
                    AppManager.GetAppById(request.Environment.AppId).MandrilAPIKey,
                    request.ReplyTo, request.To, request.Subject, request.Body, request.Template, request.Variables);

                if (result.Status == EmailResultStatus.Failed)
                {
                    return new HttpError(HttpStatusCode.NotFound, result.Reason);
                }
                return new HttpResult(new { }, HttpStatusCode.OK);
            }
            catch (KeyNotFoundException kex)
            {
                return new HttpError(HttpStatusCode.NotFound, kex.Message);
            }
        }

        public object Post(ForgotPasswordRequest request)
        {
            IUserAuthRepository authRepo = ResolveService<IUserAuthRepository>();
            UserAuth userAuth = authRepo.GetUserAuthByUserName(request.Environment.AppId, request.Email);

            if (userAuth != null)
            {
                // create hash
                if (userAuth.Meta == null)
                {
                    userAuth.Meta = new Dictionary<string, string>();
                }

                MD5 md5 = System.Security.Cryptography.MD5.Create();
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(Guid.NewGuid().ToString());
                byte[] hash = md5.ComputeHash(inputBytes);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hash.Length; i++)
                {
                    sb.Append(hash[i].ToString("X2"));
                }
                userAuth.Meta["ResetPasswordHash"] = sb.ToString();
                authRepo.SaveUserAuth(userAuth);

                // Send Email
                string subject = "ForgotPassword_ResetEmailSubject";
                string body = "ForgotPassword_ResetEmailBody";
                var subjectRes = LocalizationManager.GetResourceByKey(request.Environment.AppId, "ForgotPassword_ResetEmailSubject", true);
                var bodyRes = LocalizationManager.GetResourceByKey(request.Environment.AppId, "ForgotPassword_ResetEmailBody", true);
                EmailManager.SendHtmlMessage(
                    AppManager.GetAppById(request.Environment.AppId).MandrilAPIKey,
                    null, new string[] { userAuth.Email },
                    subjectRes == null ? subject : subjectRes.Values[request.Environment.CultureCode],
                    bodyRes == null ? body : string.Format(bodyRes.Values[request.Environment.CultureCode], string.Format("http://{0}/reset/{1}", AppManager.GetAppById(request.Environment.AppId).Hostname, sb.ToString())),
                    "reset_password_template",
                    null
                    );
                return new HttpResult(new { }, HttpStatusCode.OK);

            }
            return new HttpError("Email not found");
        }

        public object Get(VerifyPasswordResetRequest request)
        {
            IUserAuthRepository authRepo = ResolveService<IUserAuthRepository>();
            UserAuth userAuth = authRepo.GetUserAuthByResetHash(request.Environment.AppId, request.Hash);

            if (userAuth != null)
            {
                return new HttpResult(new { }, HttpStatusCode.OK);
            }
            return new HttpError("Invalid hash");
        }

        public object Post(PasswordResetRequest request)
        {
            IUserAuthRepository authRepo = ResolveService<IUserAuthRepository>();
            UserAuth userAuth = authRepo.GetUserAuthByResetHash(request.Environment.AppId, request.Hash);

            if (userAuth != null)
            {
                authRepo.ResetUserPassword(userAuth, request.Password);

                return new HttpResult(new { }, HttpStatusCode.OK);
            }
            return new HttpError("Invalid hash");
        }

        public object Get(GetCitiesByCountry request)
        {
            return LocalizationManager.GetCitiesByCountry(request.Environment.AppId, request.CountryCode);
        }

        public object Post(GetListingMoreInfo request)
        {
            return ListingManager.GetListingMoreInfo(request.Environment.AppId, request.ListingId, request.Metadata, null, request.Environment.CultureCode);
        }

        public object Get(VerifyEmailRequest request)
        {
            VerifyEmailResponse response = null;
            try
            {
                response = ProfileManager.VerifyEmailByHash(request.Environment.AppId, request.Hash);
            }
            catch (Exception ex)
            {
                response = new VerifyEmailResponse { Verified = false, ErrorMessage = ex.Message };
            }

            return new HttpResult(response, HttpStatusCode.OK);
        }
    }
}