using System;
using System.Threading.Tasks;
using BudgetBadger.Core.Purchase;
using BudgetBadger.Models;

namespace BudgetBadger.Forms.Purchase
{
    public class TrueInAppBillingPurchaseService : IPurchaseService
    {
        public TrueInAppBillingPurchaseService()
        {
        }

        public Task<Result> PurchaseAsync(string productId)
        {
            return Task.FromResult(new Result { Success = true });
        }

        public Task<Result> RestorePurchaseAsync(string productId)
        {
            return Task.FromResult(new Result { Success = true });
        }

        public Task<Result> VerifyPurchaseAsync(string productId)
        {
            return Task.FromResult(new Result { Success = true });
        }
    }
}
