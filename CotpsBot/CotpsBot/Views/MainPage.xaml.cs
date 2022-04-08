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

        protected override async void OnAppearing()
        {
            base.OnAppearing();

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
                                    var purchase = await billing.PurchaseAsync(inAppBillingProducts.First().ProductId, ItemType.Subscription);
                                    if (!purchase.IsAcknowledged)
                                    {
                                        
                                    }

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
                    Console.WriteLine(e);
                    throw;
                }
                finally
                {
                    await billing.DisconnectAsync();
                }
            }
        }
    }
}