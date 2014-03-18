using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classy.Models
{
    public class ProfileTranslation : BaseObject 
    {
        public string ProfileId { get; set; }
        public string Culture { get; set; }
        public IDictionary<string, string> Metadata { get; set; }
    }
}
