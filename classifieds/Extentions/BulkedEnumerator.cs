using System.Collections.Generic;

namespace classy.Extentions
{
    /// <summary>
    /// An enumerator that iterates a source sequence in bulks of custom size.
    /// </summary>
    public class BulkedEnumerator<T>
    {
        /// <summary>
        /// Initializes a new BulkedEnumerator instance over a sequence of items.
        /// </summary>
        public BulkedEnumerator(IEnumerable<T> source)
        {
            _sourceEnumerator = source.GetEnumerator();
            Current = null;
        }

        private readonly IEnumerator<T> _sourceEnumerator;
        private bool _exhausted;

        /// <summary>
        /// The current bulk of items from the source sequence.
        /// </summary>
        public IList<T> Current { get; private set; }

        /// <summary>
        /// Consumes a bulk of up to <see cref="count"/> items from the source sequence.
        /// </summary>
        /// <returns>
        /// True if the enumerator successfully consumed any number of items; false if the source sequence was exhausted.
        /// </returns>
        /// <remarks>
        /// Effectively, the return value from this method indicates whether or not the <see cref="Current"/> property should be inspected.
        /// The intended usage is:
        /// <code>
        ///		while (bulkEnumerator.TakeNext(bulkSize))
        ///		{
        ///			ProcessBulk(bulkEnumerator.Current);
        ///		}
        /// </code>
        /// </remarks>
        public bool TakeNext(int count)
        {
            if (_exhausted) return false;

            Current = new List<T>(count);
            while (Current.Count < count)
            {
                if (!_sourceEnumerator.MoveNext())
                {
                    // We passed the end of the source sequence -- 
                    // remember that for next time and indicate whether we managed to get any items
                    _exhausted = true;
                    return Current.Count != 0;
                }

                Current.Add(_sourceEnumerator.Current);
            }

            return true;
        }

        /// <summary>
        /// Resets the enumerator, starting again at the top of the source sequence.
        /// </summary>
        public void Reset()
        {
            _sourceEnumerator.Reset();
        }
    }
}