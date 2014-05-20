using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using Classy.DotNet.Mvc.SitemapGenerator;
using Classy.DotNet.Mvc;
using Classy.DotNet;
using Classy.Models;
using Classy.UtilRunner;
using MongoDB.Driver;
using Funq;

namespace MyHome.Sitemap
{
    public class HomelabSitemapGenerator : Classy.DotNet.Mvc.SitemapGenerator.ClassySitemapGenerator, IUtility
    {
        private readonly MongoCollection<App> _apps;
        private readonly MongoCollection<Listing> _listings;

        private readonly App App { get; set; }

        public HomelabSitemapGenerator(Container container)
        {
            var mongoDatabase = container.Resolve<MongoDatabase>();

            _apps = mongoDatabase.GetCollection<App>("apps");
            App = _apps.FindOneById("5378a5cf488c7623b5a648ab");

            _listings = mongoDatabase.GetCollection<Listing>("classifieds");
        }

        public override void GenerateStaticNodes()
        {
            foreach(var culture in App.SupportedCultures)
            {
                WriteUrlLocation(Url.RouteUrlForLocale("Home", culture.ToLower()), UpdateFrequency.Daily, DateTime.UtcNow);
                WriteUrlLocation(Url.RouteUrlForLocale("Careers", culture.ToLower()), UpdateFrequency.Weekly, DateTime.UtcNow);
                WriteUrlLocation(Url.RouteUrlForLocale("Terms", culture.Value), UpdateFrequency.Monthly, DateTime.UtcNow);
                WriteUrlLocation(Url.RouteUrlForLocale("Privacy", culture.Value), UpdateFrequency.Monthly, DateTime.UtcNow);
            }
        }

        public override void GenerateListingNodes()
        {
            // all photos
            foreach (var culture in _supportedCultures)
            {
                WriteUrlLocation(Url.RouteUrlForLocale("SearchPhoto", culture.Value), UpdateFrequency.Daily, DateTime.UtcNow);
            }

            var listingService = new Classy.DotNet.Services.ListingService();

            // room and style combinationa
            var rooms = Localizer.GetList("rooms");
            var styles = Localizer.GetList("room-styles");
            foreach(var room in rooms)
            {
                foreach(var style in styles)
                {
                    // output the room+style combination url
                    var photoMetadata = new MyHome.Models.PhotoMetadata { Room = room.Value, Style = style.Value };
                    foreach (var culture in _supportedCultures)
                    {
                        WriteUrlLocation(
                            Url.RouteUrlForLocale("SearchPhoto", culture.Value, new { filters = photoMetadata.GetSearchFilterSlug(null, null) }), UpdateFrequency.Daily, DateTime.UtcNow);
                    }

                    // all photo urls for this room and style combination
                    var page = 1;
                    var photos = listingService.SearchListings(
                        null,
                        new string[] { "Photo" },
                        new Dictionary<string, string[]> { { "Room", new string[] { room.Value } }, { "Style", new string[] { style.Value } } },
                        null,
                        null,
                        null,
                        page, 
                        200);
                    while (photos != null && photos.Results.Count > 0)
                    {
                        foreach (var photo in photos.Results)
                        {
                            foreach (var culture in _supportedCultures)
                            {
                                WriteUrlLocation(
                                    Url.RouteUrlForLocale("PhotoDetails", culture.Value, new { listingId = photo.Id, slug = photo.Title.ToSlug() }), UpdateFrequency.Daily, DateTime.UtcNow);
                            }
                        }

                        photos = listingService.SearchListings(
                            null,
                            new string[] { "Photo" },
                            new Dictionary<string, string[]> { { "Room", new string[] { room.Value } }, { "Style", new string[] { style.Value } } },
                            null,
                            null,
                            null,
                            ++page,
                            200);
                    }
                }
            }
        }

        public override void GenerateProfessionalNodes()
        {
            // all photos
            foreach (var culture in _supportedCultures)
            {
                WriteUrlLocation(Url.RouteUrlForLocale("SearchProfiles", culture.Value), UpdateFrequency.Daily, DateTime.UtcNow);
            }

            var profileService = new Classy.DotNet.Services.ProfileService();
            var localizationService = new Classy.DotNet.Services.LocalizationService();

            // category, city cominations
            var categories = Localizer.GetList("professional-categories");
            var countries = Localizer.GetList("supported-countries");

            foreach (var category in categories)
            {
                // output all the categories
                foreach (var culture in _supportedCultures)
                {
                    WriteUrlLocation(
                        Url.RouteUrlForLocale("SearchProfiles", culture.Value, new { filters = new Classy.DotNet.Mvc.ViewModels.Profiles.SearchProfileViewModel<MyHome.Models.ProfessionalMetadata> { Category = category.Value }.ToSlug() }),
                        UpdateFrequency.Daily,
                        DateTime.UtcNow);
                }

                foreach (var country in countries)
                {
                    // output all category + country combinations
                    foreach (var culture in _supportedCultures)
                    {
                        WriteUrlLocation(
                            Url.RouteUrlForLocale("SearchProfiles", culture.Value, new { filters = new Classy.DotNet.Mvc.ViewModels.Profiles.SearchProfileViewModel<MyHome.Models.ProfessionalMetadata> { Category = category.Value, Country = country.Value }.ToSlug() }),
                            UpdateFrequency.Daily,
                            DateTime.UtcNow);
                    }

                    var cities = localizationService.GetCitiesByCountry(country.Value);
                    foreach (var city in cities)
                    {
                        // output all country + category + city combinations
                        foreach (var culture in _supportedCultures)
                        {
                            WriteUrlLocation(
                                Url.RouteUrlForLocale("SearchProfiles", culture.Value, new { filters = new Classy.DotNet.Mvc.ViewModels.Profiles.SearchProfileViewModel<MyHome.Models.ProfessionalMetadata> { Category = category.Value, Country = country.Value, City = city.ToSlug() }.ToSlug() }),
                                UpdateFrequency.Daily,
                                DateTime.UtcNow);
                        }

                        // all profiles under this combination
                        var page = 1;
                        var pageSize = 500;
                        var profiles = profileService.SearchProfiles(null,
                            category.Value,
                            new Classy.DotNet.Responses.LocationView { Address = new Classy.DotNet.Responses.PhysicalAddressView { City = city, Country = country.Value } },
                            null,
                            true,
                            false,
                            page,
                            pageSize);

                        while (profiles != null && profiles.Results.Count > 0)
                        {
                            Trace.TraceInformation(string.Format("city: {0}, country: {1}, results page: {2}", country.Value, city, page));

                            foreach (var profile in profiles.Results)
                            {
                                foreach (var culture in _supportedCultures)
                                {
                                    WriteUrlLocation(Url.RouteUrlForLocale("PublicProfile", culture.Value, new { ProfileId = profile.Id, slug = profile.GetProfileName().ToSlug() }), UpdateFrequency.Daily, DateTime.UtcNow);
                                }
                            }

                            profiles = profileService.SearchProfiles(null,
                            "architects-designers",
                            new Classy.DotNet.Responses.LocationView { Address = new Classy.DotNet.Responses.PhysicalAddressView { City = city, Country = country.Value } },
                            null,
                            true,
                            false,
                            ++page,
                            pageSize);
                        }
                    }
                }
            }
        }

        public StatusCode Run(string[] args)
        {
            var app = 
            var sitemapGenerator = new HomelabSitemapGenerator(app);
        }
    }
}