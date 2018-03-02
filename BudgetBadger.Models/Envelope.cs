using System;
using System.Collections.Generic;
using System.Linq;
using BudgetBadger.Models.Extensions;
using BudgetBadger.Models.Interfaces;

namespace BudgetBadger.Models
{
    public class Envelope : BaseModel, IValidatable, IDeepCopy<Envelope>
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
    }
}
