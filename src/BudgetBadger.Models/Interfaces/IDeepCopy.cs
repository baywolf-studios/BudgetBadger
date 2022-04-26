using System;
namespace BudgetBadger.Models.Interfaces
{
    public interface IDeepCopy<T>
    {
        T DeepCopy();
    }
}
