﻿using System;
using CotpsBot.Services;
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
[assembly: ExportFont("MaterialIconsOutlined-Regular.otf", Alias = "MIOR")]
[assembly: ExportFont("MaterialIcons-Regular.ttf", Alias = "MIR")]
[assembly: ExportFont("MaterialIconsRound-Regular.otf", Alias = "MIRR")]
[assembly: ExportFont("MaterialIconsSharp-Regular.otf", Alias = "MISR")]
[assembly: ExportFont("MaterialIconsTwoTone-Regular.otf", Alias = "MITTR")]
namespace CotpsBot
{
    public partial class App : Application
    {
        #region Fields

        public static CodeTranslator Translator;
        public static RequestService ApiClient;
        public static BillingService BillingHandler;

        #endregion
        public App()
        {
            InitializeComponent();

            Translator = new CodeTranslator();
            DependencyService.RegisterSingleton<ICodeTranslator>(Translator);
            
            ApiClient = new RequestService();
            DependencyService.RegisterSingleton<IRequestService>(ApiClient);

            BillingHandler = new BillingService();
            DependencyService.RegisterSingleton<IBillingService>(BillingHandler);

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