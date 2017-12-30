using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetBadger.Core.DataAccess;
using BudgetBadger.Core.Logic;
using BudgetBadger.Models;
using BudgetBadger.Core.Extensions;

namespace BudgetBadger.Logic
{
    public class EnvelopeLogic : IEnvelopeLogic
    {
        readonly IEnvelopeDataAccess EnvelopeDataAccess;

        public EnvelopeLogic(IEnvelopeDataAccess envelopeDataAccess)
        {
            EnvelopeDataAccess = envelopeDataAccess;
        }

        public Task<Result> DeleteBudgetAsync(Budget budget)
        {
            throw new NotImplementedException();
        }

        public Task<Result<Budget>> GetBudgetAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<Result<IEnumerable<Budget>>> GetBudgetsAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<Result<IEnumerable<Budget>>> GetBudgetsAsync(DateTime dateTime)
        {
            var envelopes = await EnvelopeDataAccess.ReadEnvelopesAsync();
            var schedules = await EnvelopeDataAccess.ReadBudgetSchedulesAsync();

            var selectedSchedule = schedules.FirstOrDefault(s => s.BeginDate <= dateTime && s.EndDate >= dateTime);
            if (selectedSchedule == null)
            {
                selectedSchedule = new BudgetSchedule();
                selectedSchedule.BeginDate = new DateTime(dateTime.Year, dateTime.Month, 1);
                selectedSchedule.EndDate = selectedSchedule.BeginDate.AddMonths(1).AddTicks(-1);
                selectedSchedule.Id = selectedSchedule.BeginDate.ToGuid();
            }

            var budgets = await EnvelopeDataAccess.ReadBudgetsFromScheduleAsync(selectedSchedule.Id);
            var newBudgets = budgets.ToList();

            foreach(var envelope in envelopes.Where(e => !budgets.Any(b => b.Envelope.Id == e.Id)))
            {
                newBudgets.Add(new Budget
                {
                    Schedule = selectedSchedule.DeepCopy(),
                    Envelope = envelope.DeepCopy(),
                    Amount = 0m
                });
            }

            return new Result<IEnumerable<Budget>> { Success = true, Data = newBudgets };
        }

        public Task<Result<IEnumerable<Envelope>>> GetEnvelopesAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<Result<IEnumerable<EnvelopeGroup>>> GetEnvelopeGroupsAsync()
        {
            var envelopeGroups = await EnvelopeDataAccess.ReadEnvelopeGroupsAsync();

            return new Result<IEnumerable<EnvelopeGroup>> { Success = true, Data = envelopeGroups };
        }

        public IEnumerable<Budget> SearchBudgets(IEnumerable<Budget> budgets, string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
            {
                return budgets;
            }

            return budgets.Where(a => a.Envelope.Description.Contains(searchText));
        }

        public IEnumerable<GroupedList<Budget>> GroupBudgets(IEnumerable<Budget> budgets, bool includeDeleted = false)
        {
            var groupedBudgets = new List<GroupedList<Budget>>();
            var temp = budgets.GroupBy(a => a.Envelope?.Group?.Description);
            foreach (var tempGroup in temp)
            {
                var groupedList = new GroupedList<Budget>(tempGroup.Key, tempGroup.Key[0].ToString());
                groupedList.AddRange(tempGroup);
                groupedBudgets.Add(groupedList);
            }

            return groupedBudgets;
        }

        public async Task<Result<Budget>> UpsertBudgetAsync(Budget budget)
        {
            var result = new Result<Budget>();
            var newBudget = budget.DeepCopy();

            var envelopeResult = await UpsertEnvelope(newBudget.Envelope);
            if (envelopeResult.Success)
            {
                newBudget.Envelope = envelopeResult.Data;
            }

            var scheduleResult = await UpsertBudgetSchedule(newBudget.Schedule);
            if (scheduleResult.Success)
            {
                newBudget.Schedule = scheduleResult.Data;
            }

            if (newBudget.CreatedDateTime == null)
            {
                newBudget.CreatedDateTime = DateTime.Now;
                newBudget.ModifiedDateTime = DateTime.Now;

                await EnvelopeDataAccess.CreateBudgetAsync(newBudget);
                result.Success = true;
                result.Data = newBudget;
            }
            else
            {
                newBudget.ModifiedDateTime = DateTime.Now;

                await EnvelopeDataAccess.UpdateBudgetAsync(newBudget);
                result.Success = true;
                result.Data = newBudget;
            }

            return result;
        }

        private async Task<Result<EnvelopeGroup>> UpsertEnvelopeGroup(EnvelopeGroup group)
        {
            var result = new Result<EnvelopeGroup>();
            var newGroup = group.DeepCopy();

            if (newGroup.CreatedDateTime == null)
            {
                newGroup.CreatedDateTime = DateTime.Now;
                newGroup.ModifiedDateTime = DateTime.Now;

                await EnvelopeDataAccess.CreateEnvelopeGroupAsync(newGroup);
                result.Success = true;
                result.Data = newGroup;
            }
            else
            {
                newGroup.ModifiedDateTime = DateTime.Now;

                await EnvelopeDataAccess.UpdateEnvelopeGroupAsync(newGroup);
                result.Success = true;
                result.Data = newGroup;
            }

            return result;
        }

        private async Task<Result<Envelope>> UpsertEnvelope(Envelope envelope)
        {
            var result = new Result<Envelope>();
            var newEnvelope = envelope.DeepCopy();

            var groupResult = await UpsertEnvelopeGroup(newEnvelope.Group);
            if (groupResult.Success)
            {
                newEnvelope.Group = groupResult.Data;
            }


            if (newEnvelope.CreatedDateTime == null)
            {
                newEnvelope.CreatedDateTime = DateTime.Now;
                newEnvelope.ModifiedDateTime = DateTime.Now;

                await EnvelopeDataAccess.CreateEnvelopeAsync(newEnvelope);
                result.Success = true;
                result.Data = newEnvelope;
            }
            else
            {
                newEnvelope.ModifiedDateTime = DateTime.Now;

                await EnvelopeDataAccess.UpdateEnvelopeAsync(newEnvelope);
                result.Success = true;
                result.Data = newEnvelope;
            }

            return result;
        }

        private async Task<Result<BudgetSchedule>> UpsertBudgetSchedule(BudgetSchedule budgetSchedule)
        {
            var result = new Result<BudgetSchedule>();
            var newSchedule = budgetSchedule.DeepCopy();

            if (newSchedule.CreatedDateTime == null)
            {
                newSchedule.Id = newSchedule.BeginDate.ToGuid();
                newSchedule.CreatedDateTime = DateTime.Now;
                newSchedule.ModifiedDateTime = DateTime.Now;

                await EnvelopeDataAccess.CreateBudgetScheduleAsync(newSchedule);
                result.Success = true;
                result.Data = newSchedule;
            }
            else
            {
                newSchedule.ModifiedDateTime = DateTime.Now;

                await EnvelopeDataAccess.UpdateBudgetScheduleAsync(newSchedule);
                result.Success = true;
                result.Data = newSchedule;
            }

            return result;
        }
    }
}
