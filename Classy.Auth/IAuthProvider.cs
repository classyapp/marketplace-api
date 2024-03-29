﻿using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Auth;
using System.Collections.Generic;

namespace Classy.Auth
{
    public interface IAuthProvider
    {
        string AuthRealm { get; set; }
        string Provider { get; set; }
        string CallbackUrl { get; set; }

        /// <summary>
        /// Remove the Users Session
        /// </summary>
        /// <param name="service"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        object Logout(IServiceBase service, Auth request);

        /// <summary>
        /// The entry point for all AuthProvider providers. Runs inside the AuthService so exceptions are treated normally.
        /// Overridable so you can provide your own Auth implementation.
        /// </summary>
        object Authenticate(IServiceBase authService, IAuthSession session, Auth request);

        /// <summary>
        /// Determine if the current session is already authenticated with this AuthProvider
        /// </summary>
        bool IsAuthorized(IAuthSession session, IOAuthTokens tokens, Auth request = null);
    }

    public interface IOAuthProvider : IAuthProvider
    {
        IAuthHttpGateway AuthHttpGateway { get; set; }
        string ConsumerKey { get; set; }
        string ConsumerSecret { get; set; }
        string RequestTokenUrl { get; set; }
        string AuthorizeUrl { get; set; }
        string AccessTokenUrl { get; set; }
    }
}