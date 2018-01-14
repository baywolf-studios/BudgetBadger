using System;
using PropertyChanged;

namespace BudgetBadger.Models
{
    [AddINotifyPropertyChangedInterface]
    public class Budget
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
    }
}
