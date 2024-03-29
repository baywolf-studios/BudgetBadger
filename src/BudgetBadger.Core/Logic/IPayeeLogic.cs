﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetBadger.Models;

namespace BudgetBadger.Core.Logic
{
    public interface IPayeeLogic
    {
        Task<Result<Payee>> SavePayeeAsync(Payee payee);
        Task<Result<int>> GetPayeesCountAsync();
        Task<Result<Payee>> GetPayeeAsync(Guid id);
        Task<Result<IReadOnlyList<Payee>>> GetPayeesAsync();
        Task<Result<IReadOnlyList<Payee>>> GetPayeesForSelectionAsync();
        Task<Result<IReadOnlyList<Payee>>> GetPayeesForReportAsync();
        Task<Result<IReadOnlyList<Payee>>> GetHiddenPayeesAsync();
        Task<Result<Payee>> SoftDeletePayeeAsync(Guid id);
        Task<Result<Payee>> HidePayeeAsync(Guid id);
        Task<Result<Payee>> UnhidePayeeAsync(Guid id);

        bool FilterPayee(Payee payee, string searchText);
        bool FilterPayee(Payee payee, FilterType filterType);
    }
}
