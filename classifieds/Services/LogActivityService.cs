using System.Net;
using classy.DTO.Request.LogActivity;
using Classy.Repository;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;

namespace classy.Services
{
    public class LogActivityService : Service
    {
        public ITripleStore TripleStore { get; set; }

        public object Post(LogActivityRequest request)
        {
            int count = 0;
            var triple = TripleStore.LogActivity(request.Environment.AppId, request.SubjectId, request.Predicate, request.ObjectId, request.Metadata, ref count);

            return new HttpResult(triple, HttpStatusCode.OK);
        }

        public object Get(GetLogActivityRequest request)
        {
            var activity = TripleStore.GetLogActivity(request.Environment.AppId, request.SubjectId, request.Predicate,
                request.ObjectId);
            return new HttpResult(activity, HttpStatusCode.OK);
        }
    }
}