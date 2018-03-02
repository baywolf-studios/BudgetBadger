using System;
using System.Collections.Generic;
using System.Linq;
using BudgetBadger.Models.Extensions;
using BudgetBadger.Models.Interfaces;

namespace BudgetBadger.Models
{
    public class Budget : BaseModel, IValidatable, IDeepCopy<Budget>
    {
        Guid id;
        public Guid Id
        {
            get => id;
            set => SetProperty(ref id, value);
        }

        BudgetSchedule schedule;
        public BudgetSchedule Schedule
        {
            get => schedule;
            set => SetProperty(ref schedule, value);
        }

        Envelope envelope;
        public Envelope Envelope
        {
            get => envelope;
            set => SetProperty(ref envelope, value);
        }

        bool ignoreOverspend;
        public bool IgnoreOverspend
        {
            get => ignoreOverspend;
            set => SetProperty(ref ignoreOverspend, value);
        }

        decimal amount;
        public decimal Amount
        {
            get => amount;
            set { SetProperty(ref amount, value); OnPropertyChanged("Remaining"); }
        }

        //calculated, not stored
        decimal pastAmount;
        public decimal PastAmount
        {
            get => pastAmount;
            set { SetProperty(ref pastAmount, value); OnPropertyChanged("Remaining"); }
        }

        //calculated, not stored
        decimal activity;
        public decimal Activity
        {
            get => activity;
            set { SetProperty(ref activity, value); OnPropertyChanged("Remaining"); }
        }

        //calculated, not stored
        decimal pastActivity;
        public decimal PastActivity
        {
            get => pastActivity;
            set { SetProperty(ref pastActivity, value); OnPropertyChanged("Remaining"); }
        }

        public decimal Remaining { get { return Amount + PastAmount + Activity + PastActivity; } }

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

        //delete the deletedDateTime and rely on the envelope
        DateTime? deletedDateTime;
        public DateTime? DeletedDateTime
        {
            get => deletedDateTime;
            set { SetProperty(ref deletedDateTime, value); OnPropertyChanged("IsDeleted"); OnPropertyChanged("IsActive"); }
        }

        public bool IsDeleted { get => DeletedDateTime != null; }

        public bool IsActive { get => !IsNew && !IsDeleted; }

        public Budget()
        {
            Id = Guid.Empty;
            Schedule = new BudgetSchedule();
            Envelope = new Envelope();
        }

        public Budget DeepCopy()
        {
            Budget budget = (Budget)this.MemberwiseClone();
            budget.Schedule = this.Schedule.DeepCopy();
            budget.Envelope = this.Envelope.DeepCopy();

            return budget;
        }

        public Result Validate()
        {
            var errors = new List<string>();

            if (Envelope == null)
            {
                errors.Add("Envelope is required");
            }
            else if (!Envelope.IsValid())
            {
                errors.Add("A valid envelope is required");
            }

            if (Schedule == null)
            {
                errors.Add("Schedule is required");
            }
            else if (!Schedule.IsValid())
            {
                errors.Add("A valid schedule is required");
            }

            return new Result { Success = !errors.Any(), Message = string.Join(Environment.NewLine, errors) };
        }
    }
}
