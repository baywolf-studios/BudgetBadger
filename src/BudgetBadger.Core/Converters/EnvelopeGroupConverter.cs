using System;
using BudgetBadger.Core.Localization;
using BudgetBadger.Core.Models;
using BudgetBadger.DataAccess.Dtos;
using BudgetBadger.Logic.Models;

namespace BudgetBadger.Logic.Converters
{
    public static class EnvelopeGroupConverter
    {
        public static EnvelopeGroup Convert(EnvelopeGroupDto envelopeGroupDto)
        {
            if (envelopeGroupDto.Id == Constants.SystemEnvelopeGroupId)
            {
                return new EnvelopeGroup()
                {
                    Id = new EnvelopeGroupId(Constants.SystemEnvelopeGroupId),
                    Description = AppResources.SystemEnvelopeGroup,
                    Notes = string.Empty,
                    Hidden = false
                };
            }
            else if (envelopeGroupDto.Id == Constants.IncomeEnvelopeGroupId)
            {
                return new EnvelopeGroup()
                {
                    Id = new EnvelopeGroupId(Constants.IncomeEnvelopeGroupId),
                    Description = AppResources.IncomeEnvelopeGroup,
                    Notes = string.Empty,
                    Hidden = false
                };
            }
            else if (envelopeGroupDto.Id == Constants.DebtEnvelopeGroupId)
            {
                return new EnvelopeGroup()
                {
                    Id = new EnvelopeGroupId(Constants.DebtEnvelopeGroupId),
                    Description = AppResources.DebtEnvelopeGroup,
                    Notes = string.Empty,
                    Hidden = false
                };
            }
            else
            {
                return new EnvelopeGroup()
                {
                    Id = new EnvelopeGroupId(envelopeGroupDto.Id),
                    Description = envelopeGroupDto.Description,
                    Notes = envelopeGroupDto.Notes ?? string.Empty,
                    Hidden = envelopeGroupDto.Hidden
                };
            }
        }
    }
}

