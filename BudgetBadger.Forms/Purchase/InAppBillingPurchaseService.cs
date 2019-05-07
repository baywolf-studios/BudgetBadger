using System;
using System.Linq;
using System.Threading.Tasks;
using BudgetBadger.Core.LocalizedResources;
using BudgetBadger.Core.Purchase;
using BudgetBadger.Models;
using Plugin.InAppBilling;
using Plugin.InAppBilling.Abstractions;

namespace BudgetBadger.Forms.Purchase
{
	public class InAppBillingPurchaseService : IPurchaseService
    {
        readonly IResourceContainer _resourceContainer;
        readonly IInAppBilling _inAppBilling;

        public InAppBillingPurchaseService(IResourceContainer resourceContainer,
            IInAppBilling inAppBilling)
        {
            _resourceContainer = resourceContainer;
            _inAppBilling = inAppBilling;
        }

        // purchase stuffs
        public async virtual Task<Result> PurchaseAsync(string productId)
        {
            var result = new Result();

            try
            {
                var connected = await _inAppBilling.ConnectAsync(ItemType.InAppPurchase);

                if (!connected)
                {
                    //we are offline or can't connect, don't try to purchase
                    result.Success = false;
                    result.Message = _resourceContainer.GetResourceString("PurchaseErrorConnection");
                    return result;
                }

                //check purchases
                var purchase = await _inAppBilling.PurchaseAsync(productId, ItemType.InAppPurchase, "devId");

                //possibility that a null came through.
                if (purchase == null)
                {
                    //did not purchase
                    result.Success = false;
                    result.Message = _resourceContainer.GetResourceString("PurchaseErrorNotPurchased");
                    return result;
                }
                else
                {
                    //purchased!
                    result.Success = true;
                    return result;
                }
            }
            catch (InAppBillingPurchaseException purchaseEx)
            {
                switch (purchaseEx.PurchaseError)
                {
                    case PurchaseError.AlreadyOwned:
                        result.Message = "";
                        break;
                    case PurchaseError.AppStoreUnavailable:
                        result.Message = _resourceContainer.GetResourceString("PurchaseErrorAppStoreUnavailable");
                        break;
                    case PurchaseError.BillingUnavailable:
                        result.Message = _resourceContainer.GetResourceString("PurchaseErrorBillingUnavailable"); 
                        break;
                    case PurchaseError.GeneralError:
                        result.Message = _resourceContainer.GetResourceString("PurchaseErrorGeneralError");
                        break;
                    case PurchaseError.InvalidProduct:
                        result.Message = _resourceContainer.GetResourceString("PurchaseErrorInvalidProduct");
                        break;
                    case PurchaseError.ItemUnavailable:
                        result.Message = _resourceContainer.GetResourceString("PurchaseErrorItemUnavailable");
                        break;
                    case PurchaseError.PaymentInvalid:
                        result.Message = _resourceContainer.GetResourceString("PurchaseErrorPaymentInvalid");
                        break;
                    case PurchaseError.PaymentNotAllowed:
                        result.Message = _resourceContainer.GetResourceString("PurchaseErrorPaymentNotAllowed");
                        break;
                    case PurchaseError.ProductRequestFailed:
                        result.Message = _resourceContainer.GetResourceString("PurchaseErrorProductRequestFailed");
                        break;
                    case PurchaseError.RestoreFailed:
                        result.Message = _resourceContainer.GetResourceString("PurchaseErrorRestoreFailed");
                        break;
                    case PurchaseError.ServiceUnavailable:
                        result.Message = _resourceContainer.GetResourceString("PurchaseErrorServiceUnavailable");
                        break;
                    case PurchaseError.UserCancelled:
                        result.Message = _resourceContainer.GetResourceString("PurchaseErrorUserCancelled");
                        break;
                    default:
                        result.Message = _resourceContainer.GetResourceString("PurchaseErrorUnknown");
                        break;
                }

                //Decide if it is an error we care about
                result.Success = string.IsNullOrWhiteSpace(result.Message);

                //Display message to user
                return result;
            }
            catch (Exception ex)
            {
                //Something else has gone wrong, log it
                result.Success = false;
                result.Message = _resourceContainer.GetResourceString("PurchaseErrorUnknown");
                return result;
                //Debug.WriteLine("Issue connecting: " + ex);
            }
            finally
            {
                await _inAppBilling.DisconnectAsync();
            }
        }

        // verify will return true if not able to connect
        public async virtual Task<Result> VerifyPurchaseAsync(string productId)
        {
            var result = new Result();

            try
            { 
                var connected = await _inAppBilling.ConnectAsync(ItemType.InAppPurchase);

                if (!connected)
                {
                    result.Success = true;
                    return result;
                }

                //check purchases
                var purchases = await _inAppBilling.GetPurchasesAsync(ItemType.InAppPurchase);

                //check for null just incase
                if(purchases?.Any(p => p.ProductId.ToLower() == productId.ToLower()) ?? false)
                {
                    //Purchase verified
                    result.Success = true;
                    return result;
                }
                else
                {
                    //no purchases found
                    result.Success = false;
                    result.Message = _resourceContainer.GetResourceString("PurchaseErrorVerifyFailed");
                    return result;
                }
            }
            catch (Exception ex)
            {
                // default to be verified
                result.Success = true;
                return result;
            }
            finally
            {    
                await _inAppBilling.DisconnectAsync();
            }
        }

        // restore will return false if not able to connect
        public async virtual Task<Result> RestorePurchaseAsync(string productId)
        {
            var result = new Result();

            try
            {
                var connected = await _inAppBilling.ConnectAsync(ItemType.InAppPurchase);

                if (!connected)
                {
                    result.Success = false;
                    result.Message = _resourceContainer.GetResourceString("PurchaseErrorConnection");
                    return result;
                }

                //check purchases
                var purchases = await _inAppBilling.GetPurchasesAsync(ItemType.InAppPurchase);

                //check for null just incase
                if (purchases?.Any(p => p.ProductId.ToLower() == productId.ToLower()) ?? false)
                {
                    //Purchase verified
                    result.Success = true;
                    return result;
                }
                else
                {
                    //no purchases found
                    result.Success = false;
                    result.Message = _resourceContainer.GetResourceString("PurchaseErrorNotPurchased");
                    return result;
                }
            }
            catch (InAppBillingPurchaseException purchaseEx)
            {
                switch (purchaseEx.PurchaseError)
                {
                    case PurchaseError.AlreadyOwned:
                        result.Message = "";
                        break;
                    case PurchaseError.AppStoreUnavailable:
                        result.Message = _resourceContainer.GetResourceString("PurchaseErrorAppStoreUnavailable");
                        break;
                    case PurchaseError.BillingUnavailable:
                        result.Message = _resourceContainer.GetResourceString("PurchaseErrorBillingUnavailable");
                        break;
                    case PurchaseError.GeneralError:
                        result.Message = _resourceContainer.GetResourceString("PurchaseErrorGeneralError");
                        break;
                    case PurchaseError.InvalidProduct:
                        result.Message = _resourceContainer.GetResourceString("PurchaseErrorInvalidProduct");
                        break;
                    case PurchaseError.ItemUnavailable:
                        result.Message = _resourceContainer.GetResourceString("PurchaseErrorItemUnavailable");
                        break;
                    case PurchaseError.PaymentInvalid:
                        result.Message = _resourceContainer.GetResourceString("PurchaseErrorPaymentInvalid");
                        break;
                    case PurchaseError.PaymentNotAllowed:
                        result.Message = _resourceContainer.GetResourceString("PurchaseErrorPaymentNotAllowed");
                        break;
                    case PurchaseError.ProductRequestFailed:
                        result.Message = _resourceContainer.GetResourceString("PurchaseErrorProductRequestFailed");
                        break;
                    case PurchaseError.RestoreFailed:
                        result.Message = _resourceContainer.GetResourceString("PurchaseErrorRestoreFailed");
                        break;
                    case PurchaseError.ServiceUnavailable:
                        result.Message = _resourceContainer.GetResourceString("PurchaseErrorServiceUnavailable");
                        break;
                    case PurchaseError.UserCancelled:
                        result.Message = _resourceContainer.GetResourceString("PurchaseErrorUserCancelled");
                        break;
                    default:
                        result.Message = _resourceContainer.GetResourceString("PurchaseErrorUnknown");
                        break;
                }

                //Decide if it is an error we care about
                result.Success = string.IsNullOrWhiteSpace(result.Message);

                //Display message to user
                return result;
            }
            catch (Exception ex)
            {
                //Something else has gone wrong, log it
                result.Success = false;
                result.Message = _resourceContainer.GetResourceString("PurchaseErrorUnknown");
                return result;
            }
            finally
            {
                await _inAppBilling.DisconnectAsync();
            }
        }
    }
}
