using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace classy
{
    public static class HtmlUtilities
    {
        private static readonly Regex _tags = new Regex("<[^>]*(>|$)",
                                                RegexOptions.Singleline | RegexOptions.ExplicitCapture |
                                                RegexOptions.Compiled);

        /// <summary>
        /// remove any potentially dangerous tags from the provided raw HTML input
        /// </summary>
        public static string RemoveTags(string html)
        {
            if (html.IsNullOrEmpty()) return "";
            return _tags.Replace(html, "");
        }
    }
}