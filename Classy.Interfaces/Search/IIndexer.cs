using System;
using System.Linq.Expressions;

namespace Classy.Interfaces.Search
{
    public interface IIndexer<T>
    {
        void Index(T[] entities, string appId);
        void RemoveFromIndex(T entity, string appId);
        void Increment<TPropertyType>(string id, string appId, Expression<Func<T, TPropertyType>> property, int amount = 1);
        void Increment<TPropertyType>(string[] ids, string appId, Expression<Func<T, TPropertyType>> property, int amount = 1);
        void UpdateMultipleListings(string[] ids, int editorsRank, string appId);
    }
}
