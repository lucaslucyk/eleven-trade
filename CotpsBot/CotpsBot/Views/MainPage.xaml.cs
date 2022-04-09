using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CotpsBot.Helpers;
using CotpsBot.Services;
using Plugin.InAppBilling;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.Forms;

namespace CotpsBot.Views
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async Task CheckOrBuy()
        {
            if (CrossInAppBilling.IsSupported)
            {
                var billing = CrossInAppBilling.Current;
                try
                {
                    var connected = await billing.ConnectAsync();
                    if (connected)
                    {
                        var subscriptionsList =
                            await billing.GetProductInfoAsync(ItemType.Subscription, new []{"cotps_service"});

                        var inAppBillingProducts = subscriptionsList.ToList();
                        if (inAppBillingProducts.Any())
                        {
                            var purchased =
                                await billing.GetPurchasesAsync(ItemType.Subscription, new List<string>{inAppBillingProducts.First().ProductId});

                            if (!purchased.Any())
                            {
                                try
                                {
                                    var purchase = await billing.PurchaseAsync("cotps_service", ItemType.Subscription);
                                    
                                }
                                catch (InAppBillingPurchaseException e)
                                {
                                    App.Current.MainPage.DisplaySnackBarAsync(new ErrorSnackBar(e.PurchaseError.ToString()));
                                    
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
#if DEBUG
                    Console.WriteLine(e);
                    throw;
#else
                    App.Current.MainPage.DisplaySnackBarAsync(new ErrorSnackBar(e.ToString()));
#endif
                }
                finally
                {
                    await billing.DisconnectAsync();
                }
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        
            // await this.CheckOrBuy();
        }
    }
}