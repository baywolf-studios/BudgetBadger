using BudgetBadger.Models;
using Syncfusion.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace BudgetBadger.Forms.Comparers
{
    public class BudgetComparer : IComparer<Object>, ISortDirection
    {
        public ListSortDirection SortDirection { get; set; }

        public int Compare(object x, object y)
        {
            if (x is Budget budget)
            {
                if (SortDirection == ListSortDirection.Ascending)
                {
                    return budget.CompareTo(y);
                }
                else
                {
                    return budget.CompareTo(y) * -1;
                }
            }

            return 0;
        }
    }
}
