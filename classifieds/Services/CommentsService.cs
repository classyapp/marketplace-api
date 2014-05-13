using System.Collections.Generic;
using System.Net;
using Classy.Auth;
using classy.Manager;
using Classy.Models.Request;
using Classy.Models.Response;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;

namespace classy.Services
{
    public class CommentsService : Service
    {
        public IListingManager ListingManager { get; set; }
        public IProfileManager ProfileManager { get; set; }
        public ICollectionManager CollectionManager { get; set; }

        // add comment to post
        [CustomAuthenticate]
        public object Post(PostCommentForListing request)
        {
            try
            {
                var session = SessionAs<CustomUserSession>();
                CommentView comment = null;

                ListingManager.SecurityContext = session.ToSecurityContext();
                comment = ListingManager.AddCommentToListing(
                    request.Environment.AppId,
                    request.ListingId,
                    request.Content,
                    request.FormatAsHtml);

                comment.Profile = ProfileManager.GetProfileById(request.Environment.AppId, comment.ProfileId, null, false, false, false, false, false, false, false, request.Environment.CultureCode);

                return new HttpResult(comment, HttpStatusCode.OK);
            }
            catch (KeyNotFoundException kex)
            {
                return new HttpError(HttpStatusCode.NotFound, kex.Message);
            }
        }

        // add comment to post
        [CustomAuthenticate]
        public object Post(PostCommentForCollection request)
        {
            try
            {
                var session = SessionAs<CustomUserSession>();
                CommentView comment = null;

                CollectionManager.SecurityContext = session.ToSecurityContext();
                comment = CollectionManager.AddCommentToCollection(
                    request.Environment.AppId,
                    request.CollectionId,
                    request.Content,
                    request.FormatAsHtml);

                return new HttpResult(comment, HttpStatusCode.OK);
            }
            catch (KeyNotFoundException kex)
            {
                return new HttpError(HttpStatusCode.NotFound, kex.Message);
            }
        }
    }
}