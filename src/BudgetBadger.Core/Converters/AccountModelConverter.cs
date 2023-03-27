using System;
using BudgetBadger.Core.Models;
using BudgetBadger.Logic.Models;

namespace BudgetBadger.Logic.Converters
{
    public static class AccountModelConverter
    {
        public static AccountModel Convert(Account account)
        {
            return new AccountModel()
            {
                Id = account.Id,
                Description = account.Description,
                Notes = account.Notes,
                Group = account.Group,
                Balance = account.Balance,
                OnBudget = account.Type == AccountType.Budget,
                Payment = account.Payment,
                Pending = account.Pending,
                Posted = account.Posted,
                CreatedDateTime = DateTime.Now,
                ModifiedDateTime = DateTime.Now,
                HiddenDateTime = account.Hidden ? DateTime.Now : null,
                DeletedDateTime = null
            };
        }
    }
}

