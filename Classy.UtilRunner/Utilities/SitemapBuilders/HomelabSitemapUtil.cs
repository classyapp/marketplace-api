using Amazon.S3;
using Amazon.S3.Model;
using classy.Manager;
using Classy.Models;
using Classy.Repository;
using Funq;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Classy.UtilRunner.Utilities.SitemapBuilders
{
    public class HomelabSitemapUtil : IUtility
    {
        private Container _container;

        public HomelabSitemapUtil(Container container)
        {
            _container = container;
        }

        public StatusCode Run(string[] args)
        {
            var saveToPath = "C:\\temp\\sitemap\\";

            // clean target directory
            foreach(var f in Directory.GetFiles(saveToPath))
            {
                File.Delete(f);
            }

            var mongoDatabase = _container.Resolve<MongoDatabase>();
            var apps = mongoDatabase.GetCollection<App>("apps");
            var app = apps.FindOne(Query<App>.Where(x => x.AppId == "v1.0"));
            var listingService = _container.Resolve<IListingManager>();
            var profileService = _container.Resolve<IProfileRepository>();
            var localizationService = _container.Resolve<ILocalizationManager>();

            // generate sitemap
            var generator = new HomelabSitemapGenerator(app, listingService, profileService, localizationService);
            generator.Generate("http://www.homelab.com", saveToPath);

            // upload to s3
            var s3Client = _container.Resolve<IAmazonS3>();
            Console.WriteLine("Uploading to S3...");
            Console.WriteLine("file: sitemap.xml");
            s3Client.Upload(Path.Combine(saveToPath, "sitemap.xml"), "text/xml");
            foreach(var f in Directory.GetFiles(saveToPath, "sitemap*.xml.gz"))
            {
                Console.WriteLine("file: " + f);
                s3Client.Upload(f, "application/x-gzip");
            }

            return StatusCode.Success;
        }
    }

    public static class S3Extensions
    {
        public static void Upload(this IAmazonS3 s3Client, string filename, string contentType)
        {
            var client = new WebClient();
            byte[] buffer = client.DownloadData(filename);

            PutObjectRequest request = new PutObjectRequest();
            request.BucketName = "classy-myhome";
            request.ContentType = contentType;
            request.Key = Path.GetFileName(filename);
            request.InputStream = new MemoryStream(buffer);
            request.CannedACL = Amazon.S3.S3CannedACL.PublicReadWrite;
            var response = s3Client.PutObject(request);
        }
    }
}
