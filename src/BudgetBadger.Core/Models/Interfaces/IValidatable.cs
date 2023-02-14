using System;
namespace BudgetBadger.Core.Models.Interfaces
{
    public interface IValidatable
    {
        Result Validate();
    }
}
