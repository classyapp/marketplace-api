using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Classy.Models;
using Classy.Repository.Infrastructure;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace Classy.Repository
{
    public class MongoTempMediaFileRepository : ITempMediaFileRepository
    {
        private MongoCollection<TempMediaFile> MediaFilesCollection;

        public MongoTempMediaFileRepository(MongoDatabaseProvider db)
        {
            MediaFilesCollection = db.GetCollection<TempMediaFile>();
        }

        public string Save(TempMediaFile mediaFile)
        {
            MediaFilesCollection.Save(mediaFile);
            return mediaFile.Id;
        }

        public void Delete(string appId, string fileId)
        {
            MediaFilesCollection.Remove(Query.And(Query<TempMediaFile>.EQ(m => m.AppId, appId), Query<TempMediaFile>.EQ(m => m.Id, fileId)));
        }

        public TempMediaFile Get(string appId, string fileIdOrKey)
        {
            IMongoQuery query = Query.And(Query<TempMediaFile>.EQ(m => m.AppId, appId), Query.Or(Query<TempMediaFile>.EQ(m => m.Id, fileIdOrKey), Query<TempMediaFile>.EQ(m => m.Key, fileIdOrKey)));
            return MediaFilesCollection.Find(query).FirstOrDefault();
        }
    }
}
