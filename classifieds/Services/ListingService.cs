using ServiceStack.Common;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceStack.Common.Web;
using System.Net;
using ServiceStack.ServiceInterface.Validation;
using ServiceStack.FluentValidation;
using Classy.Models;
using Classy.Models.Response;
using Classy.Models.Request;
using Classy.Auth;
using System.IO;
using classy.Manager;
using Classy.Repository;

namespace classy.Services
{
    public class ListingService : ServiceStack.ServiceInterface.Service
    {
        public IBookingManager BookingManager { get; set; }
        public IOrderManager OrderManager { get; set; }
        public IListingManager ListingManager { get; set; }
        public IProfileManager ProfileManager { get; set; }
        public IReviewManager ReviewManager { get; set; }
        public IAnalyticsManager AnalyticsManager { get; set; }

        [CustomAuthenticate]
        public object Post(CreateProfileProxy request)
        {
            ProfileView profile = null;

            string[] content;
            var f = File.OpenText(@"C:\Users\YUVAL\Downloads\metavchim.csv");
            while (!f.EndOfStream)
            {
                var line = f.ReadLine();
                content = line.Split(',');

                profile = ProfileManager.CreateProfileProxy(
                    request.AppId,
                    new Seller {
                        ContactInfo = new ContactInfo {
                            Name = content[0]
                        } 
                    },
                    new List<CustomAttribute>() { 
                        new CustomAttribute { Key = "LicenseNo", Value = content[1] }
                    });
            }

            //var profile = ProfileManager.CreateProfileProxy(
            //    request.AppId,
            //    request.SellerInfo,
            //    request.Metadata);
                
            return new HttpResult(profile, HttpStatusCode.OK);
        }

        public object Get(GetListingById request)
        {
            var session = SessionAs<CustomUserSession>();

            var listingView = ListingManager.GetListingById(
                request.AppId,
                request.ListingId,
                session.UserAuthId,
                request.LogImpression,
                false,
                request.IncludeComments,
                request.FormatCommentsAsHtml,
                request.IncludeCommenterProfiles,
                request.IncludeProfile,
                request.IncludeFavoritedByProfiles);

            return new HttpResult(listingView, HttpStatusCode.OK);
        }

        public object Get(GetListingsByUsername request)
        {
            var listingViews = ListingManager.GetListingsByUsername(
                request.AppId,
                request.Username,
                request.IncludeComments,
                request.FormatCommentsAsHtml);

            return new HttpResult(listingViews, HttpStatusCode.OK);
        }

        public object Get(SearchListings request)
        {
            return Post(request);
        }

        public object Post(SearchListings request)
        {
            var listingViews = ListingManager.SearchListings(
                request.AppId,
                request.Tag,
                request.ListingType,
                request.Metadata,
                request.PriceMin,
                request.PriceMax,
                request.Location,
                request.IncludeComments,
                request.FormatCommentsAsHtml);

            return new HttpResult(listingViews, HttpStatusCode.OK);
        }

        // create new listing
        [CustomAuthenticate]
        public object Post(PostListing request)
        {
            var session = SessionAs<CustomUserSession>();

            var listing = ListingManager.SaveListing(
                request.AppId,
                null,
                session.UserAuthId,
                request.Title,
                request.Content,
                request.ListingType,
                request.Pricing,
                request.ContactInfo,
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
            var session = SessionAs<CustomUserSession>();

            var listing = ListingManager.AddExternalMediaToListing(
                request.AppId,
                request.ListingId,
                session.UserAuthId,
                Request.Files);

            return new HttpResult(listing, HttpStatusCode.OK);
        }

        // delete media files
        [CustomAuthenticate]
        public object Delete(DeleteExternalMedia request)
        {
            var session = SessionAs<CustomUserSession>();

            var listing = ListingManager.DeleteExternalMediaFromListing(
                request.AppId, 
                request.ListingId, 
                session.UserAuthId, 
                request.Url);

            return new HttpResult(listing, HttpStatusCode.OK);
        }

        // publish listing
        [CustomAuthenticate]
        public object Post(PublishListing request)
        {
            var session = SessionAs<CustomUserSession>();

            var listing = ListingManager.PublishListing(
                request.AppId,
                request.ListingId,
                session.UserAuthId);

            return new HttpResult(listing, HttpStatusCode.OK);
        }

        // update listing
        [CustomAuthenticate]
        public object Put(PostListing request)
        {
            var session = SessionAs<CustomUserSession>();

            var listing = ListingManager.SaveListing(
                request.AppId,
                request.ListingId,
                session.UserAuthId,
                request.Title,
                request.Content,
                null,
                request.Pricing,
                request.ContactInfo,
                request.SchedulingTemplate,
                request.Metadata);
                    
            return new HttpResult(listing, HttpStatusCode.OK);
        }

        // add comment to post
        [CustomAuthenticate]
        public object Post(PostComment request)
        {
            var session = SessionAs<CustomUserSession>();

            var comment = ListingManager.AddCommentToListing(
                request.AppId,
                request.ListingId,
                session.UserAuthId,
                request.Content,
                request.FormatAsHtml);

            return new HttpResult(comment, HttpStatusCode.OK);
        }

        // favorite a listing
        [CustomAuthenticate]
        public object Post(FavoriteListing request)
        {
            var session = SessionAs<CustomUserSession>();

            ListingManager.FavoriteListing(
                request.AppId,
                request.ListingId,
                session.UserAuthId);

            return new HttpResult(HttpStatusCode.OK);
        }

        // favorite a listing
        [CustomAuthenticate]
        public object Post(FlagListing request)
        {
            var session = SessionAs<CustomUserSession>();

            ListingManager.FlagListing(
                request.AppId,
                request.ListingId,
                session.UserAuthId,
                request.FlagReason);

            return new HttpResult(HttpStatusCode.OK);
        }

        // follow a profile
        [CustomAuthenticate]
        public object Post(FollowProfile request)
        {
            var session = SessionAs<CustomUserSession>();

            ProfileManager.FollowProfile(
                request.AppId,
                session.UserAuthId,
                request.FolloweeUsername);

            return new HttpResult(HttpStatusCode.OK);
        }

        // submit proxy claim
        [CustomAuthenticate]
        public object Post(ClaimProxyProfile request)
        {
            var session = SessionAs<CustomUserSession>();

            var claim = ProfileManager.SubmitProxyClaim(
                request.AppId,
                session.UserAuthId,
                request.ProxyProfileId,
                request.SellerInfo,
                request.Metadata);

            return new HttpResult(claim, HttpStatusCode.OK);
        }

        // approve proxy claim
        [CustomAuthenticate]
        public object Post(ApproveProxyClaim request)
        {
            var claim = ProfileManager.ApproveProxyClaim(
                request.AppId,
                request.ClaimId);

            return new HttpResult(claim, HttpStatusCode.OK);
        }

        // reject proxy claim
        [CustomAuthenticate]
        public object Post(RejectProxyClaim request)
        {
            var claim = ProfileManager.RejectProxyClaim(
                request.AppId,
                request.ClaimId);

            return new HttpResult(claim, HttpStatusCode.OK);
        }

        // get profile
        public object Get(GetProfileById request)
        {
            var session = SessionAs<CustomUserSession>();

            var profile = ProfileManager.GetProfileById(
                request.AppId,
                request.ProfileId,
                session.UserAuthId,
                request.IncludeFollowedByProfiles,
                request.IncludeFollowingProfiles,
                request.IncludeReviews,
                request.IncludeListings,
                request.LogImpression);

            return new HttpResult(profile, HttpStatusCode.OK);
        }

        [CustomAuthenticate]
        public object Put(UpdateProfile request)
        {
            var session = SessionAs<CustomUserSession>();

            if (session.UserAuthId != request.ProfileId) throw new UnauthorizedAccessException("not yours to update");

            var profile = ProfileManager.UpdateProfile(
                request.AppId,
                request.ProfileId,
                request.SellerInfo,
                request.Metadata);

            return new HttpResult(profile, HttpStatusCode.OK);
        }

        // search profiles
        public object Get(SearchProfiles request)
        {
            return Post(request);
        }

        public object Post(SearchProfiles request)
        {
            var profiles = ProfileManager.SearchProfiles(
                request.AppId,
                request.DisplayName,
                request.Category,
                request.Location,
                request.Metadata);

            return new HttpResult(profiles.Take(150), HttpStatusCode.OK);
        }

        // book timelost within listing
        [CustomAuthenticate]
        public object Post(BookListing request)
        {
            var session = SessionAs<CustomUserSession>();
            var timeslot = BookingManager.BookListing(
                request.AppId, 
                request.ListingId, 
                session.UserAuthId, 
                request.PaymentMethod, 
                request.DateRange, 
                request.Comment, 
                request.MaxDoubleBookings);

            return new HttpResult(timeslot, HttpStatusCode.OK);
        }

        // get listing bookings
        [CustomAuthenticate]
        public object Get(GetBookingsForListing request)
        {
            var session = SessionAs<CustomUserSession>();
            var bookings = BookingManager.GetBookingsForListing(
                request.AppId, 
                request.ListingId, 
                session.UserAuthId,
                new DateRange { Start = request.Start, End = request.End },
                request.InculdeCancelled);

            return new HttpResult(bookings, HttpStatusCode.OK);
        }

        public object Get(GetBookingsForProfileListings request)
        {
            var session = SessionAs<CustomUserSession>();
            if (!session.IsAuthenticated) throw new UnauthorizedAccessException("you must be logged in");
            var bookings = BookingManager.GetBookingsForProfileListings(
                request.AppId,
                session.UserAuthId,
                new DateRange { Start = request.Start, End = request.End },
                request.InculdeCancelled);

            return new HttpResult(bookings, HttpStatusCode.OK);
        }

        public object Get(GetBookingsForProfile request)
        {
            var session = SessionAs<CustomUserSession>();
            if (!session.IsAuthenticated) throw new UnauthorizedAccessException("you must be logged in");
            var bookings = BookingManager.GetBookingsForProfile(
                request.AppId, 
                session.UserAuthId, 
                new DateRange { Start = request.Start, End = request.End },
                request.InculdeCancelled);

            return new HttpResult(bookings, HttpStatusCode.OK);
        }

        // update booking timeslot
        public object Put(UpdateBooking request)
        {
            var session = SessionAs<CustomUserSession>();

            var booking = BookingManager.UpdateBooking(
                request.AppId,
                request.BookingId,
                session.UserAuthId,
                request.DateRange,
                request.Comment,
                request.MaxDoubleBookings);

            return new HttpResult(booking, HttpStatusCode.OK);
        }

        // cancel a booking
        [CustomAuthenticate]
        public object Delete(CancelBooking request)
        {
            var session = SessionAs<CustomUserSession>();
            var response = BookingManager.CancelBooking(
                request.AppId,
                request.BookingId,
                session.UserAuthId,
                request.DoRefund);
            return new HttpResult(response, HttpStatusCode.OK);
        }

        // purchase item offered in a listing
        [CustomAuthenticate]
        public object Post(PurchaseSingleItem request)
        {
            var session = SessionAs<CustomUserSession>();

            var order = OrderManager.PlaceSingleItemOrder(
                request.AppId, 
                request.ListingId,
                session.UserAuthId,
                request.PaymentMethod,
                request.ShippingAddress,
                request.SKU,
                request.Quantity);

            return new HttpResult(order, HttpStatusCode.OK);
        }

        // update quantity on single item order
        public object Put(UpdateSingleItemOrder request)
        {
            var session = SessionAs<CustomUserSession>();
            var order = OrderManager.UpdateSingleItemOrder(
                request.AppId,
                request.OrderId,
                session.UserAuthId,
                request.SKU,
                request.Quantity);

            return new HttpResult(order, HttpStatusCode.OK);
        }

        // cancel a single item order 
        [CustomAuthenticate]
        public object Delete(CancelSingleItemOrder request)
        {
            var session = SessionAs<CustomUserSession>();
            var response = OrderManager.CancelSingleItemOrder(
                request.AppId,
                request.OrderId,
                session.UserAuthId,
                request.DoRefund);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        // get orders for listing
        [CustomAuthenticate]
        public object Get(GetOrdersForListing request)
        {
            var session = SessionAs<CustomUserSession>();
            var orders = OrderManager.GetOrdersForListing(
                request.AppId,
                request.ListingId,
                session.UserAuthId,
                request.IncludeCancelled);

            return new HttpResult(orders, HttpStatusCode.OK);
        }

        // get orders for profile
        [CustomAuthenticate]
        public object Get(GetOrdersForProfile request)
        {
            var session = SessionAs<CustomUserSession>();
            if (!session.IsAuthenticated) throw new UnauthorizedAccessException("you must be logged in");
            var orders = OrderManager.GetOrdersForProfile(
                request.AppId,
                session.UserAuthId,
                request.IncludeCancelled);

            return new HttpResult(orders, HttpStatusCode.OK);
        }

        // get orders for all of the profile's listings
        [CustomAuthenticate]
        public object Get(GetOrdersForProfileListings request)
        {
            var session = SessionAs<CustomUserSession>();
            if (!session.IsAuthenticated) throw new UnauthorizedAccessException("you must be logged in");
            var orders = OrderManager.GetOrdersForProfileListings(
                request.AppId,
                session.UserAuthId,
                request.IncludeCancelled);

            return new HttpResult(orders, HttpStatusCode.OK);
        }

        // post a review for listing
        [CustomAuthenticate]
        public object Post(PostReviewForListing request)
        {
            var session = SessionAs<CustomUserSession>();
            var review = ReviewManager.PostReviewForListing(
                request.AppId,
                session.UserAuthId,
                request.ListingId,
                request.Content,
                request.Score,
                request.SubCriteria);
            return new HttpResult(review, HttpStatusCode.OK);
        }

        // post a review for profile
        [CustomAuthenticate]
        public object Post(PostReviewForProfile request)
        {
            var session = SessionAs<CustomUserSession>();
            var review = ReviewManager.PostReviewForProfile(
                request.AppId,
                session.UserAuthId,
                request.RevieweeProfileId,
                request.Content,
                request.Score,
                request.SubCriteria,
                request.ContactInfo,
                request.Metadata);
            var response = new PostReviewResponse
            {
                Review = review.TranslateTo<ReviewView>()
            };
            if (request.ReturnRevieweeProfile)
                response.RevieweeProfile = ProfileManager.GetProfileById(
                    request.AppId,
                    request.RevieweeProfileId,
                    null,
                    false,
                    false,
                    false,
                    false,
                    false);
            if (request.ReturnReviewerProfile)
                response.ReviewerProfile = ProfileManager.GetProfileById(
                    request.AppId,
                    session.UserAuthId,
                    null,
                    false,
                    false,
                    false,
                    false,
                    false);
            return new HttpResult(response, HttpStatusCode.OK);
        }

        // get a list of reviews for profile
        [CustomAuthenticate]
        public object Get(GetReviewsByProfileId request)
        {
            var reviews = ReviewManager.GetReviews(
                request.AppId,
                request.ProfileId,
                request.IncludeDrafts,
                request.IncludeOnlyDrafts);

            return new HttpResult(reviews, HttpStatusCode.OK);
        }

        // publish a review
        [CustomAuthenticate]
        public object Post(PublishOrDeleteReview request)
        {
            var session = SessionAs<CustomUserSession>();
            var review = ReviewManager.PublishReview(
                request.AppId, 
                request.ReviewId,
                session.UserAuthId);

            return new HttpResult(review, HttpStatusCode.OK);
        }
    
        // delete a review
        public object Delete(PublishOrDeleteReview request)
        {
            var session = SessionAs<CustomUserSession>();
            var review = ReviewManager.DeleteReview(
                request.AppId,
                request.ReviewId,
                session.UserAuthId);

            return new HttpResult(review, HttpStatusCode.OK);
        }

        // 
        // POST: /stats/push
        // log an activity 
        public object Post(LogActivity request)
        {
            var session = SessionAs<CustomUserSession>();
            if (!session.IsAuthenticated && request.SubjectId != "guest") throw new ApplicationException("when no user logged in, SubjectId must be 'guest'");
            var tripleView = AnalyticsManager.LogActivity(request.AppId, request.SubjectId, Classy.Models.ActivityPredicate.ProContact, request.ObjectId);
            return new HttpResult(tripleView, HttpStatusCode.OK);
        }
    }
}