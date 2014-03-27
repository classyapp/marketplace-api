using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classy.Models.Response
{
    public class ProfileTranslationView
    {
        public ProfileTranslationView()
        {
            Metadata = new Dictionary<string, string>();
        }

        public string CultureCode { get; set; }
        public string CompanyName { get; set; }
        public Dictionary<string, string> Metadata { get; set; }
    }
}
