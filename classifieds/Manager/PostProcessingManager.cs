using Classy.Models;
using Classy.Repository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Drawing;

namespace classy.Manager
{
    public class PostProcessingManager
    {
        public static void GenerateThumbnail(string listingId, string appId, MediaFile originalFile, ImageSize size, IStorageRepository storageRepo, IListingRepository listingRepo)
        {
            int height = 0, width = 0;
            switch (size)
            {
                case ImageSize.Thumbnail266x266:
                    height = 266;
                    width = 266;
                    break;
                default:
                    throw new ArgumentException(size + " is not a supported thumbnail size");
            }
            string contentType = "image/png";
            string key = Guid.NewGuid().ToString();

            using (Stream originalStream = storageRepo.GetFile(originalFile.Key))
            {
                using (Image originalImg = Image.FromStream(originalStream))
                {
                    // hack to remove any embeded thumbnail image so GetThumbnailImage will return the highest possible quality
                    originalImg.RotateFlip(System.Drawing.RotateFlipType.Rotate180FlipNone);
                    originalImg.RotateFlip(System.Drawing.RotateFlipType.Rotate180FlipNone);

                    using (Image thumbnailImg = originalImg.GetThumbnailImage(width, height, () => { return false; }, System.IntPtr.Zero))
                    {
                        using (MemoryStream memoryStream = new MemoryStream(thumbnailImg.Size.Height * thumbnailImg.Size.Width))
                        {
                            thumbnailImg.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                            storageRepo.SaveFile(key, memoryStream.GetBuffer(), contentType);
                        }
                    }
                }
            }
            MediaFile thumbnailFile = new MediaFile()
            {
                ContentType = contentType,
                ImageSize = size,
                Key = key,
                Type = MediaFileType.Image,
                Url = storageRepo.KeyToUrl(key)
            };
            listingRepo.AddExternalMedia(listingId, appId, new List<MediaFile> { thumbnailFile });
        }
    }
}