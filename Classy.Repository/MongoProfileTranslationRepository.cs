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
    public class MongoProfileTranslationRepository : IProfileTranslationRepository
    {
        private MongoCollection<ProfileTranslation> ProfileTranslationsCollection;

        public MongoProfileTranslationRepository(MongoDatabase db)
        {
            ProfileTranslationsCollection = db.GetCollection<ProfileTranslation>("profiletranslations");
        }

        public ProfileTranslation GetById(string appId, string profileId, string culture)
        {
            var query = Query<ProfileTranslation>.Where(p => p.AppId == appId && p.ProfileId == profileId && p.Culture == culture);
            return ProfileTranslationsCollection.FindOne(query);
        }

        public string Insert(ProfileTranslation translation)
        {
            try
            {
                ProfileTranslationsCollection.Insert(translation);
                return translation.Id;
            }
            catch (MongoException)
            {
                throw;
            }
        }

        public void Update(ProfileTranslation translation)
        {
            try
            {
                ProfileTranslationsCollection.Save(translation);
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
                ProfileTranslationsCollection.Remove(Query<ProfileTranslation>.Where(p => p.AppId == appId && p.ProfileId == profileId && p.Culture == culture));
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
                ProfileTranslationsCollection.Remove(Query<ProfileTranslation>.Where(p => p.AppId == appId && p.ProfileId == profileId));
            }
            catch (MongoException)
            {
                throw;
            }
        }
    }
}
