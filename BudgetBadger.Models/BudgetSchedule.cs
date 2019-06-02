using System;
using System.Collections.Generic;
using System.Linq;
using BudgetBadger.Models.Interfaces;
using Newtonsoft.Json;

namespace BudgetBadger.Models
{
    public class BudgetSchedule : BaseModel, IDeepCopy<BudgetSchedule>, IEquatable<BudgetSchedule>
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
            set { SetProperty(ref past, value); OnPropertyChanged(nameof(Balance)); }
        }

        //calculated
        decimal income;
        public decimal Income
        {
            get => income;
            set { SetProperty(ref income, value); OnPropertyChanged(nameof(ToBudget)); OnPropertyChanged(nameof(Balance)); }
        }

        //calculated 
        decimal budgeted;
        public decimal Budgeted
        {
            get => budgeted;
            set { SetProperty(ref budgeted, value); OnPropertyChanged(nameof(ToBudget)); OnPropertyChanged(nameof(Balance)); }
        }

        public decimal ToBudget { get => Income - Budgeted; }

        //calculated
        decimal overspend;
        public decimal Overspend
        {
            get => overspend;
            set { SetProperty(ref overspend, value); OnPropertyChanged(nameof(Balance)); }
        }

        public decimal Balance { get => Past + ToBudget - Overspend; }

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

        public bool IsActive { get => !IsNew; }

        public BudgetSchedule()
        {
            Id = Guid.Empty;
        }

        public BudgetSchedule DeepCopy()
        {
            return (BudgetSchedule)this.MemberwiseClone();
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
                && EndDate == p.EndDate
                && Past == p.Past
                && Income == p.Income
                && Budgeted == p.Budgeted
                && Overspend == p.Overspend;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as BudgetSchedule);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static bool operator ==(BudgetSchedule lhs, BudgetSchedule rhs)
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

        public static bool operator !=(BudgetSchedule lhs, BudgetSchedule rhs)
        {
            return !(lhs == rhs);
        }
    }
}
