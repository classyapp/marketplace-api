using classy.DTO.Request.Profile;
using classy.Operations;
using ServiceStack.Common.Web;
using ServiceStack.Messaging;
using ServiceStack.ServiceInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace classy.Services.Profile
{
    public class ProfileService : ServiceStack.ServiceInterface.Service
    {
        private readonly IMessageQueueClient _messageQueueClient;
        public ProfileService(IMessageQueueClient messageQueueClient)
        {
            _messageQueueClient = messageQueueClient;
        }
        [CustomAuthenticate]
        public object Post(RequestCustomerReview request)
        {
            try
            {
                _messageQueueClient.Publish<RequestCustomerReview>(request);
                return new HttpResult(new { }, HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                return new HttpError();
            }
        }
    }
}