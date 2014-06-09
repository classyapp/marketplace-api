using classy.DTO.Request.Images;
using classy.Manager;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;

namespace classy.Services
{
    public class ImageService : Service
    {
        public IThumbnailManager ThumbnailManager { get; set; }

        [AddHeader(ContentType = "image/jpeg")]
        [AddHeader(CacheControl = "max-age: 315360000")]
        public object Get(GetThumbnail request)
        {
            return new HttpResult(ThumbnailManager.CreateThumbnail(request.ImageKey, request.Width, request.Height), "image/jpeg");
        }

        [AddHeader(ContentType = "image/jpeg")]
        [AddHeader(CacheControl = "max-age: 315360000")]
        public object Get(GetCollageRequest request)
        {
            var responseBytes = ThumbnailManager.CreateCollage(request.ImageKeys);
            return new HttpResult(responseBytes, "image/jpeg");
        }
    }
}