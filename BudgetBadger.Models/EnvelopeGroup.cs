using System;
using System.Collections.Generic;
using System.Linq;
using BudgetBadger.Models.Interfaces;

namespace BudgetBadger.Models
{
    public class EnvelopeGroup : BaseModel, IDeepCopy<EnvelopeGroup>, IEquatable<EnvelopeGroup>, IComparable, IComparable<EnvelopeGroup>
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

        public bool IsIncome { get => Id == Constants.IncomeEnvelopeGroup.Id; }

        public bool IsSystem { get => Id == Constants.SystemEnvelopeGroup.Id; }

        public bool IsDebt { get => Id == Constants.DebtEnvelopeGroup.Id; }

        public bool IsGenericHiddenEnvelopeGroup { get => Id == Constants.GenericHiddenEnvelopeGroup.Id; }

        public EnvelopeGroup()
        {
            Id = Guid.Empty;
        }

        public EnvelopeGroup DeepCopy()
        {
            var group = (EnvelopeGroup)this.MemberwiseClone();
            group.Description = this.Description == null ? null : String.Copy(this.Description);
            group.Notes = this.Notes == null ? null : String.Copy(this.Notes);
            return group;
        }

        public bool Equals(EnvelopeGroup p)
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
                && HiddenDateTime == p.HiddenDateTime
                && DeletedDateTime == p.DeletedDateTime
                && Description == p.Description
                && Notes == p.Notes;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as EnvelopeGroup);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public int CompareTo(object obj)
        {
            return CompareTo(obj as EnvelopeGroup);
        }

        public int CompareTo(EnvelopeGroup group)
        {
            if (group is null)
            {
                return 1;
            }

            if (IsGenericHiddenEnvelopeGroup.Equals(group.IsGenericHiddenEnvelopeGroup))
            {
                if (IsDebt.Equals(group.IsDebt))
                {
                    if (IsIncome.Equals(group.IsIncome))
                    {
                        if (Description.Equals(group.Description))
                        {
                            return -1 * Nullable.Compare(CreatedDateTime, group.CreatedDateTime);
                        }
                        return String.Compare(Description, group.Description);
                    }

                    return -1 * IsIncome.CompareTo(group.IsIncome);
                }

                return -1 * IsDebt.CompareTo(group.IsDebt);
            }

            return IsGenericHiddenEnvelopeGroup.CompareTo(group.IsGenericHiddenEnvelopeGroup);
        }
    }
}
