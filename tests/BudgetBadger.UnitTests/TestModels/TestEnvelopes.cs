using System;
using BudgetBadger.Models;
using NUnit.Framework.Internal;

namespace BudgetBadger.UnitTests.TestModels
{
    public static class TestEnvelopes
    {
        public static readonly Envelope NewEnvelope = new Envelope
        {
            Description = nameof(NewEnvelope)
        };

        public static readonly Envelope ActiveEnvelope = new Envelope
        {
            Id = Guid.NewGuid(),
            Description = nameof(ActiveEnvelope),
            Group = TestEnvelopeGroups.ActiveEnvelopeGroup,
            CreatedDateTime = DateTime.Now,
            ModifiedDateTime = DateTime.Now
        };

        public static readonly Envelope HiddenEnvelope = new Envelope
        {
            Id = Guid.NewGuid(),
            Description = nameof(HiddenEnvelope),
            Group = TestEnvelopeGroups.HiddenEnvelopeGroup,
            CreatedDateTime = DateTime.Now,
            ModifiedDateTime = DateTime.Now,
            HiddenDateTime = DateTime.Now
        };

        public static readonly Envelope SoftDeletedEnvelope = new Envelope
        {
            Id = Guid.NewGuid(),
            Description = nameof(SoftDeletedEnvelope),
            Group = TestEnvelopeGroups.SoftDeletedEnvelopeGroup,
            CreatedDateTime = DateTime.Now,
            ModifiedDateTime = DateTime.Now,
            HiddenDateTime = DateTime.Now,
            DeletedDateTime = DateTime.Now
        };
    }
}
