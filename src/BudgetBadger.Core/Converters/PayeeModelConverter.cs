using System;
using BudgetBadger.Core.Localization;
using BudgetBadger.Core.Models;
using BudgetBadger.Logic.Models;

namespace BudgetBadger.Logic.Converters
{
    public static class PayeeModelConverter
    {
        public static PayeeModel Convert(Payee payee)
        {
            return new PayeeModel()
            {
                Id = payee.Id,
                Description = payee.Description,
                Notes = payee.Notes,
                Group = payee.Group,
                IsAccount = payee.Group == AppResources.PayeeTransferGroup,
                CreatedDateTime = DateTime.Now,
                ModifiedDateTime = DateTime.Now,
                HiddenDateTime = payee.Hidden ? DateTime.Now : null,
                DeletedDateTime = null
            };
        }
    }
}

