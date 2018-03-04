using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BudgetBadger.Core.Logic
{
    public interface IReportLogic
    {
        Task<Dictionary<string, decimal>> GetNetWorthReport();
    }
}
