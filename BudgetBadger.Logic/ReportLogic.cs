using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetBadger.Core.DataAccess;
using BudgetBadger.Core.Logic;
using BudgetBadger.Models;
using BudgetBadger.Models.Extensions;

namespace BudgetBadger.Logic
{
    public class ReportLogic : IReportLogic
    {
        readonly ITransactionDataAccess _transactionDataAccess;
        readonly ITransactionLogic _transactionLogic;
        readonly IAccountDataAccess _accountDataAccess;
        readonly IPayeeDataAccess _payeeDataAccess;
        readonly IEnvelopeDataAccess _envelopeDataAccess;

        public ReportLogic(ITransactionDataAccess transactionDataAccess,
                           IAccountDataAccess accountDataAccess,
                           IPayeeDataAccess payeeDataAccess,
                           IEnvelopeDataAccess envelopeDataAccess,
                           ITransactionLogic transactionLogic)
        {
            _transactionDataAccess = transactionDataAccess;
            _transactionLogic = transactionLogic;
            _accountDataAccess = accountDataAccess;
            _payeeDataAccess = payeeDataAccess;
            _envelopeDataAccess = envelopeDataAccess;
        }

        public async Task<Result<IReadOnlyList<DataPoint<DateTime, decimal>>>> GetNetWorthReport(DateTime? beginDate, DateTime? endDate)
        {
            var result = new Result<IReadOnlyList<DataPoint<DateTime, decimal>>>();
            var dataPoints = new List<DataPoint<DateTime, decimal>>();

            try
            {
                var transactions = await _transactionLogic.GetTransactionsAsync();
                var activeTransactions = transactions.Data.Where(t => t.IsActive && !t.IsTransfer);

                var earliestMonth = activeTransactions.Min(t => t.ServiceDate);
                var latestMonth = activeTransactions.Max(t => t.ServiceDate);

                var startMonth = new DateTime(earliestMonth.Year, earliestMonth.Month, 1).AddMonths(1).AddTicks(-1);
                var endMonth = new DateTime(latestMonth.Year, latestMonth.Month, 1).AddMonths(1).AddTicks(-1);

                while (startMonth <= endMonth)
                {
                    var monthTransactions = activeTransactions.Where(t => t.ServiceDate <= startMonth);
                    var monthTotal = monthTransactions.Sum(t => t.Amount ?? 0);
                    dataPoints.Add(new DataPoint<DateTime, decimal>
                    {
                        XLabel = startMonth.ToString("Y"),
                        XValue = startMonth,
                        YLabel = monthTotal.ToString("C"),
                        YValue = monthTotal
                    });
                    startMonth = startMonth.AddMonths(1);
                }

                if (beginDate.HasValue)
                {
                    var beginDateMonth = new DateTime(beginDate.Value.Year, beginDate.Value.Month, 1).AddMonths(1).AddTicks(-1);
                    dataPoints = dataPoints.Where(d => d.XValue >= beginDateMonth).ToList();
                }
                if (endDate.HasValue)
                {
                    var endDateMonth = new DateTime(endDate.Value.Year, endDate.Value.Month, 1).AddMonths(1).AddTicks(-1);
                    dataPoints = dataPoints.Where(d => d.XValue <= endDateMonth).ToList();
                }

                result.Data = dataPoints.OrderBy(d => d.XValue).ToList();
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        public async Task<Result<IReadOnlyDictionary<string, decimal>>> GetEnvelopeSpendingTotalsReport(DateTime? beginDate, DateTime? endDate)
        {
            var result = new Result<IReadOnlyDictionary<string, decimal>>();
            var dataPoints = new Dictionary<string, decimal>();

            try
            {
                var transactions = await _transactionDataAccess.ReadTransactionsAsync();
                var activeTransactions = transactions.Where(t => t.IsActive);

                if (beginDate.HasValue)
                {
                    activeTransactions = activeTransactions.Where(t => t.ServiceDate >= beginDate);
                }
                if (endDate.HasValue)
                {
                    activeTransactions = activeTransactions.Where(t => t.ServiceDate <= endDate);
                }

                var envelopes = await _envelopeDataAccess.ReadEnvelopesAsync();
                var activeEnvelopes = envelopes.Where(e =>
                                                      e.IsActive
                                                      && !e.IsSystem
                                                      && !e.Group.IsIncome
                                                      && !e.Group.IsSystem
                                                      && !e.Group.IsDebt);

                foreach (var envelope in activeEnvelopes)
                {
                    var envelopeTransactions = activeTransactions.Where(t => t.Envelope.Id == envelope.Id);
                    var envelopeTransactionsSum = envelopeTransactions.Sum(t => t.Outflow ?? 0);
                    dataPoints.Add(envelope.Description, envelopeTransactionsSum);
                }

                result.Data = dataPoints;
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        public async Task<Result<IReadOnlyDictionary<string, decimal>>> GetPayeeSpendingTotalsReport(DateTime? beginDate, DateTime? endDate)
        {
            var result = new Result<IReadOnlyDictionary<string, decimal>>();
            var dataPoints = new Dictionary<string, decimal>();

            try
            {
                var transactions = await _transactionDataAccess.ReadTransactionsAsync();
                var activeTransactions = transactions.Where(t => t.IsActive);

                if (beginDate.HasValue)
                {
                    activeTransactions = activeTransactions.Where(t => t.ServiceDate >= beginDate);
                }
                if (endDate.HasValue)
                {
                    activeTransactions = activeTransactions.Where(t => t.ServiceDate <= endDate);
                }

                var payees = await _payeeDataAccess.ReadPayeesAsync();
                var activePayees = payees.Where(p => p.IsActive);

                foreach (var payee in activePayees)
                {
                    var accountPayee = await _accountDataAccess.ReadAccountAsync(payee.Id);

                    if (!accountPayee.IsActive)
                    {
                        var payeeTransactions = activeTransactions.Where(t => t.Payee.Id == payee.Id);
                        var payeeTransactionsSum = payeeTransactions.Sum(t => t.Outflow ?? 0);
                        dataPoints.Add(payee.Description, payeeTransactionsSum);
                    }
                }

                result.Data = dataPoints;
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        public async Task<Result<IReadOnlyDictionary<DateTime, decimal>>> GetSpendingTrendsByEnvelopeReport(Guid envelopeId, DateTime? beginDate, DateTime? endDate)
        {
            var result = new Result<IReadOnlyDictionary<DateTime, decimal>>();
            var dataPoints = new Dictionary<DateTime, decimal>();

            try
            {
                var transactions = await _transactionDataAccess.ReadEnvelopeTransactionsAsync(envelopeId);
                var activeTransactions = transactions.Where(t => t.IsActive);

                if (beginDate.HasValue)
                {
                    activeTransactions = activeTransactions.Where(t => t.ServiceDate >= beginDate);
                }
                if (endDate.HasValue)
                {
                    activeTransactions = activeTransactions.Where(t => t.ServiceDate <= endDate);
                }

                var months = activeTransactions.Select(d => new DateTime(d.ServiceDate.Year, d.ServiceDate.Month, 1)).Distinct();

                var earliestMonth = activeTransactions.Min(t => t.ServiceDate);
                var latestMonth = activeTransactions.Max(t => t.ServiceDate);

                var startMonth = new DateTime(earliestMonth.Year, earliestMonth.Month, 1).AddMonths(1).AddTicks(-1);
                var endMonth = new DateTime(latestMonth.Year, latestMonth.Month, 1).AddMonths(1).AddTicks(-1);

                while (startMonth <= endMonth)
                {
                    var monthTransactions = activeTransactions.Where(t => t.ServiceDate <= startMonth);
                    var monthTotal = monthTransactions.Sum(t => t.Amount ?? 0);
                    dataPoints.Add(startMonth, monthTotal);
                    startMonth = startMonth.AddMonths(1);
                }

                result.Data = dataPoints;
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        public async Task<Result<IReadOnlyDictionary<DateTime, decimal>>> GetSpendingTrendsByPayeeReport(Guid payeeId, DateTime? beginDate, DateTime? endDate)
        {
            var result = new Result<IReadOnlyDictionary<DateTime, decimal>>();
            var dataPoints = new Dictionary<DateTime, decimal>();

            try
            {
                var transactions = await _transactionDataAccess.ReadPayeeTransactionsAsync(payeeId);
                var activeTransactions = transactions.Where(t => t.IsActive);

                if (beginDate.HasValue)
                {
                    activeTransactions = activeTransactions.Where(t => t.ServiceDate >= beginDate);
                }
                if (endDate.HasValue)
                {
                    activeTransactions = activeTransactions.Where(t => t.ServiceDate <= endDate);
                }

                var months = activeTransactions.Select(d => new DateTime(d.ServiceDate.Year, d.ServiceDate.Month, 1)).Distinct();

                var earliestMonth = activeTransactions.Min(t => t.ServiceDate);
                var latestMonth = activeTransactions.Max(t => t.ServiceDate);

                var startMonth = new DateTime(earliestMonth.Year, earliestMonth.Month, 1).AddMonths(1).AddTicks(-1);
                var endMonth = new DateTime(latestMonth.Year, latestMonth.Month, 1).AddMonths(1).AddTicks(-1);

                while (startMonth <= endMonth)
                {
                    var monthTransactions = activeTransactions.Where(t => t.ServiceDate <= startMonth);
                    var monthTotal = monthTransactions.Sum(t => t.Amount ?? 0);
                    dataPoints.Add(startMonth, monthTotal);
                    startMonth = startMonth.AddMonths(1);
                }

                result.Data = dataPoints;
                result.Success = true;
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
