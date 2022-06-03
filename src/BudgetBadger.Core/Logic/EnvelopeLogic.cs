using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetBadger.Core.DataAccess;
using BudgetBadger.Core.LocalizedResources;
using BudgetBadger.Core.Utilities;
using BudgetBadger.Models;
using BudgetBadger.Models.Extensions;

namespace BudgetBadger.Core.Logic
{
    public class EnvelopeLogic : IEnvelopeLogic
    {
        readonly IDataAccess _dataAccess;
        readonly IResourceContainer _resourceContainer;

        public EnvelopeLogic(IDataAccess dataAccess,
            IResourceContainer resourceContainer)
        {
            _dataAccess = dataAccess;
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

                    await _dataAccess.CreateBudgetAsync(budgetToUpsert).ConfigureAwait(false);
                    result.Success = true;
                    result.Data = budgetToUpsert;
                }
                else
                {
                    budgetToUpsert.ModifiedDateTime = dateTimeNow;

                    await _dataAccess.UpdateBudgetAsync(budgetToUpsert).ConfigureAwait(false);
                    result.Success = true;
                    result.Data = budgetToUpsert;
                }
            }
            catch (Exception ex)
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
                var envelopes = await _dataAccess.ReadEnvelopesAsync().ConfigureAwait(false);
                var budgets = (await _dataAccess.ReadBudgetsFromScheduleAsync(schedule.Id).ConfigureAwait(false)).ToList();

                foreach (var envelope in envelopes.Where(e => !budgets.Any(b => b.Envelope.Id == e.Id)))
                {
                    budgets.Add(new Budget
                    {
                        Schedule = schedule.DeepCopy(),
                        Envelope = envelope.DeepCopy(),
                        Amount = 0m
                    });
                }

                var populatedSchedule = await GetPopulatedBudgetSchedule(schedule).ConfigureAwait(false);

                var filteredBudgets = budgets.Where(b => FilterBudget(b, FilterType.Standard));
                var tasks = filteredBudgets.Select(b => GetPopulatedBudget(b, populatedSchedule));

                var budgetsToReturn = (await Task.WhenAll(tasks).ConfigureAwait(false)).ToList();

                if (budgets.Any(b => FilterBudget(b, FilterType.Hidden)))
                {
                    var hiddenBudgets = budgets.Where(b => FilterBudget(b, FilterType.Hidden));
                    var hiddenTasks = hiddenBudgets.Select(b => GetPopulatedBudget(b, populatedSchedule));
                    var populatedHiddenBudgets = (await Task.WhenAll(hiddenTasks).ConfigureAwait(false));

                    budgetsToReturn.Add(GetGenericHiddenBudget(populatedSchedule, populatedHiddenBudgets));
                }

                budgetsToReturn.RemoveAll(b => b.Envelope.Group.IsDebt && b.Remaining == 0 && b.Amount == 0);

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
                var envelopes = await _dataAccess.ReadEnvelopesAsync().ConfigureAwait(false);
                var budgets = (await _dataAccess.ReadBudgetsFromScheduleAsync(schedule.Id).ConfigureAwait(false)).ToList();

                foreach (var envelope in envelopes.Where(e => !budgets.Any(b => b.Envelope.Id == e.Id)))
                {
                    budgets.Add(new Budget
                    {
                        Schedule = schedule.DeepCopy(),
                        Envelope = envelope.DeepCopy(),
                        Amount = 0m
                    });
                }

                var populatedSchedule = await GetPopulatedBudgetSchedule(schedule).ConfigureAwait(false);

                var filteredBudgets = budgets.Where(b => FilterBudget(b, FilterType.Selection));
                var tasks = filteredBudgets.Select(b => GetPopulatedBudget(b, populatedSchedule));

                var budgetsToReturn = (await Task.WhenAll(tasks).ConfigureAwait(false)).ToList();

                // debt budget stuff
                var debtBudgets = budgets.Where(b => b.Envelope.Group.IsDebt);
                var populatedDebtBudgetTasks = debtBudgets.Select(b => GetPopulatedBudget(b, populatedSchedule));
                var populatedDebtBudgets = await Task.WhenAll(populatedDebtBudgetTasks).ConfigureAwait(false);
                var genericDebtBudget = GetGenericDebtBudget(populatedSchedule, populatedDebtBudgets);
                budgetsToReturn.Add(genericDebtBudget);

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
                var envelopes = await _dataAccess.ReadEnvelopesAsync().ConfigureAwait(false);
                var budgets = (await _dataAccess.ReadBudgetsFromScheduleAsync(schedule.Id).ConfigureAwait(false)).ToList();

                foreach (var envelope in envelopes.Where(e => !budgets.Any(b => b.Envelope.Id == e.Id)))
                {
                    budgets.Add(new Budget
                    {
                        Schedule = schedule.DeepCopy(),
                        Envelope = envelope.DeepCopy(),
                        Amount = 0m
                    });
                }

                var populatedSchedule = await GetPopulatedBudgetSchedule(schedule).ConfigureAwait(false);

                var filteredBudgets = budgets.Where(b => FilterBudget(b, FilterType.Hidden));
                var tasks = filteredBudgets.Select(b => GetPopulatedBudget(b, populatedSchedule));

                var budgetsToReturn = (await Task.WhenAll(tasks).ConfigureAwait(false)).ToList();

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
                var budgets = await _dataAccess.ReadBudgetsFromEnvelopeAsync(envelopeId);
                var budget = budgets.FirstOrDefault(b => b.Envelope.Id == envelopeId && b.Schedule.Id == schedule.Id);
                if (budget == null)
                {
                    budget = new Budget();
                }

                budget.Envelope = await _dataAccess.ReadEnvelopeAsync(envelopeId);

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
                var budget = await _dataAccess.ReadBudgetAsync(id).ConfigureAwait(false);
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
                var transactions = await _dataAccess.ReadEnvelopeTransactionsAsync(budget.Envelope.Id).ConfigureAwait(false);
                var activeTransactions = transactions.Where(t => t.IsActive);
                var budgets = await _dataAccess.ReadBudgetsFromEnvelopeAsync(budget.Envelope.Id).ConfigureAwait(false);

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
                var count = await _dataAccess.GetEnvelopesCountAsync();
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

        public async Task<Result<Envelope>> GetEnvelopeAsync(Guid id)
        {
            var result = new Result<Envelope>();

            try
            {
                var envelope = await _dataAccess.ReadEnvelopeAsync(id).ConfigureAwait(false);

                var populatedEnvelope = await GetPopulatedEnvelope(envelope).ConfigureAwait(false);

                result.Success = true;
                result.Data = populatedEnvelope;
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
                var envelopes = await _dataAccess.ReadEnvelopesAsync().ConfigureAwait(false);

                var tasks = envelopes.Select(GetPopulatedEnvelope);
                var populatedEnvelopes = await Task.WhenAll(tasks).ConfigureAwait(false);

                var envelopesToReturn = populatedEnvelopes.Where(e => FilterEnvelope(e, FilterType.Selection)).ToList();

                envelopesToReturn.Add(GetGenericDebtEnvelope());

                result.Success = true;
                result.Data = envelopesToReturn;
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
                var envelopes = await _dataAccess.ReadEnvelopesAsync().ConfigureAwait(false);

                var tasks = envelopes.Select(GetPopulatedEnvelope);
                var populatedEnvelopes = await Task.WhenAll(tasks).ConfigureAwait(false);

                var envelopesToReturn = populatedEnvelopes.Where(e => FilterEnvelope(e, FilterType.Report)).ToList();

                if (envelopes.Any(p => FilterEnvelope(p, FilterType.Hidden)))
                {
                    envelopesToReturn.Add(GetGenericHiddenEnvelope());
                }

                result.Success = true;
                result.Data = envelopesToReturn;
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
                var envelopes = await _dataAccess.ReadEnvelopesAsync().ConfigureAwait(false);

                var tasks = envelopes.Select(GetPopulatedEnvelope);

                var populatedEnvelopes= (await Task.WhenAll(tasks).ConfigureAwait(false)).ToList();

                var hiddenEnvelopes = populatedEnvelopes.Where(e => FilterEnvelope(e, FilterType.Hidden));

                result.Success = true;
                result.Data = hiddenEnvelopes.ToList();
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
                var envelope = await _dataAccess.ReadEnvelopeAsync(id).ConfigureAwait(false);

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

                var envelopeTransactions = await _dataAccess.ReadEnvelopeTransactionsAsync(id).ConfigureAwait(false);
                if (envelopeTransactions.Any(t => t.IsActive))
                {
                    errors.Add(_resourceContainer.GetResourceString("EnvelopeDeleteActiveTransactionsError"));
                }

                var envelopeBudgets = await _dataAccess.ReadBudgetsFromEnvelopeAsync(id).ConfigureAwait(false);
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

                await _dataAccess.UpdateEnvelopeAsync(envelope);

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
                var envelope = await _dataAccess.ReadEnvelopeAsync(id).ConfigureAwait(false);

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

                await _dataAccess.UpdateEnvelopeAsync(envelope);

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
                var envelope = await _dataAccess.ReadEnvelopeAsync(id).ConfigureAwait(false);

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

                await _dataAccess.UpdateEnvelopeAsync(envelope);

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

                await _dataAccess.CreateEnvelopeGroupAsync(groupToUpsert).ConfigureAwait(false);
                result.Success = true;
                result.Data = groupToUpsert;
            }
            else
            {
                groupToUpsert.ModifiedDateTime = dateTimeNow;

                await _dataAccess.UpdateEnvelopeGroupAsync(groupToUpsert).ConfigureAwait(false);
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
                var count = await _dataAccess.GetEnvelopeGroupsCountAsync();
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

        public async Task<Result<EnvelopeGroup>> GetEnvelopeGroupAsync(Guid id)
        {
            var result = new Result<EnvelopeGroup>();

            try
            {
                var envelopeGroup = await _dataAccess.ReadEnvelopeGroupAsync(id).ConfigureAwait(false);

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

        public async Task<Result<IReadOnlyList<EnvelopeGroup>>> GetEnvelopeGroupsAsync()
        {
            var result = new Result<IReadOnlyList<EnvelopeGroup>>();

            try
            {
                var envelopeGroups = await _dataAccess.ReadEnvelopeGroupsAsync().ConfigureAwait(false);
                var filteredEnvelopeGroups = envelopeGroups.Where(e => FilterEnvelopeGroup(e, FilterType.Standard)).ToList();
                filteredEnvelopeGroups.ForEach(e => e.TranslateEnvelopeGroup(_resourceContainer));

                if (envelopeGroups.Any(eg => FilterEnvelopeGroup(eg, FilterType.Hidden)))
                {
                    filteredEnvelopeGroups.Add(GetGenericHiddenEnvelopeGroup());
                }

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
                var envelopeGroups = await _dataAccess.ReadEnvelopeGroupsAsync().ConfigureAwait(false);
                var filteredEnvelopeGroups = envelopeGroups.Where(e => FilterEnvelopeGroup(e, FilterType.Selection)).ToList();
                filteredEnvelopeGroups.ForEach(e => e.TranslateEnvelopeGroup(_resourceContainer));
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
                var envelopeGroups = await _dataAccess.ReadEnvelopeGroupsAsync().ConfigureAwait(false);
                var filteredEnvelopeGroups = envelopeGroups.Where(e => FilterEnvelopeGroup(e, FilterType.Hidden)).ToList();
                filteredEnvelopeGroups.ForEach(e => e.TranslateEnvelopeGroup(_resourceContainer));
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
                var envelopeGroup = await _dataAccess.ReadEnvelopeGroupAsync(id).ConfigureAwait(false);

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

                var envelopes = await _dataAccess.ReadEnvelopesAsync().ConfigureAwait(false);
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

                await _dataAccess.UpdateEnvelopeGroupAsync(envelopeGroup);

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
                var envelopeGroup = await _dataAccess.ReadEnvelopeGroupAsync(id).ConfigureAwait(false);

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

                var envelopes = await _dataAccess.ReadEnvelopesAsync().ConfigureAwait(false);
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

                await _dataAccess.UpdateEnvelopeGroupAsync(envelopeGroup);

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
                var envelopeGroup = await _dataAccess.ReadEnvelopeGroupAsync(id).ConfigureAwait(false);

                // check for validation to delete
                var errors = new List<string>();

                if (envelopeGroup.IsNew || envelopeGroup.IsActive || envelopeGroup.IsDeleted)
                {
                    errors.Add(_resourceContainer.GetResourceString("EnvelopeGroupUnhideNotHiddenError"));
                }

                if (envelopeGroup.IsIncome || envelopeGroup.IsDebt || envelopeGroup.IsSystem || envelopeGroup.IsGenericHiddenEnvelopeGroup)
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

                await _dataAccess.UpdateEnvelopeGroupAsync(envelopeGroup);

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

        public bool FilterEnvelopeGroup(EnvelopeGroup envelopeGroup, FilterType filterType)
        {
            switch (filterType)
            {
                case FilterType.Standard:
                    return envelopeGroup.IsActive && !envelopeGroup.IsSystem && !envelopeGroup.IsIncome && !envelopeGroup.IsDebt;
                case FilterType.Report:
                case FilterType.Selection:
                    return envelopeGroup.IsActive && !envelopeGroup.IsSystem && !envelopeGroup.IsIncome && !envelopeGroup.IsDebt;
                case FilterType.Hidden:
                    return envelopeGroup.IsHidden && !envelopeGroup.IsDeleted && !envelopeGroup.IsSystem && !envelopeGroup.IsIncome && !envelopeGroup.IsDebt && !envelopeGroup.IsGenericHiddenEnvelopeGroup;
                case FilterType.All:
                default:
                    return true;
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

        public bool FilterEnvelope(Envelope envelope, FilterType filterType)
        {
            switch (filterType)
            {
                case FilterType.Standard:
                case FilterType.Report:
                    return envelope.IsActive
                        && !envelope.IsSystem
                        && !envelope.IsGenericDebtEnvelope
                        && !envelope.Group.IsIncome
                        && !envelope.Group.IsSystem;
                case FilterType.Selection:
                    return envelope.IsActive
                        && !envelope.IsSystem
                        && envelope.IsActive
                        && !envelope.Group.IsDebt;
                case FilterType.Hidden:
                    return !envelope.Group.IsIncome &&
                    !envelope.Group.IsDebt &&
                    !envelope.Group.IsSystem &&
                    envelope.IsHidden &&
                    !envelope.IsDeleted;
                case FilterType.All:
                default:
                    return true;
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
                return FilterEnvelope(budget.Envelope, searchText);
            }
            else
            {
                return false;
            }
        }

        public bool FilterBudget(Budget budget, FilterType filterType)
        {
            return FilterEnvelope(budget.Envelope, filterType);
        }

        protected Budget PopulateBudget(Budget budget,
                                        IEnumerable<Transaction> envelopeTransactions,
                                        IEnumerable<Budget> envelopeBudgets)
        {
            budget.Envelope = PopulateEnvelope(budget.Envelope);

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

            budget.Envelope.TranslateEnvelope(_resourceContainer);

            return budget;
        }

        protected Envelope PopulateEnvelope(Envelope envelope)
        {
            if (envelope.IsIncome)
            {
                envelope = GetIncomeEnvelope();
            }
            else if (envelope.IsBuffer)
            {
                envelope = GetBufferEnvelope();
            }
            else if (envelope.IsSystem)
            {
                envelope = GetIgnoredEnvelope();
            }
            else if (envelope.IsGenericDebtEnvelope)
            {
                envelope = GetGenericDebtEnvelope();
            }
            else if (envelope.IsGenericHiddenEnvelope)
            {
                envelope = GetGenericHiddenEnvelope();
            }
            else if (envelope.Group.IsDebt)
            {
                envelope.Group = GetDebtEnvelopeGroup();
            }

            envelope.TranslateEnvelope(_resourceContainer);

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
                var existingBudget = await _dataAccess.ReadBudgetFromScheduleAndEnvelopeAsync(budget.Schedule.Id, budget.Envelope.Id);
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

                await _dataAccess.CreateEnvelopeAsync(envelopeToUpsert).ConfigureAwait(false);
                result.Success = true;
                result.Data = envelopeToUpsert;
            }
            else
            {
                envelopeToUpsert.ModifiedDateTime = dateTimeNow;

                await _dataAccess.UpdateEnvelopeAsync(envelopeToUpsert).ConfigureAwait(false);
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

                await _dataAccess.CreateBudgetScheduleAsync(scheduleToUpsert).ConfigureAwait(false);
                result.Success = true;
                result.Data = scheduleToUpsert;
            }
            else
            {
                scheduleToUpsert.ModifiedDateTime = DateTime.Now;

                await _dataAccess.UpdateBudgetScheduleAsync(scheduleToUpsert).ConfigureAwait(false);
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

            var transactions = await _dataAccess.ReadEnvelopeTransactionsAsync(budgetToPopulate.Envelope.Id).ConfigureAwait(false);
            var budgets = await _dataAccess.ReadBudgetsFromEnvelopeAsync(budgetToPopulate.Envelope.Id).ConfigureAwait(false);

            return await Task.Run(() => PopulateBudget(budgetToPopulate, transactions, budgets));
        }

        async Task<Envelope> GetPopulatedEnvelope(Envelope envelope)
        {
            var envelopeToPopulate = envelope.DeepCopy();

            return await Task.Run(() => PopulateEnvelope(envelopeToPopulate));
        }

        async Task<BudgetSchedule> GetPopulatedBudgetSchedule(BudgetSchedule budgetSchedule)
        {
            var allAccounts = await _dataAccess.ReadAccountsAsync().ConfigureAwait(false);
            var allTransactions = await _dataAccess.ReadTransactionsAsync().ConfigureAwait(false);
            var envelopes = await _dataAccess.ReadEnvelopesAsync().ConfigureAwait(false);
            var budgets = await _dataAccess.ReadBudgetsAsync().ConfigureAwait(false);
            var newBudgetSchedule = budgetSchedule.DeepCopy();
            // get existing schedule from data access if exists
            var schedule = await _dataAccess.ReadBudgetScheduleAsync(budgetSchedule.Id).ConfigureAwait(false);
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
            debtEnvelopeGroup.TranslateEnvelopeGroup(_resourceContainer);
            return debtEnvelopeGroup;
        }

        Envelope GetGenericDebtEnvelope()
        {
            var genericDebtEnvelope = Constants.GenericDebtEnvelope.DeepCopy();
            genericDebtEnvelope.Group = GetDebtEnvelopeGroup();
            genericDebtEnvelope.TranslateEnvelope(_resourceContainer);
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

            incomeEnvelopeGroup.TranslateEnvelopeGroup(_resourceContainer);

            return incomeEnvelopeGroup;
        }

        Envelope GetIncomeEnvelope()
        {
            var incomeEnvelope = Constants.IncomeEnvelope.DeepCopy();
            incomeEnvelope.Group = GetIncomeEnvelopeGroup();
            incomeEnvelope.TranslateEnvelope(_resourceContainer);
            return incomeEnvelope;
        }

        Envelope GetBufferEnvelope()
        {
            var bufferEnvelope = Constants.BufferEnvelope.DeepCopy();
            bufferEnvelope.Group = GetIncomeEnvelopeGroup();
            bufferEnvelope.TranslateEnvelope(_resourceContainer);
            return bufferEnvelope;
        }

        EnvelopeGroup GetSystemEnvelopeGroup()
        {
            var systemEnvelopeGroup = Constants.SystemEnvelopeGroup.DeepCopy();
            systemEnvelopeGroup.TranslateEnvelopeGroup(_resourceContainer);

            return systemEnvelopeGroup;
        }

        Envelope GetIgnoredEnvelope()
        {
            var ignoredEnvelope = Constants.IgnoredEnvelope.DeepCopy();
            ignoredEnvelope.Group = GetSystemEnvelopeGroup();
            ignoredEnvelope.TranslateEnvelope(_resourceContainer);
            return ignoredEnvelope;
        }

        EnvelopeGroup GetGenericHiddenEnvelopeGroup()
        {
            var genericHiddenEnvelopeGroup = Constants.GenericHiddenEnvelopeGroup.DeepCopy();
            genericHiddenEnvelopeGroup.TranslateEnvelopeGroup(_resourceContainer);
            return genericHiddenEnvelopeGroup;
        }

        Envelope GetGenericHiddenEnvelope()
        {
            var genericHiddenEnvelope = Constants.GenericHiddenEnvelope.DeepCopy();
            genericHiddenEnvelope.Group = GetGenericHiddenEnvelopeGroup();
            genericHiddenEnvelope.TranslateEnvelope(_resourceContainer);
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
