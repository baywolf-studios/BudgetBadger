using System;
using System.Threading.Tasks;
using BudgetBadger.Core.Purchase;
using BudgetBadger.Models;

namespace BudgetBadger.Forms.Purchase
{
	public class InAppBillingPurchaseService : IPurchaseService
    {
        public InAppBillingPurchaseService()
        {
        }

        public virtual Task<Result> PurchaseAsync(string productId)
        {
            return Task.FromResult(new Result { Success = true });
        }

        // verify will return true if not able to connect
        public virtual Task<Result> VerifyPurchaseAsync(string productId)
        {
            return Task.FromResult(new Result { Success = true });
        }

        // restore will return false if not able to connect
        public virtual Task<Result> RestorePurchaseAsync(string productId)
        {
            return Task.FromResult(new Result { Success = true });
        }
    }
}
