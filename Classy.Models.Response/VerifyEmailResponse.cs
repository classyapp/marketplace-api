using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classy.Models.Response
{
    public class VerifyEmailResponse
    {
        public bool Verified { get; set; }
        public string ErrorMessage { get; set; }
    }
}
