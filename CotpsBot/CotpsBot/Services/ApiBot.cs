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
        
        private static ICodeTranslator Translator => DependencyService.Get<ICodeTranslator>();
        private static IRequestService ApiClient => DependencyService.Get<IRequestService>();
        private static IBillingService BillingHandler => DependencyService.Get<IBillingService>();
        private int _orderAttempts = 0;

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

        private static void NotifyMessage(string title, string message, string data = "", int id = 1335)
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

        private static void TryClearMessage(int notificationId)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    NotificationCenter.Current.Clear(notificationId);
                }
                catch (Exception)
                {
                    // do nothing
                }
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

        private static void SendTransactionMessage(BalanceInfo data)
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
                try
                {
                    var confirm = await ApiClient.ConfirmOrder(order.data.orderId);
                    _orderAttempts = 0;
                    if (confirm.success)
                    {
                        // operate again
                        await DoTransactions();
                    }
                    else
                    {
                        var eco = Translator.Translate("error_confirming_order");
                        NotifyMessage(Translator.Translate("order_not_confirmed"), 
                            $"{eco} {order.data.orderId}.");
                        // DependencyService.Get<IBotService>().Stop();
                    }
                }
                catch (Exception e)
                {
                    if (_orderAttempts < Settings.MaxOrderConfirmAttempts)
                    {
                        _orderAttempts += 1;
                        await DoTransactions();
                    }
                    else
                    {
                        NotifyMessage(Translator.Translate("order_not_confirmed"), 
                            Translator.Translate("error_confirming_order"));
                    }
                }
            }
        }

        private static async Task ReceiveProfits()
        {
            foreach (var level in Settings.TeamLevels)
            {
                try
                {
                    var teamInfo = await ApiClient.GetTeamInfo(level);
                    if (!teamInfo.success || teamInfo.code < 200 || teamInfo.code >= 400) continue;
                    var converted = Convert.ToDouble(teamInfo.data?.detail?.remainder);
                    // if (!double.TryParse(teamInfo.data?.detail?.remainder, out var converted)) continue;
                    if (!(converted >= 0.01)) continue;
                    var receiveResponse = await ApiClient.ReceiveProfit(level);
                    
                    if (!receiveResponse.success)
                    {
                        var residualDetail = Translator.Translate("residual_not_obtained_details");
                        NotifyMessage(Translator.Translate("residual_not_obtained"), 
                            $"{residualDetail}. (Code={receiveResponse.code})",
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

        private static void SendBtnControlStatus(bool enabled)
        {
            var message = new BtnControlMessage {Status = enabled};
            Device.BeginInvokeOnMainThread(() =>
            {
                MessagingCenter.Send<BtnControlMessage>(message, "BtnControlMessage");
            });
        }

        private static async Task<bool> EnsureSubscription()
        {
            try
            {
                await BillingHandler.Connect();
                if (BillingHandler.IsConnected)
                {
                    if (!await BillingHandler.CheckBuy())
                    {
                        // send message
                        NotifyMessage(
                            Translator.Translate("subscription_error"),
                            Translator.Translate("sub_ended_open_to_renew"),
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
                    NotifyMessage(
                        Translator.Translate("subscription_error"),
                        Translator.Translate("billing_svc_not_available_restart"),
                        id: Settings.Notifications.SubscriptionError);
                    return false;
                }
            }
            catch (Exception)
            {
                NotifyMessage(
                    Translator.Translate("subscription_error"),
                    Translator.Translate("sub_verifying_error"),
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
                NotifyMessage(Translator.Translate("no_internet_connection"), 
                    Translator.Translate("waiting_internet_to_operate"),
                    id: Settings.Notifications.NetworkError);
                return;
            }
            
            // clear no internet connection message
            TryClearMessage(Settings.Notifications.NetworkError);
            TryClearMessage(Settings.Notifications.UnknownError);

            if (!await EnsureSubscription())
            {
                DependencyService.Get<IBotService>().Stop();
                return;
            }

            // disable btn
            SendBtnControlStatus(false);

            try
            {
                var form = await GetLoginForm();
                var loginResult = await ApiClient.LoginAsync(form);

                if (loginResult.success)
                {
                    // remove possible login error
                    TryClearMessage(Settings.Notifications.LoginError);

                    // try to collect residuals
                    await ReceiveProfits();

                    // try to create and confirm orders
                    await this.DoTransactions();

                    // logout after of do actions
                    ApiClient.Logout();
                }
                else
                {
                    NotifyMessage(
                        Translator.Translate("cotps_login_error"),
                        Translator.Translate("check_credentials_and_restart_bot"),
                        id: Settings.Notifications.LoginError);
                    if (loginResult.code == 411)
                        DependencyService.Get<IBotService>().Stop();
                }

            }
            finally
            {
                // enable btn
                SendBtnControlStatus(true);
            }
        }

        public async Task<TransactionsBalance> LoginAndGetBalance()
        {
            var response = new TransactionsBalance();
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                // wait for internet connection - do nothing
                NotifyMessage(Translator.Translate("no_internet_connection"), 
                    Translator.Translate("waiting_internet_to_operate"),
                    id: Settings.Notifications.NetworkError);
                return response;
            }
            
            var form = await GetLoginForm();
            var loginResult = await ApiClient.LoginAsync(form);
            if (!loginResult.success)
            {
                NotifyMessage(
                    Translator.Translate("cotps_login_error"), 
                    Translator.Translate("check_credentials_and_restart_bot"),
                    id: Settings.Notifications.LoginError);
                if (loginResult.code == 411)
                    DependencyService.Get<IBotService>().Stop();
                return response;
            }
            
            // remove possible login error
            TryClearMessage(Settings.Notifications.LoginError);
            
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