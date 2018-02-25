using System;
using System.Collections.Generic;
using System.Linq;
using BudgetBadger.Models.Interfaces;

namespace BudgetBadger.Models
{
    public class BudgetSchedule : IValidatable, IDeepCopy<BudgetSchedule>
    {
        public Guid Id { get; set; }

        //calculated
        public string Description { get; set; }

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
