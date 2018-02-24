using System;

namespace BudgetBadger.Models
{
    public class Account
    { 
        public Guid Id { get; set; }

        public string Description { get; set; }

        public AccountType Type { get; set; }

        public string Notes { get; set; }

        //calculated
        public decimal Balance { get; set; }

        public decimal Payment { get; set; }

        public bool PaymentRequired { get => Payment > 0; }

        public bool OnBudget { get; set; }

        public bool OffBudget { get => !OnBudget; }

        public DateTime? CreatedDateTime { get; set; }

        public DateTime? ModifiedDateTime { get; set; }

        public DateTime? DeletedDateTime { get; set; }

        public bool IsNew { get => CreatedDateTime == null; }

        public bool Exists { get => !IsNew; }

        public Account()
        {
            Id = Guid.Empty;
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
