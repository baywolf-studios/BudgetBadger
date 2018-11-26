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

        public async Task<Result<IReadOnlyList<DataPoint<DateTime, decimal>>>> GetNetWorthReport(DateTime beginDate, DateTime endDate)
        {
            var result = new Result<IReadOnlyList<DataPoint<DateTime, decimal>>>();
            var dataPoints = new List<DataPoint<DateTime, decimal>>();

            try
            {
                var transactions = await Task.Run(() => _transactionLogic.GetTransactionsAsync());
                var activeTransactions = transactions.Data.Where(t => t.IsActive && !t.IsTransfer);

                var startMonth = new DateTime(beginDate.Year, beginDate.Month, 1).AddMonths(1).AddTicks(-1);
                var endMonth = new DateTime(endDate.Year, endDate.Month, 1).AddMonths(1).AddTicks(-1);

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

                var beginDateMonth = new DateTime(beginDate.Year, beginDate.Month, 1).AddMonths(1).AddTicks(-1);
                var endDateMonth = new DateTime(endDate.Year, endDate.Month, 1).AddMonths(1).AddTicks(-1);
                dataPoints = dataPoints.Where(d => d.XValue >= beginDateMonth && d.XValue <= endDateMonth).ToList();

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

        public async Task<Result<IReadOnlyList<DataPoint<Envelope, decimal>>>> GetEnvelopesSpendingReport(DateTime beginDate, DateTime endDate)
        {
            var result = new Result<IReadOnlyList<DataPoint<Envelope, decimal>>>();
            var dataPoints = new List<DataPoint<Envelope, decimal>>();

            try
            {
                var envelopeResult = await Task.Run(() => _envelopeLogic.GetEnvelopesForReportAsync());

                if (envelopeResult.Success)
                {
                    foreach (var envelope in envelopeResult.Data)
                    {
                        var envelopeTransactions = await Task.Run(() => _transactionLogic.GetEnvelopeTransactionsAsync(envelope));
                        var activeEnvelopeTransactions = envelopeTransactions.Data.Where(t => t.IsActive);
                        activeEnvelopeTransactions = activeEnvelopeTransactions.Where(t => t.ServiceDate >= beginDate && t.ServiceDate <= endDate);

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
                else
                {
                    result.Success = false;
                    result.Message = "Could not retrieve Envelopes at this time. Please try again.";
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        public async Task<Result<IReadOnlyList<DataPoint<Payee, decimal>>>> GetPayeesSpendingReport(DateTime beginDate, DateTime endDate)
        {
            var result = new Result<IReadOnlyList<DataPoint<Payee, decimal>>>();
            var dataPoints = new List<DataPoint<Payee, decimal>>();

            try
            {
                var payeeResult = await Task.Run(() => _payeeLogic.GetPayeesForReportAsync());

                if (payeeResult.Success)
                {
                    foreach (var payee in payeeResult.Data)
                    {
                        var payeeTransactions = await Task.Run(() => _transactionLogic.GetPayeeTransactionsAsync(payee));
                        var activePayeeTransactions = payeeTransactions.Data.Where(t => t.IsActive);

                        activePayeeTransactions = activePayeeTransactions.Where(t => t.ServiceDate >= beginDate && t.ServiceDate <= endDate);

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
                else
                {
                    result.Success = false;
                    result.Message = "Could not retrieve Payees at this time. Please try again.";
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        public async Task<Result<IReadOnlyList<DataPoint<DateTime, decimal>>>> GetEnvelopeTrendsReport(Guid envelopeId, DateTime beginDate, DateTime endDate)
        {
            var result = new Result<IReadOnlyList<DataPoint<DateTime, decimal>>>();
            var dataPoints = new List<DataPoint<DateTime, decimal>>();

            try
            {
                var transactions = await Task.Run(() => _transactionLogic.GetEnvelopeTransactionsAsync(new Envelope { Id = envelopeId }));
                var activeTransactions = transactions.Data.Where(t => t.IsActive && !t.IsTransfer); //maybe not need the transfer portion?

                var startMonth = new DateTime(beginDate.Year, beginDate.Month, 1).AddMonths(1).AddTicks(-1);
                var endMonth = new DateTime(endDate.Year, endDate.Month, 1).AddMonths(1).AddTicks(-1);

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

                var beginDateMonth = new DateTime(beginDate.Year, beginDate.Month, 1).AddMonths(1).AddTicks(-1);
                var endDateMonth = new DateTime(endDate.Year, endDate.Month, 1).AddMonths(1).AddTicks(-1);
                dataPoints = dataPoints.Where(d => d.XValue >= beginDateMonth && d.XValue <= endDateMonth).ToList();

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

        public async Task<Result<IReadOnlyList<DataPoint<DateTime, decimal>>>> GetPayeeTrendsReport(Guid payeeId, DateTime beginDate, DateTime endDate)
        {
            var result = new Result<IReadOnlyList<DataPoint<DateTime, decimal>>>();
            var dataPoints = new List<DataPoint<DateTime, decimal>>();

            try
            {
                var transactions = await Task.Run(() => _transactionLogic.GetPayeeTransactionsAsync(new Payee { Id = payeeId }));
                var activeTransactions = transactions.Data.Where(t => t.IsActive && !t.IsTransfer); //maybe not need the transfer portion?

                var startMonth = new DateTime(beginDate.Year, beginDate.Month, 1).AddMonths(1).AddTicks(-1);
                var endMonth = new DateTime(endDate.Year, endDate.Month, 1).AddMonths(1).AddTicks(-1);

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

                var beginDateMonth = new DateTime(beginDate.Year, beginDate.Month, 1).AddMonths(1).AddTicks(-1);
                var endDateMonth = new DateTime(endDate.Year, endDate.Month, 1).AddMonths(1).AddTicks(-1);
                dataPoints = dataPoints.Where(d => d.XValue >= beginDateMonth && d.XValue <= endDateMonth).ToList();


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
