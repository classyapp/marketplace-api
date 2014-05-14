using System;
using System.Linq.Expressions;
using Classy.Models;

namespace Classy.Interfaces.Search
{
    public interface IIndexer<T>
    {
        void Index(T[] entities);
        void RemoveFromIndex(T entity);
        void Increment<TPropertyType>(string id, Expression<Func<T, TPropertyType>> property, int amount = 1);
        void Increment<TPropertyType>(string[] ids, Expression<Func<T, TPropertyType>> property, int amount = 1);
    }
}