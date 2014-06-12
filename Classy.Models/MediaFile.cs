using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Classy.Models.Attributes;

namespace Classy.Models
{
    [Flags]
    public enum MediaFileType
    {
        Image = 1 << 0,
        File = 2
    }

    public class MediaFile
    {
        public MediaFileType Type { get; set; }
        public string Key { get; set; }
        public string ContentType { get; set; }
        public string Url { get; set; }
    }

    [MongoCollection(Name = "mediafiles")]
    public class TempMediaFile : BaseObject
    {
        public MediaFileType Type { get; set; }
        public string Key { get; set; }
        public string ContentType { get; set; }
        public string Url { get; set; }
    }
}
