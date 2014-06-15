using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Amazon.S3;
using Classy.Models;
using Classy.Repository;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace GenerateReducedImages
{
    class Program
    {
        static void Main(string[] args)
        {
            const int REDUCTED_MAX_WIDTH = 1600;

            try
            {
                // Mongo wire up
                var connectionString = ConfigurationManager.ConnectionStrings["MONGO"].ConnectionString;
                var client = new MongoClient(connectionString);
                var databaseName = MongoUrl.Create(connectionString).DatabaseName;
                var server = client.GetServer();
                var db = server.GetDatabase(databaseName);

                // Amazon wire up
                var config = new Amazon.S3.AmazonS3Config()
                {
                    ServiceURL = "s3.amazonaws.com",
                    RegionEndpoint = Amazon.RegionEndpoint.USEast1,
                    Timeout = new TimeSpan(0, 5, 0)
                };
                var s3Client = Amazon.AWSClientFactory.CreateAmazonS3Client(config);

                MongoCollection<Listing> collection = db.GetCollection<Listing>("classifieds");
                var listings = collection.Find(Query<Listing>.GT(x => x.Id, ConfigurationManager.AppSettings["listingId"]));
//                listings.Limit = 10;
                listings.OrderBy(l => l.Id).Select(l => new { ExternalMedia = l.ExternalMedia, Id = l.Id }).ToArray();

                foreach (var listing in listings)
                {
                    if (listing.ExternalMedia != null && listing.ExternalMedia.Count > 0)
                    {
                        foreach (var media in listing.ExternalMedia)
                        {
                            AmazonS3StorageRepository S3Repository = new AmazonS3StorageRepository(s3Client, ConfigurationManager.AppSettings["S3BucketName"]);

                            Trace.WriteLine("Listing " + listing.Id);
                            // check reduced image exists
                            try
                            {
                                Console.Write("\tMedia " + media.Key);
                                //Stream stream = S3Repository.GetFile(string.Format("{0}_reduced", media.Key));
                                //stream.Close();
                                //stream.Dispose();
                                //Console.Write(" - Exists");

                                // Recreate 
                                CreateRescaledImage(REDUCTED_MAX_WIDTH, collection, media, S3Repository, listing.Id);

                            }
                            catch (AmazonS3Exception)
                            {
                                Trace.Write(" - Not Exists");
                                CreateRescaledImage(REDUCTED_MAX_WIDTH, collection, media, S3Repository, listing.Id);
                            }
                            catch (Exception)
                            {
                                Trace.WriteLine(" -> Skipped");
                            }
                        }
                    }
                }

                //Process p = new Process();
                //p.StartInfo = new ProcessStartInfo { UseShellExecute = true, WindowStyle = ProcessWindowStyle.Minimized, Arguments = listings.Last().Id, FileName = "C:/Users/Michael Bar/Desktop/GenerateReducedImages/bin/Debug/GenerateReducedImages.exe" };
                //p.Start();
            }
            catch (Exception ex)
            {
                //Process p = new Process();
                //p.StartInfo = new ProcessStartInfo { UseShellExecute = true, WindowStyle = ProcessWindowStyle.Minimized, Arguments = args[0], FileName = "C:/Users/Michael Bar/Desktop/GenerateReducedImages/bin/Debug/GenerateReducedImages.exe" };
                //p.Start();
            }
        }

        private static void CreateRescaledImage(int maxWidth, MongoCollection<Listing> collection, MediaFile media, AmazonS3StorageRepository S3Repository, string listingId)
        {
            try
            {
                Stream original = S3Repository.GetFile(media.Key);
                byte[] content = Rescale(original, maxWidth);
                original.Close();
                original.Dispose();
                collection.FindAndModify(Query.EQ("_id", listingId), null, Update.Set("Reducted", true));
                S3Repository.SaveFileSync(string.Format("{0}_reduced", media.Key), content, "image/jpeg");
                Console.WriteLine(" -> Created");
            }
            catch (AmazonS3Exception s3ex)
            {
                Trace.WriteLine(" -> Unexpected error " + s3ex.Message);
            }
            catch (Exception)
            {
                Listing fullListing = collection.FindOne(Query.EQ("_id", listingId));
                fullListing.Metadata.Add("ImageError", "True");
                collection.Save(fullListing);
                Console.WriteLine(" -> Unexpected error set on listing");
            }
        }

        public static byte[] Rescale(Stream stream, int maxSize)
        {
            byte[] buffer = null;

            using (Image image = Image.FromStream(stream))
            {
                if (maxSize > image.Width)
                {
                    stream.Close();
                    stream.Dispose();
                    MemoryStream mms = new MemoryStream();
                    image.Save(mms, ImageFormat.Jpeg);
                    mms.Seek(0, SeekOrigin.Begin);
                    buffer = new byte[mms.Length];
                    mms.Read(buffer, 0, buffer.Length);
                    mms.Close();
                    mms.Dispose();
                    return buffer;
                }
                double scale = (double)image.Width / (double)image.Height;
                int newWidth = scale > 1 ? maxSize : (int)(maxSize * scale);
                int newHeight = scale > 1 ? (int)(maxSize / scale) : maxSize;

                using (var newImage = new Bitmap(newWidth, newHeight))
                {
                    using (var graphics = Graphics.FromImage(newImage))
                    {
                        graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        graphics.DrawImage(image, new Rectangle(0, 0, newWidth, newHeight));
                        Stream reducedImage = new MemoryStream();
                        newImage.Save(reducedImage, ImageFormat.Jpeg);
                        reducedImage.Seek(0, SeekOrigin.Begin);
                        buffer = new byte[reducedImage.Length];
                        reducedImage.Read(buffer, 0, buffer.Length);
                        reducedImage.Close();
                        reducedImage.Dispose();
                    }
                }
            }

            return buffer;
        }
    }
}
