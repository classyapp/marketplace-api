using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Classy.Models;

namespace Classy.Repository
{
    public interface IJobRepository
    {
        Job GetById(string appId, string jobId);
        void Save(Job job);
        IEnumerable<Job> GetByProfileId(string appId, string profileId);
    }
}