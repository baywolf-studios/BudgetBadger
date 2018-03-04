using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetBadger.Core.DataAccess;
using BudgetBadger.Core.Logic;

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

        public async Task<Dictionary<string, decimal>> GetNetWorthReport()
        {
            var result = new Dictionary<string, decimal>();

            var transactions = await _transactionDataAccess.ReadTransactionsAsync();
            var activeTransactions = transactions.Where(t => t.IsActive);
            var months = activeTransactions.Select(d => new DateTime(d.ServiceDate.Year, d.ServiceDate.Month, 1)).Distinct();

            foreach (var month in months)
            {
                var monthTransactions = activeTransactions.Where(t => t.ServiceDate.Month <= month.Month && t.ServiceDate.Year <= month.Year);
                var monthTotal = monthTransactions.Sum(t => t.Amount);
                result.Add(month.ToString("Y"), monthTotal);
            }

            return result;
        }
    }
}
