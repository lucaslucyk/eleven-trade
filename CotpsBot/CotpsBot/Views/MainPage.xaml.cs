using System;
using System.Collections.Generic;
using System.Linq;
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
        // private static IBillingService BillingHandler => DependencyService.Get<IBillingService>();
        public MainPage()
        {
            InitializeComponent();
        }

        // protected override async void OnAppearing()
        // {
        //     base.OnAppearing();
        //
        //     try
        //     {
        //         await BillingHandler.Connect();
        //         if (BillingHandler.IsConnected)
        //         {
        //             if (!await BillingHandler.CheckBuy())
        //             {
        //                 var result = await BillingHandler.Purchase();
        //                 if (result.Ok)
        //                 {
        //                     await App.Current.MainPage.DisplaySnackBarAsync(new SuccessSnackBar(result.Message));
        //                 }
        //                 else
        //                 {
        //                     await App.Current.MainPage.DisplaySnackBarAsync(new ErrorSnackBar(result.Message));
        //                 }
        //             }
        //         }
        //     }
        //     catch (Exception e)
        //     {
        //         Console.WriteLine(e);
        //         throw;
        //     }
        //     finally
        //     {
        //         await BillingHandler.Disconnect();
        //     }
        //
        //     
        //     
        // }
    }
}