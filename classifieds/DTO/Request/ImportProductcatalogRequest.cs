using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Classy.Models.Request
{
    public class ImportProductCatalogRequest
    {
        public string JobId { get; set; }

        public ImportProductCatalogRequest(string jobId)
        {
            this.JobId = jobId;
        }
    }
}