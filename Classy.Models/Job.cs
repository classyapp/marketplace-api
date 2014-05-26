using System.Collections.Generic;
using Classy.Models.Attributes;

namespace Classy.Models
{
    [MongoCollection(Name = "jobs")]
    public class Job : BaseObject
    {
        public class JobType
        {
            public static readonly string ImportProductsCatalog = "IMPORT_PRODUCTS_CATALOG";
        }

        public string ProfileId { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public MediaFile[] Attachments { get; set; }
        public Dictionary<string, object> Properties { get; set; }
        public string ProgressInfo { get; set; }
        public List<string> ImportErrors { get; set; }
    }
}