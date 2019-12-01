using BudgetBadger.Models;
using Syncfusion.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BudgetBadger.Forms.Comparers
{
    public class PayeeComparer : IComparer<Object>, ISortDirection
    {
        public ListSortDirection SortDirection { get; set; }

        public int Compare(object x, object y)
        {
            if (x is Group groupX && y is Group groupY)
            {
                var sourceX = groupX.Source.FirstOrDefault();
                var sourceY = groupY.Source.FirstOrDefault();

                return Compare(sourceX, sourceY);
            }

            if (x is Payee payee)
            {
                if (SortDirection == ListSortDirection.Ascending)
                {
                    return payee.CompareTo(y);
                }
                else
                {
                    return payee.CompareTo(y) * -1;
                }
            }

            return 0;
        }
    }
}
