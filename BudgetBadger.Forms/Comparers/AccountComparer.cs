using BudgetBadger.Models;
using Syncfusion.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace BudgetBadger.Forms.Comparers
{
    public class AccountComparer : IComparer<Object>, ISortDirection
    {
        public ListSortDirection SortDirection { get; set; }

        public int Compare(object x, object y)
        {
            if (x is Account account)
            {
                if (SortDirection == ListSortDirection.Ascending)
                {
                    return account.CompareTo(y);
                }
                else
                {
                    return account.CompareTo(y) * -1;
                }
            }

            return 0;
        }
    }
}
