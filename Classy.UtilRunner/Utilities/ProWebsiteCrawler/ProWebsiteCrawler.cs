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

namespace Classy.UtilRunner.Utilities.ProWebsiteCrawler
{
    public class ProWebsiteCrawler : IUtility
    {
        private const int BatchSize = 20;
        private readonly MongoCollection<Profile> _profiles;
        private readonly MongoCollection<Listing> _listings;

        public ProWebsiteCrawler(Container container)
        {
            var mongoDatabase = container.Resolve<MongoDatabase>();
            _profiles = mongoDatabase.GetCollection<Profile>("profiles");
        }

        public StatusCode Run(string[] args)
        {
            // configure crawler
            CrawlConfiguration crawlConfig = new CrawlConfiguration();
            crawlConfig.CrawlTimeoutSeconds = 100;
            crawlConfig.MaxConcurrentThreads = 10;
            crawlConfig.MaxPagesToCrawl = 1000;
            crawlConfig.UserAgentString = "abot v1.0 http://code.google.com/p/abot";

            // query construction
            var page = 1;
            var query = Query<Profile>.Where(x => 
                x.AppId == "v1.0" && 
                x.ProfessionalInfo != null && 
                x.ProfessionalInfo.CompanyContactInfo != null &&
                !string.IsNullOrEmpty(x.ProfessionalInfo.CompanyContactInfo.WebsiteUrl) &&
                !string.IsNullOrEmpty(x.ProfessionalInfo.CompanyContactInfo.Email) &&
                x.ProfessionalInfo.IsProxy);
            if (args.Count() > 0) query = Query.And(query, Query<Profile>.Where(x => x.Id == args[0]));

            // loop and crawl
            var results = _profiles.Find(query).SetSkip((page - 1) * BatchSize);
            while (results.Count() > 0)
            {
                foreach(var profile in results)
                {
                    var websiteUrl = profile.ProfessionalInfo.CompanyContactInfo.WebsiteUrl;
                    var crawler = new PoliteWebCrawler(crawlConfig, null, null, null, null, null, null, null, null);
                    crawler.PageCrawlCompleted += crawler_PageCrawlCompleted;
                    Console.WriteLine("Found " + websiteUrl + " ... starting crawl");
                    crawler.Crawl(new Uri(websiteUrl));
                }
                results = _profiles.Find(query).SetSkip((++page - 1) * BatchSize);
            }

            return StatusCode.Success;
        }

        void crawler_PageCrawlCompleted(object sender, PageCrawlCompletedArgs e)
        {
            Console.WriteLine("\tpage crawled: " + e.CrawledPage.Uri);
        }
    }
}
