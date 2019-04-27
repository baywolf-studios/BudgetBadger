using System;
using System.Threading.Tasks;
using BudgetBadger.Core.LocalizedResources;
using BudgetBadger.Core.Settings;
using BudgetBadger.Models;
using Plugin.InAppBilling.Abstractions;

namespace BudgetBadger.Forms.Purchase
{
	public class CachedInAppBillingPurchaseService : InAppBillingPurchaseService
    {
        readonly IResourceContainer _resourceContainer;
        readonly ISettings _settings;
        readonly string _settingsKey = "CachedInAppBillingPurchaseService";

        public CachedInAppBillingPurchaseService(IResourceContainer resourceContainer,
            IInAppBilling inAppBilling,
            ISettings settings) : base(resourceContainer, inAppBilling)
        {
            _resourceContainer = resourceContainer;
            _settings = settings;
        }

        public override async Task<Result> PurchaseAsync(string productId)
        {
            var purchaseResult = await base.PurchaseAsync(productId);

            await _settings.AddOrUpdateValueAsync(_settingsKey + productId, purchaseResult.Success.ToString());

            return purchaseResult;
        }

        public override Task<Result> VerifyPurchaseAsync(string productId)
        {
            var result = new Result();

            var cachedSettingResult = _settings.GetValueOrDefault(_settingsKey + productId);

            Boolean.TryParse(cachedSettingResult, out bool cachedResult);

            if (cachedResult)
            {
                result.Success = cachedResult;
                // verify will return true if not able to connect
                // result = await base.VerifyPurchaseAsync(productId);
            }
            else
            {
                result.Success = false;
                result.Message = _resourceContainer.GetResourceString("PurchaseErrorNotPurchased");
            }

            return Task.FromResult<Result>(result);
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
