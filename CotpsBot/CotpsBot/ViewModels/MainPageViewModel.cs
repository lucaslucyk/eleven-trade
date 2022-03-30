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
        
        private string taskMessage = "Stopped";
        private string switchMessage = "Bot Start";
        private bool switchEnabled = true;
        private bool isRunning;
        private bool botStarting;
        private ValidatableObject<string> phoneNumber;
        private ValidatableObject<string> password;
        private DateTime lastRun;
        private bool rememberPassword;
        private TransactionsBalance balance;
        
        private Timer timer;
        public IRequestService ApiClient => DependencyService.Get<IRequestService>();

        #endregion
        
        #region Constructor

        public MainPageViewModel()
        {
            this.SwitchCommand = new Command(this.SwitchClicked);
            this.RememberPasswordCommand = new Command(this.RememberTapped);
            
            this.InitializeProperties();
            this.AddValidationRules();
            this.RecoveryFormData();
            
            this.timer = new Timer(60000);
            this.timer.Elapsed += TimerElapsed;
            this.timer.AutoReset = true;
        }

        #endregion

        #region Properties
        
        public string SwitchMessage
        {
            get => this.switchMessage;
            set => this.SetProperty(ref this.switchMessage, value);
        }
        public bool SwitchEnabled
        {
            get { return this.switchEnabled; }
            set { this.SetProperty(ref this.switchEnabled, value); }
        }
        public bool RememberPassword
        {
            get { return this.rememberPassword; }
            set { this.SetProperty(ref this.rememberPassword, value); }
        }
        
        public TransactionsBalance Balance
        {
            get => this.balance;
            set => this.SetProperty(ref this.balance, value);
        }
        public DateTime LastRun
        {
            get => this.lastRun;
            set => this.SetProperty(ref this.lastRun, value);
        }
        
        public bool IsRunning
        {
            get => this.isRunning;
            set => this.SetProperty(ref this.isRunning, value);
        }
        
        public bool BotStarting
        {
            get => this.botStarting;
            set => this.SetProperty(ref this.botStarting, value);
        }

        public string TaskMessage
        {
            get => this.taskMessage;
            set => this.SetProperty(ref this.taskMessage, value);
        }
        
        public ValidatableObject<string> PhoneNumber
        {
            get { return this.phoneNumber; }
            set { this.SetProperty(ref this.phoneNumber, value); }
        }
        public ValidatableObject<string> Password
        {
            get { return this.password; }
            set { this.SetProperty(ref this.password, value); }
        }

        #endregion
        
        #region Commands
        
        public Command RememberPasswordCommand { get; set; }
        public Command SwitchCommand { get; set; }

        #endregion

        #region Methods
        
        private async void TimerElapsed(object source, ElapsedEventArgs e)
        {
            await this.DoTransactions();
        }
        
        private async void RememberTapped(object obj)
        {
            this.RememberPassword = !this.RememberPassword;
            if (!this.RememberPassword)
                this.RemoveFormData();
        }

        private async Task BotStart()
        {
            await this.DoTransactions();
            this.timer.Start();
        }
        private void BotStop()
        {
            this.timer.Stop();
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
            var _phoneNumber = await SecureStorage.GetAsync("phoneNumber");
            var _password = await SecureStorage.GetAsync("password");
            var _rememberPwd = await SecureStorage.GetAsync("rememberPwd");

            if (!String.IsNullOrEmpty(_rememberPwd))
            {
                this.PhoneNumber.Value = _phoneNumber == null ? "" : _phoneNumber;
                this.Password.Value = _password == null ? "" : _password;
            }

            this.RememberPassword = !String.IsNullOrEmpty(_rememberPwd);
        }
        private async Task SaveFormData()
        {
            await SecureStorage.SetAsync("phoneNumber", this.PhoneNumber.Value);
            await SecureStorage.SetAsync("password", this.Password.Value);
            
            await SecureStorage.SetAsync("rememberPwd", this.RememberPassword ? "1" : "");
        }
        
        private void RemoveFormData()
        {
            SecureStorage.RemoveAll();
        }
        
        private async Task DoTransactions()
        {
            try
            {
                this.SwitchEnabled = false;
                this.TaskMessage = "Loging to COTPS...";
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
                    
                    // logout after of do actions
                    ApiClient.Logout();
                }
                else
                {
                    this.IsRunning = true;
                    this.TaskMessage = "COTPS Login error.";
                    this.SwitchMessage = "Bot Stop";
                    this.LastRun = DateTime.Now;
                    await App.Current.MainPage.DisplaySnackBarAsync(new ErrorSnackBar("COTPS login error."));
                    // this.BotStop();
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
        }
        
        public bool AreFieldsValid()
        {
            bool isPhoneNumberValid = this.PhoneNumber.Validate();
            bool isPasswordValid = this.Password.Validate();
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