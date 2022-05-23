using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetBadger.Core.LocalizedResources;
using BudgetBadger.Models;

namespace BudgetBadger.Core.Logic
{
    public class ReportLogic : IReportLogic
    {
        readonly ITransactionLogic _transactionLogic;
        readonly IAccountLogic _accountLogic;
        readonly IPayeeLogic _payeeLogic;
        readonly IEnvelopeLogic _envelopeLogic;
        readonly IResourceContainer _resourceContainer;

        public ReportLogic(ITransactionLogic transactionLogic,
                           IAccountLogic accountLogic,
                           IPayeeLogic payeeLogic,
                           IEnvelopeLogic envelopeLogic,
                           IResourceContainer resourceContainer)
        {
            _transactionLogic = transactionLogic;
            _accountLogic = accountLogic;
            _payeeLogic = payeeLogic;
            _envelopeLogic = envelopeLogic;
            _resourceContainer = resourceContainer;
        }

        public async Task<Result<IReadOnlyList<DataPoint<DateTime, decimal>>>> GetNetWorthReport(DateTime beginDate, DateTime endDate)
        {
            var result = new Result<IReadOnlyList<DataPoint<DateTime, decimal>>>();
            var dataPoints = new List<DataPoint<DateTime, decimal>>();

            try
            {
                var transactions = await _transactionLogic.GetTransactionsAsync().ConfigureAwait(false);
                var activeTransactions = transactions.Data.Where(t => t.IsActive && !t.IsTransfer);

                var startMonth = new DateTime(beginDate.Year, beginDate.Month, 1).AddMonths(1).AddTicks(-1);
                var endMonth = new DateTime(endDate.Year, endDate.Month, 1).AddMonths(1).AddTicks(-1);

                while (startMonth <= endMonth)
                {
                    var monthTransactions = activeTransactions.Where(t => t.ServiceDate <= startMonth);
                    var monthTotal = monthTransactions.Sum(t => t.Amount ?? 0);
                    dataPoints.Add(new DataPoint<DateTime, decimal>
                    {
                        XLabel = _resourceContainer.GetFormattedString("{0:Y}", startMonth),
                        XValue = startMonth,
                        YLabel = _resourceContainer.GetFormattedString("{0:C}", monthTotal),
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
                var envelopeResult = await _envelopeLogic.GetEnvelopesForReportAsync().ConfigureAwait(false);

                if (envelopeResult.Success)
                {
                    foreach (var envelope in envelopeResult.Data)
                    {
                        var envelopeTransactions = await _transactionLogic.GetEnvelopeTransactionsAsync(envelope).ConfigureAwait(false);
                        var activeEnvelopeTransactions = envelopeTransactions.Data.Where(t => t.IsActive);
                        activeEnvelopeTransactions = activeEnvelopeTransactions.Where(t => t.ServiceDate >= beginDate && t.ServiceDate <= endDate);

                        var envelopeTransactionsSum = activeEnvelopeTransactions.Sum(t => t.Amount ?? 0);
                        dataPoints.Add(new DataPoint<Envelope, decimal>
                        {
                            XValue = envelope,
                            XLabel = envelope.Group.Description + " " + envelope.Description,
                            YValue = envelopeTransactionsSum,
                            YLabel = _resourceContainer.GetFormattedString("{0:C}", envelopeTransactionsSum)
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
                var payeeResult = await _payeeLogic.GetPayeesForReportAsync().ConfigureAwait(false);

                if (payeeResult.Success)
                {
                    foreach (var payee in payeeResult.Data)
                    {
                        var payeeTransactions = await _transactionLogic.GetPayeeTransactionsAsync(payee).ConfigureAwait(false);
                        var activePayeeTransactions = payeeTransactions.Data.Where(t => t.IsActive);

                        activePayeeTransactions = activePayeeTransactions.Where(t => t.ServiceDate >= beginDate && t.ServiceDate <= endDate);

                        var payeeTransactionsSum = activePayeeTransactions.Sum(t => t.Amount ?? 0);
                        dataPoints.Add(new DataPoint<Payee, decimal>
                        {
                            XValue = payee,
                            XLabel = payee.Description,
                            YValue = payeeTransactionsSum,
                            YLabel = _resourceContainer.GetFormattedString("{0:C}", payeeTransactionsSum)
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
                var transactions = new List<Transaction>();
                if (envelopeId == Constants.GenericHiddenEnvelope.Id)
                {
                    var hiddenEnvelopes = await _envelopeLogic.GetHiddenEnvelopesAsync().ConfigureAwait(false);
                    if (hiddenEnvelopes.Success)
                    {
                        foreach(var hiddenEnvelope in hiddenEnvelopes.Data)
                        {
                            var transactionResult = await _transactionLogic.GetEnvelopeTransactionsAsync(new Envelope { Id = hiddenEnvelope.Id }).ConfigureAwait(false);
                            if (transactionResult.Success)
                            {
                                transactions.AddRange(transactionResult.Data);
                            }
                        }
                    }
                }
                else
                {
                    var transactionResult = await _transactionLogic.GetEnvelopeTransactionsAsync(new Envelope { Id = envelopeId }).ConfigureAwait(false);
                    if (transactionResult.Success)
                    {
                        transactions.AddRange(transactionResult.Data);
                    }
                }
                var activeTransactions = transactions.Where(t => t.IsActive && !t.IsTransfer); //maybe not need the transfer portion?

                var startMonth = new DateTime(beginDate.Year, beginDate.Month, 1).AddMonths(1).AddTicks(-1);
                var endMonth = new DateTime(endDate.Year, endDate.Month, 1).AddMonths(1).AddTicks(-1);

                while (startMonth <= endMonth)
                {
                    var monthTransactions = activeTransactions.Where(t => t.ServiceDate <= startMonth && t.ServiceDate > startMonth.AddMonths(-1));
                    var monthTotal = monthTransactions.Sum(t => t.Amount ?? 0);
                    dataPoints.Add(new DataPoint<DateTime, decimal>
                    {
                        XLabel = _resourceContainer.GetFormattedString("{0:Y}", startMonth),
                        XValue = startMonth,
                        YLabel = _resourceContainer.GetFormattedString("{0:C}", monthTotal),
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
                var transactions = new List<Transaction>();
                if (payeeId == Constants.GenericHiddenPayee.Id)
                {
                    var hiddenPayees = await _payeeLogic.GetHiddenPayeesAsync().ConfigureAwait(false);
                    if (hiddenPayees.Success)
                    {
                        foreach (var hiddenPayee in hiddenPayees.Data)
                        {
                            var transactionResult = await _transactionLogic.GetPayeeTransactionsAsync(new Payee { Id = hiddenPayee.Id }).ConfigureAwait(false);
                            if (transactionResult.Success)
                            {
                                transactions.AddRange(transactionResult.Data);
                            }
                        }
                    }
                }
                else
                {
                    var transactionResult = await _transactionLogic.GetPayeeTransactionsAsync(new Payee { Id = payeeId }).ConfigureAwait(false);
                    if (transactionResult.Success)
                    {
                        transactions.AddRange(transactionResult.Data);
                    }
                }
                var activeTransactions = transactions.Where(t => t.IsActive && !t.IsTransfer); //maybe not need the transfer portion?

                var startMonth = new DateTime(beginDate.Year, beginDate.Month, 1).AddMonths(1).AddTicks(-1);
                var endMonth = new DateTime(endDate.Year, endDate.Month, 1).AddMonths(1).AddTicks(-1);

                while (startMonth <= endMonth)
                {
                    var monthTransactions = activeTransactions.Where(t => t.ServiceDate <= startMonth && t.ServiceDate > startMonth.AddMonths(-1));
                    var monthTotal = monthTransactions.Sum(t => t.Amount ?? 0);
                    dataPoints.Add(new DataPoint<DateTime, decimal>
                    {
                        XLabel = _resourceContainer.GetFormattedString("{0:Y}", startMonth),
                        XValue = startMonth,
                        YLabel = _resourceContainer.GetFormattedString("{0:C}", monthTotal),
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
