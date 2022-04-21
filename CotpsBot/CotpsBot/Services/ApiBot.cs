using System;
using System.Threading.Tasks;
using CotpsBot.Models.Http;
using CotpsBot.Services.Http;
using CotpsBot.Helpers;
using CotpsBot.Models;
using Plugin.LocalNotification;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace CotpsBot.Services
{
    public class ApiBot
    {
        #region Fields
        
        private static IRequestService ApiClient => DependencyService.Get<IRequestService>();
        private static IBillingService BillingHandler => DependencyService.Get<IBillingService>();

        #endregion

        #region Constructor

        public ApiBot()
        {
            // UserPhone = Settings.UserPhone;
            // UserPassword = Settings.UserPassword;
            // LoginType = Settings.APILoginType;
        }

        #endregion

        #region Properties

        // private string UserPhone { get; set; }
        // private string UserPassword { get; set; }
        // private string LoginType { get; set; }
        

        #endregion

        #region Methods

        private async Task NotifyMessage(string title, string message, string data = "", int id = 1335)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                var notification = new NotificationRequest
                {
                    BadgeNumber = 1,
                    Description = message,
                    Title = title,
                    ReturningData = data,
                    NotificationId = id
                };
                await NotificationCenter.Current.Show(notification);
            });
        }

        private async Task<LoginRequest> GetLoginForm()
        {
            return new LoginRequest
            {
                mobile = Settings.UserPhone,
                password = Settings.UserPassword,
                type = Settings.APILoginType
            };
        }

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
                    // TODO: Notify this ?
                    // await App.Current.MainPage.CotpsBotkBarAsync(new SuccessSnackBar("Order confirmed succesfully."));
                    await DoTransactions();
                }
                else
                {
                    await NotifyMessage("COTPS Order confirm error", $"Error confirming order {order.data.orderId}.");
                    // DependencyService.Get<IBotService>().Stop();
                }
            }
        }

        private async Task ReceiveProfits()
        {
            foreach (var level in Settings.TeamLevels)
            {
                var teamInfo = await ApiClient.GetTeamInfo(level);
                if (!teamInfo.success || teamInfo.code < 200 || teamInfo.code >= 400) continue;
                try
                {
                    var converted = Convert.ToDouble(teamInfo.data?.detail?.remainder);
                    // if (!double.TryParse(teamInfo.data?.detail?.remainder, out var converted)) continue;
                    if (!(converted >= 0.01)) continue;
                    var receiveResponse = await ApiClient.ReceiveProfit(level);
                    
                    if (!receiveResponse.success)
                    {
                        await NotifyMessage("Residual warning", 
                            $"Residual benefits for type {level} could not be obtained. (Code={receiveResponse.code})",
                            id: Settings.Notifications.ResidualError);
                        return;
                    }
                }
                catch (Exception)
                {
                    // ignored
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

        private async Task<bool> EnsureSubscription()
        {
            try
            {
                await BillingHandler.Connect();
                if (BillingHandler.IsConnected)
                {
                    if (!await BillingHandler.CheckBuy())
                    {
                        // send message
                        await NotifyMessage(
                            "Subscription error",
                            "Your subscription has ended. Open the app and start the bot to renew it.",
                            id: Settings.Notifications.SubscriptionError);
                        return false;
                    }
                    else
                    {
                        // subscribed
                        return true;
                    }
                }
                else
                {
                    await NotifyMessage(
                        "Subscription error",
                        "Billing service not available. Restart service manually.",
                        id: Settings.Notifications.SubscriptionError);
                    return false;
                }
            }
            catch (Exception e)
            {
                await NotifyMessage(
                    "Subscription error",
                    "An error occurred while trying to verify the subscription.",
                    id: Settings.Notifications.SubscriptionError);
                return false;
            }
            finally
            {
                await BillingHandler.Disconnect();
            }
        }

        public async Task LoginAndOperate(bool starting = false)
        {
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                // wait for internet connection - do nothing
                await NotifyMessage("Network error", "No internet connection. Waiting to make new requests.",
                    id: Settings.Notifications.NetworkError);
                return;
            }

            if (!await EnsureSubscription())
            {
                DependencyService.Get<IBotService>().Stop();
                return;
            }

            // disable btn
            SendBtnControlStatus(false);

            var form = await GetLoginForm();
            var loginResult = await ApiClient.LoginAsync(form);
            
            if (loginResult.success)
            {
                // try to collect residuals
                await this.ReceiveProfits();
                
                // try to create and confirm orders
                await this.DoTransactions();
                    
                // logout after of do actions
                ApiClient.Logout();
            }
            else
            {
                await NotifyMessage("COTPS Login error", "Check your credentials and restart bot service.");
                if (loginResult.code == 411)
                    DependencyService.Get<IBotService>().Stop();
            }
            
            // enable btn
            SendBtnControlStatus(true);
        }

        public async Task<TransactionsBalance> LoginAndGetBalance()
        {
            var response = new TransactionsBalance();
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                // wait for internet connection - do nothing
                await NotifyMessage("Network error", "No internet connection. Waiting to make new requests.",
                    id: Settings.Notifications.NetworkError);
                return response;
            }
            
            var form = await GetLoginForm();
            var loginResult = await ApiClient.LoginAsync(form);
            if (!loginResult.success)
            {
                await NotifyMessage("COTPS Login error", "Check your credentials and restart bot service.");
                if (loginResult.code == 411)
                    DependencyService.Get<IBotService>().Stop();
                return response;
            }
            
            // get data and return
            var balance = await ApiClient.GetBalance();
            response.Total = balance.userinfo.total_balance;
            response.Freeze = balance.userinfo.freeze_balance;
            response.Free = balance.userinfo.balance;
            
            // api logout
            ApiClient.Logout();
            
            return response;
        }
        
        #endregion

    }
}