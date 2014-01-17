using Classy.Models;
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
            if (from.Thumbnails.Count() > 0)
            {
                to.Thumbnails = new List<MediaThumbnailView>();
                foreach (var t in from.Thumbnails)
                {
                    to.Thumbnails.Add(t.TranslateTo<MediaThumbnailView>());
                }
            }
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