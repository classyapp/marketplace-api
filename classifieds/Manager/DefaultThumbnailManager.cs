using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
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

        public byte[] CreateCollage(string[] imageKeys)
        {
            if (imageKeys == null || imageKeys.Length <= 1 || imageKeys.Length >= 5)
                throw new ArgumentException("CreateCollage can accept between 2 to 4 images");

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
                var resizedImages = images.Select(x => 
                {
                    using (var b = new Bitmap(x))
                        return Rescale(b, 50);
                }).ToArray();

                using (var newImage = new Bitmap(1600, 1600, PixelFormat.DontCare))
                using (var graphics = Graphics.FromImage(newImage))
                {
                    graphics.FillRectangle(Brushes.White, 0, 0, 100, 100);

                    graphics.DrawImage(ImageExtensions.ConvertBytesToImage(resizedImages[0]), 0, 0, 50, 50);
                    graphics.DrawImage(ImageExtensions.ConvertBytesToImage(resizedImages[1]), 50, 0, 50, 50);
                    graphics.DrawImage(ImageExtensions.ConvertBytesToImage(resizedImages[2]), 0, 50, 50, 50);
                    graphics.DrawImage(ImageExtensions.ConvertBytesToImage(resizedImages[3]), 50, 50, 50, 50);

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
            //var collageHeight = smallestHeight;
            //var collageWidth = collageHeight;
            //var imageWidth = (int) ((collageWidth/2) - (borderSize/2));

            //newImages = imageStreams.Select(x =>
            //{
            //    var imageScaled = ReadFully(x).Rescale(collageHeight);
            //    using (var stream = new MemoryStream(imageScaled))
            //    {
            //        var newImage = new Bitmap(stream);
            //        var croppedImage = CropImage(newImage, collageHeight, imageWidth);
            //        return Image.FromStream(croppedImage);
            //    }
            //}).ToArray();

            //using (var collage = new Bitmap(collageWidth + borderSize, collageHeight, PixelFormat.DontCare))
            //{
            //    using (var g = Graphics.FromImage(collage))
            //    {
            //        g.FillRectangle(Brushes.White, 0, 0, collageWidth, collageHeight);

            //        g.DrawImage(newImages[0], 0, 0, newImages[0].Width, newImages[0].Height);
            //        g.DrawImage(newImages[1], imageWidth + borderSize, 0, newImages[1].Width, newImages[1].Height);

            //        var outputStream = new MemoryStream();
            //        collage.Save(outputStream, ImageFormat.Jpeg);
            //        return outputStream.ToArray();
            //    }
            //}
            return null;
        }

        private Stream GenerateThumbnail(Stream originalImage, int width, int height)
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
                        return memoryStream;
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

        private Stream CropImage(Bitmap imageToCrop, int newWidth, int newHeight)
        {
            var imageWidth = imageToCrop.Width;
            var imageHeight = imageToCrop.Height;

            using (var croppedImage = imageToCrop.Clone(new Rectangle { X = (imageWidth - newWidth) / 2, Y = (imageHeight - newHeight) / 2, Width = newWidth, Height = newHeight }, PixelFormat.DontCare))
            {
                var stream = new MemoryStream(newWidth * newHeight);
                croppedImage.Save(stream, ImageFormat.Jpeg);

                return stream;
            }
        }

        private byte[] Rescale(Image image, int size)
        {
            var scale = (float)((float)image.Width/(float)image.Height);

            var newWidth = (int)(scale > 1 ? size*scale : size);
            var newHeight = (int)(scale > 1 ? size : size*scale);

            using (var scaledImage = new Bitmap(image, newWidth, newHeight))
                return ImageExtensions.ConvertImageToByteArray(scaledImage);
        }

        /// <summary>
        /// Reads data from a stream until the end is reached. The
        /// data is returned as a byte array. An IOException is
        /// thrown if any of the underlying IO calls fail.
        /// </summary>
        /// <param name="stream">The stream to read data from</param>
        /// <param name="initialLength">The initial buffer length</param>
        public static byte[] ReadFully(Stream stream, int initialLength = 32768)
        {
            var buffer = new byte[initialLength];
            var read = 0;

            int chunk;
            while ((chunk = stream.Read(buffer, read, buffer.Length - read)) > 0)
            {
                read += chunk;

                // If we've reached the end of our buffer, check to see if there's
                // any more information
                if (read == buffer.Length)
                {
                    int nextByte = stream.ReadByte();

                    // End of stream? If so, we're done
                    if (nextByte == -1)
                    {
                        return buffer;
                    }

                    // Nope. Resize the buffer, put in the byte we've just
                    // read, and continue
                    var newBuffer = new byte[buffer.Length * 2];
                    Array.Copy(buffer, newBuffer, buffer.Length);
                    newBuffer[read] = (byte)nextByte;
                    buffer = newBuffer;
                    read++;
                }
            }
            // Buffer is now too big. Shrink it.
            var ret = new byte[read];
            Array.Copy(buffer, ret, read);
            return ret;
        }
    }
}