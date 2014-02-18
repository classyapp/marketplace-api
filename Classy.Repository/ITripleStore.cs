using Classy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classy.Repository
{
    public interface ITripleStore
    {
        Triple LogActivity(string appId, string subjectObjectId, string predicate, string objectObjectId, ref int count);
        void DeleteActivity(string appId, string subjectObjectId, string predicate, string objectObjectId, ref int count);
        IList<string> GetActivitySubjectList(string appId, string predicate, string objectObjectId);
        IList<string> GetActivityObjectList(string appId, string predicate, string subjectObjectId);
        void ResetActivity(string appId, string subjectObjectId, string predicate, string objectObjectId);
    }
}
