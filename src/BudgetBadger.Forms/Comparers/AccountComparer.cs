using BudgetBadger.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BudgetBadger.Forms.Comparers
{
    public class AccountComparer : IComparer<Object>
    {
        public int Compare(object x, object y)
        {
            if (x is ObservableGrouping<object, object> ogroupX && y is ObservableGrouping<object, object> ogroupY)
            {
                var osourceX = ogroupX.FirstOrDefault();
                var osourceY = ogroupY.FirstOrDefault();

                return Compare(osourceX, osourceY);
            }

            if (x is Account account)
            {
                return account.CompareTo(y);
            }

            return 0;
        }
    }
}
