using classy.Manager;
using Classy.Models;
using Classy.Repository;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;

namespace classy.Operations
{
    public class CreateThumbnailsOperator : IOperator<CreateThumbnailsRequest>
    {
        private readonly IStorageRepository _storageRepository;
        private readonly IListingRepository _listingRepository;
        private readonly IAppManager _appManager;

        public CreateThumbnailsOperator(IStorageRepository storageRepo, IListingRepository listingRepo, IAppManager appManager)
        {
            _storageRepository = storageRepo;
            _listingRepository = listingRepo;
            _appManager = appManager;
        }

        public void PerformOperation(CreateThumbnailsRequest request)
        {
            // TODO: grab media file by key instead of off the listing?
            var app = _appManager.GetAppById(request.AppId);
            var listing = _listingRepository.GetById(request.ListingId, request.AppId, true);
            var listingMediaFile = listing.ExternalMedia.Single(x => x.Key == request.MediaKey);

            int height = 0, width = 0;
            Bitmap squareImage;
            // crop to square
            using (Stream originalStream = _storageRepository.GetFile(request.MediaKey))
            {
                using (Image originalImg = Image.FromStream(originalStream))
                {
                    int minDimension;
                    int startX = 0;
                    int startY = 0;
                    if (originalImg.Height < originalImg.Width)
                    {
                        minDimension = originalImg.Height;
                        startX = (originalImg.Width - originalImg.Height) / 2;
                    }
                    else
                    {
                        minDimension = originalImg.Width;
                        startY = (originalImg.Height - originalImg.Width) / 2;
                    }

                    using (Bitmap bmp = new Bitmap(originalImg))
                    {
                        squareImage = bmp.Clone(new Rectangle(startX, startY, minDimension, minDimension), System.Drawing.Imaging.PixelFormat.DontCare);
                    }
                }
            }

            var sizes = app.ExternalMediaThumbnailSizes;

            foreach (var s in sizes)
            {
                using (var thumbnail = squareImage.GetThumbnailImage(s.X, s.Y, () => false, System.IntPtr.Zero))
                {
                    using (MemoryStream memoryStream = new MemoryStream(thumbnail.Size.Height * thumbnail.Size.Width))
                    {
                        thumbnail.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);

                        var thumbnailKey = string.Format("{0}_{1}x{2}", request.MediaKey, s.X, s.Y);
                        _storageRepository.SaveFile(thumbnailKey, memoryStream.GetBuffer(), "image/png");
                        MediaThumbnail mediaThumbnail = new MediaThumbnail()
                        {
                            Width = s.X,
                            Height = s.Y,
                            Key = thumbnailKey,
                            Url = _storageRepository.KeyToUrl(thumbnailKey)
                        };

                        listingMediaFile.Thumbnails.Add(mediaThumbnail);
                    }
                }
            }
            //TODO: any way to only update the thumbnails? i see a potential race condition here otherwise
            _listingRepository.UpdateExternalMedia(request.ListingId, request.AppId, listingMediaFile);
        }
    }
}