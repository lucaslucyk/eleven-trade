using System;
using CotpsBot.Services.Http;
using CotpsBot.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
[assembly: ExportFont("Montserrat-Bold.ttf",Alias="Montserrat-Bold")]
[assembly: ExportFont("Montserrat-Medium.ttf", Alias="Montserrat-Medium")]
[assembly: ExportFont("Montserrat-Regular.ttf", Alias="Montserrat-Regular")]
[assembly: ExportFont("Montserrat-SemiBold.ttf", Alias="Montserrat-SemiBold")]
[assembly: ExportFont("TitilliumWeb-Regular.ttf", Alias="TitilliumWeb-Regular")]
[assembly: ExportFont("TitilliumWeb-Bold.ttf", Alias="TitilliumWeb-Bold")]
[assembly: ExportFont("TitilliumWeb-SemiBold.ttf", Alias="TitilliumWeb-SemiBold")]
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