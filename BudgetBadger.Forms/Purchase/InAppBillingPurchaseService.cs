using System;
using System.Linq;
using System.Threading.Tasks;
using BudgetBadger.Core.Purchase;
using BudgetBadger.Models;
using Plugin.InAppBilling;
using Plugin.InAppBilling.Abstractions;

namespace BudgetBadger.Forms.Purchase
{
	public class InAppBillingPurchaseService : IPurchaseService
    {
        readonly IInAppBilling _inAppBilling;

        public InAppBillingPurchaseService(IInAppBilling inAppBilling)
        {
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
                    result.Message = "Unable to connect to billing service";
                    return result;
                }

                //check purchases
                var purchase = await _inAppBilling.PurchaseAsync(productId, ItemType.InAppPurchase, "devId");

                //possibility that a null came through.
                if (purchase == null)
                {
                    //did not purchase
                    result.Success = false;
                    result.Message = "Did not purchase";
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
                    case PurchaseError.AppStoreUnavailable:
                        result.Message = "Currently the app store seems to be unavailble. Try again later.";
                        break;
                    case PurchaseError.BillingUnavailable:
                        result.Message = "Billing seems to be unavailable, please try again later.";
                        break;
                    case PurchaseError.PaymentInvalid:
                        result.Message = "Payment seems to be invalid, please try again.";
                        break;
                    case PurchaseError.PaymentNotAllowed:
                        result.Message = "Payment does not seem to be enabled/allowed, please try again.";
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
                result.Message = "Unknown error";
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
                if(purchases?.Any(p => p.ProductId == productId) ?? false)
                {
                    //Purchase verified
                    result.Success = true;
                    return result;
                }
                else
                {
                    //no purchases found
                    result.Success = false;
                    result.Message = "Product purchase was not verified";
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
                    result.Message = "Unable to connect to billing service";
                    return result;
                }

                //check purchases
                var purchases = await _inAppBilling.GetPurchasesAsync(ItemType.InAppPurchase);

                //check for null just incase
                if (purchases?.Any(p => p.ProductId == productId) ?? false)
                {
                    //Purchase verified
                    result.Success = true;
                    return result;
                }
                else
                {
                    //no purchases found
                    result.Success = false;
                    result.Message = "Product purchase was not verified";
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
    }
}
