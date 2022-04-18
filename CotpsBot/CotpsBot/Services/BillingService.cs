using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CotpsBot.Helpers;
using CotpsBot.Models;
using Plugin.InAppBilling;
using Xamarin.Forms;

namespace CotpsBot.Services
{
    public class BillingService : IBillingService
    {

        private IInAppBilling _billing;
        private bool _isConnected = false;
        private bool _isSupported;
        
        public BillingService()
        {
            this._isSupported = CrossInAppBilling.IsSupported;
            
            if (this._isSupported)
                this._billing = CrossInAppBilling.Current;
        }

        public bool IsConnected
        {
            get => this._isConnected;
            private set => this._isConnected = value;
        }

        public async Task Connect()
        {
            if (!this.IsConnected && this._isSupported)
                this.IsConnected = await this._billing.ConnectAsync();
        }

        public async Task Disconnect()
        {
            if (!this.IsConnected || !this._isSupported)
                return;
            
            await this._billing.DisconnectAsync();
            this.IsConnected = false;
        }

        public async Task<IEnumerable<InAppBillingProduct>?> GetAvailableSubs()
        {
            // cant access to buy status
            if (!_isSupported)
                return null;
            
            // ensure connected
            await this.Connect();
            var available = await _billing.GetProductInfoAsync(
                ItemType.Subscription, 
                Settings.CotpsPlans.Select(sp => sp.Id).ToArray());
            return available;
        }

        // public async Task<bool> CheckDeprecatedBuy()
        // {
        //     // cant access to buy status
        //     if (!_isSupported)
        //         return false;
        //     
        //     // ensure connected
        //     await this.Connect();
        //     
        //     var purchased = await _billing.GetPurchasesAsync(
        //         ItemType.Subscription,
        //         Settings.CotpsDeprecatedSubs.ToList());
        //     
        //     return purchased.Any(purchase => Settings.CotpsDeprecatedSubs.Contains(purchase.Id) && !purchase.IsAcknowledged);
        // }
        
        public async Task<bool> CheckBuy(bool autoPurchaseAcknowledge = true)
        {
            // cant access to buy status
            if (!_isSupported)
                return false;
            
            // ensure connected
            await this.Connect();
            
            var purchased = await _billing.GetPurchasesAsync(
                ItemType.Subscription,
                Settings.CotpsPlans.Select(sp => sp.Id).ToList());
            
            var inAppBillingPurchases = purchased.ToList();
            if (inAppBillingPurchases.Any() && autoPurchaseAcknowledge && Device.RuntimePlatform == Device.Android)
            {
                foreach (var toConfirm in inAppBillingPurchases.Where(toConfirm => !toConfirm.IsAcknowledged))
                {
                    await this._billing.AcknowledgePurchaseAsync(toConfirm.PurchaseToken);
                }
            }

            return inAppBillingPurchases.Any();
        }

        public async Task<PurchaseResult> Purchase()
        {
            return await this.Purchase(Settings.CotpsPlans.First().Id);
        }
        
        public async Task<PurchaseResult> Purchase(string productId)
        {
            // cant access to buy status
            if (!_isSupported)
                return new PurchaseResult {Ok = false, Message = "Device not allowed"};
            
            // ensure connected
            await this.Connect();

            try
            {
                var purchase = await this._billing.PurchaseAsync(productId, ItemType.Subscription);

                return new PurchaseResult
                {
                    Ok = purchase.State == PurchaseState.Purchased ? true : false,
                    Message = purchase.State == PurchaseState.Purchased
                        ? "Order completed successfully."
                        : "Complete the order to proceed.",
                };
            }
            catch (InAppBillingPurchaseException e)
            {
                return new PurchaseResult
                {
                    Ok = false,
                    Message = e.PurchaseError.ToString()
                };
            }
            catch (Exception e)
            {
                return new PurchaseResult
                {
                    Ok = false,
                    Message = "Something was wrong."
                };
            }
        }
    }
}