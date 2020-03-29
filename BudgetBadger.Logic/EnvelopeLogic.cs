using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using BudgetBadger.Core.DataAccess;
using BudgetBadger.Core.LocalizedResources;
using BudgetBadger.Core.Logic;
using BudgetBadger.Models;
using BudgetBadger.Models.Extensions;

namespace BudgetBadger.Logic
{
    public class EnvelopeLogic : IEnvelopeLogic
    {
        readonly IEnvelopeDataAccess _envelopeDataAccess;
        readonly ITransactionDataAccess _transactionDataAccess;
        readonly IAccountDataAccess _accountDataAccess;
        readonly IResourceContainer _resourceContainer;

        public EnvelopeLogic(IEnvelopeDataAccess envelopeDataAccess,
            ITransactionDataAccess transactionDataAccess,
            IAccountDataAccess accountDataAccess,
            IResourceContainer resourceContainer)
        {
            _envelopeDataAccess = envelopeDataAccess;
            _transactionDataAccess = transactionDataAccess;
            _accountDataAccess = accountDataAccess;
            _resourceContainer = resourceContainer;
        }

        public async Task<Result<BudgetSchedule>> GetCurrentBudgetScheduleAsync()
        {
            var result = new Result<BudgetSchedule>();

            try
            {
                var schedule = GetBudgetScheduleFromDate(DateTime.Now);

                result.Success = true;
                result.Data = await GetPopulatedBudgetSchedule(schedule).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
                //log
            }

            return result;
        }

        public async Task<Result<BudgetSchedule>> GetPreviousBudgetSchedule(BudgetSchedule currentSchedule)
        {
            var result = new Result<BudgetSchedule>();

            var previousDate = GetPreviousBudgetScheduleDate(currentSchedule);
            var schedule = GetBudgetScheduleFromDate(previousDate);

            try
            {
                var populatedSchedule = await GetPopulatedBudgetSchedule(schedule).ConfigureAwait(false);
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

        public async Task<Result<BudgetSchedule>> GetNextBudgetSchedule(BudgetSchedule currentSchedule)
        {
            var result = new Result<BudgetSchedule>();

            var nextDate = GetNextBudgetScheduleDate(currentSchedule);
            var schedule = GetBudgetScheduleFromDate(nextDate);

            try
            {
                var populatedSchedule = await GetPopulatedBudgetSchedule(schedule).ConfigureAwait(false);
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

        public async Task<Result<BudgetSchedule>> GetBudgetSchedule(BudgetSchedule budgetSchedule)
        {
            var result = new Result<BudgetSchedule>();

            try
            {
                var schedule = await GetPopulatedBudgetSchedule(budgetSchedule);
                result.Success = true;
                result.Data = schedule;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        public async Task<Result<Budget>> SaveBudgetAsync(Budget budget)
        {
            var result = new Result<Budget>();

            try
            {

                var validationResult = await ValidateBudgetAsync(budget).ConfigureAwait(false);
                if (!validationResult.Success)
                {
                    return validationResult.ToResult<Budget>();
                }


                var budgetToUpsert = budget.DeepCopy();
                var dateTimeNow = DateTime.Now;


                var envelopeResult = await SaveEnvelopeAsync(budgetToUpsert.Envelope).ConfigureAwait(false);
                if (envelopeResult.Success)
                {
                    budgetToUpsert.Envelope = envelopeResult.Data;
                }

                var scheduleResult = await SaveBudgetScheduleAsync(budgetToUpsert.Schedule).ConfigureAwait(false);
                if (scheduleResult.Success)
                {
                    budgetToUpsert.Schedule = scheduleResult.Data;
                }

                budgetToUpsert.Amount = _resourceContainer.GetRoundedDecimal(budgetToUpsert.Amount);

                if (budgetToUpsert.IsNew)
                {
                    budgetToUpsert.Id = Guid.NewGuid();
                    budgetToUpsert.CreatedDateTime = dateTimeNow;
                    budgetToUpsert.ModifiedDateTime = dateTimeNow;

                    await _envelopeDataAccess.CreateBudgetAsync(budgetToUpsert).ConfigureAwait(false);
                    result.Success = true;
                    result.Data = budgetToUpsert;
                }
                else
                {
                    budgetToUpsert.ModifiedDateTime = dateTimeNow;

                    await _envelopeDataAccess.UpdateBudgetAsync(budgetToUpsert).ConfigureAwait(false);
                    result.Success = true;
                    result.Data = budgetToUpsert;
                }
            }
            catch(Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        public async Task<Result<IReadOnlyList<Budget>>> GetBudgetsAsync(BudgetSchedule schedule)
        {
            var result = new Result<IReadOnlyList<Budget>>();

            try
            {
                var envelopes = await _envelopeDataAccess.ReadEnvelopesAsync().ConfigureAwait(false);
                var activeOrHiddenEnvelopes = envelopes.Where(e => (e.IsActive || (e.IsHidden && !e.IsDeleted)) && !e.IsSystem && !e.Group.IsIncome);

                var budgets = await _envelopeDataAccess.ReadBudgetsFromScheduleAsync(schedule.Id).ConfigureAwait(false);
                var activeOrHiddenBudgets = budgets
                    .Where(b => (b.IsActive || (b.Envelope.IsHidden && !b.Envelope.IsDeleted)) && !b.Envelope.IsSystem && !b.Envelope.Group.IsIncome)
                    .ToList();

                foreach (var envelope in activeOrHiddenEnvelopes.Where(e => !budgets.Any(b => b.Envelope.Id == e.Id)))
                {
                    activeOrHiddenBudgets.Add(new Budget
                    {
                        Schedule = schedule.DeepCopy(),
                        Envelope = envelope.DeepCopy(),
                        Amount = 0m
                    });
                }

                var populatedSchedule = await GetPopulatedBudgetSchedule(schedule).ConfigureAwait(false);
                var tasks = activeOrHiddenBudgets.Select(b => GetPopulatedBudget(b, populatedSchedule));

                var budgetsToPopulateTemp = await Task.WhenAll(tasks).ConfigureAwait(false);
                var budgetsToReturn = budgetsToPopulateTemp.Where(b => b.Envelope.IsActive).ToList();

                if (budgetsToPopulateTemp.Any(b => b.Envelope.IsHidden && !(b.Envelope.IsIncome || b.Envelope.IsBuffer || b.Envelope.Group.IsIncome || b.Envelope.IsGenericDebtEnvelope || b.Envelope.Group.IsDebt || b.Envelope.IsSystem || b.Envelope.IsGenericHiddenEnvelope)))
                {
                    var genericHiddenBudget = GetGenericHiddenBudget(populatedSchedule, budgetsToPopulateTemp.Where(b => b.Envelope.IsHidden));
                    budgetsToReturn.Add(genericHiddenBudget);
                }

                budgetsToReturn.RemoveAll(b => b.Envelope.Group.IsDebt && b.Remaining == 0 && b.Amount == 0);

                budgetsToReturn.Sort();

                result.Success = true;
                result.Data = budgetsToReturn;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        public async Task<Result<IReadOnlyList<Budget>>> GetBudgetsForSelectionAsync(BudgetSchedule schedule)
        {
            var result = new Result<IReadOnlyList<Budget>>();

            try
            {
                var envelopes = await _envelopeDataAccess.ReadEnvelopesAsync().ConfigureAwait(false);
                var activeEnvelopes = envelopes.Where(e => !e.IsSystem && e.IsActive);

                var budgets = await _envelopeDataAccess.ReadBudgetsFromScheduleAsync(schedule.Id).ConfigureAwait(false);
                var activeBudgets = budgets.Where(b => !b.Envelope.IsSystem && b.IsActive).ToList();

                foreach (var envelope in activeEnvelopes.Where(e => !budgets.Any(b => b.Envelope.Id == e.Id)))
                {
                    activeBudgets.Add(new Budget
                    {
                        Schedule = schedule.DeepCopy(),
                        Envelope = envelope.DeepCopy(),
                        Amount = 0m
                    });
                }

                var populatedSchedule = await GetPopulatedBudgetSchedule(schedule).ConfigureAwait(false);
                var tasks = activeBudgets.Select(b => GetPopulatedBudget(b, populatedSchedule));

                var budgetsToReturnTemp = await Task.WhenAll(tasks).ConfigureAwait(false);
                var budgetsToReturn = budgetsToReturnTemp.ToList();

                var debtBudgets = budgetsToReturn.Where(b => b.Envelope.Group.IsDebt).ToList();
                budgetsToReturn.RemoveAll(b => b.Envelope.Group.IsDebt);
                var genericDebtBudget = GetGenericDebtBudget(populatedSchedule, debtBudgets);
                budgetsToReturn.Add(genericDebtBudget);

                budgetsToReturn.Sort();

                result.Success = true;
                result.Data = budgetsToReturn;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        public async Task<Result<IReadOnlyList<Budget>>> GetHiddenBudgetsAsync(BudgetSchedule schedule)
        {
            var result = new Result<IReadOnlyList<Budget>>();

            try
            {
                var envelopes = await _envelopeDataAccess.ReadEnvelopesAsync().ConfigureAwait(false);
                var hiddenEnvelopes = envelopes.Where(e => !e.Group.IsDebt && !e.Group.IsSystem && !e.Group.IsIncome && e.IsHidden && !e.IsDeleted);

                var budgets = await _envelopeDataAccess.ReadBudgetsFromScheduleAsync(schedule.Id).ConfigureAwait(false);
                var hiddenBudgets = budgets.Where(b => !b.Envelope.Group.IsIncome && !b.Envelope.Group.IsDebt && !b.Envelope.IsSystem && b.Envelope.IsHidden && !b.Envelope.IsDeleted).ToList();

                foreach (var envelope in hiddenEnvelopes.Where(e => !budgets.Any(b => b.Envelope.Id == e.Id)))
                {
                    hiddenBudgets.Add(new Budget
                    {
                        Schedule = schedule.DeepCopy(),
                        Envelope = envelope.DeepCopy(),
                        Amount = 0m
                    });
                }

                var populatedSchedule = await GetPopulatedBudgetSchedule(schedule).ConfigureAwait(false);
                var tasks = hiddenBudgets.Select(b => GetPopulatedBudget(b, populatedSchedule));

                var budgetsToReturnTemp = await Task.WhenAll(tasks).ConfigureAwait(false);
                var budgetsToReturn = budgetsToReturnTemp.ToList();

                budgetsToReturn.Sort();

                result.Success = true;
                result.Data = budgetsToReturn;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        public async Task<Result<Budget>> GetBudgetAsync(Guid envelopeId, BudgetSchedule schedule)
        {
            var result = new Result<Budget>();

            try
            {
                var budgets = await _envelopeDataAccess.ReadBudgetsFromEnvelopeAsync(envelopeId);
                var budget = budgets.FirstOrDefault(b => b.Envelope.Id == envelopeId && b.Schedule.Id == schedule.Id);
                if (budget == null)
                {
                    budget = new Budget();
                }

                budget.Envelope = await _envelopeDataAccess.ReadEnvelopeAsync(envelopeId);

                var populatedSchedule = await GetPopulatedBudgetSchedule(schedule).ConfigureAwait(false);
                var populatedBudget = await GetPopulatedBudget(budget, populatedSchedule);

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

        public async Task<Result<Budget>> GetBudgetAsync(Guid id)
        {
            var result = new Result<Budget>();

            try
            {
                var budget = await _envelopeDataAccess.ReadBudgetAsync(id).ConfigureAwait(false);
                var populatedBudget = await GetPopulatedBudget(budget).ConfigureAwait(false);
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

        public async Task<Result<IReadOnlyList<QuickBudget>>> GetQuickBudgetsAsync(Budget budget)
        {
            var result = new Result<IReadOnlyList<QuickBudget>>();
            var quickBudgets = new List<QuickBudget>();

            try
            {
                var transactions = await _transactionDataAccess.ReadEnvelopeTransactionsAsync(budget.Envelope.Id).ConfigureAwait(false);
                var activeTransactions = transactions.Where(t => t.IsActive);
                var budgets = await _envelopeDataAccess.ReadBudgetsFromEnvelopeAsync(budget.Envelope.Id).ConfigureAwait(false);

                // previous schedule amount & activity
                var previousScheduleResult = await GetPreviousBudgetSchedule(budget.Schedule).ConfigureAwait(false);
                if (previousScheduleResult.Success)
                {
                    var previousSchedule = previousScheduleResult.Data;

                    BudgetSchedule lastMonth = GetBudgetScheduleFromDate(budget.Schedule.BeginDate.AddMonths(-1));
                    if (activeTransactions.Any(t => t.ServiceDate <= lastMonth.EndDate))
                    {
                        var threeMonthsAgoActivity = new QuickBudget
                        {
                            Description = _resourceContainer.GetResourceString("QuickBudgetLastMonthActivity"),
                            Amount = (activeTransactions
                                      .Where(t => t.ServiceDate < budget.Schedule.BeginDate && t.ServiceDate >= lastMonth.BeginDate)
                                      .Sum(t2 => t2.Amount ?? 0)) * -1
                        };
                        if (threeMonthsAgoActivity.Amount != 0)
                        {
                            quickBudgets.Add(threeMonthsAgoActivity);
                        }
                    }

                    if (budgets.Any(b => b.Schedule.EndDate <= lastMonth.EndDate))
                    {
                        var yearAgoActivity = new QuickBudget
                        {
                            Description = _resourceContainer.GetResourceString("QuickBudgetLastMonthBudgeted"),
                            Amount = (budgets
                                      .Where(b => b.Schedule.BeginDate < budget.Schedule.BeginDate && b.Schedule.BeginDate >= lastMonth.BeginDate)
                                      .Sum(t2 => t2.Amount ?? 0))
                        };
                        if (yearAgoActivity.Amount != 0)
                        {
                            quickBudgets.Add(yearAgoActivity);
                        }
                    }

                    BudgetSchedule threeMonthsAgo = GetBudgetScheduleFromDate(budget.Schedule.BeginDate.AddMonths(-3));
                    if (activeTransactions.Any(t => t.ServiceDate <= threeMonthsAgo.EndDate))
                    {
                        var threeMonthsAgoActivity = new QuickBudget
                        {
                            Description = _resourceContainer.GetResourceString("QuickBudgetAvgPast3MonthsActivity"),
                            Amount = (activeTransactions
                                      .Where(t => t.ServiceDate < budget.Schedule.BeginDate && t.ServiceDate >= threeMonthsAgo.BeginDate)
                                      .Sum(t2 => t2.Amount ?? 0)) / -3
                        };
                        if (threeMonthsAgoActivity.Amount != 0)
                        {
                            quickBudgets.Add(threeMonthsAgoActivity);
                        }
                    }

                    if (budgets.Any(b => b.Schedule.EndDate <= threeMonthsAgo.EndDate))
                    {
                        var yearAgoActivity = new QuickBudget
                        {
                            Description = _resourceContainer.GetResourceString("QuickBudgetAvgPast3MonthsBudgeted"),
                            Amount = (budgets
                                      .Where(b => b.Schedule.BeginDate < budget.Schedule.BeginDate && b.Schedule.BeginDate >= threeMonthsAgo.BeginDate)
                                      .Sum(t2 => t2.Amount ?? 0)) / 3
                        };
                        if (yearAgoActivity.Amount != 0)
                        {
                            quickBudgets.Add(yearAgoActivity);
                        }
                    }

                    BudgetSchedule yearAgo = GetBudgetScheduleFromDate(budget.Schedule.BeginDate.AddYears(-1));
                    if (activeTransactions.Any(t => t.ServiceDate <= yearAgo.EndDate))
                    {
                        var threeMonthsAgoActivity = new QuickBudget
                        {
                            Description = _resourceContainer.GetResourceString("QuickBudgetAvgPastYearActivity"),
                            Amount = (activeTransactions
                                      .Where(t => t.ServiceDate < budget.Schedule.BeginDate && t.ServiceDate >= yearAgo.BeginDate)
                                      .Sum(t2 => t2.Amount ?? 0)) / -12
                        };
                        if (threeMonthsAgoActivity.Amount != 0)
                        {
                            quickBudgets.Add(threeMonthsAgoActivity);
                        }
                    }

                    if (budgets.Any(b => b.Schedule.EndDate <= yearAgo.EndDate))
                    {
                        var yearAgoActivity = new QuickBudget
                        {
                            Description = _resourceContainer.GetResourceString("QuickBudgetAvgPastYearBudgeted"),
                            Amount = (budgets
                                      .Where(b => b.Schedule.BeginDate < budget.Schedule.BeginDate && b.Schedule.BeginDate >= yearAgo.BeginDate)
                                      .Sum(t2 => t2.Amount ?? 0)) / 12
                        };
                        if (yearAgoActivity.Amount != 0)
                        {
                            quickBudgets.Add(yearAgoActivity);
                        }
                    }


                    var balance = new QuickBudget
                    {
                        Description = _resourceContainer.GetResourceString("QuickBudgetBalance"),
                        Amount = (budget.Amount ?? 0) + (budget.Remaining * -1)
                    };
                    if (balance.Amount != 0)
                    {
                        quickBudgets.Add(balance);
                    }

                    // fix amounts
                    foreach (var quickBudget in quickBudgets)
                    {
                        quickBudget.Amount = _resourceContainer.GetRoundedDecimal(quickBudget.Amount);
                    }

                    result.Success = true;
                    result.Data = quickBudgets;
                }
                else
                {
                    result.Success = false;
                    result.Message = previousScheduleResult.Message;
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }
            return result;
        }

        public async Task<Result<IReadOnlyList<Budget>>> BudgetTransferAsync(BudgetSchedule schedule, Guid fromEnvelopeId, Guid toEnvelopeId, decimal amount)
        {
            var result = new Result<IReadOnlyList<Budget>>();

            // getting latest version of schedule and saving it before getting budgets
            // to fix issue with the budget schedule not being in the database
            // when saving the updated budget amounts
            var populatedSchedule = await GetPopulatedBudgetSchedule(schedule);
            var scheduleSaveResult = await SaveBudgetScheduleAsync(populatedSchedule);
            if (!scheduleSaveResult.Success)
            {
                result.Success = false;
                result.Message = scheduleSaveResult.Message;
                return result;
            }

            var budgetsResult = await GetBudgetsAsync(populatedSchedule);

            if (!budgetsResult.Success)
            {
                result.Success = false;
                result.Message = budgetsResult.Message;
                return result;
            }

            var budgets = budgetsResult.Data;

            var fromBudget = budgets.FirstOrDefault(b => b.Envelope.Id == fromEnvelopeId);
            var toBudget = budgets.FirstOrDefault(b => b.Envelope.Id == toEnvelopeId);

            if (fromBudget == null)
            {
                return new Result<IReadOnlyList<Budget>> { Success = false, Message = _resourceContainer.GetResourceString("TransferValidFromerror") };
            }

            if (toBudget == null)
            {
                return new Result<IReadOnlyList<Budget>> { Success = false, Message = _resourceContainer.GetResourceString("TransferValidToError") };
            }

            fromBudget.Amount -= amount;
            toBudget.Amount += amount;

            var fromResult = await SaveBudgetAsync(fromBudget).ConfigureAwait(false);
            if (!fromResult.Success)
            {
                result.Success = false;
                result.Message = fromResult.Message;
                return result;
            }

            var toResult = await SaveBudgetAsync(toBudget).ConfigureAwait(false);
            if (!toResult.Success)
            {
                result.Success = false;
                result.Message = toResult.Message;
                return result;
            }

            result.Success = true;
            result.Data = new List<Budget>()
            {
                fromResult.Data,
                toResult.Data
            };

            return result;
        }

        public async Task<Result<int>> GetEnvelopesCountAsync()
        {
            var result = new Result<int>();

            try
            {
                var count = await _envelopeDataAccess.GetEnvelopesCountAsync();
                result.Success = true;
                result.Data = count;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        public async Task<Result<IReadOnlyList<Envelope>>> GetEnvelopesForSelectionAsync()
        {
            var result = new Result<IReadOnlyList<Envelope>>();

            try
            {
                var envelopes = await _envelopeDataAccess.ReadEnvelopesAsync().ConfigureAwait(false);
                var activeEnvelopes = envelopes.Where(e => !e.IsSystem && e.IsActive).ToList();

                activeEnvelopes.RemoveAll(b => b.Group.IsDebt);
                activeEnvelopes.Add(GetGenericDebtEnvelope());

                var tasks = activeEnvelopes.Select(GetPopulatedEnvelope);

                var populatedEnvelopesTemp = await Task.WhenAll(tasks).ConfigureAwait(false);
                var populatedEnvelopes = populatedEnvelopesTemp.ToList();
                populatedEnvelopes.Sort();

                result.Success = true;
                result.Data = populatedEnvelopes;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        public async Task<Result<IReadOnlyList<Envelope>>> GetEnvelopesForReportAsync()
        {
            var result = new Result<IReadOnlyList<Envelope>>();

            try
            {
                var envelopes = await _envelopeDataAccess.ReadEnvelopesAsync().ConfigureAwait(false);
                var envelopesToReturn = envelopes.Where(e => e.IsActive
                                                      && !e.IsSystem
                                                      && !e.Group.IsIncome
                                                      && !e.Group.IsSystem
                                                      && !e.Group.IsDebt).ToList();

                if (envelopes.Any(envelope => envelope.IsHidden && !envelope.IsDeleted && !(envelope.IsIncome || envelope.IsBuffer || envelope.Group.IsIncome || envelope.IsGenericDebtEnvelope || envelope.Group.IsDebt || envelope.IsSystem || envelope.IsGenericHiddenEnvelope)))
                {
                    var genericHiddenENvelope = GetGenericHiddenEnvelope();

                    envelopesToReturn.Add(genericHiddenENvelope);
                }

                var tasks = envelopesToReturn.Select(GetPopulatedEnvelope);

                var populatedEnvelopesTemp = await Task.WhenAll(tasks).ConfigureAwait(false);
                var populatedEnvelopes = populatedEnvelopesTemp.ToList();
                populatedEnvelopes.Sort();

                result.Success = true;
                result.Data = populatedEnvelopes;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        public async Task<Result<IReadOnlyList<Envelope>>> GetHiddenEnvelopesAsync()
        {
            var result = new Result<IReadOnlyList<Envelope>>();

            try
            {
                var envelopes = await _envelopeDataAccess.ReadEnvelopesAsync().ConfigureAwait(false);
                var hiddenEnvelopes = envelopes.Where(e =>
                    !e.Group.IsIncome &&
                    !e.Group.IsDebt &&
                    !e.Group.IsSystem &&
                    e.IsHidden &&
                    !e.IsDeleted).ToList();

                var tasks = hiddenEnvelopes.Select(GetPopulatedEnvelope);

                var populatedEnvelopesTemp = await Task.WhenAll(tasks).ConfigureAwait(false);
                var populatedEnvelopes = populatedEnvelopesTemp.ToList();
                populatedEnvelopes.Sort();

                result.Success = true;
                result.Data = populatedEnvelopes;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        public async Task<Result<Envelope>> SoftDeleteEnvelopeAsync(Guid id)
        {
            var result = new Result<Envelope>();

            try
            {
                var envelope = await _envelopeDataAccess.ReadEnvelopeAsync(id).ConfigureAwait(false);

                // check for validation to delete
                var errors = new List<string>();

                if (envelope.IsNew || envelope.IsDeleted || envelope.IsActive)
                {
                    errors.Add(_resourceContainer.GetResourceString("EnvelopeDeleteNotHiddenError"));
                }

                if (envelope.IsIncome || envelope.IsBuffer || envelope.Group.IsIncome || envelope.IsGenericDebtEnvelope || envelope.Group.IsDebt || envelope.IsSystem || envelope.IsGenericHiddenEnvelope)
                {
                    errors.Add(_resourceContainer.GetResourceString("EnvelopeDeleteSystemError"));
                }

                var envelopeTransactions = await _transactionDataAccess.ReadEnvelopeTransactionsAsync(id).ConfigureAwait(false);
                if (envelopeTransactions.Any(t => t.IsActive))
                {
                    errors.Add(_resourceContainer.GetResourceString("EnvelopeDeleteActiveTransactionsError"));
                }

                var envelopeBudgets = await _envelopeDataAccess.ReadBudgetsFromEnvelopeAsync(id).ConfigureAwait(false);
                if (envelopeBudgets.Any(b => b.Amount != 0))
                {
                    errors.Add(_resourceContainer.GetResourceString("EnvelopeDeleteNonZeroBudgetsError"));
                }

                if (errors.Any())
                {
                    result.Success = false;
                    result.Message = string.Join(Environment.NewLine, errors);
                    return result;
                }

                envelope.DeletedDateTime = DateTime.Now;
                envelope.ModifiedDateTime = DateTime.Now;

                await _envelopeDataAccess.UpdateEnvelopeAsync(envelope);

                result.Success = true;
                result.Data = await GetPopulatedEnvelope(envelope).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        public async Task<Result<Envelope>> HideEnvelopeAsync(Guid id)
        {
            var result = new Result<Envelope>();

            try
            {
                var envelope = await _envelopeDataAccess.ReadEnvelopeAsync(id).ConfigureAwait(false);

                // check for validation to Hide
                var errors = new List<string>();

                if (!envelope.IsActive)
                {
                    errors.Add(_resourceContainer.GetResourceString("EnvelopeHideInactiveError"));
                }

                if (envelope.IsIncome || envelope.IsBuffer || envelope.Group.IsIncome || envelope.IsGenericDebtEnvelope || envelope.Group.IsDebt || envelope.IsSystem || envelope.IsGenericHiddenEnvelope)
                {
                    errors.Add(_resourceContainer.GetResourceString("EnvelopeHideSystemError"));
                }

                if (errors.Any())
                {
                    result.Success = false;
                    result.Message = string.Join(Environment.NewLine, errors);
                    return result;
                }

                envelope.HiddenDateTime = DateTime.Now;
                envelope.ModifiedDateTime = DateTime.Now;

                await _envelopeDataAccess.UpdateEnvelopeAsync(envelope);

                result.Success = true;
                result.Data = await GetPopulatedEnvelope(envelope).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        public async Task<Result<Envelope>> UnhideEnvelopeAsync(Guid id)
        {
            var result = new Result<Envelope>();

            try
            {
                var envelope = await _envelopeDataAccess.ReadEnvelopeAsync(id).ConfigureAwait(false);

                // check for validation to delete
                var errors = new List<string>();

                if (envelope.IsNew || envelope.IsActive || envelope.IsDeleted)
                {
                    errors.Add(_resourceContainer.GetResourceString("EnvelopeUnhideNotHiddenError"));
                }

                if (envelope.IsIncome || envelope.IsBuffer || envelope.Group.IsIncome || envelope.IsGenericDebtEnvelope || envelope.Group.IsDebt || envelope.IsSystem || envelope.IsGenericHiddenEnvelope)
                {
                    errors.Add(_resourceContainer.GetResourceString("EnvelopeUnhideSystemError"));
                }

                if (errors.Any())
                {
                    result.Success = false;
                    result.Message = string.Join(Environment.NewLine, errors);
                    return result;
                }

                envelope.HiddenDateTime = null;
                envelope.ModifiedDateTime = DateTime.Now;

                await _envelopeDataAccess.UpdateEnvelopeAsync(envelope);

                result.Success = true;
                result.Data = await GetPopulatedEnvelope(envelope).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        public async Task<Result<EnvelopeGroup>> SaveEnvelopeGroupAsync(EnvelopeGroup group)
        {
            var validationResult = await ValidateEnvelopeGroupAsync(group).ConfigureAwait(false);
            if (!validationResult.Success)
            {
                return validationResult.ToResult<EnvelopeGroup>();
            }

            var result = new Result<EnvelopeGroup>();
            var groupToUpsert = group.DeepCopy();
            var dateTimeNow = DateTime.Now;

            if (groupToUpsert.IsNew)
            {
                groupToUpsert.Id = Guid.NewGuid();
                groupToUpsert.CreatedDateTime = dateTimeNow;
                groupToUpsert.ModifiedDateTime = dateTimeNow;

                await _envelopeDataAccess.CreateEnvelopeGroupAsync(groupToUpsert).ConfigureAwait(false);
                result.Success = true;
                result.Data = groupToUpsert;
            }
            else
            {
                groupToUpsert.ModifiedDateTime = dateTimeNow;

                await _envelopeDataAccess.UpdateEnvelopeGroupAsync(groupToUpsert).ConfigureAwait(false);
                result.Success = true;
                result.Data = groupToUpsert;
            }

            return result;
        }

        public async Task<Result<int>> GetEnvelopeGroupsCountAsync()
        {
            var result = new Result<int>();

            try
            {
                var count = await _envelopeDataAccess.GetEnvelopeGroupsCountAsync();
                result.Success = true;
                result.Data = count;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        public async Task<Result<IReadOnlyList<EnvelopeGroup>>> GetEnvelopeGroupsAsync()
        {
            var result = new Result<IReadOnlyList<EnvelopeGroup>>();

            try
            {
                var envelopeGroups = await _envelopeDataAccess.ReadEnvelopeGroupsAsync().ConfigureAwait(false);
                var filteredEnvelopeGroups = envelopeGroups.Where(e => e.IsActive && !e.IsSystem && !e.IsIncome && !e.IsDebt).ToList();

                if (envelopeGroups.Any(eg => eg.IsHidden && !eg.IsDeleted && !(eg.IsIncome || eg.IsIncome || eg.IsDebt || eg.IsSystem || eg.IsGenericHiddenEnvelopeGroup)))
                {
                    var genericHiddenGroup = GetGenericHiddenEnvelopeGroup();
                    filteredEnvelopeGroups.Add(genericHiddenGroup);
                }

                filteredEnvelopeGroups.Sort();
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

        public async Task<Result<IReadOnlyList<EnvelopeGroup>>> GetEnvelopeGroupsForSelectionAsync()
        {
            var result = new Result<IReadOnlyList<EnvelopeGroup>>();

            try
            {
                var envelopeGroups = await _envelopeDataAccess.ReadEnvelopeGroupsAsync().ConfigureAwait(false);
                var filteredEnvelopeGroups = envelopeGroups.Where(e => e.IsActive && !e.IsSystem && !e.IsIncome && !e.IsDebt).ToList();
                filteredEnvelopeGroups.Sort();
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

        public async Task<Result<IReadOnlyList<EnvelopeGroup>>> GetHiddenEnvelopeGroupsAsync()
        {
            var result = new Result<IReadOnlyList<EnvelopeGroup>>();

            try
            {
                var envelopeGroups = await _envelopeDataAccess.ReadEnvelopeGroupsAsync().ConfigureAwait(false);
                var filteredEnvelopeGroups = envelopeGroups.Where(e =>
                    e.IsHidden &&
                    !e.IsDeleted &&
                    !e.IsSystem &&
                    !e.IsIncome &&
                    !e.IsDebt).ToList();
                filteredEnvelopeGroups.Sort();
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

        public async Task<Result<EnvelopeGroup>> SoftDeleteEnvelopeGroupAsync(Guid id)
        {
            var result = new Result<EnvelopeGroup>();

            try
            {
                var envelopeGroup = await _envelopeDataAccess.ReadEnvelopeGroupAsync(id).ConfigureAwait(false);

                // check for validation to delete
                var errors = new List<string>();

                if (envelopeGroup.IsNew || envelopeGroup.IsDeleted || envelopeGroup.IsActive)
                {
                    errors.Add(_resourceContainer.GetResourceString("EnvelopeGroupDeleteNotHiddenError"));
                }

                if (envelopeGroup.IsIncome || envelopeGroup.IsDebt || envelopeGroup.IsSystem || envelopeGroup.IsGenericHiddenEnvelopeGroup)
                {
                    errors.Add(_resourceContainer.GetResourceString("EnvelopeGroupDeleteSystemError"));
                }

                var envelopes = await _envelopeDataAccess.ReadEnvelopesAsync().ConfigureAwait(false);
                var envelopeGroupEnvelopes = envelopes.Where(e => e.Group.Id == id);

                if (envelopeGroupEnvelopes.Any(t => t.IsActive))
                {
                    errors.Add(_resourceContainer.GetResourceString("EnvelopeGroupDeleteActiveEnvelopesError"));
                }

                if (errors.Any())
                {
                    result.Success = false;
                    result.Message = string.Join(Environment.NewLine, errors);
                    return result;
                }

                envelopeGroup.DeletedDateTime = DateTime.Now;
                envelopeGroup.ModifiedDateTime = DateTime.Now;

                await _envelopeDataAccess.UpdateEnvelopeGroupAsync(envelopeGroup);

                result.Success = true;
                result.Data = envelopeGroup;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        public async Task<Result<EnvelopeGroup>> HideEnvelopeGroupAsync(Guid id)
        {
            var result = new Result<EnvelopeGroup>();

            try
            {
                var envelopeGroup = await _envelopeDataAccess.ReadEnvelopeGroupAsync(id).ConfigureAwait(false);

                // check for validation to Hide
                var errors = new List<string>();

                if (!envelopeGroup.IsActive)
                {
                    errors.Add(_resourceContainer.GetResourceString("EnvelopeGroupHideInactiveError"));
                }

                if (envelopeGroup.IsIncome || envelopeGroup.IsDebt || envelopeGroup.IsSystem || envelopeGroup.IsGenericHiddenEnvelopeGroup)
                {
                    errors.Add(_resourceContainer.GetResourceString("EnvelopeGroupHideSystemError"));
                }

                var envelopes = await _envelopeDataAccess.ReadEnvelopesAsync().ConfigureAwait(false);
                var envelopeGroupEnvelopes = envelopes.Where(e => e.Group.Id == id);

                if (envelopeGroupEnvelopes.Any(t => t.IsActive))
                {
                    errors.Add(_resourceContainer.GetResourceString("EnvelopeGroupHideActiveEnvelopesError"));
                }

                if (errors.Any())
                {
                    result.Success = false;
                    result.Message = string.Join(Environment.NewLine, errors);
                    return result;
                }

                envelopeGroup.HiddenDateTime = DateTime.Now;
                envelopeGroup.ModifiedDateTime = DateTime.Now;

                await _envelopeDataAccess.UpdateEnvelopeGroupAsync(envelopeGroup);

                result.Success = true;
                result.Data = envelopeGroup;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        public async Task<Result<EnvelopeGroup>> UnhideEnvelopeGroupAsync(Guid id)
        {
            var result = new Result<EnvelopeGroup>();

            try
            {
                var envelopeGroup = await _envelopeDataAccess.ReadEnvelopeGroupAsync(id).ConfigureAwait(false);

                // check for validation to delete
                var errors = new List<string>();

                if (envelopeGroup.IsNew || envelopeGroup.IsActive || envelopeGroup.IsDeleted)
                {
                    errors.Add(_resourceContainer.GetResourceString("EnvelopeGroupUnhideNotHiddenError"));
                }

                if(envelopeGroup.IsIncome || envelopeGroup.IsDebt || envelopeGroup.IsSystem || envelopeGroup.IsGenericHiddenEnvelopeGroup)
                {
                    errors.Add(_resourceContainer.GetResourceString("EnvelopeGroupUnhideSystemError"));
                }

                if (errors.Any())
                {
                    result.Success = false;
                    result.Message = string.Join(Environment.NewLine, errors);
                    return result;
                }

                envelopeGroup.HiddenDateTime = null;
                envelopeGroup.ModifiedDateTime = DateTime.Now;

                await _envelopeDataAccess.UpdateEnvelopeGroupAsync(envelopeGroup);

                result.Success = true;
                result.Data = envelopeGroup;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        public bool FilterEnvelopeGroup(EnvelopeGroup envelopeGroup, string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
            {
                return true;
            }
            if (envelopeGroup != null)
            {
                return envelopeGroup.Description.ToLower().Contains(searchText.ToLower());
            }
            else
            {
                return false;
            }
        }

        public bool FilterEnvelope(Envelope envelope, string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
            {
                return true;
            }
            if (envelope != null)
            {
                return envelope.Group.Description.ToLower().Contains(searchText.ToLower())
                    || envelope.Description.ToLower().Contains(searchText.ToLower());
            }
            else
            {
                return false;
            }
        }

        public bool FilterBudget(Budget budget, string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
            {
                return true;
            }
            if (budget != null && budget.Envelope != null)
            {
                return budget.Envelope.Group.Description.ToLower().Contains(searchText.ToLower())
                             || budget.Envelope.Description.ToLower().Contains(searchText.ToLower());
            }
            else
            {
                return false;
            }
        }

        protected Budget PopulateBudget(Budget budget,
                                        IEnumerable<Transaction> envelopeTransactions,
                                        IEnumerable<Budget> envelopeBudgets)
        {
            if (budget.Envelope.IsIncome)
            {
                budget.Envelope = GetIncomeEnvelope();
            }
            else if (budget.Envelope.IsBuffer)
            {
                budget.Envelope = GetBufferEnvelope();
            }
            else if (budget.Envelope.IsSystem)
            {
                budget.Envelope = GetIgnoredEnvelope();
            }
            else if (budget.Envelope.IsGenericDebtEnvelope)
            {
                budget.Envelope = GetGenericDebtEnvelope();
            }
            else if (budget.Envelope.IsGenericHiddenEnvelope)
            {
                budget.Envelope = GetGenericHiddenEnvelope();
            }
            else if (budget.Envelope.Group.IsDebt)
            {
                budget.Envelope.Group = GetDebtEnvelopeGroup();
            }

            var activeTransactions = envelopeTransactions.Where(t => t.IsActive);

            budget.PastAmount = envelopeBudgets
                .Where(b => b.Schedule.EndDate < budget.Schedule.BeginDate)
                .Sum(b2 => b2.Amount ?? 0);

            budget.PastActivity = activeTransactions
                .Where(t => t.ServiceDate < budget.Schedule.BeginDate)
                .Sum(t2 => t2.Amount ?? 0);

            budget.Activity = activeTransactions
                .Where(t => t.ServiceDate >= budget.Schedule.BeginDate && t.ServiceDate <= budget.Schedule.EndDate)
                .Sum(t2 => t2.Amount ?? 0);

            // inheritance for ignore overspend
            if (budget.Envelope.IgnoreOverspend)
            {
                budget.IgnoreOverspend = true;
            }

            return budget;
        }

        protected Envelope PopulateEnvelope(Envelope envelope)
        {
            if (envelope.Group.IsDebt)
            {
                envelope.Group = GetDebtEnvelopeGroup();
            }

            return envelope;
        }

        protected BudgetSchedule PopulateBudgetSchedule(BudgetSchedule budgetSchedule,
                                                       IEnumerable<Account> allAccounts,
                                                       IEnumerable<Transaction> allTransactions,
                                                       IEnumerable<Envelope> envelopes,
                                                       IEnumerable<Budget> budgets)
        {
            var budgetAccounts = allAccounts.Where(a => a.OnBudget);

            var budgetTransactions = allTransactions.Where(t => t.IsActive &&
                                                           budgetAccounts.Any(b => b.Id == t.Account.Id));

            // get all income
            var incomeTransactions = budgetTransactions.Where(t => t.Envelope.IsIncome);

            var pastIncome = incomeTransactions
                .Where(t => t.ServiceDate < budgetSchedule.BeginDate)
                .Sum(t => t.Amount ?? 0);
            var currentIncome = incomeTransactions
                .Where(t => t.ServiceDate >= budgetSchedule.BeginDate && t.ServiceDate <= budgetSchedule.EndDate)
                .Sum(t => t.Amount ?? 0);

            // get all buffers
            var bufferTransactions = budgetTransactions.Where(t => t.Envelope.IsBuffer);
            var previousScheduleDate = GetPreviousBudgetScheduleDate(budgetSchedule);
            var previousSchedule = GetBudgetScheduleFromDate(previousScheduleDate);

            var pastBufferIncome = bufferTransactions
                .Where(t => t.ServiceDate < previousSchedule.BeginDate)
                .Sum(t => t.Amount ?? 0);
            var currentBufferIncome = bufferTransactions
                .Where(t => t.ServiceDate >= previousSchedule.BeginDate && t.ServiceDate <= previousSchedule.EndDate)
                .Sum(t => t.Amount ?? 0);

            var currentBudgetAmount = budgets
                .Where(b => !b.Envelope.IsIncome
                       && !b.Envelope.IsBuffer
                       && !b.Envelope.IsSystem
                       && b.Schedule.Id == budgetSchedule.Id)
                .Sum(b => b.Amount ?? 0);
            var pastBudgetAmount = budgets
                .Where(b => !b.Envelope.IsIncome
                       && !b.Envelope.IsBuffer
                       && !b.Envelope.IsSystem
                       && b.Schedule.EndDate < budgetSchedule.BeginDate)
                .Sum(b => b.Amount ?? 0);

            // past is all past income + all past budget amounts
            var past = (pastIncome + pastBufferIncome) - pastBudgetAmount;

            // income is income for this schedule
            var income = currentIncome + currentBufferIncome;

            // budgeted is amounts for this schedule
            var budgeted = currentBudgetAmount;

            // overspend is current and past budget amounts + current and past transactions (if negative)
            decimal overspend = 0;
            foreach (var envelope in envelopes.Where(e => !e.IsIncome && !e.IsBuffer && !e.IsSystem))
            {
                var envelopeTransactionsAmount = budgetTransactions
                .Where(t => t.Envelope.Id == envelope.Id
                       && t.ServiceDate <= budgetSchedule.EndDate)
                    .Sum(t => t.Amount ?? 0);

                var envelopeBudgetAmount = budgets
                    .Where(b => b.Envelope.Id == envelope.Id
                           && b.Schedule.EndDate <= budgetSchedule.EndDate)
                    .Sum(b => b.Amount ?? 0);

                var envelopeOverspend = envelopeBudgetAmount + envelopeTransactionsAmount;

                var latestBudget = budgets.FirstOrDefault(b => b.Envelope.Id == envelope.Id && b.Schedule.Id == budgetSchedule.Id);
                var latestBudgetIgnoreOverspend = latestBudget?.IgnoreOverspend ?? false;

                var ignore = latestBudgetIgnoreOverspend || envelope.IgnoreOverspend;

                if (envelopeOverspend < 0 && !ignore)
                {
                    overspend += Math.Abs(envelopeOverspend);
                }
            }

            budgetSchedule.Past = past;
            budgetSchedule.Income = income;
            budgetSchedule.Budgeted = budgeted;
            budgetSchedule.Overspend = overspend;
            // could change this
            budgetSchedule.Description = _resourceContainer.GetFormattedString("{0:Y}", budgetSchedule.BeginDate);

            return budgetSchedule;
        }

        async Task<Result> ValidateBudgetAsync(Budget budget)
        {
            var errors = new List<string>();

            if (!budget.Amount.HasValue)
            {
                errors.Add(_resourceContainer.GetResourceString("EnvelopeValidAmountError"));
            }

            if (budget.Envelope == null)
            {
                errors.Add(_resourceContainer.GetResourceString("EnvelopeValidEnvelopeExistError"));
            }
            else
            {
                var envelopeValidationResult = await ValidateEnvelopeAsync(budget.Envelope);
                if (!envelopeValidationResult.Success)
                {
                    errors.Add(_resourceContainer.GetResourceString("EnvelopeValidEnvelopeError"));
                }
            }

            if (budget.Schedule == null)
            {
                errors.Add(_resourceContainer.GetResourceString("EnvelopeValidScheduleExistError"));
            }
            else
            {
                var scheduleValidResult = await ValidateBudgetScheduleAsync(budget.Schedule);
                if (!scheduleValidResult.Success)
                {
                    errors.Add(_resourceContainer.GetResourceString("EnvelopeValidScheduleError"));
                }
            }

            if (errors.Any())
            {
                return new Result { Success = !errors.Any(), Message = string.Join(Environment.NewLine, errors) };
            }

            if (budget.Envelope.IgnoreOverspend && !budget.IgnoreOverspend)
            {
                errors.Add(_resourceContainer.GetResourceString("EnvelopeValidOverspendError"));
            }

            if (budget.Envelope.Group.IsDebt && 
                (!budget.IgnoreOverspend || !budget.Envelope.IgnoreOverspend))
            {
                errors.Add(_resourceContainer.GetResourceString("EnvelopeValidOverspendDebtError"));
            }

            if (budget.IsNew &&
                budget.Schedule != null &&
                budget.Envelope != null)
            {
                var existingBudget = await _envelopeDataAccess.ReadBudgetFromScheduleAndEnvelopeAsync(budget.Schedule.Id, budget.Envelope.Id);
                if (existingBudget.IsActive)
                {
                    errors.Add(_resourceContainer.GetResourceString("BudgetValidAlreadyExists"));
                }
            }

            return new Result { Success = !errors.Any(), Message = string.Join(Environment.NewLine, errors) };
        }

        async Task<Result> ValidateEnvelopeAsync(Envelope envelope)
        {
            var errors = new List<string>();

            if (envelope.IsIncome || envelope.IsBuffer || envelope.Group.IsIncome || envelope.IsGenericDebtEnvelope || envelope.IsSystem || envelope.IsGenericHiddenEnvelope)
            {
                errors.Add(_resourceContainer.GetResourceString("EnvelopeSaveSystemError"));
            }

            if (string.IsNullOrEmpty(envelope.Description))
            {
                errors.Add(_resourceContainer.GetResourceString("EnvelopeValidDescriptionError"));
            }

            if (envelope.Group == null)
            {
                errors.Add(_resourceContainer.GetResourceString("EnvelopeValidGroupError"));
            }
            else
            {
                var validationResult = await ValidateEnvelopeGroupAsync(envelope.Group);
                if (!validationResult.Success)
                {
                    errors.Add(_resourceContainer.GetResourceString("EnvelopeValidGroupValidError"));
                }
            }

            return new Result { Success = !errors.Any(), Message = string.Join(Environment.NewLine, errors) };
        }

        Task<Result> ValidateEnvelopeGroupAsync(EnvelopeGroup envelopeGroup)
        {
            var errors = new List<string>();

            if (envelopeGroup.IsIncome || envelopeGroup.IsSystem || envelopeGroup.IsGenericHiddenEnvelopeGroup)
            {
                errors.Add(_resourceContainer.GetResourceString("EnvelopeGroupSaveSystemError"));
            }

            if (string.IsNullOrEmpty(envelopeGroup.Description))
            {
                errors.Add(_resourceContainer.GetResourceString("EnvelopeGroupValidDescriptionError"));
            }

            return Task.FromResult(new Result { Success = !errors.Any(), Message = string.Join(Environment.NewLine, errors) });
        }

        Task<Result> ValidateBudgetScheduleAsync(BudgetSchedule budgetSchedule)
        {
            var errors = new List<string>();

            if (string.IsNullOrEmpty(budgetSchedule.Description))
            {
                errors.Add(_resourceContainer.GetResourceString("ScheduleValidDescriptionError"));
            }

            return Task.FromResult<Result>(new Result { Success = !errors.Any(), Message = string.Join(Environment.NewLine, errors) });
        }

        async Task<Result<Envelope>> SaveEnvelopeAsync(Envelope envelope)
        {
            var envelopeValid = await ValidateEnvelopeAsync(envelope);
            if (!envelopeValid.Success)
            {
                return envelopeValid.ToResult<Envelope>();
            }

            var result = new Result<Envelope>();
            var envelopeToUpsert = envelope.DeepCopy();
            var dateTimeNow = DateTime.Now;

            var groupResult = await SaveEnvelopeGroupAsync(envelopeToUpsert.Group).ConfigureAwait(false);
            if (groupResult.Success)
            {
                envelopeToUpsert.Group = groupResult.Data;
            }


            if (envelopeToUpsert.IsNew)
            {
                envelopeToUpsert.Id = Guid.NewGuid();
                envelopeToUpsert.CreatedDateTime = dateTimeNow;
                envelopeToUpsert.ModifiedDateTime = dateTimeNow;

                await _envelopeDataAccess.CreateEnvelopeAsync(envelopeToUpsert).ConfigureAwait(false);
                result.Success = true;
                result.Data = envelopeToUpsert;
            }
            else
            {
                envelopeToUpsert.ModifiedDateTime = dateTimeNow;

                await _envelopeDataAccess.UpdateEnvelopeAsync(envelopeToUpsert).ConfigureAwait(false);
                result.Success = true;
                result.Data = envelopeToUpsert;
            }

            return result;
        }

        async Task<Result<BudgetSchedule>> SaveBudgetScheduleAsync(BudgetSchedule budgetSchedule)
        {
            var scheduleValidation = await ValidateBudgetScheduleAsync(budgetSchedule);
            if (!scheduleValidation.Success)
            {
                return scheduleValidation.ToResult<BudgetSchedule>();
            }

            var result = new Result<BudgetSchedule>();
            var scheduleToUpsert = budgetSchedule.DeepCopy();

            if (scheduleToUpsert.IsNew)
            {
                scheduleToUpsert.Id = scheduleToUpsert.BeginDate.ToGuid();
                scheduleToUpsert.CreatedDateTime = DateTime.Now;
                scheduleToUpsert.ModifiedDateTime = DateTime.Now;

                await _envelopeDataAccess.CreateBudgetScheduleAsync(scheduleToUpsert).ConfigureAwait(false);
                result.Success = true;
                result.Data = scheduleToUpsert;
            }
            else
            {
                scheduleToUpsert.ModifiedDateTime = DateTime.Now;

                await _envelopeDataAccess.UpdateBudgetScheduleAsync(scheduleToUpsert).ConfigureAwait(false);
                result.Success = true;
                result.Data = scheduleToUpsert;
            }

            return result;
        }

        async Task<Budget> GetPopulatedBudget(Budget budget, BudgetSchedule budgetSchedule = null)
        {
            var budgetToPopulate = budget.DeepCopy();

            if (budgetSchedule == null)
            {
                budgetToPopulate.Schedule = await GetPopulatedBudgetSchedule(budgetToPopulate.Schedule).ConfigureAwait(false);
            }
            else
            {
                budgetToPopulate.Schedule = budgetSchedule.DeepCopy();
            }

            var transactions = await _transactionDataAccess.ReadEnvelopeTransactionsAsync(budgetToPopulate.Envelope.Id).ConfigureAwait(false);
            var budgets = await _envelopeDataAccess.ReadBudgetsFromEnvelopeAsync(budgetToPopulate.Envelope.Id).ConfigureAwait(false);

            return await Task.Run(() => PopulateBudget(budgetToPopulate, transactions, budgets));
        }

        async Task<Envelope> GetPopulatedEnvelope(Envelope envelope)
        {
            var envelopeToPopulate = envelope.DeepCopy();

            return await Task.Run(() => PopulateEnvelope(envelopeToPopulate));
        }

        async Task<BudgetSchedule> GetPopulatedBudgetSchedule(BudgetSchedule budgetSchedule)
        {
            var allAccounts = await _accountDataAccess.ReadAccountsAsync().ConfigureAwait(false);
            var allTransactions = await _transactionDataAccess.ReadTransactionsAsync().ConfigureAwait(false);
            var envelopes = await _envelopeDataAccess.ReadEnvelopesAsync().ConfigureAwait(false);
            var budgets = await _envelopeDataAccess.ReadBudgetsAsync().ConfigureAwait(false);
            var newBudgetSchedule = budgetSchedule.DeepCopy();
            // get existing schedule from data access if exists
            var schedule = await _envelopeDataAccess.ReadBudgetScheduleAsync(budgetSchedule.Id).ConfigureAwait(false);
            if (schedule.IsActive)
            {
                newBudgetSchedule = schedule.DeepCopy();
            }

            return await Task.Run(() => PopulateBudgetSchedule(newBudgetSchedule, allAccounts, allTransactions, envelopes, budgets));
        }

        BudgetSchedule GetBudgetScheduleFromDate(DateTime date)
        {
            var selectedSchedule = new BudgetSchedule();
            selectedSchedule.BeginDate = new DateTime(date.Year, date.Month, 1);
            selectedSchedule.EndDate = selectedSchedule.BeginDate.AddMonths(1).AddTicks(-1);
            selectedSchedule.Id = selectedSchedule.BeginDate.ToGuid();

            return selectedSchedule;
        }

        DateTime GetNextBudgetScheduleDate(BudgetSchedule currentSchedule)
        {
            return currentSchedule.EndDate.AddDays(1);
        }

        DateTime GetPreviousBudgetScheduleDate(BudgetSchedule currentSchedule)
        {
            return currentSchedule.BeginDate.AddDays(-1);
        }

        EnvelopeGroup GetDebtEnvelopeGroup()
        {
            var debtEnvelopeGroup = Constants.DebtEnvelopeGroup.DeepCopy();
            debtEnvelopeGroup.Description = _resourceContainer.GetResourceString(nameof(Constants.DebtEnvelopeGroup));
            return debtEnvelopeGroup;
        }

        Envelope GetGenericDebtEnvelope()
        {
            var genericDebtEnvelope = Constants.GenericDebtEnvelope.DeepCopy();
            genericDebtEnvelope.Description = _resourceContainer.GetResourceString(nameof(Constants.GenericDebtEnvelope));
            genericDebtEnvelope.Group = GetDebtEnvelopeGroup();
            return genericDebtEnvelope;
        }

        Budget GetGenericDebtBudget(BudgetSchedule schedule, IEnumerable<Budget> debtBudgets = null)
        {
            var debtBudget = new Budget
            {
                Envelope = GetGenericDebtEnvelope(),
                Schedule = schedule
            };

            if (debtBudgets != null)
            {
                debtBudget.PastAmount = debtBudgets.Sum(b => b.PastAmount);
                debtBudget.PastActivity = debtBudgets.Sum(b => b.PastActivity);
                debtBudget.Activity = debtBudgets.Sum(b => b.Activity);
                debtBudget.Amount = debtBudgets.Sum(b => b.Amount);
            }

            return debtBudget;
        }

        EnvelopeGroup GetIncomeEnvelopeGroup()
        {
            var incomeEnvelopeGroup = Constants.IncomeEnvelopeGroup.DeepCopy();

            incomeEnvelopeGroup.Description = _resourceContainer.GetResourceString(nameof(Constants.IncomeEnvelopeGroup));

            return incomeEnvelopeGroup;
        }

        Envelope GetIncomeEnvelope()
        {
            var incomeEnvelope = Constants.IncomeEnvelope.DeepCopy();
            incomeEnvelope.Description = _resourceContainer.GetResourceString(nameof(Constants.IncomeEnvelope));
            incomeEnvelope.Group = GetIncomeEnvelopeGroup();
            return incomeEnvelope;
        }

        Envelope GetBufferEnvelope()
        {
            var bufferEnvelope = Constants.BufferEnvelope.DeepCopy();
            bufferEnvelope.Description = _resourceContainer.GetResourceString(nameof(Constants.BufferEnvelope));
            bufferEnvelope.Group = GetIncomeEnvelopeGroup();
            return bufferEnvelope;
        }

        EnvelopeGroup GetSystemEnvelopeGroup()
        {
            var systemEnvelopeGroup = Constants.SystemEnvelopeGroup.DeepCopy();

            systemEnvelopeGroup.Description = _resourceContainer.GetResourceString(nameof(Constants.SystemEnvelopeGroup));

            return systemEnvelopeGroup;
        }

        Envelope GetIgnoredEnvelope()
        {
            var ignoredEnvelope = Constants.IgnoredEnvelope.DeepCopy();
            ignoredEnvelope.Description = _resourceContainer.GetResourceString(nameof(Constants.IgnoredEnvelope));
            ignoredEnvelope.Group = GetSystemEnvelopeGroup();
            return ignoredEnvelope;
        }

        EnvelopeGroup GetGenericHiddenEnvelopeGroup()
        {
            var genericHiddenEnvelopeGroup = Constants.GenericHiddenEnvelopeGroup.DeepCopy();
            genericHiddenEnvelopeGroup.Description = _resourceContainer.GetResourceString("Hidden");
            return genericHiddenEnvelopeGroup;
        }

        Envelope GetGenericHiddenEnvelope()
        {
            var genericHiddenEnvelope = Constants.GenericHiddenEnvelope.DeepCopy();
            genericHiddenEnvelope.Description = _resourceContainer.GetResourceString("Hidden");
            genericHiddenEnvelope.Group = GetGenericHiddenEnvelopeGroup();
            return genericHiddenEnvelope;
        }

        Budget GetGenericHiddenBudget(BudgetSchedule schedule, IEnumerable<Budget> hiddenBudgets = null)
        {
            var hiddenGenericBudget = new Budget();
            hiddenGenericBudget.Envelope = GetGenericHiddenEnvelope();
            hiddenGenericBudget.Schedule = schedule;

            if (hiddenBudgets != null)
            {
                hiddenGenericBudget.PastAmount = hiddenBudgets.Sum(b => b.PastAmount);
                hiddenGenericBudget.PastActivity = hiddenBudgets.Sum(b => b.PastActivity);
                hiddenGenericBudget.Activity = hiddenBudgets.Sum(b => b.Activity);
                hiddenGenericBudget.Amount = hiddenBudgets.Sum(b => b.Amount);
            }

            return hiddenGenericBudget;
        }
    }
}
