using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using BudgetBadger.Models.Extensions;
using BudgetBadger.Models.Interfaces;
using Newtonsoft.Json;

namespace BudgetBadger.Models
{
    public class Account : BaseModel, IDeepCopy<Account>, IEquatable<Account>, IComparable, IComparable<Account>
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

        // calculated
        string group;
        public string Group
        {
            get => group;
            set => SetProperty(ref group, value);
        }

        public AccountType Type
        {
            get => OnBudget ? AccountType.AccountTypeBudget : AccountType.AccountTypeReporting;
            set 
            {
                OnBudget = value == AccountType.AccountTypeBudget;
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

        DateTime? hiddenDateTime;
        public DateTime? HiddenDateTime
        {
            get => hiddenDateTime;
            set { SetProperty(ref hiddenDateTime, value); OnPropertyChanged(nameof(IsHidden)); OnPropertyChanged(nameof(IsActive)); }
        }

        public bool IsDeleted { get => DeletedDateTime != null; }

        public bool IsHidden { get => HiddenDateTime != null; }

        public bool IsActive { get => !IsNew && !IsDeleted; }

        public Account()
        {
            Id = Guid.Empty;
            OnBudget = true;
        }

        public Account DeepCopy()
        {
            Account account = (Account)this.MemberwiseClone();
            account.Description = this.Description == null ? null : String.Copy(this.Description);
            account.Notes = this.Notes == null ? null : String.Copy(this.Notes);
            account.Group = this.Group == null ? null : String.Copy(this.Group);
            return account;
        }

        public bool Equals(Account p)
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
                && Description == p.Description
                && Notes == p.Notes
                && OnBudget == p.OnBudget;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as Account);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public int CompareTo(Account account)
        {
            if (account is null)
            {
                return 1;
            }


                if (Type.Equals(account.Type))
                {
                    return String.Compare(Description, account.Description);
                }

                return Type.CompareTo(account.Type);
        }

        public int CompareTo(object obj)
        {
            return CompareTo(obj as Account);
        }
    }
}
