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
        public ICollectionManager CollectionManager { get; set; }
        public IAnalyticsManager AnalyticsManager { get; set; }
        public ILocalizationManager LocalizationManager { get; set; }
        public IAppManager AppManager { get; set; }

        [CustomAuthenticate]
        public object Post(CreateProfileProxy request)
        {
            var session = SessionAs<CustomUserSession>();
            var profile = ProfileManager.CreateProfileProxy(
                request.Environment.AppId,
                session.UserAuthId,
                request.BatchId,
                request.ProfessionalInfo,
                request.Metadata);

            return new HttpResult(profile, HttpStatusCode.OK);
        }

        public object Get(GetAppSettings request)
        {
            App app = AppManager.GetAppById(request.Environment.AppId);
            return new HttpResult(new AppView { PageSize = app.PagingPageSize, PagesCount = app.PagingPages }, HttpStatusCode.OK);
        }

        public object Get(GetListingById request)
        {
            try
            {
                var session = SessionAs<CustomUserSession>();

                var listingView = ListingManager.GetListingById(
                    request.Environment.AppId,
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
                    request.IncludeDrafts);

                return new HttpResult(listingViews, HttpStatusCode.OK);
            }
            catch (KeyNotFoundException kex)
            {
                return new HttpError(HttpStatusCode.NotFound, kex.Message);
            }
        }

        public object Get(SearchListings request)
        {
            return Post(request);
        }

        public object Post(SearchListings request)
        {
            var listingViews = ListingManager.SearchListings(
                request.Environment.AppId,
                request.Tag,
                request.ListingType,
                request.Metadata,
                request.PriceMin,
                request.PriceMax,
                request.Location,
                request.IncludeComments,
                request.FormatCommentsAsHtml,
                request.Page,
                AppManager.GetAppById(request.Environment.AppId).PagingPageSize);

            return new HttpResult(listingViews, HttpStatusCode.OK);
        }

        // create new listing
        [CustomAuthenticate]
        public object Post(PostListing request)
        {
            var session = SessionAs<CustomUserSession>();

            var listing = ListingManager.SaveListing(
                request.Environment.AppId,
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
            try
            {
                var session = SessionAs<CustomUserSession>();

                var listing = ListingManager.AddExternalMediaToListing(
                    request.Environment.AppId,
                    request.ListingId,
                    session.UserAuthId,
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

                var listing = ListingManager.DeleteExternalMediaFromListing(
                    request.Environment.AppId,
                    request.ListingId,
                    session.UserAuthId,
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
        public object Put(PostListing request)
        {
            try
            {
                var session = SessionAs<CustomUserSession>();

                var listing = ListingManager.SaveListing(
                    request.Environment.AppId,
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

                var listing = ListingManager.DeleteListing(
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

        // add comment to post
        [CustomAuthenticate]
        public object Post(PostComment request)
        {
            try
            {
                var session = SessionAs<CustomUserSession>();

                var comment = ListingManager.AddCommentToListing(
                    request.Environment.AppId,
                    request.ListingId,
                    session.UserAuthId,
                    request.Content,
                    request.FormatAsHtml);

                return new HttpResult(comment, HttpStatusCode.OK);
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

                ListingManager.FavoriteListing(
                    request.Environment.AppId,
                    request.ListingId,
                    session.UserAuthId);

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

                ListingManager.UnfavoriteListing(
                    request.Environment.AppId,
                    request.ListingId,
                    session.UserAuthId);

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

                ListingManager.FlagListing(
                    request.Environment.AppId,
                    request.ListingId,
                    session.UserAuthId,
                    request.FlagReason);

                return new HttpResult(HttpStatusCode.OK);
            }
            catch (KeyNotFoundException kex)
            {
                return new HttpError(HttpStatusCode.NotFound, kex.Message);
            }
        }

        // follow a profile
        [CustomAuthenticate]
        public object Post(FollowProfile request)
        {
            try
            {
                var session = SessionAs<CustomUserSession>();

                ProfileManager.FollowProfile(
                    request.Environment.AppId,
                    session.UserAuthId,
                    request.FolloweeProfileId);

                return new HttpResult(HttpStatusCode.OK);
            }
            catch (KeyNotFoundException kex)
            {
                return new HttpError(HttpStatusCode.NotFound, kex.Message);
            }
        }

        // submit proxy claim
        [CustomAuthenticate]
        public object Post(ClaimProxyProfile request)
        {
            try
            {
                var session = SessionAs<CustomUserSession>();

                var claim = ProfileManager.SubmitProxyClaim(
                    request.Environment.AppId,
                    session.UserAuthId,
                    request.ProxyProfileId,
                    request.ProfessionalInfo,
                    request.Metadata);

                return new HttpResult(claim, HttpStatusCode.OK);
            }
            catch (KeyNotFoundException kex)
            {
                return new HttpError(HttpStatusCode.NotFound, kex.Message);
            }
        }

        // approve proxy claim
        [CustomAuthenticate]
        public object Post(ApproveProxyClaim request)
        {
            try
            {
                var claim = ProfileManager.ApproveProxyClaim(
                    request.Environment.AppId,
                    request.ClaimId);

                return new HttpResult(claim, HttpStatusCode.OK);
            }
            catch (KeyNotFoundException kex)
            {
                return new HttpError(HttpStatusCode.NotFound, kex.Message);
            }
        }

        // reject proxy claim
        [CustomAuthenticate]
        public object Post(RejectProxyClaim request)
        {
            try
            {
                var claim = ProfileManager.RejectProxyClaim(
                    request.Environment.AppId,
                    request.ClaimId);

                return new HttpResult(claim, HttpStatusCode.OK);
            }
            catch (KeyNotFoundException kex)
            {
                return new HttpError(HttpStatusCode.NotFound, kex.Message);
            }
        }

        // get profile form session
        [CustomAuthenticate]
        public object Get(GetAutenticatedProfile request)
        {
            var session = SessionAs<CustomUserSession>();
            if (!session.IsAuthenticated) return new HttpError(HttpStatusCode.Unauthorized, "No Session");
            else
            {
                var profile = ProfileManager.GetProfileById(
                    request.Environment.AppId,
                    session.UserAuthId,
                    session.UserAuthId,
                    true,
                    true,
                    false,
                    false,
                    false,
                    true, 
                    false);
                return new HttpResult(profile);
            }
        }

        // get profile
        public object Get(GetProfileById request)
        {
            try
            {
                var session = SessionAs<CustomUserSession>();

                var profile = ProfileManager.GetProfileById(
                    request.Environment.AppId,
                    request.ProfileId,
                    session.UserAuthId,
                    request.IncludeFollowedByProfiles,
                    request.IncludeFollowingProfiles,
                    request.IncludeReviews,
                    request.IncludeListings,
                    request.IncludeCollections,
                    request.IncludeFavorites,
                    request.LogImpression);

                return new HttpResult(profile, HttpStatusCode.OK);
            }
            catch (KeyNotFoundException kex)
            {
                return new HttpError(HttpStatusCode.NotFound, kex.Message);
            }
        }

        [CustomAuthenticate]
        public object Put(UpdateProfile request)
        {
            try
            {
                var session = SessionAs<CustomUserSession>();

                if (session.UserAuthId != request.ProfileId) throw new UnauthorizedAccessException("not yours to update");

                var profile = ProfileManager.UpdateProfile(
                    request.Environment.AppId,
                    request.ProfileId,
                    request.ContactInfo,
                    request.ProfessionalInfo,
                    request.Metadata,
                    request.Fields);

                return new HttpResult(profile, HttpStatusCode.OK);
            }
            catch (KeyNotFoundException kex)
            {
                return new HttpError(HttpStatusCode.NotFound, kex.Message);
            }
        }

        // search profiles
        public object Get(SearchProfiles request)
        {
            return Post(request);
        }

        public object Post(SearchProfiles request)
        {
            var profiles = ProfileManager.SearchProfiles(
                request.Environment.AppId,
                request.DisplayName,
                request.Category,
                request.Location,
                request.Metadata,
                request.ProfessionalsOnly,
                request.Page,
                AppManager.GetAppById(request.Environment.AppId).PagingPageSize);

            return new HttpResult(profiles, HttpStatusCode.OK);
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
                    false);
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
                    false);
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
        // POST: /collection/new
        // create a new collection
        [CustomAuthenticate]
        public object Post(CreateCollection request)
        {
            try
            {
                var session = SessionAs<CustomUserSession>();
                var collection = CollectionManager.CreateCollection(
                    request.Environment.AppId,
                    session.UserAuthId,
                    request.Title,
                    request.Content,
                    request.IsPublic,
                    request.IncludedListings,
                    request.Collaborators,
                    request.PermittedViewers);
                return new HttpResult(collection, HttpStatusCode.OK);
            }
            catch (KeyNotFoundException kex)
            {
                return new HttpError(HttpStatusCode.NotFound, kex.Message);
            }
        }

        //
        // POST: /collection/{CollectionId}/listings
        // add listings to a collection
        [CustomAuthenticate]
        public object Post(AddListingsToCollection request)
        {
            try
            {
                var session = SessionAs<CustomUserSession>();
                var collection = CollectionManager.AddListingsToCollection(
                    request.Environment.AppId,
                    session.UserAuthId,
                    request.CollectionId,
                    request.IncludedListings);
                return new HttpResult(collection, HttpStatusCode.OK);
            }
            catch (KeyNotFoundException kex)
            {
                return new HttpError(HttpStatusCode.NotFound, kex.Message);
            }
        }

        //
        // POST: /collection/{CollectionId}/listings
        // add listings to a collection
        [CustomAuthenticate]
        public object Post(RemoveListingFromCollection request)
        {
            try
            {
                var session = SessionAs<CustomUserSession>();
                CollectionManager.RemoveListingsFromCollection(
                    request.Environment.AppId,
                    session.UserAuthId,
                    request.CollectionId,
                    request.ListingIds);
                return new HttpResult(request, HttpStatusCode.OK);
            }
            catch (KeyNotFoundException kex)
            {
                return new HttpError(HttpStatusCode.NotFound, kex.Message);
            }
        }

        //
        // PUT: /collection/{CollectionId}
        // update collection
        [CustomAuthenticate]
        public object Put(UpdateCollection request)
        {
            try
            {
                var session = SessionAs<CustomUserSession>();
                var collection = CollectionManager.UpdateCollection(
                    request.Environment.AppId,
                    session.UserAuthId,
                    request.CollectionId,
                    request.Title,
                    request.Content,
                    request.IncludedListings);
                return new HttpResult(collection, HttpStatusCode.OK);
            }
            catch (KeyNotFoundException kex)
            {
                return new HttpError(HttpStatusCode.NotFound, kex.Message);
            }
        }

        //
        // POST: /collection/{CollectionId}/submit
        // add listings to a collection
        [CustomAuthenticate]
        public object Post(SubmitCollectionForEditorialApproval request)
        {
            try
            {
                var collection = CollectionManager.SubmitCollectionForEditorialApproval(
                    request.Environment.AppId,
                    request.CollectionId);
                return new HttpResult(collection, HttpStatusCode.OK);
            }
            catch (KeyNotFoundException kex)
            {
                return new HttpError(HttpStatusCode.NotFound, kex.Message);
            }
        }

        //
        // GET: /collection/list/approved
        // get a list of approved collections
        public object Get(GetApprovedCollections request)
        {
            var collections = CollectionManager.GetApprovedCollections(
                request.Environment.AppId,
                request.Categories,
                request.MaxCollections);
            return new HttpResult(collections , HttpStatusCode.OK);
        }

        //
        // GET: /collection/{CollectionId}
        // get collection by id
        public object Get(GetCollectionById request)
        {
            try
            {
                var session = SessionAs<CustomUserSession>();
                var collection = CollectionManager.GetCollectionById(
                    request.Environment.AppId,
                    request.CollectionId,
                    session.UserAuthId,
                    request.IncludeProfile,
                    request.IncludeDrafts,
                    request.IncludeListings,
                    request.IncreaseViewCounter,
                    request.IncreaseViewCounterOnListings);
                return new HttpResult(collection, HttpStatusCode.OK);
            }
            catch (KeyNotFoundException kex)
            {
                return new HttpError(HttpStatusCode.NotFound, kex.Message);
            }
        }

        //
        // GET: /profile/{ProfileId}/collection/list
        // get collections by profile id
        public object Get(GetCollectionByProfileId request)
        {
            try
            {
                var collection = CollectionManager.GetCollectionsByProfileId(
                    request.Environment.AppId,
                    request.ProfileId);
                return new HttpResult(collection, HttpStatusCode.OK);
            }
            catch (KeyNotFoundException kex)
            {
                return new HttpError(HttpStatusCode.NotFound, kex.Message);
            }
        }

        //
        // GET: /resource/{Key}
        // get resource by key
        public object Get(GetResourceByKey request)
        {
            var resource = LocalizationManager.GetResourceByKey(request.Environment.AppId, request.Key);
            return new HttpResult(resource, HttpStatusCode.OK);
        }

        //
        // GET: /resource/keys
        // get all available resource keys for an app
        public object Get(GetResourceKeysForApp request)
        {
            var resourceKeys = LocalizationManager.GetResourceKeysForApp(request.Environment.AppId);
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
        public object Post(SetResourceValues request)
        {
            var resource = LocalizationManager.SetResourceValues(request.Environment.AppId, request.Key, request.Values);
            return new HttpResult(resource, HttpStatusCode.OK);
        }

        //
        // POST: /resource/list/{Key}
        // set resource values
        public object Post(SetResourceListValues request)
        {
            var listResource = LocalizationManager.SetListResourceValues(request.Environment.AppId, request.Key, request.ListItems);
            return new HttpResult(listResource, HttpStatusCode.OK);
        }
    }
}