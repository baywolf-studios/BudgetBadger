using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetBadger.Core.DataAccess;
using BudgetBadger.Core.Logic;
using BudgetBadger.Models;

namespace BudgetBadger.Logic
{
    public class ReportLogic : IReportLogic
    {
        readonly ITransactionDataAccess _transactionDataAccess;
        readonly IAccountDataAccess _accountDataAccess;
        readonly IPayeeDataAccess _payeeDataAccess;
        readonly IEnvelopeDataAccess _envelopeDataAccess;

        public ReportLogic(ITransactionDataAccess transactionDataAccess,
                           IAccountDataAccess accountDataAccess,
                           IPayeeDataAccess payeeDataAccess,
                           IEnvelopeDataAccess envelopeDataAccess)
        {
            _transactionDataAccess = transactionDataAccess;
            _accountDataAccess = accountDataAccess;
            _payeeDataAccess = payeeDataAccess;
            _envelopeDataAccess = envelopeDataAccess;
        }

        public async Task<Result<IEnumerable<ReportDataPoint>>> GetNetWorthReport()
        {
            var result = new Result<IEnumerable<ReportDataPoint>>();
            var dataPoints = new List<ReportDataPoint>();

            try
            {
                var transactions = await _transactionDataAccess.ReadTransactionsAsync();
                var activeTransactions = transactions.Where(t => t.IsActive);
                var months = activeTransactions.Select(d => new DateTime(d.ServiceDate.Year, d.ServiceDate.Month, 1)).Distinct();

                var earliestMonth = activeTransactions.Min(t => t.ServiceDate);
                var latestMonth = activeTransactions.Max(t => t.ServiceDate);

                var startMonth = new DateTime(earliestMonth.Year, earliestMonth.Month, 1).AddMonths(1).AddTicks(-1);
                var endMonth = new DateTime(latestMonth.Year, latestMonth.Month, 1).AddMonths(1).AddTicks(-1);

                int count = 0;
                //something on these lines 
                while (startMonth <= endMonth)
                {
                    var monthTransactions = activeTransactions.Where(t => t.ServiceDate <= startMonth);
                    var monthTotal = monthTransactions.Sum(t => t.Amount);
                    var dataPoint = new ReportDataPoint
                    {
                        X = count,
                        XLabel = startMonth.ToString("Y"),
                        Y = Convert.ToDouble(monthTotal),
                        YLabel = monthTotal.ToString("C")
                    };
                    dataPoints.Add(dataPoint);
                    count++;
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
