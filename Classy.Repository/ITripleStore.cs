using Classy.Models;
using System.Collections.Generic;

namespace Classy.Repository
{
    public interface ITripleStore
    {
        Triple LogActivity(string appId, string subjectObjectId, string predicate, string objectObjectId, Dictionary<string, string> metadata, ref int count);
        Triple GetLogActivity(string appId, string subjectId, string predicate, string objectId);
        void DeleteActivity(string appId, string subjectObjectId, string predicate, string objectObjectId, ref int count);
        IList<string> GetActivitySubjectList(string appId, string predicate, string objectObjectId);
        IList<string> GetActivityObjectList(string appId, string predicate, string subjectObjectId);
        void ResetActivity(string appId, string subjectObjectId, string predicate, string objectObjectId);
    }
}
