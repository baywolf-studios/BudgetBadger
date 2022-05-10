using System;
namespace BudgetBadger.Models.Interfaces
{
    public interface IValidatable
    {
        Result Validate();
    }
}
