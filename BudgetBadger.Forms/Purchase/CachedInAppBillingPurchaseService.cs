using System;
using System.Threading.Tasks;
using BudgetBadger.Core.Settings;
using BudgetBadger.Models;
using Plugin.InAppBilling.Abstractions;

namespace BudgetBadger.Forms.Purchase
{
	public class CachedInAppBillingPurchaseService : InAppBillingPurchaseService
    {
        readonly ISettings _settings;
        readonly string _settingsKey = "CachedInAppBillingPurchaseService";

        public CachedInAppBillingPurchaseService(IInAppBilling inAppBilling, ISettings settings) : base(inAppBilling)
        {
            _settings = settings;
        }

        public override async Task<Result> PurchaseAsync(string productId)
        {
            var purchaseResult = await base.PurchaseAsync(productId);

            await _settings.AddOrUpdateValueAsync(_settingsKey + productId, purchaseResult.Success.ToString());

            return purchaseResult;
        }

        public override async Task<Result> VerifyPurchaseAsync(string productId)
        {
            var cachedSettingResult = _settings.GetValueOrDefault(_settingsKey + productId);

            Boolean.TryParse(cachedSettingResult, out bool cachedResult);

            // verify will return true if not able to connect
            var currentResult = await base.VerifyPurchaseAsync(productId);

            return new Result { Success = (cachedResult && currentResult.Success), Message = currentResult.Message };
        }

        public override async Task<Result> RestorePurchaseAsync(string productId)
        {
            var restoreResult = await base.RestorePurchaseAsync(productId);

            // restore will return false if not able to connect
            await _settings.AddOrUpdateValueAsync(_settingsKey + productId, restoreResult.Success.ToString());

            return restoreResult;
        }
    }
}
