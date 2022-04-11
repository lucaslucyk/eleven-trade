using System;
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


namespace CotpsBot.ViewModels
{
    public class MainPageViewModel : BaseViewModel
    {
        #region Fields

        private string _taskMessage = "Stopped";
        private string _switchMessage = "Bot Start";
        private bool _switchEnabled = true;
        private bool _isRunning;
        private bool _botStarting;
        private ValidatableObject<string> _phoneNumber;
        private ValidatableObject<string> _password;
        private DateTime _lastRun;
        private bool _rememberPassword;
        private TransactionsBalance _balance;
        private static IBillingService BillingHandler => DependencyService.Get<IBillingService>();

        #endregion

        #region Constructor

        public MainPageViewModel()
        {
            this.SwitchCommand = new Command(this.SwitchClicked);
            this.RememberPasswordCommand = new Command(this.RememberTapped);

            this.InitializeProperties();
            this.AddValidationRules();
            this.RecoveryFormData();

            HandleReceivedMessages();
        }

        #endregion

        #region Properties

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

        #endregion

        #region Methods

        private void RememberTapped(object obj)
        {
            this.RememberPassword = !this.RememberPassword;
            if (!this.RememberPassword)
                RemoveFormData();
        }

        private async Task<bool> EnsureSuscribtion()
        {
            try
            {
                await BillingHandler.Connect();
                if (BillingHandler.IsConnected)
                {
                    if (!await BillingHandler.CheckBuy())
                    {
                        var result = await BillingHandler.Purchase();
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
                    await App.Current.MainPage.DisplaySnackBarAsync(new ErrorSnackBar("Billing service not available."));
                    return false;
                }
            }
            catch (Exception e)
            {
                await App.Current.MainPage.DisplaySnackBarAsync(new ErrorSnackBar("An error occurred while trying to buy the COTPS service."));
                return false;
            }
            finally
            {
                await BillingHandler.Disconnect();
            }
        }

        public async void SwitchClicked(object obj)
        {
            this.BotStarting = true;

            if (!await this.EnsureSuscribtion())
            {
                this.BotStarting = false;
                return;
            }
            
            var svcRunning = DependencyService.Get<IBotService>().GetStatus();
            if (!this.AreFieldsValid() && !svcRunning)
            {
                await App.Current.MainPage.DisplaySnackBarAsync(new ErrorSnackBar("Phone and password required."));
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

        private async void RecoveryFormData()
        {
            var phoneNumber = await SecureStorage.GetAsync("phoneNumber");
            var password = await SecureStorage.GetAsync("password");
            var rememberPwd = await SecureStorage.GetAsync("rememberPwd");

            if (!String.IsNullOrEmpty(rememberPwd))
            {
                this.PhoneNumber.Value = phoneNumber == null ? "" : phoneNumber;
                this.Password.Value = password == null ? "" : password;
            }

            this.RememberPassword = !String.IsNullOrEmpty(rememberPwd);
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
            this.SwitchMessage = this.IsRunning ? "BOT STOP" : "BOT START";
            this.LastRun = DependencyService.Get<IBotService>().GetLastRun();

            if (this.IsRunning && this.SwitchEnabled)
                this.Balance = await DependencyService.Get<IBotService>().GetBalance();
        }

        private async void InitializeProperties()
        {
            this.PhoneNumber = new ValidatableObject<string>();
            this.Password = new ValidatableObject<string>();
            this.Balance = new TransactionsBalance();

            await this.RefreshScreenData();
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
                    this.SwitchMessage = message.IsRunning ? "BOT STOP" : "BOT START";
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