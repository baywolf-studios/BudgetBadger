using System;
namespace BudgetBadger.Core.Models.Interfaces
{
    public interface IDeepCopy<T>
    {
        T DeepCopy();
    }
}
