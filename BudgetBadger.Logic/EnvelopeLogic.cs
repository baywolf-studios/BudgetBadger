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
        readonly IAccountDataAccess AccountDataAccess;

        public EnvelopeLogic(IEnvelopeDataAccess envelopeDataAccess, ITransactionDataAccess transactionDataAccess, IAccountDataAccess accountDataAccess)
        {
            EnvelopeDataAccess = envelopeDataAccess;
            TransactionDataAccess = transactionDataAccess;
            AccountDataAccess = accountDataAccess;

            // move these to the data access
            envelopeDataAccess.CreateEnvelopeGroupAsync(Constants.IncomeEnvelopeGroup);
            envelopeDataAccess.CreateEnvelopeAsync(Constants.IncomeEnvelope);
            envelopeDataAccess.CreateEnvelopeAsync(Constants.BufferEnvelope);

            envelopeDataAccess.CreateEnvelopeGroupAsync(Constants.SystemEnvelopeGroup);
            envelopeDataAccess.CreateEnvelopeAsync(Constants.IgnoredEnvelope);

            envelopeDataAccess.CreateEnvelopeGroupAsync(Constants.DebtEnvelopeGroup);
        }

        public Task<Result> DeleteBudgetAsync(Budget budget)
        {
            throw new NotImplementedException();
        }

        public async Task<Result<Budget>> GetBudgetAsync(Guid id)
        {
            var result = new Result<Budget>();

            try
            {
                var budget = await EnvelopeDataAccess.ReadBudgetAsync(id);
                var populatedBudget = await GetPopulatedBudget(budget);
                result.Success = true;
                result.Data = populatedBudget;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        public Task<Result<IEnumerable<Budget>>> GetBudgetsAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<Result<IEnumerable<Budget>>> GetBudgetsAsync(BudgetSchedule schedule, bool isSelection = false)
        {
            var result = new Result<IEnumerable<Budget>>();

            var envelopes = await EnvelopeDataAccess.ReadEnvelopesAsync();
            var activeEnvelopes = envelopes.Where(e => !e.IsSystem());

            var budgets = await EnvelopeDataAccess.ReadBudgetsFromScheduleAsync(schedule.Id);
            var activeBudgets = budgets.Where(b => !b.Envelope.IsSystem()).ToList();

            foreach (var envelope in activeEnvelopes.Where(e => !budgets.Any(b => b.Envelope.Id == e.Id)))
            {
                activeBudgets.Add(new Budget
                {
                    Schedule = schedule.DeepCopy(),
                    Envelope = envelope.DeepCopy(),
                    Amount = 0m
                });
            }

            var populatedSchedule = await GetPopulatedBudgetSchedule(schedule);
            var tasks = activeBudgets.Select(b => GetPopulatedBudget(b, populatedSchedule));

            var budgetsToReturnTemp = await Task.WhenAll(tasks);
            var budgetsToReturn = budgetsToReturnTemp.ToList();

            if (isSelection)
            {
                var debtBudgets = budgetsToReturn.Where(b => b.Envelope.Group.IsDebt()).ToList();
                budgetsToReturn.RemoveAll(b => b.Envelope.Group.IsDebt());
                var genericDebtBudget = new Budget
                {
                    Envelope = Constants.GenericDebtEnvelope,
                    Amount = debtBudgets.Sum(b => b.Amount),
                    Activity = debtBudgets.Sum(b => b.Activity),
                    PastAmount = debtBudgets.Sum(b => b.PastAmount),
                    PastActivity = debtBudgets.Sum(b => b.PastActivity),
                    IgnoreOverspend = true
                };

                budgetsToReturn.Add(genericDebtBudget);
            }
            else
            {
                budgetsToReturn.RemoveAll(b => b.Envelope.Group.IsIncome());
                budgetsToReturn.RemoveAll(b => b.Envelope.Group.IsDebt() && b.Remaining == 0 && b.Amount == 0);
            }

            result.Success = true;
            result.Data = budgetsToReturn;

            return result;
        }

        public Task<Result<IEnumerable<Envelope>>> GetEnvelopesAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<Result<IEnumerable<EnvelopeGroup>>> GetEnvelopeGroupsAsync()
        {
            var result = new Result<IEnumerable<EnvelopeGroup>>();

            try
            {
                var envelopeGroups = await EnvelopeDataAccess.ReadEnvelopeGroupsAsync();
                var filteredEnvelopeGroups = envelopeGroups.Where(e => !e.IsSystem() && !e.IsIncome() && !e.IsDebt()); 
                result.Success = true;
                result.Data = filteredEnvelopeGroups;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
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

        public ILookup<string, Budget> GroupBudgets(IEnumerable<Budget> budgets)
        {
            var groupedBudgets = budgets.ToLookup(b => b.Envelope.Group.Description);

            return groupedBudgets;
        }

        public async Task<Result<Budget>> SaveBudgetAsync(Budget budget)
        {
            var result = new Result<Budget>();
            var budgetToUpsert = budget.DeepCopy();
            var dateTimeNow = DateTime.Now;

            var envelopeResult = await SaveEnvelopeAsync(budgetToUpsert.Envelope);
            if (envelopeResult.Success)
            {
                budgetToUpsert.Envelope = envelopeResult.Data;
            }

            var scheduleResult = await SaveBudgetScheduleAsync(budgetToUpsert.Schedule);
            if (scheduleResult.Success)
            {
                budgetToUpsert.Schedule = scheduleResult.Data;
            }

            if (budgetToUpsert.IsNew)
            {
                budgetToUpsert.Id = Guid.NewGuid();
                budgetToUpsert.CreatedDateTime = dateTimeNow;
                budgetToUpsert.ModifiedDateTime = dateTimeNow;

                await EnvelopeDataAccess.CreateBudgetAsync(budgetToUpsert);
                result.Success = true;
                result.Data = budgetToUpsert;
            }
            else
            {
                budgetToUpsert.ModifiedDateTime = dateTimeNow;

                await EnvelopeDataAccess.UpdateBudgetAsync(budgetToUpsert);
                result.Success = true;
                result.Data = budgetToUpsert;
            }

            return result;
        }

        public async Task<Result<EnvelopeGroup>> SaveEnvelopeGroupAsync(EnvelopeGroup group)
        {
            var result = new Result<EnvelopeGroup>();
            var groupToUpsert = group.DeepCopy();
            var dateTimeNow = DateTime.Now;

            if (groupToUpsert.IsNew)
            {
                groupToUpsert.Id = Guid.NewGuid();
                groupToUpsert.CreatedDateTime = dateTimeNow;
                groupToUpsert.ModifiedDateTime = dateTimeNow;

                await EnvelopeDataAccess.CreateEnvelopeGroupAsync(groupToUpsert);
                result.Success = true;
                result.Data = groupToUpsert;
            }
            else
            {
                groupToUpsert.ModifiedDateTime = dateTimeNow;

                await EnvelopeDataAccess.UpdateEnvelopeGroupAsync(groupToUpsert);
                result.Success = true;
                result.Data = groupToUpsert;
            }

            return result;
        }

        private async Task<Result<Envelope>> SaveEnvelopeAsync(Envelope envelope)
        {
            var result = new Result<Envelope>();
            var envelopeToUpsert = envelope.DeepCopy();
            var dateTimeNow = DateTime.Now;

            var groupResult = await SaveEnvelopeGroupAsync(envelopeToUpsert.Group);
            if (groupResult.Success)
            {
                envelopeToUpsert.Group = groupResult.Data;
            }


            if (envelopeToUpsert.IsNew)
            {
                envelopeToUpsert.Id = Guid.NewGuid();
                envelopeToUpsert.CreatedDateTime = dateTimeNow;
                envelopeToUpsert.ModifiedDateTime = dateTimeNow;

                await EnvelopeDataAccess.CreateEnvelopeAsync(envelopeToUpsert);
                result.Success = true;
                result.Data = envelopeToUpsert;
            }
            else
            {
                envelopeToUpsert.ModifiedDateTime = dateTimeNow;

                await EnvelopeDataAccess.UpdateEnvelopeAsync(envelopeToUpsert);
                result.Success = true;
                result.Data = envelopeToUpsert;
            }

            return result;
        }

        private async Task<Result<BudgetSchedule>> SaveBudgetScheduleAsync(BudgetSchedule budgetSchedule)
        {
            var result = new Result<BudgetSchedule>();
            var scheduleToUpsert = budgetSchedule.DeepCopy();

            if (scheduleToUpsert.IsNew)
            {
                scheduleToUpsert.Id = scheduleToUpsert.BeginDate.ToGuid();
                scheduleToUpsert.CreatedDateTime = DateTime.Now;
                scheduleToUpsert.ModifiedDateTime = DateTime.Now;

                await EnvelopeDataAccess.CreateBudgetScheduleAsync(scheduleToUpsert);
                result.Success = true;
                result.Data = scheduleToUpsert;
            }
            else
            {
                scheduleToUpsert.ModifiedDateTime = DateTime.Now;

                await EnvelopeDataAccess.UpdateBudgetScheduleAsync(scheduleToUpsert);
                result.Success = true;
                result.Data = scheduleToUpsert;
            }

            return result;
        }

        private async Task<Budget> GetPopulatedBudget(Budget budget, BudgetSchedule budgetSchedule = null)
        {
            var budgetToPopulate = budget.DeepCopy();

            if (budgetSchedule == null)
            {
                budgetToPopulate.Schedule = await GetPopulatedBudgetSchedule(budgetToPopulate.Schedule);
            }
            else
            {
                budgetToPopulate.Schedule = budgetSchedule.DeepCopy();
            }

            var transactions = await TransactionDataAccess.ReadEnvelopeTransactionsAsync(budgetToPopulate.Envelope.Id);
            var budgets = await EnvelopeDataAccess.ReadBudgetsFromEnvelopeAsync(budgetToPopulate.Envelope.Id);

            budgetToPopulate.PastAmount = budgets
                .Where(b => b.Schedule.EndDate < budgetToPopulate.Schedule.BeginDate)
                    .Sum(b2 => b2.Amount);

            budgetToPopulate.PastActivity = transactions
                .Where(t => t.ServiceDate < budgetToPopulate.Schedule.BeginDate)
                .Sum(t2 => t2.Amount);

            budgetToPopulate.Activity = transactions
                .Where(t => t.ServiceDate >= budgetToPopulate.Schedule.BeginDate && t.ServiceDate <= budgetToPopulate.Schedule.EndDate)
                .Sum(t2 => t2.Amount);

            return budgetToPopulate;
        }

        private async Task<BudgetSchedule> GetPopulatedBudgetSchedule(BudgetSchedule budgetSchedule)
        {
            var allAccounts = await AccountDataAccess.ReadAccountsAsync();
            var budgetAccounts = allAccounts.Where(a => a.OnBudget);

            var allTransactions = await TransactionDataAccess.ReadTransactionsAsync();
            var budgetTransactions = allTransactions.Where(t => budgetAccounts.Any(b => b.Id == t.Account.Id));

            var envelopes = await EnvelopeDataAccess.ReadEnvelopesAsync();

            // get all income
            var incomeTransactions = budgetTransactions.Where(t => t.Envelope.IsIncome());

            var pastIncome = incomeTransactions
                .Where(t => t.ServiceDate < budgetSchedule.BeginDate)
                .Sum(t => t.Amount);
            var currentIncome = incomeTransactions
                .Where(t => t.ServiceDate >= budgetSchedule.BeginDate && t.ServiceDate <= budgetSchedule.EndDate)
                .Sum(t => t.Amount);

            // get all buffers
            var bufferTransactions = budgetTransactions.Where(t => t.Envelope.IsBuffer());
            var previousScheduleDate = GetPreviousBudgetScheduleDate(budgetSchedule);
            var previousSchedule = GetBudgetScheduleFromDate(previousScheduleDate);

            var pastBufferIncome = bufferTransactions
                .Where(t => t.ServiceDate < previousSchedule.BeginDate)
                .Sum(t => t.Amount);
            var currentBufferIncome = bufferTransactions
                .Where(t => t.ServiceDate >= previousSchedule.BeginDate && t.ServiceDate <= previousSchedule.EndDate)
                .Sum(t => t.Amount);

            // get all budget amounts
            var budgets = await EnvelopeDataAccess.ReadBudgetsAsync();

            var currentBudgetAmount = budgets
                .Where(b => !b.Envelope.IsIncome()
                       && !b.Envelope.IsBuffer()
                       && !b.Envelope.IsSystem()
                       && b.Schedule.Id == budgetSchedule.Id)
                .Sum(b => b.Amount);
            var pastBudgetAmount = budgets
                .Where(b => !b.Envelope.IsIncome()
                       && !b.Envelope.IsBuffer()
                       && !b.Envelope.IsSystem()
                       && b.Schedule.EndDate < budgetSchedule.BeginDate)
                .Sum(b => b.Amount);

            // past is all past income + all past budget amounts
            var past = (pastIncome + pastBufferIncome) - pastBudgetAmount;

            // income is income for this schedule
            var income = currentIncome + currentBufferIncome;

            // budgeted is amounts for this schedule
            var budgeted = currentBudgetAmount;

            // overspend is current and past budget amounts + current and past transactions (if negative)
            decimal overspend = 0;
            foreach (var envelope in envelopes.Where(e => !e.IsIncome() && !e.IsBuffer() && !e.IsSystem()))
            {
                var envelopeTransactionsAmount = budgetTransactions
                .Where(t => t.Envelope.Id == envelope.Id
                       && t.ServiceDate <= budgetSchedule.EndDate)
                .Sum(t => t.Amount);

                var envelopeBudgetAmount = budgets
                    .Where(b => b.Envelope.Id == envelope.Id
                           && b.Schedule.EndDate <= budgetSchedule.EndDate)
                    .Sum(b => b.Amount);

                var envelopeOverspend = envelopeBudgetAmount + envelopeTransactionsAmount;

                var latestBudget = budgets.FirstOrDefault(b => b.Envelope.Id == envelope.Id && b.Schedule.Id == budgetSchedule.Id);
                var latestBudgetIgnoreOverspend = latestBudget?.IgnoreOverspend ?? false;

                var ignore = latestBudgetIgnoreOverspend || envelope.IgnoreOverspend;

                if (envelopeOverspend < 0 && !ignore)
                {
                    overspend += Math.Abs(envelopeOverspend);
                }
            }

            var newBudgetSchedule = budgetSchedule.DeepCopy();
            // get existing schedule from data access if exists
            var schedule = await EnvelopeDataAccess.ReadBudgetScheduleAsync(budgetSchedule.Id);
            if (schedule.Exists)
            {
                newBudgetSchedule = schedule.DeepCopy();
            }
            newBudgetSchedule.Past = past;
            newBudgetSchedule.Income = income;
            newBudgetSchedule.Budgeted = budgeted;
            newBudgetSchedule.Overspend = overspend;
            // could change this
            newBudgetSchedule.Description = newBudgetSchedule.BeginDate.ToString("Y");

            return newBudgetSchedule;
        }

        private BudgetSchedule GetBudgetScheduleFromDate(DateTime date)
        {
            var selectedSchedule = new BudgetSchedule();
            selectedSchedule.BeginDate = new DateTime(date.Year, date.Month, 1);
            selectedSchedule.EndDate = selectedSchedule.BeginDate.AddMonths(1).AddTicks(-1);
            selectedSchedule.Id = selectedSchedule.BeginDate.ToGuid();

            return selectedSchedule;
        }

        public async Task<Result<BudgetSchedule>> GetCurrentBudgetScheduleAsync()
        {
            var result = new Result<BudgetSchedule>();

            try
            {
                var schedule = GetBudgetScheduleFromDate(DateTime.Now);

                result.Success = true;
                result.Data = await GetPopulatedBudgetSchedule(schedule);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
                //log
            }

            return result;
        }

        private DateTime GetNextBudgetScheduleDate(BudgetSchedule currentSchedule)
        {
            return currentSchedule.EndDate.AddDays(1);
        }

        public async Task<Result<BudgetSchedule>> GetNextBudgetSchedule(BudgetSchedule currentSchedule)
        {
            var result = new Result<BudgetSchedule>();
            
            var nextDate = GetNextBudgetScheduleDate(currentSchedule);
            var schedule = GetBudgetScheduleFromDate(nextDate);

            try
            {
                var populatedSchedule = await GetPopulatedBudgetSchedule(schedule);
                result.Success = true;
                result.Data = populatedSchedule;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        private DateTime GetPreviousBudgetScheduleDate(BudgetSchedule currentSchedule)
        {
            return currentSchedule.BeginDate.AddDays(-1);
        }

        public async Task<Result<BudgetSchedule>> GetPreviousBudgetSchedule(BudgetSchedule currentSchedule)
        {
            var result = new Result<BudgetSchedule>();

            var previousDate = GetPreviousBudgetScheduleDate(currentSchedule);
            var schedule = GetBudgetScheduleFromDate(previousDate);

            try
            {
                var populatedSchedule = await GetPopulatedBudgetSchedule(schedule);
                result.Success = true;
                result.Data = populatedSchedule;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

    }
}
