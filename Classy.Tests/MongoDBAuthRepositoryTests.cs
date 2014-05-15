using Classy.Auth;
using Classy.Models;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classy.Tests
{
    class MongoDBAuthRepositoryTests : MongoDBAuthRepository 
    {
       
        public MongoDBAuthRepositoryTests(MongoDatabase db, bool createMissingCollections) : base(db,createMissingCollections)
        {
            
        }
 
        public override UserAuth RemoveUser(string appId, string userNameOrEmail)
        {
            //var isEmail = userNameOrEmail.Contains("@");
            var collection = mongoDatabase.GetCollection<UserAuth>(UserAuth_Col);


            IMongoQuery query = Query.EQ("UserName", userNameOrEmail);
            
            
            UserAuth userAuth = collection.FindOne(Query.And(
                query,
                Query.EQ("AppId", appId)));


            if (userAuth != null)
            {
                collection.Remove(Query.And(query, Query.EQ("AppId", appId)));

                var profileCollection = mongoDatabase.GetCollection<Profile>("profiles");
                profileCollection.Remove(Query.And(Query.EQ("AppId", appId), Query<Profile>.EQ(x => x.Id, userAuth.Id.ToString())));
            }
            return userAuth;
        }
    }
}
