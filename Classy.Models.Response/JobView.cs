using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classy.Models.Response
{
    public class JobView
    {
        public string JobId { get; set; }
        public string Status { get; set; }
        public int Succeeded { get; set; }
        public int Failed { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}