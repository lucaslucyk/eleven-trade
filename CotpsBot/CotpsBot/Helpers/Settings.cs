using System;
using System.Collections.Generic;
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

        // service
        public static readonly double ServiceInterval = 300.0;

        // subscriptions
        public static readonly string[] CotpsDeprecatedSubs = {"cotps_service"};
        public static readonly string[] CotpsAvailableSubs = {
            "cotps_service_month",
            "cotps_service_year",
            "cotps_service_week"
        };
        
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