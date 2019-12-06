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
    }
}
