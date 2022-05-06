using System;
using System.Collections.Generic;
using CotpsBot.Models;
using Plugin.Settings;
using Plugin.Settings.Abstractions;

namespace CotpsBot.Helpers
{
    public static class Settings
    {
        private static ISettings AppSettings => CrossSettings.Current;

        #region API

        public static readonly TimeSpan APITimeOut = TimeSpan.FromSeconds(10);
        public static readonly string APIBaseUrl = "https://www.cotps.com:8443";
        public static readonly string APILoginType = "mobile";
        public static readonly string APILoginUrl = "/api/mine/sso/user_login_check";
        public static readonly string APIBalanceUrl = "/api/mine/user/getDealInfo";
        public static readonly string APIOrderCreateUrl = "/api/mine/user/createOrder";
        public static readonly string APIOrderSubmitUrl = "/api/mine/user/submitOrder";
        public static readonly string APITeamInfoUrl = "/api/mine/user/getShareDetal";
        public static readonly string APIReceiveProfitUrl = "/api/mine/user/receiveProfit";

        #endregion

        #region Constants
        
        // base
        public static readonly string CurrentVersion = "1.7.15";
        
        // order confirm attempts
        public static readonly int MaxOrderConfirmAttempts = 5;

        // notifications
        public static readonly NotificationsIds Notifications = new NotificationsIds();

        // service
        public static readonly double ServiceInterval = 300.0;
        public static readonly int[] TeamLevels = {1, 2, 3};

        // subscriptions
        public static readonly SubscriptionPlan[] CotpsPlans = {
            new SubscriptionPlan{
                Id = "et_cotps_year",
                Name = "COTPS Service (weekly)",
                Description = "Weekly (9 U$D/week)",
                Price = "9.0",
                Interval = "Week"
            },
            new SubscriptionPlan
            {
                Id = "cotps_service",
                Name = "COTPS Service (monthly)",
                Description = "Monthly (30 U$D/month)",
                Price = "30.0",
                Interval = "Month"
            },
            new SubscriptionPlan{
                Id = "et_cotps_week",
                Name = "COTPS Service (yearly)",
                Description = "Yearly (300 U$D/year)",
                Price = "300.0",
                Interval = "Year"
            },
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