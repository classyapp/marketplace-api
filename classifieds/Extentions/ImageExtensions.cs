using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace classy.Extentions
{
    public static class ImageExtensions
    {
        public static byte[] Rescale(this byte[] original, int maxSize)
        {
            byte[] buffer = null;

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

                            using (var reducedImage = new MemoryStream())
                            {
                                newImage.Save(reducedImage, ImageFormat.Jpeg);

                                // TODO: what is this for ??? (we already saved the image)
                                reducedImage.Seek(0, SeekOrigin.Begin);

                                buffer = new byte[reducedImage.Length];
                                reducedImage.Read(buffer, 0, buffer.Length);

                                reducedImage.Close();
                            }
                        }
                    }
                }

                memoryStream.Close();
            }

            return buffer;
        }
    }
}