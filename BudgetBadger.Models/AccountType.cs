using System;
using System.Collections.Generic;
using System.Text;

namespace BudgetBadger.Models
{
    public abstract class AccountType : Enumeration
    {
        public static AccountType Budget = new BudgetAccountType();
        public static AccountType Reporting = new ReportingAccountType();

        protected AccountType(int id, string name)
            : base(id, name)
        {
        }

        private class BudgetAccountType : AccountType
        {
            public BudgetAccountType() : base(1, "Budget")
            { }
        }

        private class ReportingAccountType : AccountType
        {
            public ReportingAccountType() : base(2, "Reporting")
            { }
        }
    }
}
