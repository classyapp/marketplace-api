using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Classy.Models;

namespace classy.Manager
{
    public class ManagerSecurityContext
    {
        public bool IsAuthenticated { get; set; }
        public string AuthenticatedProfileId { get; set; }
        public bool IsAdmin { get; set; }
    }

    public interface IManager
    {
        ManagerSecurityContext SecurityContext { get; set; }
        Env Environment { get; set; }
    }
}
