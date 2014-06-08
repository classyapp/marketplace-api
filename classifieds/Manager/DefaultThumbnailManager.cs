using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using Amazon.S3;
using classy.Extentions;
using Classy.Repository;
using ServiceStack.Text;

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
                Stream memoryStream;

                var imageKey = originKey.StartsWith("profile_img") ? originKey : originKey + "_reduced";
                Stream originalImage = null;
                try
                {
                    originalImage = StorageRepository.GetFile(imageKey);
                }
                catch (AmazonS3Exception ex)
                {
                    originalImage = StorageRepository.GetFile(originKey);
                }

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

        public Stream CreateCollage(string[] imageKeys)
        {
            if (imageKeys == null || imageKeys.Length <= 1 || imageKeys.Length >= 5)
                throw new ArgumentException("CreateCollage can accept between 2 to 4 images");

            var imageCount = imageKeys.Length;
            var imageStreams = imageKeys.Select(x => StorageRepository.GetFile(x + "_reduced")).ToArray();
            var images = imageStreams.Select(Image.FromStream).ToArray();

            // get the smallest width & height in the bunch
            var smallestWidth = images.Min(x => x.Width);
            var smallestHeight = images.Min(x => x.Height);

            if (imageCount == 4)
            {
                // create 4 squares in the size of the smallest image
                var imageSize = Math.Min(smallestWidth, smallestHeight);
                var newImages = imageStreams.Select(x =>
                {
                    var byteArray = x.ReadFully().Rescale(imageSize);
                    using (var stream = new MemoryStream(byteArray))
                        return Image.FromStream(stream);
                }).ToArray();
                var newImageSize = (imageSize * 2) + 10;

                var newImage = new Bitmap(newImageSize, newImageSize, PixelFormat.DontCare);
                var graphics = Graphics.FromImage(newImage);
                graphics.FillRectangle(Brushes.White, 0, 0, newImageSize, newImageSize);

                graphics.DrawImage(newImages[0], 0, 0, imageSize, imageSize);
                //graphics.DrawImage();
                //graphics.DrawImage();
                //graphics.DrawImage();

                var outputStream = new MemoryStream();
                newImage.Save(outputStream, ImageFormat.Jpeg);
                return outputStream;
            }
            else if (imageCount == 3)
            {
                
            }
            else // this means there are only 2 images
            {
                // they both should be in the size of the smallest height and smallest width
                // and place them side by side
                var collageHeight = smallestHeight;
                var collageWidth = collageHeight;


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

                if (height == 0)
                {
                    // rescale 
                    newWidth = width;
                    newHeight = height = (int)((float)width * ((float)source.Height / (float)source.Width));
                }
                else
                {
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
                }

                // Check that we are not upscaling
                if (newWidth > source.Width && newHeight > source.Height)
                {
                    memoryStream = new MemoryStream();
                    source.Save(memoryStream, ImageFormat.Jpeg);
                    return memoryStream;
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