using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace BudgetBadger.Core.Utilities
{
    public static class SortExtensions
    {
        public static void Sort<T>(this ObservableCollection<T> collection, Comparison<T> comparison = null)
        {
            var sortableList = new List<T>(collection);
            if (comparison == null)
                sortableList.Sort();
            else
                sortableList.Sort(comparison);

            for (var i = 0; i < sortableList.Count; i++)
            {
                var oldIndex = collection.IndexOf(sortableList[i]);
                if (oldIndex != i)
                {
                    collection.Move(oldIndex, i);
                }
            }
        }
    }
}
