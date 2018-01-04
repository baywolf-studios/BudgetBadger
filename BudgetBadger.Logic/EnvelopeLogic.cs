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
        readonly ITransactionDataAccess TransactionDataAccess;

        public EnvelopeLogic(IEnvelopeDataAccess envelopeDataAccess, ITransactionDataAccess transactionDataAccess)
        {
            EnvelopeDataAccess = envelopeDataAccess;
            TransactionDataAccess = transactionDataAccess;
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

        public async Task<Result<IEnumerable<Budget>>> GetBudgetsAsync(BudgetSchedule schedule)
        {
            var result = new Result<IEnumerable<Budget>>();

            var envelopes = await EnvelopeDataAccess.ReadEnvelopesAsync();

            var budgets = await EnvelopeDataAccess.ReadBudgetsFromScheduleAsync(schedule.Id);
            var newBudgets = budgets.ToList();

            foreach(var envelope in envelopes.Where(e => !budgets.Any(b => b.Envelope.Id == e.Id)))
            {
                newBudgets.Add(new Budget
                {
                    Schedule = schedule.DeepCopy(),
                    Envelope = envelope.DeepCopy(),
                    Amount = 0m
                });
            }

            var tasks = newBudgets.Select(b => FillBudget(b));

            result.Success = true;
            result.Data = await Task.WhenAll(tasks);

            return result;
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

            return budgets.Where(a => a.Envelope.Description.ToLower().Contains(searchText.ToLower()));
        }

        public IEnumerable<EnvelopeGroup> SearchEnvelopeGroups(IEnumerable<EnvelopeGroup> envelopeGroup, string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
            {
                return envelopeGroup;
            }

            return envelopeGroup.Where(a => a.Description.ToLower().Contains(searchText.ToLower()));
        }

        public IEnumerable<GroupedList<Budget>> GroupBudgets(IEnumerable<Budget> budgets, bool includeIncome = false)
        {
            // always hide the transfer envelope
            var newBudgets = budgets.Where(b => b.Id != Constants.TransferEnvelope.Id);

            var groupedBudgets = new List<GroupedList<Budget>>();
            var temp = budgets.GroupBy(a => a.Envelope?.Group?.Description);
            foreach (var tempGroup in temp)
            {
                var groupedList = new GroupedList<Budget>(tempGroup.Key, tempGroup.Key[0].ToString());
                groupedList.AddRange(tempGroup);
                groupedBudgets.Add(groupedList);
            }

            if (!includeIncome)
            {
                groupedBudgets.RemoveAll(g => g.FirstOrDefault()?.Envelope?.Group?.Id == Constants.SystemEnvelopeGroup.Id);
            }

            return groupedBudgets;
        }

        public async Task<Result<Budget>> UpsertBudgetAsync(Budget budget)
        {
            var result = new Result<Budget>();
            var newBudget = budget.DeepCopy();

            var envelopeResult = await UpsertEnvelopeAsync(newBudget.Envelope);
            if (envelopeResult.Success)
            {
                newBudget.Envelope = envelopeResult.Data;
            }

            var scheduleResult = await UpsertBudgetScheduleAsync(newBudget.Schedule);
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

        public async Task<Result<EnvelopeGroup>> UpsertEnvelopeGroupAsync(EnvelopeGroup group)
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

        private async Task<Result<Envelope>> UpsertEnvelopeAsync(Envelope envelope)
        {
            var result = new Result<Envelope>();
            var newEnvelope = envelope.DeepCopy();

            var groupResult = await UpsertEnvelopeGroupAsync(newEnvelope.Group);
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

        private async Task<Result<BudgetSchedule>> UpsertBudgetScheduleAsync(BudgetSchedule budgetSchedule)
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

        private async Task<Budget> FillBudget(Budget budget)
        {
            var newBudget = budget.DeepCopy();
            var transactions = await TransactionDataAccess.ReadEnvelopeTransactionsAsync(newBudget.Envelope.Id);
            var budgets = await EnvelopeDataAccess.ReadBudgetsFromEnvelopeAsync(newBudget.Envelope.Id);
            BudgetSchedule schedule = newBudget.Schedule;

            // will get the previous schedules amounts and activities for the buffer envelope since it lags
            if (newBudget.Envelope.IsBuffer())
            {
                var scheduleResult = await GetPreviousBudgetScheduleAsync(newBudget.Schedule);
                if (scheduleResult.Success)
                {
                    schedule = scheduleResult.Data;
                }
            }

            newBudget.PastAmount = budgets
                .Where(b => b.Schedule.EndDate < schedule.BeginDate)
                    .Sum(b2 => b2.Amount);

            newBudget.PastActivity = transactions
                .Where(t => t.ServiceDate < schedule.BeginDate)
                .Sum(t2 => t2.Amount);

            newBudget.Activity = transactions
                .Where(t => t.ServiceDate >= schedule.BeginDate && t.ServiceDate <= schedule.EndDate)
                .Sum(t2 => t2.Amount);

            return newBudget;
        }

        public async Task<Result<BudgetScheduleOverview>> GetEnvelopesOverview(BudgetSchedule budgetSchedule)
        {
            var result = new Result<BudgetScheduleOverview>();
            var budgetResult = await GetBudgetsAsync(budgetSchedule);

            if (budgetResult.Success)
            {
                var budgets = budgetResult.Data;

                var pastBudgeted = budgets
                    .Where(b => !b.Envelope.IsIncome() && !b.Envelope.IsBuffer())
                    .Sum(b => b.PastAmount);

                var pastIncome = budgets
                    .Where(b => b.Envelope.IsIncome() || b.Envelope.IsBuffer())
                    .Sum(b => b.PastActivity);

                var past = pastIncome - pastBudgeted;

                var budgeted = budgets
                    .Where(b => !b.Envelope.IsIncome() && !b.Envelope.IsBuffer())
                    .Sum(b => b.Amount);

                var income = budgets.Where(b => b.Envelope.IsIncome() || b.Envelope.IsBuffer()).Sum(b => b.Activity);

                result.Success = true;
                result.Data = new BudgetScheduleOverview(past, income, budgeted);
            }
            else
            {
                result.Success = false;
                result.Message = budgetResult.Message;
            }

            return result;
        }

        public async Task<Result<BudgetSchedule>> GetCurrentBudgetScheduleAsync(DateTime dateTime)
        {
            var result = new Result<BudgetSchedule>();
            var schedules = await EnvelopeDataAccess.ReadBudgetSchedulesAsync();

            var selectedSchedule = schedules.FirstOrDefault(s => s.BeginDate <= dateTime && s.EndDate >= dateTime);
            if (selectedSchedule == null)
            {
                selectedSchedule = new BudgetSchedule();
                selectedSchedule.BeginDate = new DateTime(dateTime.Year, dateTime.Month, 1);
                selectedSchedule.EndDate = selectedSchedule.BeginDate.AddMonths(1).AddTicks(-1);
                selectedSchedule.Id = selectedSchedule.BeginDate.ToGuid();
            }

            result.Success = true;
            result.Data = selectedSchedule;

            return result;
        }

        public async Task<Result<BudgetSchedule>> GetNextBudgetScheduleAsync(BudgetSchedule currentSchedule)
        {
            var dateTime = currentSchedule.EndDate.AddDays(1);

            var schedule = await GetCurrentBudgetScheduleAsync(dateTime);

            return schedule;
        }

        public async Task<Result<BudgetSchedule>> GetPreviousBudgetScheduleAsync(BudgetSchedule currentSchedule)
        {
            var dateTime = currentSchedule.BeginDate.AddDays(-1);

            var schedule = await GetCurrentBudgetScheduleAsync(dateTime);

            return schedule;
        }
    }
}
