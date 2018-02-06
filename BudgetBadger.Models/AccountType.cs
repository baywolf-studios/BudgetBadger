using System;

namespace BudgetBadger.Models
{
    public class AccountType
    {
        public Guid Id { get; set; }

        public string Description { get; set; }

        public DateTime? CreatedDateTime { get; set; }

        public DateTime? ModifiedDateTime { get; set; }

        public DateTime? DeletedDateTime { get; set; }

        public bool IsNew { get => CreatedDateTime == null; }

        public bool Exists { get => !IsNew; }

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
