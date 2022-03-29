using System;
using Plugin.Settings;
using Plugin.Settings.Abstractions;

namespace OutsideWorks.Helpers
{
    public static class Settings
    {
        private static ISettings AppSettings
        {
            get { return CrossSettings.Current; }
        }

        #region API

        public static readonly string APIBaseUrl = "https://www.cotps.com:8443";
        public static readonly string APILoginType = "mobile";
        public static readonly string APILoginUrl = "/api/mine/sso/user_login_check";
        public static readonly string APIBalanceUrl = "/api/mine/user/getDealInfo";
        public static readonly string APIOrderCreateUrl = "/api/mine/user/createOrder";
        public static readonly string APIOrderSubmitUrl = "/api/mine/user/submitOrder";

        #endregion
    }
}