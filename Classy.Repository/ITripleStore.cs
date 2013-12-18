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
        Triple LogActivity(string appId, string subjectObjectId, ActivityPredicate predicate, string objectObjectId, ref bool tripleAlreadyExists);
        IList<string> GetActivitySubjectList(string appId, ActivityPredicate predicate, string objectObjectId);
        IList<string> GetActivityObjectList(string appId, ActivityPredicate predicate, string subjectObjectId);
    }
}
