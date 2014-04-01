using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace classy.Manager
{
    public interface IEmailManager
    {
        EmailResult SendHtmlMessage(string apiKey, string replyTo, string[] to, string subject, string body, string template, Dictionary<string, string> variables);
    }
}