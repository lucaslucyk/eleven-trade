using System;
using System.Diagnostics;
using System.Threading.Tasks;
using CotpsBot.Models.Http;
using CotpsBot.Services.Http;
using CotpsBot.Validators;
using CotpsBot.Validators.Rules;
using OutsideWorks.Helpers;
using Xamarin.Forms;

namespace CotpsBot.ViewModels
{
    public class MainPageViewModel: BaseViewModel
    {
        #region Fields
        
        private string taskMessage = String.Empty;
        private bool isRunning;
        private ValidatableObject<string> phoneNumber;
        private ValidatableObject<string> password;
        public IRequestService ApiClient => DependencyService.Get<IRequestService>();

        #endregion
        
        #region Constructor

        public MainPageViewModel()
        {
            this.SwitchCommand = new Command(this.SwitchClicked);
            
            this.InitializeProperties();
            this.AddValidationRules();
        }

        #endregion

        #region Properties

        public bool IsRunning
        {
            get => this.isRunning;
            set => this.SetProperty(ref this.isRunning, value);
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

        public Command SwitchCommand { get; set; }

        #endregion

        #region Methods

        public async void SwitchClicked(object obj)
        {
            this.IsRunning = !this.IsRunning;
            await this.DoTransactions();
        }

        private async Task DoTransactions()
        {
            try
            {
                this.TaskMessage = "Loging to COTPS...";
                var form = new LoginRequest
                {
                    mobile = this.PhoneNumber.Value,
                    password = this.Password.Value,
                    type = Settings.APILoginType
                };
                var loginResult = await ApiClient.LoginAsync(form);
                if (loginResult != null)
                {
                    this.TaskMessage = "Bot running! :)";
                }
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
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