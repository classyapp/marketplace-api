using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace classy.Extentions
{
    public static class ImageExtensions
    {
        /// <summary>
        /// This method rescales an image to a maximum given size.
        /// The maximum size will be the constraint on the image's width
        /// and the height will be resized according to the original scale of the image.
        /// Note: this means the height can turn out to be smaller than the given maxSize.
        /// </summary>
        /// <param name="original">The image to resize represented as a byte array</param>
        /// <param name="maxSize">Maximum width of image to resize to</param>
        /// <returns>A byte array of the newly resized image</returns>
        public static byte[] Rescale(this byte[] original, int maxSize)
        {
            using (var memoryStream = new MemoryStream(original))
            {
                using (var image = Image.FromStream(memoryStream))
                {
                    if (maxSize > image.Width)
                    {
                        memoryStream.Close();
                        return original;
                    }

                    double scale = (double) image.Width/(double) image.Height;
                    int newWidth = scale > 1 ? maxSize : (int) (maxSize*scale);
                    int newHeight = scale > 1 ? (int) (maxSize/scale) : maxSize;

                    using (var newImage = new Bitmap(newWidth, newHeight))
                    {
                        using (var graphics = Graphics.FromImage(newImage))
                        {
                            graphics.SmoothingMode = SmoothingMode.AntiAlias;
                            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                            graphics.DrawImage(image, new Rectangle(0, 0, newWidth, newHeight));

                            return ConvertImageToByteArray(newImage);
                        }
                    }
                }
            }
        }

        public static byte[] RescaleToHeight(this byte[] original, int maxHeight)
        {
            using (var memoryStream = new MemoryStream(original))
            {
                using (var image = Image.FromStream(memoryStream))
                {
                    if (maxHeight > image.Height)
                    {
                        memoryStream.Close();
                        return original;
                    }

                    var scale = (double)image.Width / (double)image.Height;
                    var newHeight = maxHeight;
                    var newWidth = (int)(newHeight * scale);
                    
                    using (var newImage = new Bitmap(newWidth, newHeight))
                    {
                        using (var graphics = Graphics.FromImage(newImage))
                        {
                            graphics.SmoothingMode = SmoothingMode.AntiAlias;
                            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                            graphics.DrawImage(image, new Rectangle(0, 0, newWidth, newHeight));

                            return ConvertImageToByteArray(newImage);
                        }
                    }
                }
            }
        }

        private static byte[] ConvertImageToByteArray(Image image)
        {
            using (var ms = new MemoryStream())
            {
                image.Save(ms, ImageFormat.Jpeg);
                return ms.ToArray();
            }
        }
    }
}