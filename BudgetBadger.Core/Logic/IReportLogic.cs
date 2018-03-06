using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using BudgetBadger.Models;

namespace BudgetBadger.Core.Logic
{
    public interface IReportLogic
    {
        Task<Result<IReadOnlyDictionary<DateTime, decimal>>> GetNetWorthReport(); // some enum for month vs year vs week
        Task<Result<IReadOnlyDictionary<string, decimal>>> GetEnvelopeSpendingTotalsReport();
        Task<Result<IReadOnlyDictionary<string, decimal>>> GetPayeeSpendingTotalsReport();
        Task<Result<IReadOnlyDictionary<DateTime, decimal>>> GetSpendingTrendsByEnvelopeReport(Guid envelopeId);
        Task<Result<IReadOnlyDictionary<DateTime, decimal>>> GetSpendingTrendsByPayeeReport(Guid payeeId);
    }
}
