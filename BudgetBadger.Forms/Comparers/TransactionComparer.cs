using BudgetBadger.Models;
using Syncfusion.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace BudgetBadger.Forms.Comparers
{
    public class TransactionComparer : IComparer<Object>, ISortDirection
    {
        public ListSortDirection SortDirection { get; set; }

        public int Compare(object x, object y)
        {
            if (x is Transaction transaction)
            {
                if (SortDirection == ListSortDirection.Ascending)
                {
                    return transaction.CompareTo(y);
                }
                else
                {
                    return transaction.CompareTo(y) * -1;
                }
            }

            return 0;
        }
    }
}
