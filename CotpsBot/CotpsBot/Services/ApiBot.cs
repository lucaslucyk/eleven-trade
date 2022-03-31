using System;
using System.Threading.Tasks;
using CotpsBot.Models.Http;
using CotpsBot.Services.Http;
using CotpsBot.Helpers;
using CotpsBot.Models;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.Forms;

namespace CotpsBot.Services
{
    public class ApiBot
    {
        #region Fields

        private static IRequestService ApiClient => DependencyService.Get<IRequestService>();

        #endregion

        #region Methods

        private void SendTransactionMessage(BalanceInfo data)
        {
            var message = new TransactionsBalance
            {
                Total = data.total_balance,
                Freeze = data.freeze_balance,
                Free = data.balance
            };
            Device.BeginInvokeOnMainThread(() =>
            {
                MessagingCenter.Send<TransactionsBalance>(message, "TransactionsBalance");
            });
        }

        private async Task Operate()
        {
            var order = await ApiClient.CreateOrder();
            if (order.success)
            {
                var confirm = await ApiClient.ConfirmOrder(order.data.orderId);

                if (confirm.success)
                {
                    await App.Current.MainPage.DisplaySnackBarAsync(new SuccessSnackBar("Order confirmed succesfully."));
                    await DoTransactions();
                }
                else
                {
                    await App.Current.MainPage.DisplaySnackBarAsync(new ErrorSnackBar(confirm.msg));
                }
            }
        }

        private async Task DoTransactions()
        {
            var balanceResponse = await ApiClient.GetBalance();
            if (balanceResponse.success)
            {
                SendTransactionMessage(balanceResponse.userinfo);
                
                var converted = Convert.ToDouble(balanceResponse.userinfo.balance);
                if (converted >= 5.0)
                {
                    await Operate();
                }
            }
        }

        private void SendBtnControlStatus(bool enabled)
        {
            var message = new BtnControlMessage {Status = enabled};
            Device.BeginInvokeOnMainThread(() =>
            {
                MessagingCenter.Send<BtnControlMessage>(message, "BtnControlMessage");
            });
        }
        public async Task LoginAndOperate(bool starting = false)
        {
            // disable btn
            SendBtnControlStatus(false);
            
            var form = new LoginRequest
            {
                mobile = Settings.UserPhone,
                password = Settings.UserPassword,
                type = Settings.APILoginType
            };
            var loginResult = await ApiClient.LoginAsync(form);
            
            if (loginResult.success)
            {
                // try to create and confirm orders
                await this.DoTransactions();
                    
                // logout after of do actions
                ApiClient.Logout();
            }
            else
            {
                await App.Current.MainPage.DisplaySnackBarAsync(new ErrorSnackBar("COTPS login error."));
                if (starting)
                    DependencyService.Get<IBotService>().Stop();
            }
            
            // enable btn
            SendBtnControlStatus(true);
        }
        
        #endregion

    }
}