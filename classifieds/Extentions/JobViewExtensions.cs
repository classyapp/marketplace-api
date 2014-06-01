using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Classy.Models;
using Classy.Models.Response;

namespace classy.Extentions
{
    public static class JobViewExtensions
    {
        public static IList<JobView> ToJobViewList(this IEnumerable<Job> from)
        {
            return from.Select(j => j.ToJobView()).ToList();
        }

        public static JobView ToJobView(this Job from)
        {
            return new JobView 
            {
                JobId = from.Id,
                Status = from.Status,
                Succeeded = from.Succeeded,
                Failed = from.Failed,
                CreatedAt = from.Created,
                UpdatedAt = from.UpdatedAt
            };
        }
    }
}