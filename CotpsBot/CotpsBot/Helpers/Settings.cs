using System;
using Plugin.Settings;
using Plugin.Settings.Abstractions;

namespace CotpsBot.Helpers
{
    public static class Settings
    {
        private static ISettings AppSettings => CrossSettings.Current;

        #region API

        public static readonly string APIBaseUrl = "https://www.cotps.com:8443";
        public static readonly string APILoginType = "mobile";
        public static readonly string APILoginUrl = "/api/mine/sso/user_login_check";
        public static readonly string APIBalanceUrl = "/api/mine/user/getDealInfo";
        public static readonly string APIOrderCreateUrl = "/api/mine/user/createOrder";
        public static readonly string APIOrderSubmitUrl = "/api/mine/user/submitOrder";

        #endregion

        #region Constants

        public static readonly double ServiceInterval = 60.0;

        #endregion

        #region Credentials

        public static string UserPhone
        {
            get => AppSettings.GetValueOrDefault(nameof(UserPhone), string.Empty);
            set => AppSettings.AddOrUpdateValue(nameof(UserPhone), value);
        }
        
        public static string UserPassword
        {
            get => AppSettings.GetValueOrDefault(nameof(UserPassword), string.Empty);
            set => AppSettings.AddOrUpdateValue(nameof(UserPassword), value);
        }

        #endregion
    }
}