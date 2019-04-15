using System;
using System.Collections.Generic;
using System.Linq;
using BudgetBadger.Models.Interfaces;

namespace BudgetBadger.Models
{
    public class EnvelopeGroup : BaseModel, IValidatable, IDeepCopy<EnvelopeGroup>, IEquatable<EnvelopeGroup>, IPropertyCopy<EnvelopeGroup>
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

        public bool IsDeleted { get => DeletedDateTime != null; }

        public bool IsActive { get => !IsNew && !IsDeleted; }

        public bool IsIncome
        {
            get => Id == Constants.IncomeEnvelopeGroup.Id;
        }

        public bool IsSystem
        {
            get => Id == Constants.SystemEnvelopeGroup.Id;
        }

        public bool IsDebt
        {
            get => Id == Constants.DebtEnvelopeGroup.Id;
        }

        public EnvelopeGroup()
        {
            Id = Guid.Empty;
        }

        public EnvelopeGroup DeepCopy()
        {
            return (EnvelopeGroup)this.MemberwiseClone();
        }

        public void PropertyCopy(EnvelopeGroup item)
        {
            Description = item.description;
            Notes = item.Notes;
            CreatedDateTime = item.CreatedDateTime;
            ModifiedDateTime = item.ModifiedDateTime;
            DeletedDateTime = item.DeletedDateTime;
        }

        public Result Validate()
        {
            var errors = new List<string>();

            if (string.IsNullOrEmpty(Description))
            {
                errors.Add("Envelope group description is required");
            }

            return new Result { Success = !errors.Any(), Message = string.Join(Environment.NewLine, errors) };
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
            return Id == p.Id;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as EnvelopeGroup);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static bool operator ==(EnvelopeGroup lhs, EnvelopeGroup rhs)
        {
            // Check for null on left side.
            if (lhs is null)
            {
                if (rhs is null)
                {
                    // null == null = true.
                    return true;
                }

                // Only the left side is null.
                return false;
            }
            // Equals handles case of null on right side.
            return lhs.Equals(rhs);
        }

        public static bool operator !=(EnvelopeGroup lhs, EnvelopeGroup rhs)
        {
            return !(lhs == rhs);
        }
    }
}
