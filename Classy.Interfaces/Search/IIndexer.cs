using System;
using System.Linq.Expressions;
using Classy.Models;

namespace Classy.Interfaces.Search
{
    public interface IIndexer<T>
    {
        void Index(T[] entities, string appId);
        void RemoveFromIndex(T entity, string appId);
        void Increment<TPropertyType>(string id, string appId, Expression<Func<T, TPropertyType>> property, int amount = 1);
        void Increment<TPropertyType>(string[] ids, string appId, Expression<Func<T, TPropertyType>> property, int amount = 1);
    }
}
