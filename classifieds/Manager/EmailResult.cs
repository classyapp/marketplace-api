using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace classy.Manager
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
