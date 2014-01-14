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
        public static void GenerateThumbnails(MediaFile image, string listingId, string appId, IStorageRepository storageRepo, IListingRepository listingRepo)
        {
            int height = 0, width = 0;
            Bitmap squareImage;
            // crop to square
            using (Stream originalStream = storageRepo.GetFile(image.Key))
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

            Point[] sizes = new Point[] { new Point { X = 266, Y = 266 } };

            foreach(var s in sizes) {
                using (var thumbnail = squareImage.GetThumbnailImage(s.X, s.Y, () => false, System.IntPtr.Zero)) 
                {
                    using (MemoryStream memoryStream = new MemoryStream(thumbnail.Size.Height * thumbnail.Size.Width))
                    {
                        thumbnail.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);

                        var thumbnailKey = string.Format("{0}_{1}x{2}", image.Key, s.X, s.Y);
                        storageRepo.SaveFile(thumbnailKey, memoryStream.GetBuffer(), "image/png");
                        MediaThumbnail mediaThumbnail = new MediaThumbnail()
                        {
                            Width = s.X,
                            Height = s.Y,
                            Key = thumbnailKey,
                            Url = storageRepo.KeyToUrl(thumbnailKey)
                        };

                        image.Thumbnails.Add(mediaThumbnail);
                    }
                }
            }
            listingRepo.UpdateExternalMedia(listingId, appId, image);
        }
    }
}