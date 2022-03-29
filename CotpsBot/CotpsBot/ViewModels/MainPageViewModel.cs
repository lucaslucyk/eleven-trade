using System;
using System.Diagnostics;
using CotpsBot.Validators;
using CotpsBot.Validators.Rules;
using Xamarin.Forms;

namespace CotpsBot.ViewModels
{
    public class MainPageViewModel: BaseViewModel
    {
        #region Fields

        private string taskMessage = String.Empty;
        private ValidatableObject<string> phoneNumber;
        private ValidatableObject<string> password;

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

        public void SwitchClicked(object obj)
        {
            this.DoTransactions();
        }

        private void DoTransactions()
        {
            
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