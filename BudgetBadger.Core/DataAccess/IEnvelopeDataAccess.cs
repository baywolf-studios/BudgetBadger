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
        Task<IEnumerable<Envelope>> ReadEnvelopesAsync();
        Task UpdateEnvelopeAsync(Envelope envelope);
        Task DeleteEnvelopeAsync(Guid id);

        Task CreateBudgetAsync(Budget budget);
        Task<Budget> ReadBudgetAsync(Guid id);
        Task<IEnumerable<Budget>> ReadBudgetsAsync();
        Task<IEnumerable<Budget>> ReadBudgetsFromScheduleAsync(Guid scheduleId);
        Task UpdateBudgetAsync(Budget budget);
        Task DeleteBudgetAsync(Guid id);

        Task CreateEnvelopeGroupAsync(EnvelopeGroup envelopeGroup);
        Task<EnvelopeGroup> ReadEnvelopeGroupAsync(Guid id);
        Task<IEnumerable<EnvelopeGroup>> ReadEnvelopeGroupsAsync();
        Task UpdateEnvelopeGroupAsync(EnvelopeGroup envelopeGroup);
        Task DeleteEnvelopeGroupAsync(Guid id);

        Task CreateBudgetScheduleAsync(BudgetSchedule budgetSchedule);
        Task<BudgetSchedule> ReadBudgetScheduleAsync(Guid id);
        Task<IEnumerable<BudgetSchedule>> ReadBudgetSchedulesAsync();
        Task UpdateBudgetScheduleAsync(BudgetSchedule budgetSchedule);
        Task DeleteBudgetScheduleAsync(Guid id);
    }
}
