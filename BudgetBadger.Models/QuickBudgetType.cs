using System;
namespace BudgetBadger.Models
{
    public enum QuickBudgetType
    {
        QuickBudgetLastMonthActivity,
        QuickBudgetLastMonthBudgeted,
        QuickBudgetAvgPast3MonthsActivity,
        QuickBudgetAvgPast3MonthsBudgeted,
        QuickBudgetAvgPastYearActivity,
        QuickBudgetAvgPastYearBudgeted,
        QuickBudgetBalance
    }
}
