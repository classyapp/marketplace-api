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
using Abot.Crawler;
using Abot.Poco;
using System.Drawing;
using System.Drawing.Imaging;
using System.Web;

namespace Classy.UtilRunner.Utilities.ProWebsiteCrawler
{
    public class ProfileImage
    {
        public string Url { get; set; }
        public Image Image { get; set; }
    }

    public class ProWebsiteCrawler : IUtility
    {
        private const int BatchSize = 20;
        private const int MinPixels = 300;
        private readonly MongoCollection<Profile> _profiles;
        private readonly IStorageRepository _storage;
        private readonly IListingRepository _listingRepo;
        private readonly ICollectionRepository _collectionRepo;
        private const string AppId = "v1.0";
        private IDictionary<string, IList<ProfileImage>> ProfileImages;
        private IDictionary<string, string> Profiles;

        public ProWebsiteCrawler(Container container)
        {
            var mongoDatabase = container.Resolve<MongoDatabase>();
            _profiles = mongoDatabase.GetCollection<Profile>("profiles");
            _storage = container.TryResolve<IStorageRepository>();
            _listingRepo = container.TryResolve<IListingRepository>();
            _collectionRepo = container.TryResolve<ICollectionRepository>();
            ProfileImages = new Dictionary<string, IList<ProfileImage>>();
            Profiles = new Dictionary<string, string>();
        }

        public StatusCode Run(string[] args)
        {
            // configure crawler
            CrawlConfiguration crawlConfig = new CrawlConfiguration();
            crawlConfig.CrawlTimeoutSeconds = 100;
            crawlConfig.MaxConcurrentThreads = 10;
            crawlConfig.MaxPagesToCrawl = 100;
            crawlConfig.IsHttpRequestAutoRedirectsEnabled = false;
            crawlConfig.UserAgentString = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_9_2) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/36.0.1944.0 Safari/537.36";

            // query construction
            var skip = 0;
            var query = Query<Profile>.Where(x => 
                x.AppId == AppId && 
                x.ProfessionalInfo != null && 
                x.ProfessionalInfo.CompanyContactInfo != null && 
                !string.IsNullOrEmpty(x.ProfessionalInfo.CompanyContactInfo.WebsiteUrl) &&
                !string.IsNullOrEmpty(x.ProfessionalInfo.CompanyContactInfo.Email) &&
                x.ProfessionalInfo.CompanyContactInfo.Location.Address.Country == "IL" &&
                x.ProfessionalInfo.IsProxy);
            if (args.Count() > 0) query = Query.And(query, Query<Profile>.Where(x => x.Id == args[0]));

            // loop and crawl
            var results = _profiles.Find(query).SetSkip(skip).SetLimit(BatchSize);
            while (results.ToList().Count > 0)
            {
                foreach(var profile in results)
                {
                    var websiteUrl = profile.ProfessionalInfo.CompanyContactInfo.WebsiteUrl;
                    if (!websiteUrl.StartsWith("http")) websiteUrl = "http://" + websiteUrl;

                    var crawler = new PoliteWebCrawler(crawlConfig, null, null, null, null, null, null, null, null);
                    crawler.PageCrawlCompleted += crawler_PageCrawlCompleted;
                    var host = new Uri(websiteUrl).Host;
                    if (!Profiles.ContainsKey(host))
                    {
                        ProfileImages.Add(host, new List<ProfileImage>());
                        Profiles.Add(host, profile.Id);

                        Console.WriteLine("Website " + websiteUrl + " (" + profile.Id + ") ... starting crawl");
                        var result = crawler.Crawl(new Uri(websiteUrl));
                        CreateCollection(host);
                    }
                }

                if (args.Count() > 0) break; // if a specific id was passed in, and we don't break, we'll go into an endless loop
                skip += BatchSize;
                results = _profiles.Find(query).SetSkip(skip).SetLimit(BatchSize);
            }

            return StatusCode.Success;
        }

        void crawler_PageCrawlCompleted(object sender, PageCrawlCompletedArgs e)
        {
            if (e.CrawledPage.Uri.OriginalString.EndsWith(".jpg") ||
                e.CrawledPage.Uri.OriginalString.EndsWith(".png") ||
                e.CrawledPage.Uri.OriginalString.EndsWith(".gif"))
            {
                var imgUrl = e.CrawledPage.Uri.OriginalString;
                using (var client = new WebClient())
                {
                    try
                    {
                        var imgStream = new MemoryStream(client.DownloadData(imgUrl));
                        var image = Image.FromStream(imgStream);
                        if (image.Width >= MinPixels && image.Height >= MinPixels)
                        {
                            if (!ProfileImages[e.CrawledPage.Uri.Host].Any(x => x.Url == imgUrl))
                            {
                                Console.WriteLine("Found " + imgUrl + ": " + image.Width + "x" + image.Height);
                                ProfileImages[e.CrawledPage.Uri.Host].Add(new ProfileImage { Image = image, Url = e.CrawledPage.Uri.ToString() });
                            }
                        }
                    }
                    catch
                    {
                        Console.WriteLine("Skipping " + imgUrl + " due to exception");
                    }
                }
            }
            else
            {
                Console.WriteLine("page crawled: " + e.CrawledPage.Uri);

                var imageNodes = e.CrawledPage.HtmlDocument.DocumentNode.SelectNodes("//img");
                if (imageNodes != null)
                {
                    foreach (var img in imageNodes)
                    {
                        if (img.Attributes.Contains("src"))
                        {
                            var imgUrl = ToAbsolute(img.Attributes["src"].Value, e.CrawledPage.Uri);
                            using (var client = new WebClient())
                            {
                                try
                                {
                                    var imgStream = new MemoryStream(client.DownloadData(imgUrl));
                                    var image = Image.FromStream(imgStream);
                                    if (image.Width >= MinPixels && image.Height >= MinPixels)
                                    {
                                        if (!ProfileImages[e.CrawledPage.Uri.Host].Any(x => x.Url == imgUrl))
                                        {
                                            Console.WriteLine("Found " + imgUrl + ": " + image.Width + "x" + image.Height);
                                            ProfileImages[e.CrawledPage.Uri.Host].Add(new ProfileImage { Image = image, Url = e.CrawledPage.Uri.ToString() });
                                        }
                                    }
                                }
                                catch
                                {
                                    Console.WriteLine("Skipping " + imgUrl + " due to exception");
                                }
                            }
                        }
                    }
                }
            }
        }

        private string ToAbsolute(string value, Uri uri)
        {
            if (value.StartsWith("http")) return value;
            return uri.Scheme + "://" + uri.Host + "/" + value.TrimStart('/');
        }

        private void CreateCollection(string host)
        {
            List<ProfileImage> images = ProfileImages[host].ToList();
            var profileId = Profiles[host];

            if (images.Count > 0)
            {
                // create collection
                var collection = new Collection
                {
                    AppId = AppId,
                    ProfileId = profileId,
                    Type = "WebPhotos",
                    Title = "Web Clips",
                    IsPublic = true,
                    IncludedListings = new List<IncludedListing>()
                };

                // sort images
                images.Sort((img1, img2) => Math.Max(img2.Image.Width, img2.Image.Height).CompareTo(Math.Max(img1.Image.Height, img1.Image.Width)));
                foreach (var img in images.Take(images.Count > 10 ? 10 : images.Count))
                {
                    try
                    {
                        // save media file
                        var imgStream = new MemoryStream();
                        img.Image.Save(imgStream, ImageFormat.Jpeg);
                        var imgBytes = imgStream.ToArray();
                        var key = Guid.NewGuid().ToString();
                        _storage.SaveFile(key, imgBytes, "image/jpeg");
                        var mediaFile = new MediaFile
                        {
                            Key = key,
                            Url = _storage.KeyToUrl(key),
                            Type = MediaFileType.Image,
                            ContentType = "image/jpeg"
                        };

                        // create listing
                        var listing = new Listing
                        {
                            AppId = AppId,
                            ProfileId = profileId,
                            IsPublished = true,
                            ListingType = "Photo",
                            Metadata = new Dictionary<string, string>
                        {
                            { "IsWebPhoto", "True" },
                            { "CopyrightMessage", img.Url },
                            { "Room", "other" },
                            { "Style", "other" }
                        },
                            ExternalMedia = new List<MediaFile> { mediaFile }
                        };
                        var id = _listingRepo.Insert(listing);

                        // add listing to collection
                        collection.IncludedListings.Add(new IncludedListing
                        {
                            ListingType = "Photo",
                            Id = id
                        });

                        Console.WriteLine("\t\t" + img.Image.Width + "x" + img.Image.Height);
                    }
                    catch
                    {
                        Console.WriteLine("Skipping " + img.Url + " due to exception");
                    }
                }

                // update collection
                _collectionRepo.Update(collection);
            }
        }
    }
}
