using System;
using System.Collections.Generic;
using System.Linq;
using BudgetBadger.Models.Extensions;
using BudgetBadger.Models.Interfaces;

namespace BudgetBadger.Models
{
    public class Transaction : ObservableBase, IDeepCopy<Transaction>, IEquatable<Transaction>, IComparable, IComparable<Transaction>
    {
        Guid id;
        public Guid Id
        {
            get => id;
            set => SetProperty(ref id, value);
        }

        decimal? amount;
        public decimal? Amount
        {
            get => amount;
            set { SetProperty(ref amount, value); RaisePropertyChanged(nameof(Outflow)); RaisePropertyChanged(nameof(Inflow)); }
        }

        public decimal? Outflow
        {
            get
            {
                if (Amount.HasValue && Amount <= 0)
                {
                    return Math.Abs(Amount.Value);
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (value.HasValue || !(Amount.HasValue && Amount > 0))
                {
                    Amount = -1 * value;
                }
            }
        }

        public decimal? Inflow
        {
            get
            {
                if (Amount.HasValue && Amount >= 0)
                {
                    return Amount.Value;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (value.HasValue || !(Amount.HasValue && Amount < 0))
                {
                    Amount = value;
                }
            }
        }

        bool posted;
        public bool Posted
        {
            get => posted;
            set
            {
                SetProperty(ref posted, value);
                RaisePropertyChanged(nameof(Pending));
                RaisePropertyChanged(nameof(Reconciled));
                RaisePropertyChanged(nameof(TransactionStatus));
            }
        }

        public bool Pending { get => !Posted; }

        DateTime? reconciledDateTime;
        public DateTime? ReconciledDateTime
        {
            get => reconciledDateTime;
            set
            {
                SetProperty(ref reconciledDateTime, value);
                RaisePropertyChanged(nameof(Pending));
                RaisePropertyChanged(nameof(Posted));
                RaisePropertyChanged(nameof(Reconciled));
                RaisePropertyChanged(nameof(TransactionStatus));
            }
        }

        public bool Reconciled { get => ReconciledDateTime.HasValue; }

        public TransactionStatus TransactionStatus
        {
            get
            {
                if (Reconciled)
                {
                    return TransactionStatus.Reconciled;
                }
                if (Posted)
                {
                    return TransactionStatus.Posted;
                }

                return TransactionStatus.Pending;
            }
        }

        Account account;
        public Account Account
        {
            get => account;
            set => SetProperty(ref account, value);
        }

        Payee payee;
        public Payee Payee
        {
            get => payee;
            set { SetProperty(ref payee, value); RaisePropertyChanged(nameof(IsTransfer)); }
        }

        Envelope envelope;
        public Envelope Envelope
        {
            get => envelope;
            set => SetProperty(ref envelope, value);
        }

        Guid? splitId;
        public Guid? SplitId
        {
            get => splitId;
            set { SetProperty(ref splitId, value); RaisePropertyChanged(nameof(IsSplit)); }
        }

        public bool IsCombined
        {
            get => Id == SplitId;
        }

        public bool IsSplit
        {
            get => SplitId.HasValue;
        }

        DateTime serviceDate;
        public DateTime ServiceDate
        {
            get => serviceDate.Date;
            set => SetProperty(ref serviceDate, value.Date);
        }

        string notes;
        public string Notes
        {
            get => notes;
            set => SetProperty(ref notes, value);
        }

        public bool IsTransfer { get => Payee.IsAccount; }

        DateTime? createdDateTime;
        public DateTime? CreatedDateTime
        {
            get => createdDateTime;
            set { SetProperty(ref createdDateTime, value); RaisePropertyChanged(nameof(IsNew)); RaisePropertyChanged(nameof(IsActive)); }
        }

        public bool IsNew { get => CreatedDateTime == null; }

        DateTime? modifiedDateTime;
        public DateTime? ModifiedDateTime
        {
            get => modifiedDateTime;
            set => SetProperty(ref modifiedDateTime, value);
        }

        DateTime? deletedDateTime;
        public DateTime? DeletedDateTime
        {
            get => deletedDateTime;
            set { SetProperty(ref deletedDateTime, value); RaisePropertyChanged(nameof(IsDeleted)); RaisePropertyChanged(nameof(IsActive)); }
        }

        public bool IsDeleted { get => DeletedDateTime != null; }

        public bool IsActive { get => !IsNew && !IsDeleted; }

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
            transaction.Notes = this.Notes == null ? null : String.Copy(this.Notes);
            return transaction;
        }

        public bool Equals(Transaction p)
        {
            // If parameter is null, return false.
            if (p is null)
            {
                return false;
            }

            // Optimization for a common success case.
            if (Object.ReferenceEquals(this, p))
            {
                return true;
            }

            // If run-time types are not exactly the same, return false.
            if (this.GetType() != p.GetType())
            {
                return false;
            }

            // Return true if the fields match.
            // Note that the base class is not invoked because it is
            // System.Object, which defines Equals as reference equality.
            return Id == p.Id
                && CreatedDateTime == p.CreatedDateTime
                && ModifiedDateTime == p.ModifiedDateTime
                && DeletedDateTime == p.DeletedDateTime
                && Notes == p.Notes
                && Amount == p.Amount
                && Posted == p.Posted
                && ReconciledDateTime == p.ReconciledDateTime
                && ServiceDate == p.ServiceDate
                && SplitId == p.SplitId
                && Account.Equals(p.Account)
                && Payee.Equals(p.Payee)
                && Envelope.Equals(p.Envelope);
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as Transaction);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public int CompareTo(Transaction transaction)
        {
            if (transaction is null)
            {
                return 1;
            }

            if (ServiceDate.Date.Equals(transaction.ServiceDate.Date))
            {
                return -1 * Nullable.Compare(CreatedDateTime, transaction.CreatedDateTime);
            }

            return -1 * ServiceDate.Date.CompareTo(transaction.ServiceDate.Date);
        }

        public int CompareTo(object obj)
        {
            return CompareTo(obj as Transaction);
        }
    }
}
