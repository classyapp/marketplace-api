using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using classy.DTO.Request;
using Classy.Models;
using Classy.UtilRunner;
using MongoDB.Driver;
using Funq;
using classy.Manager;
using Classy.Models.Response;
using Classy.Repository;

namespace Classy.UtilRunner.Utilities.SitemapBuilders
{
    public class HomelabSitemapGenerator : BaseSitemapIndexGenerator
    {
        private string[] _supportedCultures = { "fr", "en", "he", "nl" };
        private int _count = 0;

        public HomelabSitemapGenerator(
            App app,
            IListingManager listingService,
            IProfileRepository profileService,
            ILocalizationManager localizationService)
        {
            _app = app;
            _listingService = listingService;
            _profileService = profileService;
            _localizationService = localizationService;
        }

        protected override void GenerateUrlNodes()
        {
            GenerateStaticNodes();
            GenerateListingNodes();
            GenerateProfessionalNodes();

            Console.WriteLine(string.Format("Finished writing {0} URLs.", _count));
        }

        public void GenerateStaticNodes()
        {
            foreach(var culture in _supportedCultures)
            {
                WriteUrlLocation(culture.ToLower(), UpdateFrequency.Daily, DateTime.UtcNow);
                WriteUrlLocation(culture.ToLower() + "/careers", UpdateFrequency.Weekly, DateTime.UtcNow);
                WriteUrlLocation(culture.ToLower() + "/terms", UpdateFrequency.Monthly, DateTime.UtcNow);
                WriteUrlLocation(culture.ToLower() + "/privacy", UpdateFrequency.Monthly, DateTime.UtcNow);
            }
            _count += 4;
        }

        public void GenerateListingNodes()
        {
            var keywords = new Dictionary<string, HashSet<string>>();

            // all photos
            foreach (var culture in _supportedCultures)
            {
                WriteUrlLocation(culture.ToLower() + "/photo", UpdateFrequency.Daily, DateTime.UtcNow);
                _count++;
            }

            // room and style combinations
            var rooms = _localizationService.GetListResourceByKey(_app.AppId, "rooms");
            var styles = _localizationService.GetListResourceByKey(_app.AppId, "room-styles");
            foreach(var room in rooms.ListItems)
            {
                foreach(var style in styles.ListItems)
                {
                    // output the room+style combination url
                    Console.WriteLine(string.Format("room: {0}, style: {1}", room.Value, style.Value));
                    foreach (var culture in _supportedCultures)
                    {
                        WriteUrlLocation(culture.ToLower() + "/photo/" + room.Value + "/" + style.Value, UpdateFrequency.Daily, DateTime.UtcNow);
                        _count++;
                    }
                }
            }

            // all individual photo urls
            var page = 1;
            var pageSize = 1000;
            var photos = _listingService.SearchListings(
                _app.AppId,
                null,
                null,
                null,
                new[] { "Photo" },
                null,
                null,
                null,
                null,
                false,
                false,
                page,
                pageSize,
                SortMethod.Popularity,
                "en");
            while (photos != null && photos.Results.Count > 0)
            {
                Console.WriteLine(string.Format("page: {0}, results: {1}", page, photos.Results.Count()));

                foreach (var photo in photos.Results)
                {
                    foreach (var culture in _supportedCultures)
                    {
                        WriteUrlLocation(culture.ToLower() + "/photo/" + photo.Id + "--" + System.Web.HttpUtility.UrlEncode(ToSlug(photo.Title)), UpdateFrequency.Daily, DateTime.UtcNow);
                        _count++;
                        var photoKeywords = photo.TranslatedKeywords != null && photo.TranslatedKeywords.ContainsKey(culture) ? photo.TranslatedKeywords[culture] : null;
                        if (photoKeywords != null)
                        {
                            foreach (var k in photoKeywords)
                            {
                                if (!keywords.ContainsKey(culture.ToLower())) keywords.Add(culture.ToLower(), new HashSet<string>());
                                keywords[culture.ToLower()].Add(k);
                            }
                        }
                    }
                }

                photos = _listingService.SearchListings(
                    _app.AppId,
                    null,
                    null,
                    null,
                    new[] { "Photo" },
                    null,
                    null,
                    null,
                    null,
                    false,
                    false,
                    ++page,
                    pageSize,
                    SortMethod.Popularity,
                    "en");
            }

            // add search routes
            foreach (var culture in _supportedCultures)
            {
                Console.WriteLine(string.Format("Found {0} keywords for culture {1}", keywords[culture.ToLower()].Count, culture.ToLower()));
                foreach(var k in keywords[culture.ToLower()])
                {
                    WriteUrlLocation(culture.ToLower() + "/photo/" + System.Web.HttpUtility.UrlEncode(k.Replace(" ", "-")), UpdateFrequency.Daily, DateTime.UtcNow);
                    _count++;
                }
            }
        }

        public void GenerateProfessionalNodes()
        {
            // all photos
            foreach (var culture in _supportedCultures)
            {
                WriteUrlLocation(culture.ToLower() + "/profile/search", UpdateFrequency.Daily, DateTime.UtcNow);
                _count++;
            }

            // category, city cominations
            var categories = _localizationService.GetListResourceByKey(_app.AppId, "professional-categories");
            var countries = _localizationService.GetListResourceByKey(_app.AppId, "supported-countries");

            foreach (var category in categories.ListItems)
            {
                // output all the categories
                foreach (var culture in _supportedCultures)
                {
                    WriteUrlLocation(culture.ToLower() + "/profile/search/" + category.Value, UpdateFrequency.Daily, DateTime.UtcNow);
                    _count++;
                }

                foreach (var country in countries.ListItems)
                {
                    // output all category + country combinations
                    foreach (var culture in _supportedCultures)
                    {
                        WriteUrlLocation(culture.ToLower() + "/profile/search/" + category.Value + "/" + country.Value, UpdateFrequency.Daily, DateTime.UtcNow);
                        _count++;
                    }

                    var cities = _localizationService.GetCitiesByCountry(_app.AppId, country.Value);
                    foreach (var city in cities)
                    {
                        // output all country + category + city combinations
                        Console.WriteLine(string.Format("category: {0}, city: {1}, country: {2}", category.Value, city, country.Value));
                        foreach (var culture in _supportedCultures)
                        {
                            WriteUrlLocation(culture.ToLower() + "/profile/search/" + category.Value + "/" + city + "/" + country.Value, UpdateFrequency.Daily, DateTime.UtcNow);
                            _count++;
                        }
                    }
                }
            }

            // individual profiles
            var page = 1;
            var pageSize = 1000;
            long count = 0;
            var profiles = _profileService.Search(
                _app.AppId,
                null,
                null,
                null,
                null,
                true,
                false,
                page,
                pageSize,
                ref count,
                "en");

            while (profiles != null && profiles.Count() > 0)
            {
                Console.WriteLine(string.Format("page: {0}, results: {1}", page, profiles.Count()));

                foreach (var profile in profiles)
                {
                    foreach (var culture in _supportedCultures)
                    {
                        WriteUrlLocation(culture.ToLower() + "/profile/" + profile.Id + "/" + System.Web.HttpUtility.UrlEncode(ToSlug(GetProfileName(profile))), UpdateFrequency.Daily, DateTime.UtcNow);
                        _count++;
                    }
                }

                profiles = _profileService.Search(
                    _app.AppId,
                    null,
                    null,
                    null,
                    null,
                    true,
                    false,
                    ++page,
                    pageSize,
                    ref count,
                    "en");
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