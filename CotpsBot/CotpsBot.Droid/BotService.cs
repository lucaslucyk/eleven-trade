using System;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using CotpsBot.Helpers;
using CotpsBot.Models;
using CotpsBot.Services;
using Plugin.LocalNotification;
using Xamarin.Forms;

[assembly:Xamarin.Forms.Dependency(typeof(CotpsBot.Droid.BotService))]
namespace CotpsBot.Droid
{
    [Service(ForegroundServiceType = Android.Content.PM.ForegroundService.TypeDataSync)]
    public class BotService : Service, IBotService
    {
        #region Fields

        CancellationTokenSource _cts;
        private ApiBot _apiBot = new ApiBot();

        #endregion

        #region Properties

        private bool IsRunning { get; set; } = false;
        private bool IsBusy { get; set; } = false;
        private DateTime LastRun { get; set; }

        #endregion

        public override IBinder OnBind(Intent intent)
        {
            throw new System.NotImplementedException();
        }

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags,
            int startId)
        {
            switch (intent.Action)
            {
                case "START_SERVICE":
                    RegisterNotification();
                    break;
                case "STOP_SERVICE":
                    StopForeground(true);
                    StopSelfResult(startId);
                    break;
            }

            return StartCommandResult.NotSticky;
        }

        private void RegisterNotification()
        {
            var channel = new NotificationChannel("ServiceChannel", "Bot Service", NotificationImportance.Max);
            channel.Importance = NotificationImportance.High;
            channel.EnableLights(true);
            channel.EnableVibration(true);
            channel.SetShowBadge(true);
            
            var manager = (NotificationManager) MainActivity.ActivityCurrent.GetSystemService(Context.NotificationService);
            manager?.CreateNotificationChannel(channel);
            
            var notification = new Notification.Builder(this, "ServiceChannel")
                .SetContentTitle("COTPS Service")
                .SetContentText("COTPS Service is working.")
                .SetSmallIcon(Resource.Drawable.eleven_trade_icon_small)
                .SetOngoing(true)
                .Build();

            Device.BeginInvokeOnMainThread(() =>
            {
                StartForeground(100, notification);
            });
        }

        private void SendServiceMessage()
        {
            var message = new ServiceMessage
            {
                IsRunning = this.IsRunning,
                LastRun = DateTime.Now
            };
            Device.BeginInvokeOnMainThread(() =>
            {
                MessagingCenter.Send<ServiceMessage>(message, "ServiceMessage");
            });
        }

        private async void MainLoop()
        {
            try
            {
                while (this.IsRunning)
                {
                    if ((DateTime.Now - LastRun).TotalSeconds > Settings.ServiceInterval)
                    {
                        this.IsBusy = true;
                        this.LastRun = DateTime.Now;
                        await _apiBot.LoginAndOperate();
                        SendServiceMessage();
                        this.IsBusy = false;
                    }
                    await Task.Delay(1000);
                }
                
                // send stop data
                SendServiceMessage();
            }
            catch (Exception e)
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    var notification = new NotificationRequest
                    {
                        BadgeNumber = 1,
                        Description = $"Restarting COTPS service. Reason: {e.Message}",
                        Title = "COTPS Service",
                        ReturningData = "COTPS Service",
                        NotificationId = Settings.Notifications.ServiceError
                    };
                    await NotificationCenter.Current.Show(notification);
                    
                });

                try
                {
                    Stop();
                    Start();
                }
                catch (Exception exception)
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        var restartNotify = new NotificationRequest
                        {
                            BadgeNumber = 1,
                            Description = $"COTPS Service could not be restarted. Reason: {exception.Message}",
                            Title = "COTPS Restart Error",
                            ReturningData = "COTPS Restart Error",
                            NotificationId = Settings.Notifications.RestartError
                        };
                        await NotificationCenter.Current.Show(restartNotify);
                    });
                }

                // await Task.Delay(200);
                // // restart
                // await Restart();
            }
        }

        public bool GetBusyStatus()
        {
            return this.IsBusy;
        }
        public bool GetStatus()
        {
            return this.IsRunning;
        }

        public DateTime GetLastRun()
        {
            return this.LastRun;
        }

        public async Task<TransactionsBalance> GetBalance()
        {
            return await _apiBot.LoginAndGetBalance();
        }
        
        public void Start()
        {
            var startService = new Intent(MainActivity.ActivityCurrent, typeof(BotService));
            startService.SetAction("START_SERVICE");
            MainActivity.ActivityCurrent.StartService(startService);

            this.IsRunning = true;
            _cts = new CancellationTokenSource();
            Task.Run(MainLoop, _cts.Token);
        }

        public void Stop()
        {
            var stopIntent = new Intent(MainActivity.ActivityCurrent, this.Class);
            stopIntent.SetAction("STOP_SERVICE");
            MainActivity.ActivityCurrent.StartService(stopIntent);

            this.IsRunning = false;
            if (_cts != null && !_cts.IsCancellationRequested)
                _cts.Cancel();
        }

        public async Task Restart()
        {
            try
            {
                Stop();
                await Task.Delay(200);
                Start();
            }
            catch (Exception exception)
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    var restartNotify = new NotificationRequest
                    {
                        BadgeNumber = 1,
                        Description = $"COTPS Service could not be restarted. Reason: {exception.Message}",
                        Title = "COTPS Restart Error",
                        ReturningData = "COTPS Restart Error",
                        NotificationId = Settings.Notifications.RestartError
                    };
                    await NotificationCenter.Current.Show(restartNotify);
                });
            }
        }
    }
}