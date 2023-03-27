using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using BudgetBadger.Core.Models;

namespace BudgetBadger.Core.Logic
{
    public interface IReportLogic
    {
        Task<Result<IReadOnlyList<DataPoint<DateTime, decimal>>>> GetNetWorthReport(DateTime beginDate, DateTime endDate);
        Task<Result<IReadOnlyList<DataPoint<Envelope, decimal>>>> GetEnvelopesSpendingReport(DateTime beginDate, DateTime endDate);
        Task<Result<IReadOnlyList<DataPoint<PayeeModel, decimal>>>> GetPayeesSpendingReport(DateTime beginDate, DateTime endDate);
        Task<Result<IReadOnlyList<DataPoint<DateTime, decimal>>>> GetEnvelopeTrendsReport(Guid envelopeId, DateTime beginDate, DateTime endDate);
        Task<Result<IReadOnlyList<DataPoint<DateTime, decimal>>>> GetPayeeTrendsReport(Guid payeeId, DateTime beginDate, DateTime endDate);
    }
}
