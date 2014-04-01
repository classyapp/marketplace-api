using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace classy.Manager
{
    public class MandrillEmailManager : IEmailManager
    {
        public EmailResult SendHtmlMessage(string apiKey, string replyTo, string[] to, string subject, string body, string template,  Dictionary<string, string> variables)
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

            EmailResult result = new EmailResult();
            result.Status = results.Any(r => r.Status == Mandrill.EmailResultStatus.Rejected || r.Status == Mandrill.EmailResultStatus.Invalid) ? EmailResultStatus.Failed : EmailResultStatus.Sent;
            if (result.Status == EmailResultStatus.Failed)
            {
                foreach (var item in results)
                {
                    result.Reason += string.IsNullOrEmpty(item.RejectReason) ? string.Empty : (item.RejectReason + "\r\n");
                }
            }

            return result;
        }
    }
}