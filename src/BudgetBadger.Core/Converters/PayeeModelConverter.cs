using System;
using BudgetBadger.Core.Dtos;
using BudgetBadger.Core.Models;

namespace BudgetBadger.Core.Converters
{
	public static class PayeeModelConverter
	{
		public static PayeeModel Convert(PayeeDto payeeDto)
		{
			if (payeeDto.Id == Constants.StartingBalancePayee.Id)
			{
				return new StartingBalancePayeeModel();
			}
			else
			{
				return new PayeeModel()
				{
					Id = payeeDto.Id,
					Description = payeeDto.Description,
					Notes = payeeDto.Notes ?? string.Empty
				};
			}
		}

        public static PayeeModel Convert(AccountDto accountDto)
        {
			return new AccountPayeeModel()
			{
				Id = accountDto.Id,
				Description = accountDto.Description,
				Notes = accountDto.Notes ?? string.Empty
			};
        }
    }
}

