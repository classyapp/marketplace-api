using System;
using System.Web;
using System.Web.Caching;

namespace classy.Cache
{
    public class DefaultCache<T> : ICache<T> where T : class
    {
        private const int DefaultCacheExpiration = 30;

        public void Add(string key, T value)
        {
            Add(key, value, DefaultCacheExpiration);
        }

        public void Add(string key, T value, int timeToExpire)
        {
            var httpContext = HttpContext.Current;
            if (httpContext == null)
                return;

            httpContext.Cache.Add(key, value, null, DateTime.Now.AddMinutes(timeToExpire),
                System.Web.Caching.Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
        }

        public T Get(string key)
        {
            var httpContext = HttpContext.Current;
            if (httpContext == null)
                return null;

            return httpContext.Cache.Get(key) as T;
        }
    }
}