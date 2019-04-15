using System;
using System.Collections.Generic;
using System.Linq;
using BudgetBadger.Models.Extensions;
using BudgetBadger.Models.Interfaces;

namespace BudgetBadger.Models
{
    public class Envelope : BaseModel, IValidatable, IDeepCopy<Envelope>, IEquatable<Envelope>, IPropertyCopy<Envelope>
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

        EnvelopeGroup group;
        public EnvelopeGroup Group
        {
            get => group;
            set => SetProperty(ref group, value);
        }

        bool ignoreOverspend;
        public bool IgnoreOverspend
        {
            get => ignoreOverspend;
            set => SetProperty(ref ignoreOverspend, value);
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
            get => Id == Constants.IncomeEnvelope.Id;
        }

        public bool IsBuffer
        {
            get => Id == Constants.BufferEnvelope.Id;
        }

        public bool IsSystem
        {
            get => Id == Constants.IgnoredEnvelope.Id;
        }

        public bool IsGenericDebtEnvelope
        {
            get => Id == Constants.GenericDebtEnvelope.Id;
        }

        public string ExtendedDescription
        {
            get => Group?.Description + " - " + Description;
        }

        public Envelope()
        {
            Id = Guid.Empty;
            Group = new EnvelopeGroup();
        }

        public Envelope DeepCopy()
        {
            Envelope envelope = (Envelope)this.MemberwiseClone();
            envelope.Group = this.Group.DeepCopy();

            return envelope;
        }

        public void PropertyCopy(Envelope item)
        {
            Description = item.description;
            Notes = item.Notes;
            Group.PropertyCopy(item.Group);
            IgnoreOverspend = item.IgnoreOverspend;
            CreatedDateTime = item.CreatedDateTime;
            ModifiedDateTime = item.ModifiedDateTime;
            DeletedDateTime = item.DeletedDateTime;
        }

        public Result Validate()
        {
            var errors = new List<string>();

            if (string.IsNullOrEmpty(Description))
            {
                errors.Add("Envelope description is required");
            }

            if (Group == null)
            {
                errors.Add("Envelope group is required");
            }
            else if (!Group.IsValid())
            {
                errors.Add("A valid envelope group is required");
            }

            return new Result { Success = !errors.Any(), Message = string.Join(Environment.NewLine, errors) };
        }

        public bool Equals(Envelope p)
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
            return this.Equals(obj as Envelope);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static bool operator ==(Envelope lhs, Envelope rhs)
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

        public static bool operator !=(Envelope lhs, Envelope rhs)
        {
            return !(lhs == rhs);
        }
    }
}
