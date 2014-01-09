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

            Tuple<int, int, Action<string>>[] imageThumbnails = new Tuple<int,int,Action<string>>[] { 
                new Tuple<int, int, Action<string>>(266, 266, (key) => { image.Thumbnail_266x266_Key = key; })
            };
        
            foreach(var thum in imageThumbnails) {
                using (var thumbnail = squareImage.GetThumbnailImage(thum.Item1, thum.Item2, () => false, System.IntPtr.Zero)) 
                {
                    using (MemoryStream memoryStream = new MemoryStream(thumbnail.Size.Height * thumbnail.Size.Width))
                    {
                        var thumbnailKey = string.Format("{0}_{1}x{2}", image.Key, thum.Item1, thum.Item2);
                        thumbnail.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                        storageRepo.SaveFile(thumbnailKey, memoryStream.GetBuffer(), "image/png");
                        thum.Item3(thumbnailKey);
                    }
                }
            }
            listingRepo.UpdateExternalMedia(listingId, appId, image);
        }
    }
}