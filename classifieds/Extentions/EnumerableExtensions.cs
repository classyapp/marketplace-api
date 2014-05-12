using System;
using System.Collections.Generic;
using System.Linq;

namespace classy.Extentions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> collection)
        {
            return collection ?? Enumerable.Empty<T>();
        }

        public static IEnumerable<IList<T>> Bulks<T>(this IEnumerable<T> source, int bulkSize)
        {
            return Bulks(source, bulkSize, size => size);
        }

        public static IEnumerable<IList<T>> Bulks<T>(this IEnumerable<T> source, int initialBulkSize, Func<int, int> bulkSizeModifier)
        {
            if (source == null) throw new ArgumentNullException("source");

            var bulkSize = initialBulkSize;
            if (bulkSize <= 0) throw new ArgumentOutOfRangeException("bulkSize", "bulk size must be positive");

            var bulker = new BulkedEnumerator<T>(source);

            // Consume the sequence in bulks until we hit its end
            while (bulker.TakeNext(bulkSize))
            {
                yield return bulker.Current;
                bulkSize = bulkSizeModifier(bulkSize);
            }
        }
    }
}