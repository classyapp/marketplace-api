﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using ServiceStack.Common;
using ServiceStack.Text;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using ServiceStack.ServiceInterface.Auth;

namespace Classy.Auth
{
    public class MongoDBAuthRepository : IUserAuthRepository, IClearable
    {
        // http://www.mongodb.org/display/DOCS/How+to+Make+an+Auto+Incrementing+Field
        class Counters
        {
            public int Id { get; set; }
            public int UserAuthCounter { get; set; }
            public int UserOAuthProviderCounter { get; set; }
        }

        //http://stackoverflow.com/questions/3588623/c-sharp-regex-for-a-username-with-a-few-restrictions
        public Regex ValidUserNameRegEx = new Regex(@"^(?=.{3,45}$)([A-Za-z0-9][._-]?)*$", RegexOptions.Compiled);

        private readonly MongoDatabase mongoDatabase;

        // UserAuth collection name
        private static string UserAuth_Col
        {
            get
            {
                return typeof(UserAuth).Name;
            }
        }
        // UserOAuthProvider collection name
        private static string UserOAuthProvider_Col
        {
            get
            {
                return typeof(UserOAuthProvider).Name;
            }
        }
        // Counters collection name
        private static string Counters_Col
        {
            get
            {
                return typeof(Counters).Name;
            }
        }

        public MongoDBAuthRepository(bool createMissingCollections)
        {
            var client = new MongoClient("mongodb://localhost");
            var server = client.GetServer();
            var db = server.GetDatabase("classifieds");

            this.mongoDatabase = db;

            if (createMissingCollections)
            {
                CreateMissingCollections();
            }

            if (!CollectionsExists())
            {
                throw new InvalidOperationException("One of the collections needed by MongoDBAuthRepository is missing." +
                                                    "You can call MongoDBAuthRepository constructor with the parameter CreateMissingCollections set to 'true'  " +
                                                    "to create the needed collections.");
            }
        }
        public bool CollectionsExists()
        {
            return (mongoDatabase.CollectionExists(UserAuth_Col))
                    && (mongoDatabase.CollectionExists(UserOAuthProvider_Col))
                    && (mongoDatabase.CollectionExists(Counters_Col));

        }

        public void CreateMissingCollections()
        {
            if (!mongoDatabase.CollectionExists(UserAuth_Col))
                mongoDatabase.CreateCollection(UserAuth_Col);

            if (!mongoDatabase.CollectionExists(UserOAuthProvider_Col))
                mongoDatabase.CreateCollection(UserOAuthProvider_Col);

            if (!mongoDatabase.CollectionExists(Counters_Col))
            {
                mongoDatabase.CreateCollection(Counters_Col);

                var CountersCollection = mongoDatabase.GetCollection<Counters>(Counters_Col);
                Counters counters = new Counters();
                CountersCollection.Save(counters);
            }
        }

        public void DropAndReCreateCollections()
        {
            if (mongoDatabase.CollectionExists(UserAuth_Col))
                mongoDatabase.DropCollection(UserAuth_Col);

            if (mongoDatabase.CollectionExists(UserOAuthProvider_Col))
                mongoDatabase.DropCollection(UserOAuthProvider_Col);

            if (mongoDatabase.CollectionExists(Counters_Col))
                mongoDatabase.DropCollection(Counters_Col);

            CreateMissingCollections();
        }

        private void ValidateNewUser(UserAuth newUser, string password)
        {
            newUser.ThrowIfNull("newUser");
            password.ThrowIfNullOrEmpty("password");

            if (newUser.UserName.IsNullOrEmpty() && newUser.Email.IsNullOrEmpty())
                throw new ArgumentNullException("UserName or Email is required");

            if (!newUser.UserName.IsNullOrEmpty())
            {
                if (!ValidUserNameRegEx.IsMatch(newUser.UserName))
                    throw new ArgumentException("UserName contains invalid characters", "UserName");
            }
        }



        public UserAuth CreateUserAuth(UserAuth newUser, string password)
        {
            ValidateNewUser(newUser, password);

            AssertNoExistingUser(mongoDatabase, newUser);

            var saltedHash = new SaltedHash();
            string salt;
            string hash;
            saltedHash.GetHashAndSaltString(password, out hash, out salt);
            var digestHelper = new DigestAuthFunctions();
            newUser.DigestHA1Hash = digestHelper.CreateHa1(newUser.UserName, DigestAuthProvider.Realm, password);
            newUser.PasswordHash = hash;
            newUser.Salt = salt;
            newUser.CreatedDate = DateTime.UtcNow;
            newUser.ModifiedDate = newUser.CreatedDate;

            SaveUser(newUser);
            return newUser;
        }

        private void SaveUser(UserAuth userAuth)
        {
            if (userAuth.Id == default(int))
                userAuth.Id = IncUserAuthCounter();
            var usersCollection = mongoDatabase.GetCollection<UserAuth>(UserAuth_Col);
            usersCollection.Save(userAuth);
        }

        private int IncUserAuthCounter()
        {
            return IncCounter("UserAuthCounter").UserAuthCounter;
        }

        private int IncUserOAuthProviderCounter()
        {
            return IncCounter("UserOAuthProviderCounter").UserOAuthProviderCounter;
        }


        private Counters IncCounter(string counterName)
        {
            var CountersCollection = mongoDatabase.GetCollection<Counters>(Counters_Col);
            var incId = Update.Inc(counterName, 1);
            var query = Query.Null;
            FindAndModifyResult counterIncResult = CountersCollection.FindAndModify(query, SortBy.Null, incId, true);
            Counters updatedCounters = counterIncResult.GetModifiedDocumentAs<Counters>();
            return updatedCounters;
        }
        private static void AssertNoExistingUser(MongoDatabase mongoDatabase, UserAuth newUser, UserAuth exceptForExistingUser = null)
        {
            if (newUser.UserName != null)
            {
                var existingUser = GetUserAuthByUserName(mongoDatabase, newUser.AppId, newUser.UserName);
                if (existingUser != null
                    && (exceptForExistingUser == null || existingUser.Id != exceptForExistingUser.Id))
                    throw new ArgumentException("User {0} already exists".Fmt(newUser.UserName));
            }
            if (newUser.Email != null)
            {
                var existingUser = GetUserAuthByUserName(mongoDatabase, newUser.AppId, newUser.Email);
                if (existingUser != null
                    && (exceptForExistingUser == null || existingUser.Id != exceptForExistingUser.Id))
                    throw new ArgumentException("Email {0} already exists".Fmt(newUser.Email));
            }
        }

        public UserAuth UpdateUserAuth(UserAuth existingUser, UserAuth newUser, string password)
        {
            ValidateNewUser(newUser, password);

            AssertNoExistingUser(mongoDatabase, newUser, existingUser);

            var hash = existingUser.PasswordHash;
            var salt = existingUser.Salt;
            if (password != null)
            {
                var saltedHash = new SaltedHash();
                saltedHash.GetHashAndSaltString(password, out hash, out salt);
            }
            // If either one changes the digest hash has to be recalculated
            var digestHash = existingUser.DigestHA1Hash;
            if (password != null || existingUser.UserName != newUser.UserName)
            {
                var digestHelper = new DigestAuthFunctions();
                digestHash = digestHelper.CreateHa1(newUser.UserName, DigestAuthProvider.Realm, password);
            }
            newUser.Id = existingUser.Id;
            newUser.PasswordHash = hash;
            newUser.Salt = salt;
            newUser.DigestHA1Hash = digestHash;
            newUser.CreatedDate = existingUser.CreatedDate;
            newUser.ModifiedDate = DateTime.UtcNow;
            SaveUser(newUser);

            return newUser;
        }

        public UserAuth GetUserAuthByUserName(string appId, string userNameOrEmail)
        {
            return GetUserAuthByUserName(mongoDatabase, appId, userNameOrEmail);
        }

        private static UserAuth GetUserAuthByUserName(MongoDatabase mongoDatabase, string appId, string userNameOrEmail)
        {
            var isEmail = userNameOrEmail.Contains("@");
            var collection = mongoDatabase.GetCollection<UserAuth>(UserAuth_Col);

            IMongoQuery query = isEmail
                ? Query.EQ("Email", userNameOrEmail)
                : Query.EQ("UserName", userNameOrEmail);

            UserAuth userAuth = collection.FindOne(Query.And(
                query,
                Query.EQ("AppId", appId)));
            return userAuth;
        }

        public bool TryAuthenticate(string appId, string userName, string password, out UserAuth userAuth)
        {
            //userId = null;
            userAuth = GetUserAuthByUserName(appId, userName);
            if (userAuth == null) return false;

            var saltedHash = new SaltedHash();
            if (saltedHash.VerifyHashString(password, userAuth.PasswordHash, userAuth.Salt))
            {
                //userId = userAuth.Id.ToString(CultureInfo.InvariantCulture);
                return true;
            }

            userAuth = null;
            return false;
        }
        public bool TryAuthenticate(string appId, Dictionary<string, string> digestHeaders, string PrivateKey, int NonceTimeOut, string sequence, out UserAuth userAuth)
        {
            //userId = null;
            userAuth = GetUserAuthByUserName(appId, digestHeaders["username"]);
            if (userAuth == null) return false;

            var digestHelper = new DigestAuthFunctions();
            if (digestHelper.ValidateResponse(digestHeaders, PrivateKey, NonceTimeOut, userAuth.DigestHA1Hash, sequence))
            {
                //userId = userAuth.Id.ToString(CultureInfo.InvariantCulture);
                return true;
            }
            userAuth = null;
            return false;
        }

        public void LoadUserAuth(IAuthSession session, IOAuthTokens tokens)
        {
            session.ThrowIfNull("session");

            var userAuth = GetUserAuth(session, tokens);
            LoadUserAuth(session, userAuth);
        }

        private void LoadUserAuth(IAuthSession session, UserAuth userAuth)
        {
            if (userAuth == null) return;

            session.PopulateWith(userAuth);
            session.UserAuthId = userAuth.Id.ToString(CultureInfo.InvariantCulture);
            session.ProviderOAuthAccess = GetUserOAuthProviders(session.GetEnvironment().AppId, session.UserAuthId)
                .ConvertAll(x => (IOAuthTokens)x);

        }

        public UserAuth GetUserAuth(string appId, string userAuthId)
        {
            var collection = mongoDatabase.GetCollection<UserAuth>(UserAuth_Col);
            UserAuth userAuth = collection.FindOne(Query<UserAuth>.Where(x => x.Id == int.Parse(userAuthId) && x.AppId == appId));
            return userAuth;
        }

        public void SaveUserAuth(IAuthSession authSession)
        {
            var userAuth = !authSession.UserAuthId.IsNullOrEmpty()
                ? GetUserAuth(authSession.GetEnvironment().AppId, authSession.UserAuthId)
                : authSession.TranslateTo<UserAuth>();

            if (userAuth.Id == default(int) && !authSession.UserAuthId.IsNullOrEmpty())
                userAuth.Id = int.Parse(authSession.UserAuthId);

            userAuth.ModifiedDate = DateTime.UtcNow;
            if (userAuth.CreatedDate == default(DateTime))
                userAuth.CreatedDate = userAuth.ModifiedDate;

            var collection = mongoDatabase.GetCollection<UserAuth>(UserAuth_Col);
            SaveUser(userAuth);
        }

        public void SaveUserAuth(UserAuth userAuth)
        {
            userAuth.ModifiedDate = DateTime.UtcNow;
            if (userAuth.CreatedDate == default(DateTime))
                userAuth.CreatedDate = userAuth.ModifiedDate;

            SaveUser(userAuth);
        }

        public List<UserOAuthProvider> GetUserOAuthProviders(string appId, string userAuthId)
        {
            var id = int.Parse(userAuthId);

            IMongoQuery query = Query.And(
                Query.EQ("UserAuthId", int.Parse(userAuthId)),
                Query.EQ("AppId", appId)
                );

            var collection = mongoDatabase.GetCollection<UserOAuthProvider>(UserOAuthProvider_Col);
            MongoCursor<UserOAuthProvider> queryResult = collection.Find(query);
            return queryResult.ToList();

        }

        public UserAuth GetUserAuth(IAuthSession authSession, IOAuthTokens tokens)
        {
            if (!authSession.UserAuthId.IsNullOrEmpty())
            {
                var userAuth = GetUserAuth(authSession.GetEnvironment().AppId, authSession.UserAuthId);
                if (userAuth != null) return userAuth;
            }
            if (!authSession.UserAuthName.IsNullOrEmpty())
            {
                var userAuth = GetUserAuthByUserName(authSession.GetEnvironment().AppId, authSession.UserAuthName);
                if (userAuth != null) return userAuth;
            }

            if (tokens == null || tokens.Provider.IsNullOrEmpty() || tokens.UserId.IsNullOrEmpty())
                return null;

            var query = Query.And(
                            Query.EQ("Provider", tokens.Provider),
                            Query.EQ("UserId", tokens.UserId),
                            Query.EQ("AppId", authSession.GetEnvironment().AppId)
                        );

            var providerCollection = mongoDatabase.GetCollection<UserOAuthProvider>(UserOAuthProvider_Col);
            var oAuthProvider = providerCollection.FindOne(query);


            if (oAuthProvider != null)
            {
                var userAuthCollection = mongoDatabase.GetCollection<UserAuth>(UserAuth_Col);
                var userAuth = userAuthCollection.FindOneById(oAuthProvider.UserAuthId);
                return userAuth;
            }
            return null;
        }

        public string CreateOrMergeAuthSession(IAuthSession authSession, IOAuthTokens tokens)
        {
            var userAuth = GetUserAuth(authSession, tokens) ?? new UserAuth();

            var query = Query.And(
                            Query.EQ("Provider", tokens.Provider),
                            Query.EQ("UserId", tokens.UserId),
                            Query.EQ("AppId", authSession.GetEnvironment().AppId)
                        );
            var providerCollection = mongoDatabase.GetCollection<UserOAuthProvider>(UserOAuthProvider_Col);
            var oAuthProvider = providerCollection.FindOne(query);

            if (oAuthProvider == null)
            {
                oAuthProvider = new UserOAuthProvider
                {
                    Provider = tokens.Provider,
                    UserId = tokens.UserId,
                    AppId = authSession.GetEnvironment().AppId
                };
            }

            oAuthProvider.PopulateMissing(tokens);
            userAuth.PopulateMissing(oAuthProvider);

            userAuth.ModifiedDate = DateTime.UtcNow;
            if (userAuth.CreatedDate == default(DateTime))
                userAuth.CreatedDate = userAuth.ModifiedDate;

            SaveUser(userAuth);

            if (oAuthProvider.Id == default(int))
                oAuthProvider.Id = IncUserOAuthProviderCounter();

            oAuthProvider.UserAuthId = userAuth.Id;

            if (oAuthProvider.CreatedDate == default(DateTime))
                oAuthProvider.CreatedDate = userAuth.ModifiedDate;
            oAuthProvider.ModifiedDate = userAuth.ModifiedDate;

            providerCollection.Save(oAuthProvider);

            return oAuthProvider.UserAuthId.ToString(CultureInfo.InvariantCulture);
        }

        public void Clear()
        {
            DropAndReCreateCollections();
        }
    }
}