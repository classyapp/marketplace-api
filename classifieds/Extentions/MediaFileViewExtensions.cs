﻿using Classy.Models;
using Classy.Models.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceStack.Common;
using Classy.Repository;

namespace classy
{
    public static class MediaFileExtentions
    {
        public static MediaFileView ToMediaFileView(this MediaFile from)
        {
            var to = from.TranslateTo<MediaFileView>();
            return to;
        }

        public static IList<MediaFileView> ToMediaFileList(this IEnumerable<MediaFile> from)
        {
            var to = new List<MediaFileView>();
            foreach (var f in from)
            {
                to.Add(f.ToMediaFileView());
            }
            return to;
        }
    }
}