using System;
using System.Collections.Generic;
using System.Linq;
using BudgetBadger.Models.Extensions;
using BudgetBadger.Models.Interfaces;

namespace BudgetBadger.Models
{
    public class Transaction : IValidatable, IDeepCopy<Transaction>
    {
        public Guid Id { get; set; }

        public decimal Amount { get; set; }

        public decimal Outflow
        {
            get
            {
                return Amount < 0 ? Math.Abs(Amount) : 0;
            }
            set
            {
                Amount = -1 * value;
            }
        }

        public decimal Inflow
        {
            get
            {
                return Amount > 0 ? Amount : 0;
            }
            set
            {
                Amount = value;
            }
        }

        public bool Pending { get { return !Posted; } }

        public bool Posted { get; set; }

        public DateTime? ReconciledDateTime { get; set; }

        public Account Account { get; set; }

        public Payee Payee { get; set; }

        public Envelope Envelope { get; set; }

        public Guid? LinkedId { get; set; }

        public DateTime ServiceDate { get; set; }

        public string Notes { get; set; }

        public DateTime? CreatedDateTime { get; set; }

        public DateTime? ModifiedDateTime { get; set; }

        public DateTime? DeletedDateTime { get; set; }

        public bool IsTransfer { get => Payee.IsAccount; }

        public bool IsNew { get => CreatedDateTime == null; }

        public bool Exists { get => !IsNew; }

        public Transaction()
        {
            Id = Guid.Empty;
            ServiceDate = DateTime.Now;
            Account = new Account();
            Payee = new Payee();
            Envelope = new Envelope();
        }

        public Transaction DeepCopy()
        {
            Transaction transaction = (Transaction)this.MemberwiseClone();
            transaction.Account = this.Account.DeepCopy();
            transaction.Payee = this.Payee.DeepCopy();
            transaction.Envelope = this.Envelope.DeepCopy();
            return transaction;
        }

        public Result Validate()
        {
            var errors = new List<string>();

            if (Envelope == null || Envelope.IsNew)
            {
                errors.Add("Envelope is required");
            }

            if (Payee == null || Payee.IsNew)
            {
                errors.Add("Payee is required");
            }

            if (Account == null || Account.IsNew)
            {
                errors.Add("Account is required");
            }

            return new Result { Success = !errors.Any(), Message = string.Join(Environment.NewLine, errors) };
        }
    }
}
