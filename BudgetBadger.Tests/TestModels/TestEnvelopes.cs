using System;
using BudgetBadger.Models;

namespace BudgetBadger.Tests.TestModels
{
    public static class TestEnvelopes
    {
        public static readonly EnvelopeGroup NewEnvelopeGroup = new EnvelopeGroup
        {
            Description = nameof(NewEnvelopeGroup)
        };

        public static readonly EnvelopeGroup ActiveEnvelopeGroup = new EnvelopeGroup
        {
            Id = Guid.NewGuid(),
            Description = nameof(ActiveEnvelopeGroup),
            CreatedDateTime = DateTime.Now,
            ModifiedDateTime = DateTime.Now
        };

        public static readonly EnvelopeGroup HiddenEnvelopeGroup = new EnvelopeGroup
        {
            Id = Guid.NewGuid(),
            Description = nameof(HiddenEnvelopeGroup),
            CreatedDateTime = DateTime.Now,
            ModifiedDateTime = DateTime.Now,
            HiddenDateTime = DateTime.Now
        };

        public static readonly EnvelopeGroup SoftDeletedEnvelopeGroup = new EnvelopeGroup
        {
            Id = Guid.NewGuid(),
            Description = nameof(SoftDeletedEnvelopeGroup),
            CreatedDateTime = DateTime.Now,
            ModifiedDateTime = DateTime.Now,
            HiddenDateTime = DateTime.Now,
            DeletedDateTime = DateTime.Now
        };

        public static readonly Envelope NewEnvelope = new Envelope
        {
            Description = nameof(NewEnvelope)
        };

        public static readonly Envelope ActiveEnvelope = new Envelope
        {
            Id = Guid.NewGuid(),
            Description = nameof(ActiveEnvelope),
            CreatedDateTime = DateTime.Now,
            ModifiedDateTime = DateTime.Now
        };

        public static readonly Envelope HiddenEnvelope = new Envelope
        {
            Id = Guid.NewGuid(),
            Description = nameof(HiddenEnvelope),
            CreatedDateTime = DateTime.Now,
            ModifiedDateTime = DateTime.Now,
            HiddenDateTime = DateTime.Now
        };

        public static readonly Envelope SoftDeletedEnvelope = new Envelope
        {
            Id = Guid.NewGuid(),
            Description = nameof(SoftDeletedEnvelope),
            CreatedDateTime = DateTime.Now,
            ModifiedDateTime = DateTime.Now,
            HiddenDateTime = DateTime.Now,
            DeletedDateTime = DateTime.Now
        };
    }
}
