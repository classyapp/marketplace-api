using ServiceStack.ServiceInterface.Auth;
using System.Collections.Generic;

namespace Classy.Auth
{
    public interface IUserAuthRepository
    {
        UserAuth CreateUserAuth(UserAuth newUser, string password);
        UserAuth UpdateUserAuth(UserAuth existingUser, UserAuth newUser, string password);
        UserAuth GetUserAuthByUserName(string appId, string userNameOrEmail);
        UserAuth GetUserAuthByResetHash(string appId, string resetHash);
        bool TryAuthenticate(string appId, string userName, string password, out UserAuth userAuth);
        bool TryAuthenticate(string appId, Dictionary<string, string> digestHeaders, string PrivateKey, int NonceTimeOut, string sequence, out UserAuth userAuth);
        void LoadUserAuth(IAuthSession session, IOAuthTokens tokens);
        UserAuth GetUserAuth(string appId, string userAuthId);
        void SaveUserAuth(IAuthSession authSession);
        void SaveUserAuth(UserAuth userAuth);
        List<UserOAuthProvider> GetUserOAuthProviders(string appId, string userAuthId);
        UserAuth GetUserAuth(IAuthSession authSession, IOAuthTokens tokens);
        string CreateOrMergeAuthSession(IAuthSession authSession, IOAuthTokens tokens);
        void ResetUserPassword(UserAuth userAuth, string password);
    }
}