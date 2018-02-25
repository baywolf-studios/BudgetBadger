using System;
using System.Collections.Generic;
using System.Linq;
using BudgetBadger.Models.Extensions;
using BudgetBadger.Models.Interfaces;

namespace BudgetBadger.Models
{
    public class Budget : IValidatable, IDeepCopy<Budget>
    {
        public Guid Id { get; set; }

        public BudgetSchedule Schedule { get; set; }

        public Envelope Envelope { get; set; }

        public decimal Amount { get; set; }

        public bool IgnoreOverspend { get; set; }

        //calculated, not stored
        public decimal PastAmount { get; set; }

        //calculated, not stored
        public decimal Activity { get; set; }

        //calculated, not stored
        public decimal PastActivity { get; set; }

        public decimal Remaining { get { return Amount + PastAmount + Activity + PastActivity; } }

        public DateTime? CreatedDateTime { get; set; }

        public DateTime? ModifiedDateTime { get; set; }

        public DateTime? DeletedDateTime { get; set; }

        public bool IsNew { get => CreatedDateTime == null; }

        public bool Exists { get => !IsNew; }

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
