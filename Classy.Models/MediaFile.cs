using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Classy.Models
{
    [Flags]
    public enum MediaFileType
    {
        Image = 1 << 0
    }

    // Added for the image size reduction utility
    [MongoDB.Bson.Serialization.Attributes.BsonIgnoreExtraElements]
    public class MediaFile
    {
        public MediaFileType Type { get; set; }
        public string Key { get; set; }
        public string ContentType { get; set; }
        public string Url { get; set; }
    }
}
