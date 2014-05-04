using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using Classy.Repository;

namespace classy.Manager
{
    public class DefaultThumbnailManager : IThumbnailManager
    {
        private IStorageRepository StorageRepository;
        private IAppManager AppManager;

        public DefaultThumbnailManager(
            IAppManager appManager,
            IStorageRepository storageRepository)
        {
            AppManager = appManager;
            StorageRepository = storageRepository;
        }

        public Stream CreateThumbnail(string originKey, int width, int height)
        {
            try
            {
                if (height == 0) height = width;
                Stream memoryStream;

                originKey = originKey.StartsWith("profile_img") ? originKey : originKey + "_reduced";
                Stream originalImage = StorageRepository.GetFile(originKey);
                lock (originalImage)
                {
                    memoryStream = GenerateThumbnail(originalImage, width, height);
                    originalImage.Close();
                    originalImage.Dispose();
                }
                return memoryStream;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private Stream GenerateThumbnail(Stream originalImage, int width, int height)
        {
            MemoryStream memoryStream = null;
            using (Image source = Image.FromStream(originalImage))
            {
                // Resize the original
                int newWidth = 0;
                int newHeight = 0;

                if ((float)height / (float)source.Height > (float)width / (float)source.Width)
                {
                    newHeight = height;
                    newWidth = (int)(width * (((float)height / (float)source.Height) / ((float)width / (float)source.Width)));
                }
                else
                {
                    newHeight = (int)(height * (((float)width / (float)source.Width) / ((float)height / (float)source.Height)));
                    newWidth = width;
                }

                using (Bitmap bitmap = new Bitmap(newWidth, newHeight))
                {
                    Graphics g = Graphics.FromImage((Image)bitmap);
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                    g.DrawImage(source, 0, 0, newWidth, newHeight);
                    g.Dispose();

                    // Crop
                    using (Bitmap thumbnail = bitmap.Clone(new Rectangle { X = (newWidth - width) / 2, Y = (newHeight - height) / 2, Width = width, Height = height }, PixelFormat.DontCare))
                    {
                        memoryStream = new MemoryStream(width * height);
                        thumbnail.Save(memoryStream, ImageFormat.Jpeg);
                    }
                }
            }

            return memoryStream;
        }

    }
}