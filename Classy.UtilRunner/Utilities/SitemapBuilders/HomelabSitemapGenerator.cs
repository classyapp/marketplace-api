using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using Classy.Models;
using Classy.UtilRunner;
using MongoDB.Driver;
using Funq;
using classy.Manager;
using Classy.Models.Response;
using Classy.Repository;

namespace Classy.UtilRunner.Utilities.SitemapBuilders
{
    public class HomelabSitemapGenerator : ClassySitemapGenerator
    {
        private readonly MongoCollection<App> _apps;
        private readonly IListingManager _listingService;
        private readonly ILocalizationManager _localizationService;
        private readonly IProfileRepository _profileService;
        private string[] _supportedCultures = { "fr", "en", "he", "nl" };
        private App App { get; set; }

        public HomelabSitemapGenerator(Container container)
        {
            var mongoDatabase = container.Resolve<MongoDatabase>();

            _apps = mongoDatabase.GetCollection<App>("apps");
            App = _apps.FindOneById("5378a5cf488c7623b5a648ab"); // AppId == "v1.0"

            _listingService = container.Resolve<IListingManager>();
            _profileService = container.Resolve<IProfileRepository>();
            _localizationService = container.Resolve<ILocalizationManager>();
        }

        public override void GenerateStaticNodes()
        {
            foreach(var culture in _supportedCultures )
            {
                WriteUrlLocation(culture.ToLower(), UpdateFrequency.Daily, DateTime.UtcNow);
                WriteUrlLocation(culture.ToLower() + "/careers", UpdateFrequency.Weekly, DateTime.UtcNow);
                WriteUrlLocation(culture.ToLower() + "/terms", UpdateFrequency.Monthly, DateTime.UtcNow);
                WriteUrlLocation(culture.ToLower() + "/privacy", UpdateFrequency.Monthly, DateTime.UtcNow);
            }
        }

        public override void GenerateListingNodes()
        {
            // all photos
            foreach (var culture in _supportedCultures)
            {
                WriteUrlLocation(culture.ToLower() + "/photo", UpdateFrequency.Daily, DateTime.UtcNow);
            }

            // room and style combinations
            var rooms = _localizationService.GetListResourceByKey(App.AppId, "rooms");
            var styles = _localizationService.GetListResourceByKey(App.AppId, "room-styles");
            foreach(var room in rooms.ListItems)
            {
                foreach(var style in styles.ListItems)
                {
                    // output the room+style combination url
                    foreach (var culture in _supportedCultures)
                    {
                        WriteUrlLocation(culture.ToLower() + "/photo/" + room.Value + "/" + style.Value, UpdateFrequency.Daily, DateTime.UtcNow);
                    }

                    // all photo urls for this room and style combination
                    var page = 1;
                    var photos = _listingService.SearchListings(
                        App.AppId,
                        null,
                        new string[] { "Photo" },
                        new Dictionary<string, string[]> { { "Room", new string[] { room.Value } }, { "Style", new string[] { style.Value } } },
                        null,
                        null,
                        null,
                        false,
                        false,
                        page, 
                        200,
                        "en");
                    while (photos != null && photos.Results.Count > 0)
                    {
                        foreach (var photo in photos.Results)
                        {
                            foreach (var culture in _supportedCultures)
                            {
                                WriteUrlLocation(culture.ToLower() + "/photo/" + photo.Id + "--" + ToSlug(photo.Title), UpdateFrequency.Daily, DateTime.UtcNow);
                            }
                        }

                        photos = _listingService.SearchListings(
                            App.AppId,
                            null,
                            new string[] { "Photo" },
                            new Dictionary<string, string[]> { { "Room", new string[] { room.Value } }, { "Style", new string[] { style.Value } } },
                            null,
                            null,
                            null,
                            false,
                            false,
                            ++page,
                            200,
                            "en");
                    }
                }
            }
        }

        public override void GenerateProfessionalNodes()
        {
            // all photos
            foreach (var culture in _supportedCultures)
            {
                WriteUrlLocation(culture.ToLower() + "/profile/search", UpdateFrequency.Daily, DateTime.UtcNow);
            }

            // category, city cominations
            var categories = _localizationService.GetListResourceByKey(App.AppId, "professional-categories");
            var countries = _localizationService.GetListResourceByKey(App.AppId, "supported-countries");

            foreach (var category in categories.ListItems)
            {
                // output all the categories
                foreach (var culture in _supportedCultures)
                {
                    WriteUrlLocation(culture.ToLower() + "/profile/search/" + category.Value, UpdateFrequency.Daily, DateTime.UtcNow);
                }

                foreach (var country in countries.ListItems)
                {
                    // output all category + country combinations
                    foreach (var culture in _supportedCultures)
                    {
                        WriteUrlLocation(culture.ToLower() + "/profile/search/" + category.Value + "/" + country.Value, UpdateFrequency.Daily, DateTime.UtcNow);
                    }

                    var cities = _localizationService.GetCitiesByCountry(App.AppId, country.Value);
                    foreach (var city in cities)
                    {
                        // output all country + category + city combinations
                        foreach (var culture in _supportedCultures)
                        {
                            WriteUrlLocation(culture.ToLower() + "/profile/search/" + category.Value + "/" + city + "/" + country.Value, UpdateFrequency.Daily, DateTime.UtcNow);
                        }

                        // all profiles under this combination
                        var page = 1;
                        var pageSize = 500;
                        long count = 0;
                        var profiles = _profileService.Search(
                            App.AppId,
                            null,
                            category.Value,
                            new Location { Address = new PhysicalAddress { City = city, Country = country.Value } },
                            null,
                            true,
                            false,
                            page,
                            pageSize,
                            ref count,
                            "en");

                        while (profiles != null && profiles.Count() > 0)
                        {
                            Console.WriteLine(string.Format("city: {0}, country: {1}, page: {2}, results: {3}", city, country.Value, page, profiles.Count()));

                            foreach (var profile in profiles)
                            {
                                foreach (var culture in _supportedCultures)
                                {
                                    WriteUrlLocation(culture.ToLower() + "/profile/" + profile.Id + "/" + ToSlug(GetProfileName(profile)), UpdateFrequency.Daily, DateTime.UtcNow);
                                }
                            }

                            profiles = _profileService.Search(
                                App.AppId,
                                null,
                                category.Value,
                                new Location{ Address = new PhysicalAddress { City = city, Country = country.Value } },
                                null,
                                true,
                                false,
                                ++page,
                                pageSize,
                                ref count,
                                "en");
                        }
                    }
                }
            }
        }

        private string ToSlug(string content)
        {
            return content != null ? content.ToLower()
                .Replace("?", string.Empty)
                .Replace("-", string.Empty)
                .Replace("/", string.Empty)
                .Replace("&", "-and-")
                .Replace("+", "-and-")
                .Replace(".", string.Empty)
                .Replace("  ", " ")
                .Replace(" ", "-") : null;
        }

        private string GetProfileName(Profile profile)
        {
            string name = null;
            if (string.IsNullOrEmpty(profile.ContactInfo.FirstName) && string.IsNullOrEmpty(profile.ContactInfo.LastName)) name = null;
            else name = string.Concat(profile.ContactInfo.FirstName, " ", profile.ContactInfo.LastName);
            if (profile.ContactInfo == null && !profile.IsProfessional) return "unknown";
            string output;
            if (profile.IsProxy) output = profile.ProfessionalInfo.CompanyName;
            else if (profile.IsProfessional) output = profile.ProfessionalInfo.CompanyName;
            else output = string.IsNullOrWhiteSpace(name) ? profile.UserName : name;
            return output ?? "unknown";
        }
    }
}