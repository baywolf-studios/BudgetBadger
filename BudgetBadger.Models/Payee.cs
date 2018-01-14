using System;
using PropertyChanged;

namespace BudgetBadger.Models
{
    [AddINotifyPropertyChangedInterface]
    public class Payee
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
    }
}
