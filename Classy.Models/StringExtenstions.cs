using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Classy.Models
{
    public static class StringExtenstions
    {
        public static string FormatAsHtml(this string from)
        {
            // link tags
            var tagRegex = new Regex(@"\B#\w\w+");
            from = tagRegex.Replace(from, delegate(Match match)
            {
                return string.Format("<a href=\"/tag/{0}\">#{0}</a>", match.Value.TrimStart('#'));
            });

            // link users
            var userRegex = new Regex(@"\B@\w\w+");
            from = userRegex.Replace(from, delegate(Match match)
            {
                return string.Format("<a href=\"/profile/{0}\">@{0}</a>", match.Value.TrimStart('@'));
            });

            // TODO: link links :)

            return from;
        }

        public static string[] ExtractHashtags(this string from)
        {
            var tagRegex = new Regex(@"\B#\w\w+");
            var hashtags = tagRegex.Matches(from)
                .Cast<Match>()
                .Select(m => m.Value)
                .ToArray();
            return hashtags;
        }

        public static string[] ExtractUsernames(this string from)
        {
            var tagRegex = new Regex(@"\B@\w\w+");
            var usernames = tagRegex.Matches(from)
                .Cast<Match>()
                .Select(m => m.Value)
                .ToArray();
            return usernames;
        }
    }
}