using System;
using System.Collections.Generic;
using PropertyChanged;

namespace BudgetBadger.Models
{
    [AddINotifyPropertyChangedInterface]
    public class GroupedList<T> : List<T>
    {
        public string DisplayName { get; set; }
        public string ShortName { get; set; }

        public GroupedList()
        {
        }

        public GroupedList(string displayName, string shortName)
        {
            DisplayName = displayName;
            ShortName = shortName;
        }
    }
}
