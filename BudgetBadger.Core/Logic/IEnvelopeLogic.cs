using System;
using System.Collections.Generic;
using System.Linq;
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
        Task<Result<IEnumerable<Budget>>> GetBudgetsAsync(BudgetSchedule schedule, bool isSelection = false);
        Task<Result<Budget>> GetBudgetAsync(Guid id);
        Task<Result<Budget>> SaveBudgetAsync(Budget budget);

        Task<Result<IEnumerable<Envelope>>> GetEnvelopesAsync();
        Task<Result> DeleteEnvelopeAsync(Envelope envelope);

        Task<Result<IEnumerable<EnvelopeGroup>>> GetEnvelopeGroupsAsync();
        Task<Result<EnvelopeGroup>> SaveEnvelopeGroupAsync(EnvelopeGroup envelopeGroup);
        Task<Result> DeleteEnvelopeGroupAsync(EnvelopeGroup envelopeGroup);

        IEnumerable<Budget> SearchBudgets(IEnumerable<Budget> budgets, string searchText);
        ILookup<string, Budget> GroupBudgets(IEnumerable<Budget> budgets);

        IEnumerable<EnvelopeGroup> SearchEnvelopeGroups(IEnumerable<EnvelopeGroup> envelopeGroups, string searchText);
    }
}
