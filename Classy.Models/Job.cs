using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classy.Models
{
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
    }
}