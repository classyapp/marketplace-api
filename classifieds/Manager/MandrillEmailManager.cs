﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Classy.Interfaces.Managers;

namespace classy.Manager
{
    public class MandrillEmailManager : IEmailManager
    {
        private IAppManager AppManager;

        public MandrillEmailManager(IAppManager appManager)
        {
            AppManager = appManager;
        }

        public EmailResult SendHtmlMessage(string apiKey, string replyTo, string[] to, string subject, string body, string template, Dictionary<string, string> variables)
        {
            List<Mandrill.EmailResult> results = null;

            // check body direction and correct if needed
            System.Text.RegularExpressions.Regex dirRegex = new System.Text.RegularExpressions.Regex("[\\p{IsHebrew}]");
            if (dirRegex.IsMatch(body.Substring(0, Math.Min(body.Length, 10))))
            {
                body = "<div style=\"direction: rtl\">" + body + "</div>";
            }

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
                message.from_email = replyTo;
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