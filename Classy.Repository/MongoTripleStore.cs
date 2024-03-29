﻿using Classy.Models;
using Classy.Repository.Infrastructure;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System.Collections.Generic;
using System.Linq;

namespace Classy.Repository
{
    public class MongoTripleStore : ITripleStore
    {
        private MongoCollection<Triple> TripleCollection;

        public MongoTripleStore(MongoDatabaseProvider db)
        {
            TripleCollection = db.GetCollection<Triple>();
        }

        public Triple LogActivity(string appId, string subjectObjectId, string predicate, string objectObjectId, Dictionary<string, string> metadata, ref int count)
        {
            count = 1;
            var triple = new Triple
            {
                AppId = appId,
                SubjectId = subjectObjectId,
                Predicate = predicate.ToString(),
                ObjectId = objectObjectId,
                Metadata = metadata,
                Count = count
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
                triple.Id = existingTriple.Id;
                triple.Count = ++existingTriple.Count;
                count = triple.Count;
            }

            TripleCollection.Save(triple);
            return triple;
        }

        public Triple GetLogActivity(string appId, string subjectId, string predicate, string objectId)
        {
            var query = Query.And(new IMongoQuery[] {
                Query<Triple>.EQ(x => x.AppId, appId),
                Query<Triple>.EQ(x => x.SubjectId, subjectId),
                Query<Triple>.EQ(x => x.Predicate, predicate.ToString()),
                Query<Triple>.EQ(x => x.ObjectId, objectId)
            });
            return TripleCollection.FindOne(query);
        }

        public void DeleteActivity(string appId, string subjectObjectId, string predicate, string objectObjectId, ref int count)
        {
            var query = Query.And(new IMongoQuery[] {
                Query<Triple>.EQ(x => x.AppId, appId),
                Query<Triple>.EQ(x => x.SubjectId, subjectObjectId),
                Query<Triple>.EQ(x => x.Predicate, predicate.ToString()),
                Query<Triple>.EQ(x => x.ObjectId, objectObjectId)
            });
            var existingTriple = TripleCollection.FindOne(query);
            if (existingTriple != null)
            {
                existingTriple.Count--;
                TripleCollection.Save(existingTriple);
            }
        }

        public void ResetActivity(string appId, string subjectObjectId, string predicate, string objectObjectId)
        {
            var query = Query.And(new IMongoQuery[] {
                Query<Triple>.EQ(x => x.AppId, appId),
                Query<Triple>.EQ(x => x.SubjectId, subjectObjectId),
                Query<Triple>.EQ(x => x.Predicate, predicate.ToString()),
                Query<Triple>.EQ(x => x.ObjectId, objectObjectId)
            });
            var existingTriple = TripleCollection.FindOne(query);
            if (existingTriple != null)
            {
                existingTriple.Count = 0;
                TripleCollection.Save(existingTriple);
            }
        }

        public IList<string> GetActivitySubjectList(string appId, string predicate, string objectObjectId)
        {
            var query = Query<Triple>.Where(x => 
                    x.AppId == appId &&
                    x.Predicate == predicate.ToString() && 
                    x.ObjectId == objectObjectId &&
                    x.Count > 0);
            var subjectList = (from t in TripleCollection.Find(query) select t.SubjectId).ToList();
            return subjectList;
        }

        public IList<string> GetActivityObjectList(string appId, string predicate, string subjectObjectId)
        {
            var query = Query<Triple>.Where(x => 
                    x.AppId == appId &&
                    x.Predicate == predicate.ToString() &&
                    x.SubjectId == subjectObjectId &&
                    x.Count > 0);
            var objectList = (from t in TripleCollection.Find(query) select t.ObjectId).ToList();
            return objectList;
        }
    }
}