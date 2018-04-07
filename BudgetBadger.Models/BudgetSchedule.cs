﻿using System;
using System.Collections.Generic;
using System.Linq;
using BudgetBadger.Models.Interfaces;

namespace BudgetBadger.Models
{
    public class BudgetSchedule : BaseModel, IValidatable, IDeepCopy<BudgetSchedule>
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

        public Result Validate()
        {
            var errors = new List<string>();

            if (string.IsNullOrEmpty(Description))
            {
                errors.Add("Budget schedule description is required");
            }

            return new Result { Success = !errors.Any(), Message = string.Join(Environment.NewLine, errors) };
        }
    }
}
