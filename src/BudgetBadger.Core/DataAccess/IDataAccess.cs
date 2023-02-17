﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using BudgetBadger.Core.Dtos;
using BudgetBadger.Core.Models;

namespace BudgetBadger.Core.DataAccess
{
	public interface IDataAccess
	{
		Task Init();

		Task CreateAccountAsync(Account account);
        Task<Account> ReadAccountAsync(Guid id);
		Task<IReadOnlyList<Account>> ReadAccountsAsync();
        Task UpdateAccountAsync(Account account);

        Task CreateEnvelopeAsync(Envelope envelope);
		Task<Envelope> ReadEnvelopeAsync(Guid id);
		Task<IReadOnlyList<Envelope>> ReadEnvelopesAsync();
		Task UpdateEnvelopeAsync(Envelope envelope);

		Task CreateBudgetAsync(Budget budget);
		Task<Budget> ReadBudgetAsync(Guid id);
		Task<IReadOnlyList<Budget>> ReadBudgetsAsync();
		Task<IReadOnlyList<Budget>> ReadBudgetsFromScheduleAsync(Guid scheduleId);
		Task<IReadOnlyList<Budget>> ReadBudgetsFromEnvelopeAsync(Guid envelopeId);
		Task<Budget> ReadBudgetFromScheduleAndEnvelopeAsync(Guid scheduleId, Guid envelopeId);
		Task UpdateBudgetAsync(Budget budget);

		Task CreateEnvelopeGroupAsync(EnvelopeGroup envelopeGroup);
		Task<EnvelopeGroup> ReadEnvelopeGroupAsync(Guid id);
		Task<IReadOnlyList<EnvelopeGroup>> ReadEnvelopeGroupsAsync();
		Task UpdateEnvelopeGroupAsync(EnvelopeGroup envelopeGroup);

		Task CreateBudgetScheduleAsync(BudgetSchedule budgetSchedule);
		Task<BudgetSchedule> ReadBudgetScheduleAsync(Guid id);
		Task<IReadOnlyList<BudgetSchedule>> ReadBudgetSchedulesAsync();
		Task UpdateBudgetScheduleAsync(BudgetSchedule budgetSchedule);
		
		Task CreatePayeeAsync(Payee payee);
        Task<Payee> ReadPayeeAsync(Guid id);
		Task<IReadOnlyList<Payee>> ReadPayeesAsync();
        Task UpdatePayeeAsync(Payee payee);

        Task CreateTransactionAsync(Transaction transaction);
        Task<Transaction> ReadTransactionAsync(Guid id);
		Task<IReadOnlyList<Transaction>> ReadAccountTransactionsAsync(Guid accountId);
		Task<IReadOnlyList<Transaction>> ReadPayeeTransactionsAsync(Guid payeeId);
        Task<IReadOnlyList<Transaction>> ReadEnvelopeTransactionsAsync(Guid envelopeId);
		Task<IReadOnlyList<Transaction>> ReadSplitTransactionsAsync(Guid splitId);
		Task<IReadOnlyList<Transaction>> ReadTransactionsAsync();
		Task UpdateTransactionAsync(Transaction transaction);


        Task CreateAccountDtoAsync(AccountDto account);
        Task<IReadOnlyList<AccountDto>> ReadAccountDtosAsync(IEnumerable<Guid> accountIds = null);
        Task UpdateAccountDtoAsync(AccountDto account);

        Task CreatePayeeDtoAsync(PayeeDto payee);
        Task<IReadOnlyList<PayeeDto>> ReadPayeeDtosAsync(IEnumerable<Guid> payeeIds = null);
        Task UpdatePayeeDtoAsync(PayeeDto payee);

        Task CreateEnvelopeGroupDtoAsync(EnvelopeGroupDto envelopeGroup);
        Task<IReadOnlyList<EnvelopeGroupDto>> ReadEnvelopeGroupDtosAsync(IEnumerable<Guid> envelopeGroupIds = null);
        Task UpdateEnvelopeGroupDtoAsync(EnvelopeGroupDto envelopeGroup);

        Task CreateEnvelopeDtoAsync(EnvelopeDto envelope);
        Task<IReadOnlyList<EnvelopeDto>> ReadEnvelopeDtosAsync(IEnumerable<Guid> envelopeIds = null, IEnumerable<Guid> envelopeGroupIds = null);
        Task UpdateEnvelopeDtoAsync(EnvelopeDto envelope);

        Task CreateBudgetPeriodDtoAsync(BudgetPeriodDto budgetPeriod);
        Task<IReadOnlyList<BudgetPeriodDto>> ReadBudgetPeriodDtosAsync(IEnumerable<Guid> budgetPeriodIds = null);
        Task UpdateBudgetPeriodDtoAsync(BudgetPeriodDto budgetPeriod);

        Task CreateBudgetDtoAsync(BudgetDto budget);
        Task<IReadOnlyList<BudgetDto>> ReadBudgetDtosAsync(
            IEnumerable<Guid> budgetIds = null,
            IEnumerable<Guid> budgetPeriodIds = null,
            IEnumerable<Guid> envelopeIds = null);
        Task UpdateBudgetDtoAsync(BudgetDto budget);

        Task CreateTransactionDtoAsync(TransactionDto transaction);
        Task<IReadOnlyList<TransactionDto>> ReadTransactionDtosAsync(
            IEnumerable<Guid> transactionIds = null,
            IEnumerable<Guid> accountIds = null,
            IEnumerable<Guid> payeeIds = null,
            IEnumerable<Guid> envelopeIds = null,
            IEnumerable<Guid> splitIds = null);
        Task UpdateTransactionDtoAsync(TransactionDto transaction);
    }
}

