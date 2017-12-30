using System;
using PropertyChanged;

namespace BudgetBadger.Models
{
    [AddINotifyPropertyChangedInterface]
    public class Result
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    [AddINotifyPropertyChangedInterface]
    public class Result<T> : Result
    {
        public T Data { get; set; }
    }
}
