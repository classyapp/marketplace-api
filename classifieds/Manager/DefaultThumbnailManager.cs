using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using Classy.Repository;

namespace classy.Manager
{
    public class DefaultThumbnailManager : IThumbnailManager
    {
        private IStorageRepository StorageRepository;

        public DefaultThumbnailManager(
            IStorageRepository storageRepository)
        {
            StorageRepository = storageRepository;
        }

        public Stream CreateThumbnail(string originKey, int width, int height)
        {
            try
            {
                if (height == 0) height = width;
                MemoryStream memoryStream;
                Bitmap squareImage;

                Stream originalImage = StorageRepository.GetFile(originKey);

                // Crop to square
                //using (Image originalImg = Image.FromStream(originalImage))
                //{
                //    int minDimension;
                //    int startX = 0;
                //    int startY = 0;
                //    if (originalImg.Height < originalImg.Width)
                //    {
                //        minDimension = originalImg.Height;
                //        startX = (originalImg.Width - originalImg.Height) / 2;
                //    }
                //    else
                //    {
                //        minDimension = originalImg.Width;
                //        startY = (originalImg.Height - originalImg.Width) / 2;
                //    }

                //    using (Bitmap bmp = new Bitmap(originalImg))
                //    {
                //        squareImage = bmp.Clone(new Rectangle(startX, startY, minDimension, minDimension), System.Drawing.Imaging.PixelFormat.DontCare);
                //    }
                //}

                // Create thumb
                using (Bitmap bmp = new Bitmap(originalImage))
                {
                    using (var thumbnail = bmp.GetThumbnailImage(width, height, () => false, System.IntPtr.Zero))
                    {
                        memoryStream = new MemoryStream(thumbnail.Size.Height * thumbnail.Size.Width);
                        thumbnail.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                    }
                }
                return memoryStream;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}