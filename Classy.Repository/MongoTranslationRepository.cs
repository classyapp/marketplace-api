using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Classy.Models;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace Classy.Repository
{
    public class MongoTranslationRepository : ITranslationRepository
    {
        private MongoCollection<Translation> TranslationsCollection;

        public MongoTranslationRepository(MongoDatabase db)
        {
            TranslationsCollection = db.GetCollection<Translation>("translations");
        }

        public Translation GetById(string appId, string profileId, string culture)
        {
            var query = Query<Translation>.Where(p => p.AppId == appId && p.ProfileId == profileId && p.Culture == culture);
            return TranslationsCollection.FindOne(query);
        }

        public string Insert(Translation translation)
        {
            try
            {
                TranslationsCollection.Insert(translation);
                return translation.Id;
            }
            catch (MongoException)
            {
                throw;
            }
        }

        public void Update(Translation translation)
        {
            try
            {
                TranslationsCollection.Save(translation);
            }
            catch (MongoException)
            {
                throw;
            }
        }

        public void Delete(string appId, string profileId, string culture)
        {
            try
            {
                TranslationsCollection.Remove(Query<Translation>.Where(p => p.AppId == appId && p.ProfileId == profileId && p.Culture == culture));
            }
            catch (MongoException)
            {
                throw;
            }
        }

        public void DeleteAll(string appId, string profileId)
        {
            try
            {
                TranslationsCollection.Remove(Query<Translation>.Where(p => p.AppId == appId && p.ProfileId == profileId));
            }
            catch (MongoException)
            {
                throw;
            }
        }
    }
}
