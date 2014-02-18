using Classy.Models.Response;
using Classy.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceStack.Common;

namespace classy.Manager
{
    public class DefaultAnalyticsManager : IAnalyticsManager
    {
        private ITripleStore TripleStore;

        public DefaultAnalyticsManager(ITripleStore tripleStore)
        {
            TripleStore = tripleStore;
        }

        public TripleView LogActivity(
            string appId, 
            string subjectId, 
            string predicate, 
            string objectId)
        {
            int count = 0;
            var triple = TripleStore.LogActivity(appId, subjectId, predicate, objectId, ref count);
            return triple.TranslateTo<TripleView>();
        }
    }
}