using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace classy.Manager
{
    public class DefaultEmailManager : IEmailManager
    {
        public List<Mandrill.EmailResult> SendHtmlMessage(string apiKey, string replyTo, string[] to, string subject, string body, string template,  Dictionary<string, string> variables)
        {
            List<Mandrill.EmailResult> results = null;

            var message = new Mandrill.EmailMessage
            {
                subject = subject,
                to = to.Select(recipient => new Mandrill.EmailAddress(recipient)).ToList(),
                html = body
            };

            if (!string.IsNullOrEmpty(replyTo))
            {
                message.AddHeader("Reply-To", replyTo);
            }

            if (variables != null)
            {
                foreach (var variable in variables)
                {
                    message.AddGlobalVariable(variable.Key, variable.Value);
                }
            }

            var api = new Mandrill.MandrillApi(apiKey);
            if (string.IsNullOrEmpty(template))
            {
                results = api.SendMessage(message);
            }
            else
            {
                results = api.SendMessage(message, template, null);
            }

            return results;
        }
    }
}