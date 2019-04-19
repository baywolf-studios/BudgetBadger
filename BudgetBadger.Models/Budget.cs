using System;
using System.Collections.Generic;
using System.Linq;
using BudgetBadger.Models.Extensions;
using BudgetBadger.Models.Interfaces;
using Newtonsoft.Json;

namespace BudgetBadger.Models
{
    public class Budget : BaseModel, IDeepCopy<Budget>, IEquatable<Budget>, IComparable, IComparable<Budget>
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

        public OverspendingType HandleOverspend
        {
            get
            {
                if (Envelope != null && Envelope.IgnoreOverspend)
                {
                    return OverspendingType.AlwaysIgnore;
                }
                else if (IgnoreOverspend)
                {
                    return OverspendingType.Ignore;
                }
                else
                {
                    return OverspendingType.DoNotIgnore;
                }
            }
            set
            {
                IgnoreOverspend = (value == OverspendingType.Ignore || value == OverspendingType.AlwaysIgnore);
                if (Envelope != null)
                {
                    Envelope.IgnoreOverspend = (value == OverspendingType.AlwaysIgnore);
                }
            }
        }

        bool ignoreOverspend;
        public bool IgnoreOverspend
        {
            get => ignoreOverspend;
            set
            {
                SetProperty(ref ignoreOverspend, value);
                if (value == false && Envelope != null)
                {
                    Envelope.IgnoreOverspend = false;
                }
            }
        }

        decimal? amount;
        public decimal? Amount
        {
            get => amount;
            set { SetProperty(ref amount, value); OnPropertyChanged(nameof(Remaining)); }
        }

        //calculated, not stored
        decimal pastAmount;
        public decimal PastAmount
        {
            get => pastAmount;
            set { SetProperty(ref pastAmount, value); OnPropertyChanged(nameof(Remaining)); OnPropertyChanged(nameof(Past)); }
        }

        //calculated, not stored
        decimal activity;
        public decimal Activity
        {
            get => activity;
            set { SetProperty(ref activity, value); OnPropertyChanged(nameof(Remaining)); }
        }

        //calculated, not stored
        decimal pastActivity;
        public decimal PastActivity
        {
            get => pastActivity;
            set { SetProperty(ref pastActivity, value); OnPropertyChanged(nameof(Remaining)); OnPropertyChanged(nameof(Past)); }
        }

        public decimal Past { get => PastAmount + PastActivity; }

        public decimal Remaining { get { return (Amount ?? 0) + PastAmount + Activity + PastActivity; } }

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

        public bool IsActive { get => !IsNew && !Envelope.IsDeleted; }

        public Budget()
        {
            Id = Guid.Empty;
            Schedule = new BudgetSchedule();
            Envelope = new Envelope();
        }

        public Budget DeepCopy()
        {
            var serial = JsonConvert.SerializeObject(this);
            return JsonConvert.DeserializeObject<Budget>(serial);
        }

        public bool Equals(Budget p)
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
            return JsonConvert.SerializeObject(this) == JsonConvert.SerializeObject(p);
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as Budget);
        }

        public override int GetHashCode()
        {
            return JsonConvert.SerializeObject(this).GetHashCode();
        }

        public static bool operator ==(Budget lhs, Budget rhs)
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

        public static bool operator !=(Budget lhs, Budget rhs)
        {
            return !(lhs == rhs);
        }

        public int CompareTo(object obj)
        {
            return CompareTo(obj as Budget);
        }

        public int CompareTo(Budget budget)
        {
            if (budget is null)
            {
                return 1;
            }

            return Envelope.CompareTo(budget.Envelope);
        }
    }
}
