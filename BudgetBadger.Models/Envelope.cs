using System;
using PropertyChanged;

namespace BudgetBadger.Models
{
    [AddINotifyPropertyChangedInterface]
    public class Envelope
    {
        public Guid Id { get; set; }

        public string Description { get; set; }

        public string Notes { get; set; }

        public EnvelopeGroup Group { get; set; }

        public DateTime? CreatedDateTime { get; set; }

        public DateTime? ModifiedDateTime { get; set; }

        public DateTime? DeletedDateTime { get; set; }

        public bool IsNew { get => CreatedDateTime == null; }

        public bool Exists { get => !IsNew; }

        public Envelope()
        {
            Id = Guid.Empty;
            Group = new EnvelopeGroup();
        }

        public Envelope DeepCopy()
        {
            Envelope envelope = (Envelope)this.MemberwiseClone();
            envelope.Group = this.Group.DeepCopy();

            return envelope;
        }
    }
}
