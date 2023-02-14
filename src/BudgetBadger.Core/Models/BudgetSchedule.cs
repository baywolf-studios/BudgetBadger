using System;
using System.Collections.Generic;
using System.Linq;
using BudgetBadger.Core.Models.Interfaces;

namespace BudgetBadger.Core.Models
{
    public class BudgetSchedule : ObservableBase, IDeepCopy<BudgetSchedule>, IEquatable<BudgetSchedule>
    {
        Guid id;
        public Guid Id
        {
            get => id;
            set => SetProperty(ref id, value);
        }

        //calculated
        string description;
        public string Description
        {
            get => description;
            set => SetProperty(ref description, value);
        }

        DateTime beginDate;
        public DateTime BeginDate
        {
            get => beginDate;
            set => SetProperty(ref beginDate, value);
        }

        DateTime endDate;
        public DateTime EndDate
        {
            get => endDate;
            set => SetProperty(ref endDate, value);
        }

        //calculated
        decimal past;
        public decimal Past
        {
            get => past;
            set { SetProperty(ref past, value); RaisePropertyChanged(nameof(Balance)); }
        }

        //calculated
        decimal income;
        public decimal Income
        {
            get => income;
            set { SetProperty(ref income, value); RaisePropertyChanged(nameof(ToBudget)); RaisePropertyChanged(nameof(Balance)); }
        }

        //calculated 
        decimal budgeted;
        public decimal Budgeted
        {
            get => budgeted;
            set { SetProperty(ref budgeted, value); RaisePropertyChanged(nameof(ToBudget)); RaisePropertyChanged(nameof(Balance)); }
        }

        public decimal ToBudget { get => Income - Budgeted; }

        //calculated
        decimal overspend;
        public decimal Overspend
        {
            get => overspend;
            set { SetProperty(ref overspend, value); RaisePropertyChanged(nameof(Balance)); }
        }

        public decimal Balance { get => Past + ToBudget - Overspend; }

        DateTime? createdDateTime;
        public DateTime? CreatedDateTime
        {
            get => createdDateTime;
            set { SetProperty(ref createdDateTime, value); RaisePropertyChanged(nameof(IsNew)); RaisePropertyChanged(nameof(IsActive)); }
        }

        public bool IsNew { get => CreatedDateTime == null; }

        DateTime? modifiedDateTime;
        public DateTime? ModifiedDateTime
        {
            get => modifiedDateTime;
            set => SetProperty(ref modifiedDateTime, value);
        }

        public bool IsActive { get => !IsNew; }

        public BudgetSchedule()
        {
            Id = Guid.Empty;
        }

        public BudgetSchedule DeepCopy()
        {
            var schedule = (BudgetSchedule)this.MemberwiseClone();
            schedule.Description = this.Description == null ? null : String.Copy(this.Description);
            return schedule;
        }

        public bool Equals(BudgetSchedule p)
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
                && Description == p.Description
                && BeginDate == p.BeginDate
                && EndDate == p.EndDate;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as BudgetSchedule);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
