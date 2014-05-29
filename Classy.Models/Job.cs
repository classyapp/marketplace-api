using System;
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
        public int Succeeded { get; set; }
        public int Failed { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<string> Errors { get; set; }

        public Job()
        {
            Errors = new List<string>();
        }
    }
}