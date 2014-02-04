﻿using Classy.Models;
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

        public DefaultLocalizationManager(ILocalizationRepository localizationRepository)
        {
            LocalizationRepository = localizationRepository;
        }

        public LocalizationResourceView GetResourceByKey(string appId, string key)
        {
            var resource = LocalizationRepository.GetResourceByKey(appId, key);
            return resource.TranslateTo<LocalizationResourceView>();
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
                    if (resource.Values.ContainsKey(k)) resource.Values[k] = values[k];
                    else resource.Values.Add(k, values[k]);
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
                    if (listResource.ListItems.Any(x => x.Value == item.Value)) listResource.ListItems.Single(x => x.Value == item.Value).Text = item.Text ;
                    else listResource.ListItems.Add(item);
                }
            }
            // save 
            LocalizationRepository.SetListResource(listResource);
            // return
            return listResource.ToLocalizationListResourceView();
        }

        public IList<string> GetResourceKeysForApp(string appId)
        {
            return LocalizationRepository.GetResourceKeysForApp(appId);
        }
    }
}