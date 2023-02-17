using System;
using BudgetBadger.Core.Dtos;
using BudgetBadger.Core.Models;

namespace BudgetBadger.Core.Converters
{
    public static class PayeeEditModelConverter
    {
        public static PayeeEditModel Convert(PayeeDto payeeDto)
        {
            return new PayeeEditModel
            {
                Id = payeeDto.Id,
                Description = payeeDto.Description,
                Notes = payeeDto.Notes ?? string.Empty,
                Hidden = payeeDto.Hidden
            };
        }

        public static PayeeDto Convert(PayeeEditModel payeeEditModel)
        {
            return new PayeeDto
            {
                Id = payeeEditModel.Id == Guid.Empty ? Guid.NewGuid() : payeeEditModel.Id,
                Description = payeeEditModel.Description,
                Notes = payeeEditModel.Notes == string.Empty ? null : payeeEditModel.Notes,
                ModifiedDateTime = DateTime.Now,
                Hidden = payeeEditModel.Hidden
            };
        }
    }
}

