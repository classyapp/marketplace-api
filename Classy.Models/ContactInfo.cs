using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Classy.Models
{
    public class ContactInfo
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }

        public Location Location { get; set; }

        public string WebsiteUrl { get; set; }
        public string FacebookUrl { get; set; }
        public string TwitterUsername { get; set; }
        public string LinkedInProfileUrl { get; set; }
    }
}
