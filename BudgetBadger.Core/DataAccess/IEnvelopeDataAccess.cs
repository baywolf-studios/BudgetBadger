using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BudgetBadger.Models;

namespace BudgetBadger.Core.DataAccess
{
    public interface IEnvelopeDataAccess
    {
        Task CreateEnvelopeAsync(Envelope envelope);
        Task<Envelope> ReadEnvelopeAsync(Guid id);
        Envelope ReadGenericDebtEnvelope();
        Task<IReadOnlyList<Envelope>> ReadEnvelopesAsync();
        Task UpdateEnvelopeAsync(Envelope envelope);
        Task SoftDeleteEnvelopeAsync(Guid id);
        Task HideEnvelopeAsync(Guid id);
        Task UnhideEnvelopeAsync(Guid id);
        Task PurgeEnvelopesAsync(DateTime deletedBefore);
        Task<int> GetEnvelopesCountAsync();

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
        Task SoftDeleteEnvelopeGroupAsync(Guid id);
        Task HideEnvelopeGroupAsync(Guid id);
        Task UnhideEnvelopeGroupAsync(Guid id);
        Task PurgeEnvelopeGroupsAsync(DateTime deletedBefore);
        Task<int>GetEnvelopeGroupsCountAsync();

        Task CreateBudgetScheduleAsync(BudgetSchedule budgetSchedule);
        Task<BudgetSchedule> ReadBudgetScheduleAsync(Guid id);
        Task<IReadOnlyList<BudgetSchedule>> ReadBudgetSchedulesAsync();
        Task UpdateBudgetScheduleAsync(BudgetSchedule budgetSchedule);
        Task DeleteBudgetScheduleAsync(Guid id);
    }
}
