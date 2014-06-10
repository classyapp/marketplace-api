using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Classy.CatalogImportWorker
{
    public class WebClientWithTimeout : WebClient
    {
        private int _timeout = 0;

        public WebClientWithTimeout(int timeout)
        {
            _timeout = timeout;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = base.GetWebRequest(address);
            if (request != null)
            {
                request.Timeout = _timeout;
            }
            return request;
        }
    }
}
