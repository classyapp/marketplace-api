using Classy.Models;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Classy.Repository
{
    public class MongoTripleStore : ITripleStore
    {
        static MongoClient Client = new MongoClient("mongodb://localhost");
        static MongoServer Server;
        static MongoDatabase Db;
        static MongoCollection<Triple> TripleCollection;

        static MongoTripleStore()
        {
            Server = Client.GetServer();
            Db = Server.GetDatabase("classifieds");
            TripleCollection = Db.GetCollection<Triple>("triples");
        }

        public Triple LogActivity(string appId, string subjectObjectId, ActivityPredicate predicate, string objectObjectId, ref bool tripleAlreadyExists)
        {
            var triple = new Triple
            {
                AppId = appId,
                SubjectId = subjectObjectId,
                Predicate = predicate.ToString(),
                ObjectId = objectObjectId
            };

            var query = Query.And(new IMongoQuery[] {
                Query<Triple>.EQ(x => x.AppId, appId),
                Query<Triple>.EQ(x => x.SubjectId, subjectObjectId),
                Query<Triple>.EQ(x => x.Predicate, predicate.ToString()),
                Query<Triple>.EQ(x => x.ObjectId, objectObjectId)
            });
            var existingTriple = TripleCollection.FindOne(query);
            if (existingTriple != null)
            {
                tripleAlreadyExists = true;
                return null;
            }

            TripleCollection.Save(triple);
            return triple;
        }

        public IList<string> GetActivitySubjectList(string appId, ActivityPredicate predicate, string objectObjectId)
        {
            var query = Query.And(
                    Query<Triple>.EQ(x => x.AppId, appId),
                    Query<Triple>.EQ(x => x.Predicate, predicate.ToString()),
                    Query<Triple>.EQ(x => x.ObjectId, objectObjectId)
                );
            var subjectList = (from t in TripleCollection.Find(query) select t.SubjectId).ToList();
            return subjectList;
        }

        public IList<string> GetActivityObjectList(string appId, ActivityPredicate predicate, string subjectObjectId)
        {
            var query = Query.And(
                    Query<Triple>.EQ(x => x.AppId, appId),
                    Query<Triple>.EQ(x => x.Predicate, predicate.ToString()),
                    Query<Triple>.EQ(x => x.SubjectId, subjectObjectId)
                );
            var objectList = (from t in TripleCollection.Find(query) select t.ObjectId).ToList();
            return objectList;
        }
    }
}