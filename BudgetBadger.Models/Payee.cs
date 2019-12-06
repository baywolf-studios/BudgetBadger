using System;
using System.Collections.Generic;
using System.Linq;
using BudgetBadger.Models.Interfaces;

namespace BudgetBadger.Models
{
    public class Payee : BaseModel, IDeepCopy<Payee>, IEquatable<Payee>, IComparable, IComparable<Payee>
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
            set { SetProperty(ref description, value); OnPropertyChanged(nameof(Group)); }
            }

        string notes;
        public string Notes
        {
            get => notes;
            set => SetProperty(ref notes, value);
        }

        //calculated
        bool isAccount;
        public bool IsAccount
        {
            get => isAccount;
            set { SetProperty(ref isAccount, value); OnPropertyChanged(nameof(Group)); }
        }

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

        public bool IsActive { get => !IsNew && !IsDeleted && !IsHidden; }

        public bool IsStartingBalance
        {
            get => Id == Constants.StartingBalancePayee.Id;
        }

        public bool IsGenericHiddenPayee
        {
            get => Id == Constants.GenericHiddenPayee.Id;
        }

        // calculated
        string group;
        public string Group
        {
            get => group;
            set => SetProperty(ref group, value);
        }

        public string ExtendedDescription
        {
            get
            {
                if (IsAccount)
                {
                    return string.Format("{0} - {1}", Group, Description);
                }

                return Description;
            }
        }

        public Payee()
        {
            Id = Guid.Empty;
        }

        public Payee DeepCopy()
        {
            
            var payee = (Payee)this.MemberwiseClone();

            payee.Description = this.Description == null ? null : String.Copy(this.Description);
            payee.Notes = this.Notes == null ? null : String.Copy(this.Notes);
            payee.Group = this.Group == null ? null : String.Copy(this.Group);

            return payee;
        }

        public bool Equals(Payee p)
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
                && Notes == p.Notes;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as Payee);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public int CompareTo(Payee payee)
        {
            if (payee is null)
            {
                return 1;
            }

            if (IsGenericHiddenPayee.Equals(payee.IsGenericHiddenPayee))
            {
                if (IsAccount.Equals(payee.IsAccount))
                {
                    if (Group.Equals(payee.Group))
                    {
                        return String.Compare(Description, payee.Description);
                    }

                    return String.Compare(Group, payee.Group);
                }

                return -1 * IsAccount.CompareTo(payee.IsAccount);
            }

            return IsGenericHiddenPayee.CompareTo(payee.IsGenericHiddenPayee);
        }

        public int CompareTo(object obj)
        {
            return CompareTo(obj as Payee);
        }
    }
}
