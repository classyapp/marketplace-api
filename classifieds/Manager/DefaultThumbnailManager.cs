using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Amazon.S3;
using classy.Extentions;
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

        public byte[] CreateThumbnail(string originKey, int width, int height)
        {
            try
            {
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
                    originalImage.Close();
                    originalImage.Dispose();
                    return GenerateThumbnail(originalImage, width, height);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public byte[] CreateCollage(string[] imageKeys)
        {
            if (imageKeys == null || imageKeys.Length <= 1 || imageKeys.Length >= 5)
                throw new ArgumentException("CreateCollage can accept between 2 to 4 images");

            const int borderSize = 10;

            byte[][] resizedImages;
            var imageCount = imageKeys.Length;
            var imageStreams = imageKeys.Select(x => StorageRepository.GetFile(x + "_reduced")).ToArray();
            var images = imageStreams.Select(Image.FromStream).ToArray();
            imageStreams.ForEach(x => 
            {
                if (x == null) return;
                x.Close();
                x.Dispose();
            });
            
            if (imageCount == 4)
            {
                // create 4 squares in the size of the smallest image
                resizedImages = images.Select(x => 
                {
                    using (var b = new Bitmap(x))
                        return RescaleAndCrop(b, 400, 400);
                }).ToArray();

                using (var newImage = new Bitmap(800 + borderSize, 800 + borderSize))
                using (var graphics = Graphics.FromImage(newImage))
                {
                    graphics.FillRectangle(Brushes.White, 0, 0, 800 + borderSize, 800 + borderSize);

                    graphics.DrawImage(ImageExtensions.ConvertBytesToImage(resizedImages[0]), 0, 0, 400, 400);
                    graphics.DrawImage(ImageExtensions.ConvertBytesToImage(resizedImages[1]), 400 + borderSize, 0, 400, 400);
                    graphics.DrawImage(ImageExtensions.ConvertBytesToImage(resizedImages[2]), 0, 400 + borderSize, 400, 400);
                    graphics.DrawImage(ImageExtensions.ConvertBytesToImage(resizedImages[3]), 400 + borderSize, 400 + borderSize, 400, 400);

                    using (var outputStream = new MemoryStream())
                    {
                        newImage.Save(outputStream, ImageFormat.Jpeg);
                        return outputStream.ToArray();
                    }
                }
            }

            if (imageCount == 3)
            {
                return null;
            }

            // this means there are only 2 images
            // they both should be in the size of the smallest height and smallest width
            // and place them side by side

            resizedImages = images.Select(x =>
            {
                using (var b = new Bitmap(x))
                    return RescaleAndCrop(b, 400, 810);
            }).ToArray();

            using (var newImage = new Bitmap(800 + borderSize, 800 + borderSize))
            using (var graphics = Graphics.FromImage(newImage))
            {
                graphics.FillRectangle(Brushes.White, 0, 0, 800 + borderSize, 800 + borderSize);

                graphics.DrawImage(ImageExtensions.ConvertBytesToImage(resizedImages[0]), 0, 0, 400, 800 + borderSize);
                graphics.DrawImage(ImageExtensions.ConvertBytesToImage(resizedImages[1]), 400 + borderSize, 0, 400, 800 + borderSize);

                using (var outputStream = new MemoryStream())
                {
                    newImage.Save(outputStream, ImageFormat.Jpeg);
                    return outputStream.ToArray();
                }
            }

            return null;
        }

        private byte[] GenerateThumbnail(Stream originalImage, int width, int height)
        {
            using (Image source = Image.FromStream(originalImage))
            {
                // Resize the original
                var newWidth = 0;
                var newHeight = 0;

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
                    using (var memoryStream = new MemoryStream())
                    {
                        source.Save(memoryStream, ImageFormat.Jpeg);
                        return memoryStream.ToArray();
                    }
                }

                using (var bitmap = new Bitmap(newWidth, newHeight))
                {
                    using (var g = Graphics.FromImage(bitmap))
                    {
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.DrawImage(source, 0, 0, newWidth, newHeight);
                    }

                    return CropImage(bitmap, width, height);
                }
            }
        }

        private byte[] CropImage(Bitmap imageToCrop, int newWidth, int newHeight)
        {
            var imageWidth = imageToCrop.Width;
            var imageHeight = imageToCrop.Height;

            using (var croppedImage = imageToCrop.Clone(new Rectangle { X = (imageWidth - newWidth) / 2, Y = (imageHeight - newHeight) / 2, Width = newWidth, Height = newHeight }, PixelFormat.DontCare))
            {
                using (var stream = new MemoryStream(newWidth*newHeight))
                {
                    croppedImage.Save(stream, ImageFormat.Jpeg);
                    return stream.ToArray();
                }
            }
        }

        private byte[] RescaleAndCrop(Image image, int width, int height)
        {
            int newHeight = 0, newWidth = 0;
            var imageScale = (float) ((float) image.Width / (float) image.Height);
            var newImageScale = (float)((float)width / (float)height);

            if (imageScale > newImageScale)
            {
                // resize the image according to the new image's height and the original image's scale
                newHeight = height;
                newWidth = (int)(height * imageScale);
            }
            else
            {
                // resize the image according to the new image's width and the original image's scale
                newWidth = width;
                newHeight = (int)(width / imageScale);
            }
            
            using (var scaledImage = new Bitmap(image, newWidth, newHeight))
                return CropImage(scaledImage, width, height);
        }
    }
}