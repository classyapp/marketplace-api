using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Classy.Interfaces.Managers
{
    public enum EmailResultStatus
    {
        Sent,
        Failed
    }

    public class EmailResult
    {
        public EmailResultStatus Status { get; set; }
        public string Reason { get; set; }
    }
}
