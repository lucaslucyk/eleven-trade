using System;
using CotpsBot.Services.Http;
using CotpsBot.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]

namespace CotpsBot
{
    public partial class App : Application
    {
        #region Fields

        public static RequestService ApiClient;

        #endregion
        public App()
        {
            InitializeComponent();

            ApiClient = new RequestService();
            DependencyService.RegisterSingleton<IRequestService>(ApiClient);

            MainPage = new MainPage();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}