using BudgetBadger.Models;
using Syncfusion.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BudgetBadger.Forms.Comparers
{
    public class EnvelopeGroupComparer : IComparer<Object>, ISortDirection
    {
        public ListSortDirection SortDirection { get; set; }

        public int Compare(object x, object y)
        {
            if (x is ObservableGrouping<object, object> ogroupX && y is ObservableGrouping<object, object> ogroupY)
            {
                var osourceX = ogroupX.FirstOrDefault();
                var osourceY = ogroupY.FirstOrDefault();

                return Compare(osourceX, osourceY);
            }

            if (x is Group groupX && y is Group groupY)
            {
                var sourceX = groupX.Source.FirstOrDefault();
                var sourceY = groupY.Source.FirstOrDefault();

                return Compare(sourceX, sourceY);
            }

            if (x is EnvelopeGroup envelope)
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