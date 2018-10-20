using System;
using System.Threading.Tasks;
using BudgetBadger.Models;

namespace BudgetBadger.Core.Purchase
{
    public interface IPurchaseService
    {
        Task<Result> VerifyPurchaseAsync(string productId);
        Task<Result> PurchaseAsync(string productId);
        Task<Result> RestorePurchaseAsync(string productId);
    }
}
