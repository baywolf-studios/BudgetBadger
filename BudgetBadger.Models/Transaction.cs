﻿using System;
using PropertyChanged;

namespace BudgetBadger.Models
{
    [AddINotifyPropertyChangedInterface]
    public class Transaction
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
    }
}
