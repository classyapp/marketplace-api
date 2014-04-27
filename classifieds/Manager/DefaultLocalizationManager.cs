using Classy.Models;
using Classy.Models.Response;
using Classy.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceStack.Common;

namespace classy.Manager
{
    public class DefaultLocalizationManager : ILocalizationManager
    {
        private ILocalizationRepository LocalizationRepository;
        private IProfileRepository ProfileRepository;

        public DefaultLocalizationManager(ILocalizationRepository localizationRepository, IProfileRepository profileRepository)
        {
            LocalizationRepository = localizationRepository;
            ProfileRepository = profileRepository;
        }

        public LocalizationResourceView GetResourceByKey(string appId, string key, bool processMarkdown = true)
        {
            var resource = LocalizationRepository.GetResourceByKey(appId, key);
            if (resource != null && processMarkdown)
            {
                var keys = new List<string>(resource.Values.Keys);
                foreach(var k in keys)
                {
                    resource.Values[k] = resource.Values[k].Contains("\r\n") ? (new MarkdownSharp.Markdown()).Transform(resource.Values[k]) : resource.Values[k];
                }
            }

            return resource == null ? null : resource.TranslateTo<LocalizationResourceView>();
        }

        public LocalizationResourceView SetResourceValues(string appId, string key, IDictionary<string, string> values)
        {
            // get the respource and set new values
            var resource = LocalizationRepository.GetResourceByKey(appId, key);
            if (resource == null)
            {
                resource = new LocalizationResource
                {
                    AppId = appId,
                    Key = key,
                    Values = values
                };
            }
            else
            {
                if (resource.Values == null) resource.Values = new Dictionary<string, string>();
                foreach (var k in values.Keys)
                {
                    var val = HtmlUtilities.RemoveTags(values[k]);
                    if (resource.Values.ContainsKey(k)) resource.Values[k] = val;
                    else resource.Values.Add(k, val);
                }
            }
            // save 
            LocalizationRepository.SetResource(resource);
            // return
            return resource.TranslateTo<LocalizationResourceView>();
        }


        public LocalizationListResourceView GetListResourceByKey(string appId, string key)
        {
            var listResource = LocalizationRepository.GetListResourceByKey(appId, key);
            return listResource.ToLocalizationListResourceView();
        }

        public LocalizationListResourceView SetListResourceValues(string appId, string key, IList<ListItem> listItems)
        {
            var listResource = LocalizationRepository.GetListResourceByKey(appId, key);
            if (listResource == null)
            {
                listResource = new LocalizationListResource
                {
                    AppId = appId,
                    Key = key,
                    ListItems = listItems
                };
            }
            else
            {
                if (listResource.ListItems == null) listResource.ListItems = new List<ListItem>();
                foreach (var item in listItems)
                {
                    if (listResource.ListItems.Any(x => x.Value == item.Value))
                    {
                        var existingItem = listResource.ListItems.Single(x => x.Value == item.Value);
                        foreach(var e in item.Text)
                        {
                            if (existingItem.Text.ContainsKey(e.Key)) existingItem.Text[e.Key] = e.Value;
                            else existingItem.Text.Add(e);
                        }
                    }
                    else listResource.ListItems.Add(item);
                }
            }
            // save 
            LocalizationRepository.SetListResource(listResource);
            // return
            return listResource.ToLocalizationListResourceView();
        }

        public IList<LocalizationResourceView> GetResourcesForApp(string appId)
        {
            return LocalizationRepository.GetResourcesForApp(appId).TranslateTo<List<LocalizationResourceView>>();
        }

        public IList<string> GetCitiesByCountry(string appId, string countryCode)
        {
            return ProfileRepository.GetDistinctCitiesByCountry(appId, countryCode);
        }
    }
}