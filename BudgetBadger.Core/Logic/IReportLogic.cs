using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using BudgetBadger.Models;

namespace BudgetBadger.Core.Logic
{
    public interface IReportLogic
    {
        Task<Result<IReadOnlyDictionary<DateTime, decimal>>> GetNetWorthReport();
        Task<Result<IReadOnlyDictionary<string, decimal>>> GetSpendingByEnvelopeReport();
    }
}
