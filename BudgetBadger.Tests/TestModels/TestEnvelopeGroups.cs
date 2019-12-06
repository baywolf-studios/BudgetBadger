using System;
using BudgetBadger.Models;

namespace BudgetBadger.Tests.TestModels
{
    public static class TestEnvelopeGroups
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
    }
}
