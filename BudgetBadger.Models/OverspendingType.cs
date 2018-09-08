using System;
using System.Collections.Generic;
using System.Text;

namespace BudgetBadger.Models
{
    public abstract class OverspendingType : Enumeration
    {
        public static OverspendingType DoNotIgnore = new DoNotIgnoreOverspendingType();
        public static OverspendingType Ignore = new IgnoreOverspendingType();
        public static OverspendingType AlwaysIgnore = new AlwaysIgnoreOverspendingType();

        protected OverspendingType(int id, string name)
            : base(id, name)
        {
        }

        private class DoNotIgnoreOverspendingType : OverspendingType
        {
            public DoNotIgnoreOverspendingType() : base(1, "Don't Ignore")
            { }
        }

        private class IgnoreOverspendingType : OverspendingType
        {
            public IgnoreOverspendingType() : base(2, "Ignore")
            { }
        }

        private class AlwaysIgnoreOverspendingType : OverspendingType
        {
            public AlwaysIgnoreOverspendingType() : base(2, "Always Ignore")
            { }
        }
    }
}
