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

        AccountType type;
        public AccountType Type
        {
            get => type;
            set => SetProperty(ref type, value);
        }

        string notes;
        public string Notes
        {
            get => notes;
            set => SetProperty(ref notes, value);
        }

        bool onBudget;
        public bool OnBudget
        {
            get => onBudget;
            set { SetProperty(ref onBudget, value); OnPropertyChanged("OffBudget"); }
        }

        public bool OffBudget { get => !OnBudget; }

        //calculated
        decimal balance;
        public decimal Balance
        {
            get => balance;
            set => SetProperty(ref balance, value);
        }

        decimal payment;
        public decimal Payment
        {
            get => payment;
            set { SetProperty(ref payment, value); OnPropertyChanged("PaymentRequired"); }
        }

        public bool PaymentRequired { get => Payment > 0; }

        DateTime? createdDateTime;
        public DateTime? CreatedDateTime
        {
            get => createdDateTime;
            set { SetProperty(ref createdDateTime, value); OnPropertyChanged("IsNew"); OnPropertyChanged("IsActive"); }
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
            set { SetProperty(ref deletedDateTime, value); OnPropertyChanged("IsDeleted"); OnPropertyChanged("IsActive"); }
        }

        public bool IsDeleted { get => DeletedDateTime != null; }

        public bool IsActive { get => !IsNew && !IsDeleted; }

        public Account()
        {
            Id = Guid.Empty;
            Type = new AccountType();
            OnBudget = true;
        }

        public Account DeepCopy()
        {
            Account account = (Account)this.MemberwiseClone();
            account.Type = this.Type?.DeepCopy();
            return account;
        }

        public Result Validate()
        {
            var errors = new List<string>();

            if (string.IsNullOrEmpty(Description))
            {
                errors.Add("Account description is required");
            }

            if (Type == null)
            {
                errors.Add("Account type is required");
            }
            else if (!Type.IsValid())
            {
                errors.Add("A valid account type is required");
            }

            return new Result { Success = !errors.Any(), Message = string.Join(Environment.NewLine, errors) };
        }
    }
}
