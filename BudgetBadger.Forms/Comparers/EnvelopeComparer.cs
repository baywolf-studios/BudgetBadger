using BudgetBadger.Models;
using Syncfusion.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace BudgetBadger.Forms.Comparers
{
    public class EnvelopeComparer : IComparer<Object>, ISortDirection
    {
        public ListSortDirection SortDirection { get; set; }

        public int Compare(object x, object y)
        {
            if (x is Envelope envelope)
            {
                if (SortDirection == ListSortDirection.Ascending)
                {
                    return envelope.CompareTo(y);
                }
                else
                {
                    return envelope.CompareTo(y) * -1;
                }
            }

            return 0;
        }
    }
}

