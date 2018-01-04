using System;
namespace BudgetBadger.Models
{
    public class EnvelopesOverview
    {
        public decimal Past { get; private set; }

        public decimal Income { get; private set; }

        public decimal Budgeted { get; private set; }

        public decimal AvailableToBudget { get => Past + Income - Budgeted; }

        public EnvelopesOverview(decimal past, decimal income, decimal budgeted)
        {
            Past = past;
            Income = income;
            Budgeted = budgeted;
        }
    }
}
