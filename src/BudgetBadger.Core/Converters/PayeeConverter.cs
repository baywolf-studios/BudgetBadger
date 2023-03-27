using System;
using BudgetBadger.Core.Localization;
using BudgetBadger.DataAccess.Dtos;
using BudgetBadger.Logic.Models;

namespace BudgetBadger.Logic.Converters
{
    public static class PayeeConverter
    {
        public static Payee Convert(PayeeDto payeeDto)
        {
            if (payeeDto.Id == Core.Models.Constants.StartingBalancePayee.Id)
            {
                return new Payee()
                {
                    Id = new PayeeId(Core.Models.Constants.StartingBalancePayeeId),
                    Description = AppResources.StartingBalancePayee,
                    Notes = string.Empty,
                    Hidden = false,
                    Group = AppResources.StartingBalancePayee
                };
            }
            else
            {
                return new Payee()
                {
                    Id = new PayeeId(payeeDto.Id),
                    Description = payeeDto.Description,
                    Notes = payeeDto.Notes ?? string.Empty,
                    Hidden = payeeDto.Hidden,
                    Group = !string.IsNullOrEmpty(payeeDto.Description) ? payeeDto.Description[0].ToString().ToUpper() : string.Empty
                };
            }
        }

        public static Payee Convert(AccountDto accountDto)
        {
            return new Payee()
            {
                Id = new PayeeId(accountDto.Id),
                Description = accountDto.Description,
                Notes = accountDto.Notes ?? string.Empty,
                Hidden = accountDto.Hidden,
                Group = AppResources.PayeeTransferGroup
            };
        }
    }
}

