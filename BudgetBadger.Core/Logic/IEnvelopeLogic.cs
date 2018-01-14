using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BudgetBadger.Models;

namespace BudgetBadger.Core.Logic
{
    public interface IEnvelopeLogic
    {
        Task<Result<BudgetSchedule>> GetCurrentBudgetScheduleAsync();
        Task<Result<BudgetSchedule>> GetPreviousBudgetSchedule(BudgetSchedule currentSchedule);
        Task<Result<BudgetSchedule>> GetNextBudgetSchedule(BudgetSchedule currentSchedule);

        Task<Result<IEnumerable<Budget>>> GetBudgetsAsync();
        Task<Result<IEnumerable<Budget>>> GetBudgetsAsync(BudgetSchedule schedule);
        Task<Result<Budget>> GetBudgetAsync(Guid id);
        Task<Result<Budget>> UpsertBudgetAsync(Budget budget);
        Task<Result> DeleteBudgetAsync(Budget budget);

        Task<Result<IEnumerable<Envelope>>> GetEnvelopesAsync();

        Task<Result<IEnumerable<EnvelopeGroup>>> GetEnvelopeGroupsAsync();
        Task<Result<EnvelopeGroup>> UpsertEnvelopeGroupAsync(EnvelopeGroup envelopeGroup);

        IEnumerable<Budget> SearchBudgets(IEnumerable<Budget> budgets, string searchText);
        IEnumerable<GroupedList<Budget>> GroupBudgets(IEnumerable<Budget> budgets, bool includeIncome = false);

        IEnumerable<EnvelopeGroup> SearchEnvelopeGroups(IEnumerable<EnvelopeGroup> envelopeGroups, string searchText);
    }
}
