using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BudgetBadger.Models;

namespace BudgetBadger.Core.DataAccess
{
	public interface IDataAccess
	{
		Task Init();

		Task CreateAccountAsync(Account account);
		Task<Account> ReadAccountAsync(Guid id);
		Task<IReadOnlyList<Account>> ReadAccountsAsync();
		Task UpdateAccountAsync(Account account);
		Task DeleteAccountAsync(Guid id);
		Task<int> GetAccountsCountAsync();
		
		Task CreateEnvelopeAsync(Envelope envelope);
		Task<Envelope> ReadEnvelopeAsync(Guid id);
		Task<IReadOnlyList<Envelope>> ReadEnvelopesAsync();
		Task UpdateEnvelopeAsync(Envelope envelope);
		Task DeleteEnvelopeAsync(Guid id);
		Task<int> GetEnvelopesCountAsync();

		Task CreateBudgetAsync(Budget budget);
		Task<Budget> ReadBudgetAsync(Guid id);
		Task<IReadOnlyList<Budget>> ReadBudgetsAsync();
		Task<IReadOnlyList<Budget>> ReadBudgetsFromScheduleAsync(Guid scheduleId);
		Task<IReadOnlyList<Budget>> ReadBudgetsFromEnvelopeAsync(Guid envelopeId);
		Task<Budget> ReadBudgetFromScheduleAndEnvelopeAsync(Guid scheduleId, Guid envelopeId);
		Task UpdateBudgetAsync(Budget budget);
		Task DeleteBudgetAsync(Guid id);

		Task CreateEnvelopeGroupAsync(EnvelopeGroup envelopeGroup);
		Task<EnvelopeGroup> ReadEnvelopeGroupAsync(Guid id);
		Task<IReadOnlyList<EnvelopeGroup>> ReadEnvelopeGroupsAsync();
		Task UpdateEnvelopeGroupAsync(EnvelopeGroup envelopeGroup);
		Task DeleteEnvelopeGroupAsync(Guid id);
		Task<int>GetEnvelopeGroupsCountAsync();

		Task CreateBudgetScheduleAsync(BudgetSchedule budgetSchedule);
		Task<BudgetSchedule> ReadBudgetScheduleAsync(Guid id);
		Task<IReadOnlyList<BudgetSchedule>> ReadBudgetSchedulesAsync();
		Task UpdateBudgetScheduleAsync(BudgetSchedule budgetSchedule);
		Task DeleteBudgetScheduleAsync(Guid id);
		
		Task CreatePayeeAsync(Payee payee);
		Task<Payee> ReadPayeeAsync(Guid id);
		Task<IReadOnlyList<Payee>> ReadPayeesAsync();
		Task UpdatePayeeAsync(Payee payee);
		Task DeletePayeeAsync(Guid id);
		Task<int> GetPayeesCountAsync();
		
		Task CreateTransactionAsync(Transaction transaction);
		Task<Transaction> ReadTransactionAsync(Guid id);
		Task<IReadOnlyList<Transaction>> ReadAccountTransactionsAsync(Guid accountId);
		Task<IReadOnlyList<Transaction>> ReadPayeeTransactionsAsync(Guid payeeId);
		Task<IReadOnlyList<Transaction>> ReadEnvelopeTransactionsAsync(Guid envelopeId);
		Task<IReadOnlyList<Transaction>> ReadSplitTransactionsAsync(Guid splitId);
		Task<IReadOnlyList<Transaction>> ReadTransactionsAsync();
		Task UpdateTransactionAsync(Transaction transaction);
		Task DeleteTransactionAsync(Guid id);
		Task<int> GetTransactionsCountAsync();
	}
}

