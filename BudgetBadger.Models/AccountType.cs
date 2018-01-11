using System;
using PropertyChanged;

namespace BudgetBadger.Models
{
    [AddINotifyPropertyChangedInterface]
    public class AccountType
    {
        public Guid Id { get; set; }

        public string Description { get; set; }

        public AccountType()
        {
            Id = Guid.Empty;
        }

        public AccountType DeepCopy()
        {
            return (AccountType)this.MemberwiseClone();
        }
    }
}
