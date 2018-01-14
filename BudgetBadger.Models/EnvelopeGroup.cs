using System;
using PropertyChanged;

namespace BudgetBadger.Models
{
    [AddINotifyPropertyChangedInterface]
    public class EnvelopeGroup
    {
        public Guid Id { get; set; }

        public string Description { get; set; }

        public string Notes { get; set; }

        public DateTime? CreatedDateTime { get; set; }

        public DateTime? ModifiedDateTime { get; set; }

        public DateTime? DeletedDateTime { get; set; }

        public bool IsNew { get => CreatedDateTime == null; }

        public bool Exists { get => !IsNew; }

        public EnvelopeGroup()
        {
            Id = Guid.Empty;
        }

        public EnvelopeGroup DeepCopy()
        {
            return (EnvelopeGroup)this.MemberwiseClone();
        }
    }
}
