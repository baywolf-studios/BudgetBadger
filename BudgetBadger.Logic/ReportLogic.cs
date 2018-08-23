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
        readonly ITransactionLogic _transactionLogic;
        readonly IAccountLogic _accountLogic;
        readonly IPayeeLogic _payeeLogic;
        readonly IEnvelopeLogic _envelopeLogic;

        public ReportLogic(ITransactionLogic transactionLogic,
                           IAccountLogic accountLogic,
                           IPayeeLogic payeeLogic,
                           IEnvelopeLogic envelopeLogic)
        {
            _transactionLogic = transactionLogic;
            _accountLogic = accountLogic;
            _payeeLogic = payeeLogic;
            _envelopeLogic = envelopeLogic;
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

        public async Task<Result<IReadOnlyList<DataPoint<Envelope, decimal>>>> GetEnvelopesSpendingReport(DateTime? beginDate, DateTime? endDate)
        {
            var result = new Result<IReadOnlyList<DataPoint<Envelope, decimal>>>();
            var dataPoints = new List<DataPoint<Envelope, decimal>>();

            try
            {
                var envelopeResult = await _envelopeLogic.GetEnvelopesForSelectionAsync();
                var activeEnvelopes = envelopeResult.Data.Where(e =>
                                                                e.IsActive
                                                                && !e.IsSystem
                                                                && !e.Group.IsIncome
                                                                && !e.Group.IsSystem
                                                                && !e.Group.IsDebt);
                
                foreach (var envelope in activeEnvelopes)
                {
                    var envelopeTransactions = await _transactionLogic.GetEnvelopeTransactionsAsync(envelope);
                    var activeEnvelopeTransactions = envelopeTransactions.Data.Where(t => t.IsActive);
                    if (beginDate.HasValue)
                    {
                        activeEnvelopeTransactions = activeEnvelopeTransactions.Where(t => t.ServiceDate >= beginDate);
                    }
                    if (endDate.HasValue)
                    {
                        activeEnvelopeTransactions = activeEnvelopeTransactions.Where(t => t.ServiceDate <= endDate);
                    }
                    var envelopeTransactionsSum = activeEnvelopeTransactions.Sum(t => t.Amount ?? 0);
                    dataPoints.Add(new DataPoint<Envelope, decimal>
                    {
                        XValue = envelope,
                        XLabel = envelope.Group.Description + " " + envelope.Description,
                        YValue = envelopeTransactionsSum,
                        YLabel = envelopeTransactionsSum.ToString("C")
                    });
                }

                result.Data = dataPoints.OrderBy(d => d.YValue).ToList();
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        public async Task<Result<IReadOnlyList<DataPoint<Payee, decimal>>>> GetPayeesSpendingReport(DateTime? beginDate, DateTime? endDate)
        {
            var result = new Result<IReadOnlyList<DataPoint<Payee, decimal>>>();
            var dataPoints = new List<DataPoint<Payee, decimal>>();

            try
            {
                var payeeResult = await _payeeLogic.GetPayeesAsync();
                var activePayees = payeeResult.Data.Where(e => e.IsActive && !e.IsAccount);

                foreach (var payee in activePayees)
                {
                    var payeeTransactions = await _transactionLogic.GetPayeeTransactionsAsync(payee);
                    var activePayeeTransactions = payeeTransactions.Data.Where(t => t.IsActive);

                    if (beginDate.HasValue)
                    {
                        activePayeeTransactions = activePayeeTransactions.Where(t => t.ServiceDate >= beginDate);
                    }
                    if (endDate.HasValue)
                    {
                        activePayeeTransactions = activePayeeTransactions.Where(t => t.ServiceDate <= endDate);
                    }
                    var payeeTransactionsSum = activePayeeTransactions.Sum(t => t.Amount ?? 0);
                    dataPoints.Add(new DataPoint<Payee, decimal>
                    {
                        XValue = payee,
                        XLabel = payee.Description,
                        YValue = payeeTransactionsSum,
                        YLabel = payeeTransactionsSum.ToString("C")
                    });
                }

                result.Data = dataPoints.OrderBy(d => d.YValue).ToList();
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        public async Task<Result<IReadOnlyList<DataPoint<DateTime, decimal>>>> GetEnvelopeTrendsReport(Guid envelopeId, DateTime? beginDate, DateTime? endDate)
        {
            var result = new Result<IReadOnlyList<DataPoint<DateTime, decimal>>>();
            var dataPoints = new List<DataPoint<DateTime, decimal>>();

            try
            {
                var transactions = await _transactionLogic.GetEnvelopeTransactionsAsync(new Envelope { Id = envelopeId });
                var activeTransactions = transactions.Data.Where(t => t.IsActive && !t.IsTransfer); //maybe not need the transfer portion?

                var earliestMonth = activeTransactions.Min(t => t.ServiceDate);
                var latestMonth = activeTransactions.Max(t => t.ServiceDate);

                var startMonth = new DateTime(earliestMonth.Year, earliestMonth.Month, 1).AddMonths(1).AddTicks(-1);
                var endMonth = new DateTime(latestMonth.Year, latestMonth.Month, 1).AddMonths(1).AddTicks(-1);

                while (startMonth <= endMonth)
                {
                    var monthTransactions = activeTransactions.Where(t => t.ServiceDate <= startMonth && t.ServiceDate > startMonth.AddMonths(-1));
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

        public async Task<Result<IReadOnlyList<DataPoint<DateTime, decimal>>>> GetPayeeTrendsReport(Guid payeeId, DateTime? beginDate, DateTime? endDate)
        {
            var result = new Result<IReadOnlyList<DataPoint<DateTime, decimal>>>();
            var dataPoints = new List<DataPoint<DateTime, decimal>>();

            try
            {
                var transactions = await _transactionLogic.GetPayeeTransactionsAsync(new Payee { Id = payeeId });
                var activeTransactions = transactions.Data.Where(t => t.IsActive && !t.IsTransfer); //maybe not need the transfer portion?

                var earliestMonth = activeTransactions.Min(t => t.ServiceDate);
                var latestMonth = activeTransactions.Max(t => t.ServiceDate);

                var startMonth = new DateTime(earliestMonth.Year, earliestMonth.Month, 1).AddMonths(1).AddTicks(-1);
                var endMonth = new DateTime(latestMonth.Year, latestMonth.Month, 1).AddMonths(1).AddTicks(-1);

                while (startMonth <= endMonth)
                {
                    var monthTransactions = activeTransactions.Where(t => t.ServiceDate <= startMonth && t.ServiceDate > startMonth.AddMonths(-1));
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
    }
}
