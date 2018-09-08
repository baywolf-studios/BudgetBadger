using System;
using System.Collections.Generic;
using System.Linq;
using BudgetBadger.Models.Extensions;
using BudgetBadger.Models.Interfaces;

namespace BudgetBadger.Models
{
    public class Account : BaseModel, IValidatable, IDeepCopy<Account>
    {
        Guid id;
        public Guid Id
        {
            get => id;
            set => SetProperty(ref id, value);
        }

        string description;
        public string Description
        {
            get => description;
            set => SetProperty(ref description, value);
        }

        string notes;
        public string Notes
        {
            get => notes;
            set => SetProperty(ref notes, value);
        }
        
        public AccountType Type
        {
            get => OnBudget ? AccountType.Budget : AccountType.Reporting;
            set 
            {
                OnBudget = value == AccountType.Budget;
            }
        }

        bool onBudget;
        public bool OnBudget
        {
            get => onBudget;
            set { SetProperty(ref onBudget, value); OnPropertyChanged(nameof(OffBudget)); OnPropertyChanged(nameof(Type)); }
        }

        public bool OffBudget { get => !OnBudget; }

        //calculated
        decimal? balance;
        public decimal? Balance
        {
            get => balance;
            set => SetProperty(ref balance, value);
        }

        //calculated
        decimal? pending;
        public decimal? Pending
        {
            get => pending;
            set => SetProperty(ref pending, value);
        }

        //calculated
        decimal? posted;
        public decimal? Posted
        {
            get => posted;
            set => SetProperty(ref posted, value);
        }

        decimal payment;
        public decimal Payment
        {
            get => payment;
            set { SetProperty(ref payment, value); OnPropertyChanged(nameof(PaymentRequired)); }
        }

        public bool PaymentRequired { get => Payment > 0; }

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

        public Account()
        {
            Id = Guid.Empty;
            OnBudget = true;
        }

        public Account DeepCopy()
        {
            Account account = (Account)this.MemberwiseClone();
            return account;
        }

        public Result Validate()
        {
            var errors = new List<string>();

            if (IsNew && !Balance.HasValue)
            {
                errors.Add("Balance is required");
            }

            if (string.IsNullOrEmpty(Description))
            {
                errors.Add("Account description is required");
            }

            return new Result { Success = !errors.Any(), Message = string.Join(Environment.NewLine, errors) };
        }
    }
}
