using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace classy.Operations
{
    public class ImportProductCatalogJob
    {
        public string JobId { get; set; }

        public ImportProductCatalogJob(string jobId)
        {
            this.JobId = jobId;
        }
    }
}
