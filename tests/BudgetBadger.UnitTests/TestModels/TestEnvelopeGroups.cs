using System;
using BudgetBadger.Core.Models;

namespace BudgetBadger.UnitTests.TestModels
{
    public static class TestEnvelopeGroups
    {
        public static readonly EnvelopeGroupModel NewEnvelopeGroup = new EnvelopeGroupModel
        {
            Description = nameof(NewEnvelopeGroup)
        };

        public static readonly EnvelopeGroupModel ActiveEnvelopeGroup = new EnvelopeGroupModel
        {
            Id = Guid.NewGuid(),
            Description = nameof(ActiveEnvelopeGroup),
            CreatedDateTime = DateTime.Now,
            ModifiedDateTime = DateTime.Now
        };

        public static readonly EnvelopeGroupModel HiddenEnvelopeGroup = new EnvelopeGroupModel
        {
            Id = Guid.NewGuid(),
            Description = nameof(HiddenEnvelopeGroup),
            CreatedDateTime = DateTime.Now,
            ModifiedDateTime = DateTime.Now,
            HiddenDateTime = DateTime.Now
        };

        public static readonly EnvelopeGroupModel SoftDeletedEnvelopeGroup = new EnvelopeGroupModel
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
