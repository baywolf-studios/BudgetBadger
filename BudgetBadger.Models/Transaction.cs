using System;
using PropertyChanged;

namespace BudgetBadger.Models
{
    [AddINotifyPropertyChangedInterface]
    public class Transaction
    {
        public Guid Id { get; set; }

        public decimal Amount { get; set; }

        public decimal Outflow { get; set; }

        public decimal Inflow { get; set; }

        public string Status { get; set; } 

        public Account Account { get; set; }

        public Payee Payee { get; set; }

        public Envelope Envelope { get; set; }

        public DateTime ServiceDate { get; set; }

        public string Notes { get; set; }

        public DateTime? CreatedDateTime { get; set; }

        public DateTime? ModifiedDateTime { get; set; }

        public DateTime? DeletedDateTime { get; set; }

        public Transaction()
        {
            Id = Guid.NewGuid();
            Status = "pending";
            ServiceDate = DateTime.Now;
        }

        public Transaction DeepCopy()
        {
            Transaction transaction = (Transaction)this.MemberwiseClone();
            transaction.Account = this.Account.DeepCopy();
            transaction.Payee = this.Payee.DeepCopy();
            transaction.Envelope = this.Envelope.DeepCopy();
            return transaction;
        }
    }
}
