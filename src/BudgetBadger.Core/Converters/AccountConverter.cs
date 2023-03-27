using System;
using System.Collections.Generic;
using System.Linq;
using BudgetBadger.Core.Localization;
using BudgetBadger.Core.Models;
using BudgetBadger.DataAccess;
using BudgetBadger.DataAccess.Dtos;
using BudgetBadger.Logic.Models;

namespace BudgetBadger.Logic.Converters
{
    public static class AccountConverter
    {
        public static Account Convert(AccountDto accountDto,
            IEnumerable<TransactionDto> transactions,
            IEnumerable<BudgetDto> budgets,
            IEnumerable<BudgetPeriodDto> budgetPeriods)
        {
            var nonDeletedTransactions = transactions.Distinct().Where(t => !t.Deleted).ToList();
            var accountTransactions = nonDeletedTransactions.Where(t => t.AccountId == accountDto.Id).ToList();
            var payeeTransactions = nonDeletedTransactions.Where(t => t.PayeeId == accountDto.Id).ToList();

            var pending = accountTransactions.Where(a => a.Pending).Sum(t => t.Amount)
                - payeeTransactions.Where(a => a.Pending).Sum(t => t.Amount);

            var posted = accountTransactions.Where(a => a.Posted).Sum(t => t.Amount)
                - payeeTransactions.Where(t => t.Posted).Sum(t => t.Amount);

            var balance = pending + posted;

            // payment 
            var today = DateTime.Today;

            var debt = accountTransactions.Where(a => a.EnvelopeId == accountDto.Id && a.ServiceDate.Date <= today).Sum(t => t.Amount)
                - payeeTransactions.Where(a => a.EnvelopeId == accountDto.Id && a.ServiceDate.Date <= today).Sum(t => t.Amount);

            var debtBudgets = budgets.Select(b =>
                (
                    BudgetDto: b,
                    BudgetPeriodDto: budgetPeriods.FirstOrDefault(p => p.Id == b.BudgetPeriodId)
                ));

            var amountBudgetedToPayDownDebt = debtBudgets.Where(b => b.BudgetPeriodDto.BeginDate.Date <= today).Sum(b => b.BudgetDto.Amount);

            var payment = amountBudgetedToPayDownDebt + debt - balance;

            return new Account()
            {
                Id = new AccountId(accountDto.Id),
                Description = accountDto.Description,
                Notes = accountDto.Notes ?? string.Empty,
                Type = accountDto.OnBudget ? AccountType.Budget : AccountType.Reporting,
                Pending = pending,
                Posted = posted,
                Balance = balance,
                Payment = payment >= 0 ? payment : 0,
                Hidden = accountDto.Hidden,
                Group = accountDto.OnBudget ? AppResources.AccountTypeBudget : AppResources.AccountTypeReporting
            };
        }
    }
}

