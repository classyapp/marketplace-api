using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using classy.Manager;
using Classy.Models.Request;
using Classy.Models.Response;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;

namespace classy.Services
{
    public class MediaService : Service
    {
        public ITempMediaFileManager TempMediaManager { get; set; }
        public IListingManager ListingManager { get; set; }

        public object Post(SaveTempMediaRequest request)
        {
            if (Request.Files.Length != 1)
            {
                return new HttpResult
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Response = new { Error = "Please send exactly one file." }
                };
            }

            string fileId = TempMediaManager.StoreTempFile(request.Environment.AppId, Request.Files[0]);

            return new HttpResult
            {
                StatusCode = HttpStatusCode.OK,
                Response = new SaveTempMediaResponse { FileId = fileId }
            };
        }

        public object Delete(DeleteTempMediaRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ListingId))
            {
            TempMediaManager.DeleteTempFile(request.Environment.AppId, request.FileId);
            }
            else
            {
                ListingManager.DeleteExternalMediaFromListingById(request.Environment.AppId, request.ListingId, request.FileId);
            }

            return new HttpResult
            {
                StatusCode = HttpStatusCode.OK,
                Response = new { FileId = request.FileId }
            };
        }
    }
}