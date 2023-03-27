using System;
using BudgetBadger.Core.Models;
using BudgetBadger.Logic.Models;

namespace BudgetBadger.Core.Converters
{
    public static class EnvelopeGroupModelConverter
    {
        public static EnvelopeGroupModel Convert(EnvelopeGroup envelopeGroup)
        {
            return new EnvelopeGroupModel()
            {
                Id = envelopeGroup.Id,
                Description = envelopeGroup.Description,
                Notes = envelopeGroup.Notes,
                CreatedDateTime = DateTime.Now,
                ModifiedDateTime = DateTime.Now,
                HiddenDateTime = envelopeGroup.Hidden ? DateTime.Now : null,
                DeletedDateTime = null
            };
        }
    }
}

