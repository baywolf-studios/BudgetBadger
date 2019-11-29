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
        Task<Result<BudgetSchedule>> GetBudgetSchedule(BudgetSchedule budgetSchedule);

        Task<Result<Budget>> SaveBudgetAsync(Budget budget);
        Task<Result<IReadOnlyList<Budget>>> GetBudgetsAsync(BudgetSchedule schedule);
        Task<Result<IReadOnlyList<Budget>>> GetBudgetsForSelectionAsync(BudgetSchedule schedule);
        Task<Result<IReadOnlyList<Budget>>> GetHiddenBudgetsAsync(BudgetSchedule schedule);
        Task<Result<Budget>> GetBudgetAsync(Guid envelopeId, BudgetSchedule schedule);
        Task<Result<Budget>> GetBudgetAsync(Guid id);
        Task<Result<IReadOnlyList<QuickBudget>>> GetQuickBudgetsAsync(Budget budget);
        Task<Result> BudgetTransferAsync(BudgetSchedule schedule, Guid fromBudgetId, Guid toBudgetId, decimal amount);

        Task<Result<int>> GetEnvelopesCountAsync();
        Task<Result<IReadOnlyList<Envelope>>> GetEnvelopesForSelectionAsync();
        Task<Result<IReadOnlyList<Envelope>>> GetEnvelopesForReportAsync();
        Task<Result<IReadOnlyList<Envelope>>> GetHiddenEnvelopesAsync();
        Task<Result> SoftDeleteEnvelopeAsync(Guid id);
        Task<Result> HideEnvelopeAsync(Guid id);
        Task<Result> UnhideEnvelopeAsync(Guid id);

        Task<Result<EnvelopeGroup>> SaveEnvelopeGroupAsync(EnvelopeGroup envelopeGroup);
        Task<Result<int>> GetEnvelopeGroupsCountAsync();
        Task<Result<IReadOnlyList<EnvelopeGroup>>> GetEnvelopeGroupsAsync();
        Task<Result<IReadOnlyList<EnvelopeGroup>>> GetEnvelopeGroupsForSelectionAsync();
        Task<Result<IReadOnlyList<EnvelopeGroup>>> GetHiddenEnvelopeGroupsAsync();
        Task<Result> SoftDeleteEnvelopeGroupAsync(Guid id);
        Task<Result> HideEnvelopeGroupAsync(Guid id);
        Task<Result> UnhideEnvelopeGroupAsync(Guid id);

        bool FilterEnvelopeGroup(EnvelopeGroup envelopeGroup, string searchText);
        bool FilterEnvelope(Envelope envelope, string searchText);
        bool FilterBudget(Budget budget, string searchText);

        
        // removing these
        Task<Result> DeleteEnvelopeGroupAsync(Guid id);
        Task<Result> UndoDeleteEnvelopeGroupAsync(Guid id);
        Task<Result> DeleteEnvelopeAsync(Guid id);
        Task<Result> UndoDeleteEnvelopeAsync(Guid id);
        Task<Result<IReadOnlyList<EnvelopeGroup>>> GetDeletedEnvelopeGroupsAsync();
        Task<Result<IReadOnlyList<Envelope>>> GetDeletedEnvelopesAsync();
    }
}
