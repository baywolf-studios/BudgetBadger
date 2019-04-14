using System;
namespace BudgetBadger.Models.Interfaces
{
    public interface IPropertyCopy<T>
    {
        void PropertyCopy(T item);
    }
}
