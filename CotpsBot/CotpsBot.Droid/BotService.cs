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
            if (intent.Action == "START_SERVICE")
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine("Service started");
#endif
                RegisterNotification();
            }
            else if (intent.Action == "STOP_SERVICE")
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine("Service stopped");
#endif
                StopForeground(true);
                StopSelfResult(startId);
            }

            return StartCommandResult.NotSticky;
        }

        private void RegisterNotification()
        {
            NotificationChannel channel =
                new NotificationChannel("ServiceChannel", "Bot Service", NotificationImportance.Max);
            NotificationManager manager =
                (NotificationManager) MainActivity.ActivityCurrent.GetSystemService(Context.NotificationService);
            manager.CreateNotificationChannel(channel);
            Notification notification = new Notification.Builder(this, "ServiceChannel")
                .SetContentTitle("COTPS Service working")
                .SetSmallIcon(Resource.Drawable.abc_star_black_48dp)
                .SetOngoing(true)
                .Build();

            StartForeground(100, notification);
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
            while (this.IsRunning)
            {
                if ((DateTime.Now - LastRun).TotalSeconds > Settings.ServiceInterval)
                {
                    this.LastRun = DateTime.Now;
                    await _apiBot.LoginAndOperate();
                    SendServiceMessage();
                }
                await Task.Delay(1000);
            }
            
            // send stop data
            SendServiceMessage();
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
            Intent startService = new Intent(MainActivity.ActivityCurrent, typeof(BotService));
            startService.SetAction("START_SERVICE");
            MainActivity.ActivityCurrent.StartService(startService);

            this.IsRunning = true;
            _cts = new CancellationTokenSource();
            Task.Run(MainLoop, _cts.Token);
        }

        public void Stop()
        {
            Intent stopIntent = new Intent(MainActivity.ActivityCurrent, this.Class);
            stopIntent.SetAction("STOP_SERVICE");
            MainActivity.ActivityCurrent.StartService(stopIntent);

            this.IsRunning = false;
            if (_cts != null && !_cts.IsCancellationRequested)
                _cts.Cancel();
        }
    }
}