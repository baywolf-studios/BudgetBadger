using System;
using System.Collections.Generic;
using System.Linq;
using BudgetBadger.Models.Interfaces;

namespace BudgetBadger.Models
{
    public class Payee : IValidatable, IDeepCopy<Payee>
    {
        public Guid Id { get; set; }

        public string Description { get; set; }

        public string Notes { get; set; }

        public DateTime? CreatedDateTime { get; set; }

        public DateTime? ModifiedDateTime { get; set; }

        public DateTime? DeletedDateTime { get; set; }

        //calculated
        public bool IsAccount { get; set; }

        public bool IsNew { get => CreatedDateTime == null; }

        public bool Exists { get => !IsNew; }

        public Payee()
        {
            Id = Guid.Empty;
        }

        public Payee DeepCopy()
        {
            return (Payee)this.MemberwiseClone();
        }

        public Result Validate()
        {
            var errors = new List<string>();

            if (string.IsNullOrEmpty(Description))
            {
                errors.Add("Payee Description Required");
            }

            return new Result { Success = !errors.Any(), Message = string.Join(Environment.NewLine, errors) };
        }
    }
}
