using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetBadger.Core.Models;
using BudgetBadger.Logic.Models;

namespace BudgetBadger.Core.Logic
{
    public interface IEnvelopeLogic
    {
        //Task<Result2<BudgetPeriod>> SearchBudgetPeriods(DateTime date);
        //Task<Result2<BudgetPeriodId>> CreateBudgetPeriod(DateTime date);
        //Task<Result2<BudgetPeriod>> ReadBudgetPeriod(BudgetPeriodId id);

        Task<ItemsResponse<EnvelopeGroup>> SearchEnvelopeGroupsAsync(bool? hidden = null,
            bool? isSystem = null,
            bool? isIncome = null,
            bool? isDebt = null);
        Task<DataResponse<EnvelopeGroupId>> CreateEnvelopeGroupAsync(string description, string notes, bool hidden);
        Task<DataResponse<EnvelopeGroup>> ReadEnvelopeGroupAsync(EnvelopeGroupId envelopeGroupId);
        Task<Response> UpdateEnvelopeGroupAsync(EnvelopeGroupId envelopeGroupId, string description, string notes, bool hidden);
        Task<Response> DeleteEnvelopeGroupAsync(EnvelopeGroupId envelopeGroupId);

        //SearchEnvelopes(EnvelopeGroupId envelopeGroupId, searchfilter?)
        //Task<ItemsResponse<Envelope>> SearchEnvelopes(bool? editable, bool? selectable, bool? hidden);
        //CreateEnvelope(description, notes, groupid, IgnoreOverspend)
        //ReadEnvelope(id)
        //UpdateEnvelope(description, notes, groupid, id)
        //DeleteEnvelope(id)

        //SearchBudgets(envelopeid periodid searchfilter?)
        //Task<ItemsResponse<Budget>> SearchBudgetsAsync(
        //    IEnumerable<Guid> envelopeIds,
        //    IEnumerable<Guid> budgetPeriodIds,
        //    bool? editable,
        //    bool? selectable,
        //    bool? hidden);
        //CreateBudget(envelopeid, periodid, IgnoreOverspend, amount)
        //ReadBudget(id)
        //UpdateBudget(id, amount, IgnoreOverspend)

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
        Task<Result<IReadOnlyList<Budget>>> BudgetTransferAsync(BudgetSchedule schedule, Guid fromBudgetId, Guid toBudgetId, decimal amount);

        Task<Result<Envelope>> GetEnvelopeAsync(Guid id);
        Task<Result<IReadOnlyList<Envelope>>> GetEnvelopesForSelectionAsync();
        Task<Result<IReadOnlyList<Envelope>>> GetEnvelopesForReportAsync();
        Task<Result<IReadOnlyList<Envelope>>> GetHiddenEnvelopesAsync();
        Task<Result<Envelope>> SoftDeleteEnvelopeAsync(Guid id);
        Task<Result<Envelope>> HideEnvelopeAsync(Guid id);
        Task<Result<Envelope>> UnhideEnvelopeAsync(Guid id);

        bool FilterEnvelope(Envelope envelope, string searchText);
        bool FilterEnvelope(Envelope envelope, FilterType filterType);
        bool FilterBudget(Budget budget, string searchText);
        bool FilterBudget(Budget budget, FilterType filterType);
    }
}
