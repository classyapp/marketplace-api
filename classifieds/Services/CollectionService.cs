using System.Collections.Generic;
using System.Net;
using Classy.Auth;
using classy.Manager;
using Classy.Models;
using Classy.Models.Request;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;

namespace classy.Services
{
    public class CollectionService : Service
    {
        public ICollectionManager CollectionManager { get; set; }

        //
        // POST: /collection/new
        // create a new collection
        [CustomAuthenticate]
        public object Post(CreateCollection request)
        {
            try
            {
                var session = SessionAs<CustomUserSession>();
                CollectionManager.SecurityContext = session.ToSecurityContext();

                var collection = CollectionManager.CreateCollection(
                    request.Environment.AppId,
                    request.ProfileId,
                    request.Type,
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
                CollectionManager.SecurityContext = session.ToSecurityContext();

                var collection = CollectionManager.AddListingsToCollection(
                    request.Environment.AppId,
                    request.CollectionId,
                    request.IncludedListings,
                    request.Environment.CultureCode);
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
                CollectionManager.SecurityContext = session.ToSecurityContext();
                var collection = CollectionManager.UpdateCollection(
                    request.Environment.AppId,
                    request.CollectionId,
                    request.Title,
                    request.Content,
                    request.IncludedListings,
                    request.Environment.CultureCode);
                return new HttpResult(collection, HttpStatusCode.OK);
            }
            catch (KeyNotFoundException kex)
            {
                return new HttpError(HttpStatusCode.NotFound, kex.Message);
            }
        }

        //
        // DELETE: /collection/{CollectionId}
        // delete collection
        [CustomAuthenticate]
        public object Delete(DeleteCollection request)
        {
            try
            {
                var session = SessionAs<CustomUserSession>();
                CollectionManager.SecurityContext = session.ToSecurityContext();
                CollectionManager.DeleteCollection(
                    request.Environment.AppId,
                    request.CollectionId);
                return new HttpResult(true, HttpStatusCode.OK);
            }
            catch (KeyNotFoundException kex)
            {
                return new HttpError(HttpStatusCode.NotFound, kex.Message);
            }
        }

        //
        // POST: /collection/{CollectionId}/cover
        // set collection cover photos
        public object Post(SetCollectionCoverPhotos request)
        {
            try
            {
                var session = SessionAs<CustomUserSession>();
                CollectionManager.SecurityContext = session.ToSecurityContext();

                var collection = CollectionManager.UpdateCollectionCover(
                    request.Environment.AppId,
                    request.CollectionId,
                    request.Keys,
                    request.Environment.CultureCode);
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
                request.MaxCollections,
                request.Environment.CultureCode);
            return new HttpResult(collections, HttpStatusCode.OK);
        }

        //
        // GET: /collection/{CollectionId}
        // get collection by id
        public object Get(GetCollectionById request)
        {
            try
            {
                var session = SessionAs<CustomUserSession>();
                CollectionManager.SecurityContext = session.ToSecurityContext();

                var collection = CollectionManager.GetCollectionById(
                    request.Environment.AppId,
                    request.CollectionId,
                    request.IncludeProfile,
                    request.IncludeListings,
                    request.IncludeDrafts,
                    request.IncreaseViewCounter,
                    request.IncreaseViewCounterOnListings,
                    request.IncludeComments,
                    request.FormatCommentsAsHtml,
                    request.IncludeCommenterProfiles,
                    request.Environment.CultureCode);
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
                    request.ProfileId,
                    request.CollectionType,
                    request.Environment.CultureCode);
                return new HttpResult(collection, HttpStatusCode.OK);
            }
            catch (KeyNotFoundException kex)
            {
                return new HttpError(HttpStatusCode.NotFound, kex.Message);
            }
        }

        [CustomAuthenticate]
        public object Get(GetCollectionTranslation request)
        {
            return new HttpResult(CollectionManager.GetCollectionTranslation(request.Environment.AppId, request.CollectionId, request.CultureCode), HttpStatusCode.OK);
        }

        [CustomAuthenticate]
        public object Post(SetCollectionTranslation request)
        {
            try
            {
                var session = SessionAs<CustomUserSession>();
                CollectionManager.SecurityContext = session.ToSecurityContext();
                CollectionManager.SetCollectionTranslation(
                    request.Environment.AppId,
                    request.CollectionId,
                    new CollectionTranslation { Culture = request.CultureCode, Metadata = new Dictionary<string, string>(), Title = request.Title, Content = request.Content });

                return new HttpResult(new { ObjectId = request.CollectionId, Culture = request.CultureCode }, HttpStatusCode.OK);
            }
            catch (KeyNotFoundException kex)
            {
                return new HttpError(HttpStatusCode.NotFound, kex.Message);
            }
        }

        [CustomAuthenticate]
        public object Delete(DeleteCollectionTranslation request)
        {
            try
            {
                var session = SessionAs<CustomUserSession>();
                CollectionManager.SecurityContext = session.ToSecurityContext();
                CollectionManager.DeleteCollectionTranslation(
                    request.Environment.AppId,
                    request.CollectionId,
                    request.CultureCode);

                return new HttpResult(new { ObjectId = request.CollectionId, Culture = request.CultureCode }, HttpStatusCode.OK);
            }
            catch (KeyNotFoundException kex)
            {
                return new HttpError(HttpStatusCode.NotFound, kex.Message);
            }
        }
    }
}