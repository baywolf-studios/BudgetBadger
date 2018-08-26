using System;
using System.Collections.Generic;
using System.Linq;
using BudgetBadger.Models.Extensions;
using BudgetBadger.Models.Interfaces;

namespace BudgetBadger.Models
{
    public class Transaction : BaseModel, IValidatable, IDeepCopy<Transaction>
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
            set { SetProperty(ref amount, value); OnPropertyChanged(nameof(Outflow)); OnPropertyChanged(nameof(Inflow)); }
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
            set => SetProperty(ref posted, value);
        }

        public bool Pending { get => !Posted; }

        DateTime? reconciledDateTime;
        public DateTime? ReconciledDateTime
        {
            get => reconciledDateTime;
            set { SetProperty(ref reconciledDateTime, value); OnPropertyChanged(nameof(IsReconciled)); }
        }

        public bool IsReconciled { get => ReconciledDateTime.HasValue; }

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
            set { SetProperty(ref payee, value); OnPropertyChanged(nameof(IsTransfer)); }
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
            set { SetProperty(ref splitId, value); OnPropertyChanged(nameof(IsSplit)); }
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
            get => serviceDate;
            set => SetProperty(ref serviceDate, value);
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
            set { SetProperty(ref createdDateTime, value); OnPropertyChanged(nameof(IsNew)); OnPropertyChanged(nameof(IsActive)); }
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
            set { SetProperty(ref deletedDateTime, value); OnPropertyChanged(nameof(IsDeleted)); OnPropertyChanged(nameof(IsActive)); }
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
            return transaction;
        }

        public Result Validate()
        {
            var errors = new List<string>();

            if (!Amount.HasValue)
            {
                errors.Add("Amount is required");
            }

            if (Envelope == null)
            {
                errors.Add("Envelope is required");
            }
            else if (!Envelope.IsValid())
            {
                errors.Add("A valid envelope is required");
            }

            if (Payee == null)
            {
                errors.Add("Payee is required");
            }
            else if (!Payee.IsValid())
            {
                errors.Add("A valid payee is required");
            }

            if (Account == null)
            {
                errors.Add("Account is required");
            }
            else if (!Account.IsValid())
            {
                errors.Add("A valid account is required");
            }

            return new Result { Success = !errors.Any(), Message = string.Join(Environment.NewLine, errors) };
        }
    }
}
