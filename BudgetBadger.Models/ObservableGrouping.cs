using System;
using System.Collections.Generic;

namespace BudgetBadger.Models
{
    /// <summary>
	/// Grouping of items by key into ObservableRange
	/// </summary>
	public class ObservableGrouping<TKey, TItem> : ObservableRangeCollection<TItem>
    {
        /// <summary>
        /// Gets the key.
        /// </summary>
        /// <value>The key.</value>
        public TKey Key { get; }

        /// <summary>
        /// Returns list of items in the grouping.
        /// </summary>
        public new IList<TItem> Items => base.Items;

        /// <summary>
        /// Initializes a new instance of the Grouping class.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="items">Items.</param>
        public ObservableGrouping(TKey key, IEnumerable<TItem> items)
        {
            Key = key;
            AddRange(items);
        }


    }

}
