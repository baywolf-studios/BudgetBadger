using System;
using PropertyChanged;

namespace BudgetBadger.Models
{
    [AddINotifyPropertyChangedInterface]
    public class BudgetSchedule
    {
        public Guid Id { get; set; }

        public DateTime BeginDate { get; set; }

        public DateTime EndDate { get; set; }

        public DateTime? CreatedDateTime { get; set; }

        public DateTime? ModifiedDateTime { get; set; }

        public DateTime? DeletedDateTime { get; set; }

        public BudgetSchedule()
        {
            Id = Guid.NewGuid();
        }

        public BudgetSchedule DeepCopy()
        {
            return (BudgetSchedule)this.MemberwiseClone();
        }
    }
}
