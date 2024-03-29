﻿using System;
using System.Collections.Generic;
using System.Linq;
using BudgetBadger.Models.Extensions;
using BudgetBadger.Models.Interfaces;

namespace BudgetBadger.Models
{
    public class Budget : ObservableBase, IDeepCopy<Budget>, IEquatable<Budget>, IComparable, IComparable<Budget>
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
                    return OverspendingType.OverspendingTypeAlwaysIgnore;
                }
                else if (IgnoreOverspend)
                {
                    return OverspendingType.OverspendingTypeIgnore;
                }
                else
                {
                    return OverspendingType.OverspendingTypeDoNotIgnore;
                }
            }
            set
            {
                var isIgnoreOverspend = (value == OverspendingType.OverspendingTypeIgnore || value == OverspendingType.OverspendingTypeAlwaysIgnore);
                var isIgnoreOverspendAlways = (value == OverspendingType.OverspendingTypeAlwaysIgnore);
                if (Envelope != null)
                {
                    Envelope.IgnoreOverspend = isIgnoreOverspendAlways;
                }
                IgnoreOverspend = isIgnoreOverspend;
            }
        }

        bool ignoreOverspend;
        public bool IgnoreOverspend
        {
            get => ignoreOverspend;
            set
            {
                if (SetProperty(ref ignoreOverspend, value))
                {
                    RaisePropertyChanged(nameof(HandleOverspend));
                }
                if (value == false && Envelope != null && Envelope.IgnoreOverspend != false)
                {
                    Envelope.IgnoreOverspend = false;
                }
            }
        }

        decimal? amount;
        public decimal? Amount
        {
            get => amount;
            set { SetProperty(ref amount, value); RaisePropertyChanged(nameof(Remaining)); }
        }

        //calculated, not stored
        decimal pastAmount;
        public decimal PastAmount
        {
            get => pastAmount;
            set { SetProperty(ref pastAmount, value); RaisePropertyChanged(nameof(Remaining)); RaisePropertyChanged(nameof(Past)); }
        }

        //calculated, not stored
        decimal activity;
        public decimal Activity
        {
            get => activity;
            set { SetProperty(ref activity, value); RaisePropertyChanged(nameof(Remaining)); }
        }

        //calculated, not stored
        decimal pastActivity;
        public decimal PastActivity
        {
            get => pastActivity;
            set { SetProperty(ref pastActivity, value); RaisePropertyChanged(nameof(Remaining)); RaisePropertyChanged(nameof(Past)); }
        }

        public decimal Past { get => PastAmount + PastActivity; }

        public decimal Remaining { get { return (Amount ?? 0) + PastAmount + Activity + PastActivity; } }

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

        public bool IsActive { get => !IsNew && Envelope.IsActive; }

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
            return Id == p.Id
                && CreatedDateTime == p.CreatedDateTime
                && ModifiedDateTime == p.ModifiedDateTime
                && Schedule.Equals(p.Schedule)
                && Envelope.Equals(p.Envelope)
                && IgnoreOverspend == p.IgnoreOverspend
                && Amount == p.Amount;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as Budget);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
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
