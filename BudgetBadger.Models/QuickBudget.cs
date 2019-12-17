using System;

namespace BudgetBadger.Models
{
    public class QuickBudget : BaseModel
    {
        QuickBudgetType type;
        public QuickBudgetType Type
        {
            get => type;
            set => SetProperty(ref type, value);
        }

        string description;
        public string Description
        {
            get => description;
            set => SetProperty(ref description, value);
        }

        decimal amount;
        public decimal Amount
        {
            get => amount;
            set => SetProperty(ref amount, value);
        }
    }
}
