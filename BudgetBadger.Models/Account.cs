using System;
using PropertyChanged;

namespace BudgetBadger.Models
{
    [AddINotifyPropertyChangedInterface]
    public class Account
    {
        public Guid Id { get; set; }

        public string Description { get; set; }

        public AccountType Type { get; set; }

        public string Notes { get; set; }

        public decimal Balance { get; set; }

        public bool OnBudget { get; set; }

        public DateTime? CreatedDateTime { get; set; }

        public DateTime? ModifiedDateTime { get; set; }

        public DateTime? DeletedDateTime { get; set; }

        public Account()
        {
            Id = Guid.NewGuid();
            Type = new AccountType();
        }

        public Account DeepCopy()
        {
            Account account = (Account)this.MemberwiseClone();
            account.Type = this.Type.DeepCopy();
            return account;
        }
    }
}
