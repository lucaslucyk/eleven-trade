using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CotpsBot.Models;
using CotpsBot.Services;
using CotpsBot.Validators;
using CotpsBot.Validators.Rules;
using CotpsBot.Helpers;
using Plugin.LocalNotification;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml.Internals;


namespace CotpsBot.ViewModels
{
    public class MainPageViewModel : BaseViewModel
    {
        #region Fields

        private string _taskMessage = "Stopped";
        private string _switchMessage = Translator.Translate("BOT START");
        private bool _switchEnabled = true;
        private bool _isRunning;
        private bool _botStarting;
        private bool _noInternetConnection;
        private ValidatableObject<string> _phoneNumber;
        private ValidatableObject<string> _password;
        private DateTime _lastRun;
        private bool _rememberPassword;
        private TransactionsBalance _balance;
        private static ICodeTranslator Translator => DependencyService.Get<ICodeTranslator>();
        private static IBillingService BillingHandler => DependencyService.Get<IBillingService>();
        
        #endregion

        #region Constructor

        public MainPageViewModel()
        {
            this.SwitchCommand = new Command(this.SwitchClicked);
            this.RememberPasswordCommand = new Command(this.RememberTapped);
            this.NoInternetCommand = new Command(this.NoInternetTapped);

            Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;

            this.InitializeProperties();
            this.AddValidationRules();
            this.RecoveryFormData();

            HandleReceivedMessages();
        }

        ~MainPageViewModel()
        {
            Connectivity.ConnectivityChanged -= Connectivity_ConnectivityChanged;
        }

        #endregion

        #region Properties

        public bool NoInternetConnection
        {
            get => this._noInternetConnection;
            set => this.SetProperty(ref this._noInternetConnection, value);
        }
        public string SwitchMessage
        {
            get => this._switchMessage;
            set => this.SetProperty(ref this._switchMessage, value);
        }

        public bool SwitchEnabled
        {
            get => this._switchEnabled;
            set => this.SetProperty(ref this._switchEnabled, value);
        }

        public bool RememberPassword
        {
            get => this._rememberPassword;
            set => this.SetProperty(ref this._rememberPassword, value);
        }

        public TransactionsBalance Balance
        {
            get => this._balance;
            private set => this.SetProperty(ref this._balance, value);
        }

        public DateTime LastRun
        {
            get => this._lastRun;
            set => this.SetProperty(ref this._lastRun, value);
        }

        public bool IsRunning
        {
            get => this._isRunning;
            set => this.SetProperty(ref this._isRunning, value);
        }

        public bool BotStarting
        {
            get => this._botStarting;
            set => this.SetProperty(ref this._botStarting, value);
        }

        public string TaskMessage
        {
            get => this._taskMessage;
            set => this.SetProperty(ref this._taskMessage, value);
        }

        public ValidatableObject<string> PhoneNumber
        {
            get => this._phoneNumber;
            private set => this.SetProperty(ref this._phoneNumber, value);
        }

        public ValidatableObject<string> Password
        {
            get => this._password;
            private set => this.SetProperty(ref this._password, value);
        }

        #endregion

        #region Commands

        public Command RememberPasswordCommand { get; set; }
        public Command SwitchCommand { get; set; }
        public Command NoInternetCommand { get; set; }

        #endregion

        #region Methods
        
        private void Connectivity_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            this.NoInternetConnection = e.NetworkAccess != NetworkAccess.Internet;
        }
        
        private async void NoInternetTapped(object obj)
        {
            var msg = this.IsRunning
                ? Translator.Translate("no_internet_tapped_running")
                : Translator.Translate("no_internet_tapped_inactive");
            await App.Current.MainPage.DisplayAlert("Info", msg, "OK");
        }
        
        private void RememberTapped(object obj)
        {
            this.RememberPassword = !this.RememberPassword;
            if (!this.RememberPassword)
                RemoveFormData();
        }

        private async Task<bool> EnsureSubscription()
        {
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                await App.Current.MainPage.DisplaySnackBarAsync(new ErrorSnackBar(Translator.Translate("no_internet_verify_sub")));
                return false;
            }
            try
            {
                await BillingHandler.Connect();
                if (BillingHandler.IsConnected)
                {
                    if (!await BillingHandler.CheckBuy(true))
                    {
                        var subToBuy = await App.Current.MainPage.DisplayActionSheet(
                            "Select a subscription plan to continue",
                            "Cancel", 
                            null, 
                            Settings.CotpsPlans.Select(sp => sp.Description).ToArray());

                        if (subToBuy == null)
                            return false;
                        // var buyId = Settings.CotpsPlans.First(sp => sp.Description == subToBuy).Id;
                        var result = await BillingHandler.Purchase(Settings.CotpsPlans.First(sp => sp.Description == subToBuy).Id);
                        if (result.Ok)
                        {
                            await App.Current.MainPage.DisplaySnackBarAsync(new SuccessSnackBar(result.Message));
                            // suscribed from now
                            return true;
                        }
                        else
                        {
                            await App.Current.MainPage.DisplaySnackBarAsync(new ErrorSnackBar(result.Message));
                            return false;
                        }
                    }
                    else
                    {
                        // subscribed
                        return true;
                    }
                }
                else
                {
                    await App.Current.MainPage.DisplaySnackBarAsync(new ErrorSnackBar(Translator.Translate("billing_service_not_available")));
                    return false;
                }
            }
            catch (Exception e)
            {
                await App.Current.MainPage.DisplaySnackBarAsync(new ErrorSnackBar(Translator.Translate("error_trying_buy_cotps")));
                return false;
            }
            finally
            {
                await BillingHandler.Disconnect();
            }
        }

        private async void SwitchClicked(object obj)
        {
            try
            {
                this.BotStarting = true;
            
                var svcRunning = DependencyService.Get<IBotService>().GetStatus();
            
                // allow stop but not start if not subscription
                if (!svcRunning)
                {
                    if (!await this.EnsureSubscription())
                    {
                        this.BotStarting = false;
                        return;
                    }
                }
            
                if (!this.AreFieldsValid() && !svcRunning)
                {
                    await App.Current.MainPage.DisplaySnackBarAsync(new ErrorSnackBar(Translator.Translate("phone_password_required")));
                    this.BotStarting = false;
                    return;
                }

                // save credentials in settings for api service
                if (!svcRunning)
                {
                    Settings.UserPhone = this.PhoneNumber.Value;
                    Settings.UserPassword = this.Password.Value;

                    // store credentials
                    if (this.RememberPassword)
                        await this.SaveFormData();
                }
            
                if (!svcRunning)
                {
                    DependencyService.Get<IBotService>().Start();
                    // this.SwitchMessage = "BOT STOP";
                }
                else
                {
                    DependencyService.Get<IBotService>().Stop();
                    // this.SwitchMessage = "BOT START";
                }
            
                if (DependencyService.Get<IBotService>().GetStatus())
                    await this.RefreshScreenData();
            
                this.BotStarting = false;
            }
            catch (TaskCanceledException e)
            {
                this.BotStarting = false;
                this.SwitchEnabled = true;
                DependencyService.Get<IBotService>().Stop();
                await App.Current.MainPage.DisplaySnackBarAsync(
                    new ErrorSnackBar(Translator.Translate("cant_get_data")));
            }
        }

        private async void RecoveryFormData()
        {
            var phoneNumber = await SecureStorage.GetAsync("phoneNumber");
            var password = await SecureStorage.GetAsync("password");
            var rememberPwd = await SecureStorage.GetAsync("rememberPwd");

            if (!string.IsNullOrEmpty(rememberPwd))
            {
                this.PhoneNumber.Value = phoneNumber ?? "";
                this.Password.Value = password ?? "";
            }

            this.RememberPassword = !string.IsNullOrEmpty(rememberPwd);
        }

        private async Task SaveFormData()
        {
            await SecureStorage.SetAsync("phoneNumber", this.PhoneNumber.Value);
            await SecureStorage.SetAsync("password", this.Password.Value);
            await SecureStorage.SetAsync("rememberPwd", this.RememberPassword ? "1" : "");
        }

        private static void RemoveFormData()
        {
            SecureStorage.RemoveAll();
        }

        private async Task RefreshScreenData()
        {
            // start data when open
            this.IsRunning = DependencyService.Get<IBotService>().GetStatus();
            this.SwitchMessage = Translator.Translate(this.IsRunning ? "BOT STOP" : "BOT START");
            this.LastRun = DependencyService.Get<IBotService>().GetLastRun();

            if (this.SwitchEnabled)
                this.SwitchEnabled = !DependencyService.Get<IBotService>().GetBusyStatus();

            if (this.IsRunning && this.SwitchEnabled)
            {
                if (Connectivity.NetworkAccess == NetworkAccess.Internet)
                {
                    this.Balance = await DependencyService.Get<IBotService>().GetBalance();
                }
                else
                {
                    if (App.Current.MainPage != null)
                        await App.Current.MainPage.DisplaySnackBarAsync(new ErrorSnackBar(Translator.Translate("cant_get_data")));
                }
                
            }
        }

        private async void InitializeProperties()
        {
            this.NoInternetConnection = Connectivity.NetworkAccess != NetworkAccess.Internet;
            this.PhoneNumber = new ValidatableObject<string>();
            this.Password = new ValidatableObject<string>();
            this.Balance = new TransactionsBalance();

            try
            {
                await this.RefreshScreenData();
            }
            catch (Exception exc)
            {
                string msg;
                if (exc is OperationCanceledException || exc is TimeoutException || exc is TaskCanceledException)
                {
                    msg = Translator.Translate("cant_get_data");
                }
                else
                {
                    msg = Translator.Translate("something_wrong_try_again");
                }
                if (App.Current.MainPage != null && msg != string.Empty)
                    await App.Current.MainPage.DisplaySnackBarAsync(new ErrorSnackBar(msg));
            }
        }

        private bool AreFieldsValid()
        {
            var isPhoneNumberValid = this.PhoneNumber.Validate();
            var isPasswordValid = this.Password.Validate();
            return isPhoneNumberValid && isPasswordValid;
        }
        
        private void AddValidationRules()
        {
            this.PhoneNumber.Validations.Add(new IsNotNullOrEmptyRule<string> { ValidationMessage = "Phone number Required" });
            this.Password.Validations.Add(new IsNotNullOrEmptyRule<string> { ValidationMessage = "Password Required" });
        }

        void HandleReceivedMessages()
        {
            MessagingCenter.Subscribe<ServiceMessage>(this, "ServiceMessage", message =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    this.IsRunning = message.IsRunning;
                    this.LastRun = message.LastRun;
                    this.SwitchMessage = Translator.Translate(message.IsRunning ? "BOT STOP" : "BOT START");
                });
            });
            
            MessagingCenter.Subscribe<TransactionsBalance>(this, "TransactionsBalance", message =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    this.Balance = message;
                });
            });
            
            MessagingCenter.Subscribe<BtnControlMessage>(this, "BtnControlMessage", message =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    this.SwitchEnabled = message.Status;
                });
            });
        }
        
        #endregion
    }
}