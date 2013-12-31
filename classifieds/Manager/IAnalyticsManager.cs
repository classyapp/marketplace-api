using Classy.Models;
using Classy.Models.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace classy.Manager
{
    public interface IAnalyticsManager
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="subjectId"></param>
        /// <param name="predicate"></param>
        /// <param name="objectId"></param>
        /// <returns></returns>
        TripleView LogActivity(
            string appId, 
            string subjectId, 
            string predicate, 
            string objectId);
    }
}
