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

        Task<Result<IReadOnlyList<Budget>>> GetBudgetsAsync(BudgetSchedule schedule);
        Task<Result<IReadOnlyList<Budget>>> GetBudgetsForSelectionAsync(BudgetSchedule schedule);
        Task<Result<Budget>> GetBudgetAsync(Guid id);
        Task<Result> ValidateBudgetAsync(Budget budget);
        Task<Result<Budget>> SaveBudgetAsync(Budget budget);

        Task<Result<IReadOnlyList<Envelope>>> GetEnvelopesForSelectionAsync();
        Task<Result<IReadOnlyList<Envelope>>> GetEnvelopesForReportAsync();
        Task<Result<IReadOnlyList<Envelope>>> GetDeletedEnvelopesAsync();
        Task<Result> DeleteEnvelopeAsync(Guid id);
        Task<Result> UndoDeleteEnvelopeAsync(Guid id);

        Task<Result<IReadOnlyList<EnvelopeGroup>>> GetEnvelopeGroupsAsync();
        Task<Result<IReadOnlyList<EnvelopeGroup>>> GetDeletedEnvelopeGroupsAsync();
        Task<Result> ValidateEnvelopeGroupAsync(EnvelopeGroup envelopeGroup);
        Task<Result<EnvelopeGroup>> SaveEnvelopeGroupAsync(EnvelopeGroup envelopeGroup);
        Task<Result> DeleteEnvelopeGroupAsync(Guid id);
        Task<Result> UndoDeleteEnvelopeGroupAsync(Guid id);

        IReadOnlyList<Envelope> SearchEnvelopes(IEnumerable<Envelope> envelopes, string searchText);
        IReadOnlyList<Budget> SearchBudgets(IEnumerable<Budget> budgets, string searchText);
        IReadOnlyList<IGrouping<string, Budget>> GroupBudgets(IEnumerable<Budget> budgets);

        IReadOnlyList<EnvelopeGroup> SearchEnvelopeGroups(IEnumerable<EnvelopeGroup> envelopeGroups, string searchText);

        Task<Result<IReadOnlyList<QuickBudget>>> GetQuickBudgetsAsync(Budget budget);
    }
}
