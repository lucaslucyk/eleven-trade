using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Timers;
using CotpsBot.Models;
using CotpsBot.Models.Http;
using CotpsBot.Services.Http;
using CotpsBot.Validators;
using CotpsBot.Validators.Rules;
using OutsideWorks.Helpers;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.Essentials;
using Xamarin.Forms;
using Timer = System.Timers.Timer;

namespace CotpsBot.ViewModels
{
    public class MainPageViewModel: BaseViewModel
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
        
        private readonly Timer _timer;
        private static IRequestService ApiClient => DependencyService.Get<IRequestService>();

        #endregion
        
        #region Constructor

        public MainPageViewModel()
        {
            this.SwitchCommand = new Command(this.SwitchClicked);
            this.RememberPasswordCommand = new Command(this.RememberTapped);
            
            this.InitializeProperties();
            this.AddValidationRules();
            this.RecoveryFormData();
            
            this._timer = new Timer(300000);
            this._timer.Elapsed += TimerElapsed;
            this._timer.AutoReset = true;
            
            // try to start
            this.TryToStart();
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
        
        private async void TimerElapsed(object source, ElapsedEventArgs e)
        {
            await this.CotpsOperate();
        }
        
        private async void RememberTapped(object obj)
        {
            this.RememberPassword = !this.RememberPassword;
            if (!this.RememberPassword)
                RemoveFormData();
        }

        private async void TryToStart()
        {
            if (this.AreFieldsValid())
                await BotStart();
        }
        private async Task BotStart()
        {
            await this.CotpsOperate(true);
            if (this.IsRunning)
                this._timer.Start();
        }
        private void BotStop()
        {
            this._timer.Stop();
            this.IsRunning = false;
            this.TaskMessage = "Stopped";
            this.SwitchMessage = "Bot Start";
        }
        public async void SwitchClicked(object obj)
        {
            this.BotStarting = true;
            if (this.AreFieldsValid())
            {
                if (this.RememberPassword)
                    await this.SaveFormData();

                if (!this.IsRunning)
                {
                    await this.BotStart();
                }
                else
                {
                    this.BotStop();
                }
            }
            else
            {
                await App.Current.MainPage.DisplaySnackBarAsync(new ErrorSnackBar("Check your credentials."));
            }
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

        private void UpdateBalance(BalanceInfo data)
        {
            this.Balance.Total = data.total_balance;
            this.Balance.Freeze = data.freeze_balance;
            this.Balance.Free = data.balance;
            
            OnPropertyChanged("Balance");
        }

        private async Task Operate()
        {
            this.TaskMessage = "Creating order...";
            var order = await ApiClient.CreateOrder();
            if (order.success)
            {
                this.TaskMessage = $"Confirming order {order.data.orderId}...";
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
                UpdateBalance(balanceResponse.userinfo);

                var converted = Convert.ToDouble(balanceResponse.userinfo.balance);
                if (converted >= 5.0)
                {
                    await Operate();
                }
            }

            this.TaskMessage = "Running! :)";
        }
        
        private async Task CotpsOperate(bool starting = false)
        {
            try
            {
                this.SwitchEnabled = false;
                this.TaskMessage = "Logging into COTPS...";
                var form = new LoginRequest
                {
                    mobile = this.PhoneNumber.Value,
                    password = this.Password.Value,
                    type = Settings.APILoginType
                };
                var loginResult = await ApiClient.LoginAsync(form);
                if (loginResult.success)
                {
                    this.IsRunning = true;
                    this.TaskMessage = "Running! :)";
                    this.SwitchMessage = "Bot Stop";
                    this.LastRun = DateTime.Now;
                    
                    // try to create and confirm orders
                    await this.DoTransactions();
                    
                    // logout after of do actions
                    ApiClient.Logout();
                }
                else
                {
                    
                    this.TaskMessage = "COTPS Login error.";
                    this.SwitchMessage = "Bot Stop";
                    this.LastRun = DateTime.Now;
                    await App.Current.MainPage.DisplaySnackBarAsync(new ErrorSnackBar("COTPS login error."));
                    if (starting)
                    {
                        this.IsRunning = false;
                        this.BotStop();
                    }
                    else
                    {
                        this.IsRunning = true;
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
                this.SwitchEnabled = true;
            }
        }
        
        private void InitializeProperties()
        {
            this.PhoneNumber = new ValidatableObject<string>();
            this.Password = new ValidatableObject<string>();
            this.Balance = new TransactionsBalance();
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
        
        #endregion
    }
}