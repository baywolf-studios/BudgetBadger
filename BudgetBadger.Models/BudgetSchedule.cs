﻿using System;
using PropertyChanged;

namespace BudgetBadger.Models
{
    [AddINotifyPropertyChangedInterface]
    public class BudgetSchedule
    {
        public Guid Id { get; set; }

        public DateTime BeginDate { get; set; }

        public DateTime EndDate { get; set; }

        //calculated
        public decimal Past { get; set; }

        //calculated
        public decimal Income { get; set; }

        //calculated 
        public decimal Budgeted { get; set; }

        public decimal ToBudget { get => Income - Budgeted; }

        //calculated
        public decimal Overspend { get; set; }

        public decimal Balance { get => Past + ToBudget - Overspend; }

        public DateTime? CreatedDateTime { get; set; }

        public DateTime? ModifiedDateTime { get; set; }

        public DateTime? DeletedDateTime { get; set; }

        public bool IsNew { get => CreatedDateTime == null; }

        public bool Exists { get => !IsNew; }

        public BudgetSchedule()
        {
            Id = Guid.Empty;
        }

        public BudgetSchedule DeepCopy()
        {
            return (BudgetSchedule)this.MemberwiseClone();
        }
    }
}
