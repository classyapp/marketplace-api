using System;
using System.Collections.Generic;
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
                        return GenerateThumbnail(originalImage, width, height);
                    }
                }
                finally
                {
                    if (originalImage != null)
                    {
                        originalImage.Close();
                        originalImage.Dispose();
                    }
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

            const int collageSize = 500;
            var borderSize = (int)Math.Ceiling(Decimal.Divide(collageSize, 80));

            var resizedImages = new List<byte[]>(4);
            var imageCount = imageKeys.Length;
            var images = new List<Image>(4);
            imageKeys.ForEach(x => {
                using (var stream = StorageRepository.GetFile(x + "_reduced"))
                    images.Add(Image.FromStream(stream));
            });
            
            if (imageCount == 4)
            {
                // create 4 squares in the size of the smallest image
                var imageSize = (collageSize - borderSize) / 2;
                resizedImages = images.Select(x => 
                {
                    using (var b = new Bitmap(x))
                        return RescaleAndCrop(b, imageSize, imageSize);
                }).ToList();

                using (var newImage = new Bitmap(collageSize, collageSize))
                using (var graphics = Graphics.FromImage(newImage))
                {
                    graphics.FillRectangle(Brushes.White, 0, 0, collageSize, collageSize);

                    graphics.DrawImage(ImageExtensions.ConvertBytesToImage(resizedImages[0]), 0, 0, imageSize, imageSize);
                    graphics.DrawImage(ImageExtensions.ConvertBytesToImage(resizedImages[1]), imageSize + borderSize, 0, imageSize, imageSize);
                    graphics.DrawImage(ImageExtensions.ConvertBytesToImage(resizedImages[2]), 0, imageSize + borderSize, imageSize, imageSize);
                    graphics.DrawImage(ImageExtensions.ConvertBytesToImage(resizedImages[3]), imageSize + borderSize, imageSize + borderSize, imageSize, imageSize);

                    using (var outputStream = new MemoryStream())
                    {
                        newImage.Save(outputStream, ImageFormat.Jpeg);
                        return outputStream.ToArray();
                    }
                }
            }

            if (imageCount == 3)
            {
                var collageImageWidth = (collageSize - borderSize) / 2;
                using (var b = new Bitmap(images[0]))
                    resizedImages.Add(RescaleAndCrop(b, collageImageWidth, collageSize));
                for (var i = 1; i <= 2; i++)
                {
                    using (var b = new Bitmap(images[i]))
                        resizedImages.Add(RescaleAndCrop(b, collageImageWidth, collageImageWidth));
                }

                using (var newImage = new Bitmap(collageSize, collageSize))
                using (var graphics = Graphics.FromImage(newImage))
                {
                    graphics.FillRectangle(Brushes.White, 0, 0, collageSize, collageSize);

                    graphics.DrawImage(ImageExtensions.ConvertBytesToImage(resizedImages[0]), 0, 0, collageImageWidth, collageSize);
                    graphics.DrawImage(ImageExtensions.ConvertBytesToImage(resizedImages[1]), collageImageWidth + borderSize, 0, collageImageWidth, collageImageWidth);
                    graphics.DrawImage(ImageExtensions.ConvertBytesToImage(resizedImages[2]), collageImageWidth + borderSize, collageImageWidth + borderSize, collageImageWidth, collageImageWidth);

                    using (var outputStream = new MemoryStream())
                    {
                        newImage.Save(outputStream, ImageFormat.Jpeg);
                        return outputStream.ToArray();
                    }
                }
            }

            // this means there are only 2 images
            // they both should be in the size of the smallest height and smallest width
            // and place them side by side
            var imageWidth = (collageSize - borderSize) / 2;
            var imageHeight = collageSize;
            resizedImages = images.Select(x =>
            {
                using (var b = new Bitmap(x))
                    return RescaleAndCrop(b, imageWidth, imageHeight);
            }).ToList();

            using (var newImage = new Bitmap(collageSize, collageSize))
            using (var graphics = Graphics.FromImage(newImage))
            {
                graphics.FillRectangle(Brushes.White, 0, 0, collageSize, collageSize);

                graphics.DrawImage(ImageExtensions.ConvertBytesToImage(resizedImages[0]), 0, 0, imageWidth, imageHeight);
                graphics.DrawImage(ImageExtensions.ConvertBytesToImage(resizedImages[1]), imageWidth + borderSize, 0, imageWidth, imageHeight);

                using (var outputStream = new MemoryStream())
                {
                    newImage.Save(outputStream, ImageFormat.Jpeg);
                    return outputStream.ToArray();
                }
            }
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
            using (var stream = new MemoryStream(newWidth*newHeight))
            {
                croppedImage.Save(stream, ImageFormat.Jpeg);
                return stream.ToArray();
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