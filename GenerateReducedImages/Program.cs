using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
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
            var listings = collection.Find(Query.And(Query.NotExists("Reducted"))).OrderBy(l => l.Id).Select(l => new { ExternalMedia = l.ExternalMedia, Id = l.Id }).ToArray();

            foreach (var listing in listings)
            {
                if (listing.ExternalMedia != null && listing.ExternalMedia.Count > 0)
                {
                    foreach (var media in listing.ExternalMedia)
                    {
                        AmazonS3StorageRepository S3Repository = new AmazonS3StorageRepository(s3Client, ConfigurationManager.AppSettings["S3BucketName"]);

                        Console.WriteLine("Listing " + listing.Id);
                        // check reduced image exists
                        try
                        {
                            Console.Write("\tMedia " + media.Key);
                            Stream stream = S3Repository.GetFile(string.Format("{0}_reduced", media.Key));
                            stream.Close();
                            stream.Dispose();
                            Console.Write(" - Exists");

                            // Recreate 
                            CreateRescaledImage(REDUCTED_MAX_WIDTH, collection, media, S3Repository, listing.Id);

                        }
                        catch (AmazonS3Exception)
                        {
                            Console.Write(" - Not Exists");
                            CreateRescaledImage(REDUCTED_MAX_WIDTH, collection, media, S3Repository, listing.Id);
                        }
                        catch (Exception)
                        {
                            Console.WriteLine(" -> Skipped");
                        }
                    }
                }
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
                S3Repository.SaveFile(string.Format("{0}_reduced", media.Key), content, "image/jpeg", false, null);
                Console.WriteLine(" -> Created");
            }
            catch (AmazonS3Exception s3ex)
            {
                Console.WriteLine(" -> Unexpected error " + s3ex.Message);
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
