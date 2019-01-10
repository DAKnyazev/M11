﻿using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;

namespace M11.Droid
{
    [Activity(Label = "M11", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        public static string ChannelId = "M11-15-58-Knyazev-ChannelId";
        private int checkBalanceIntervalInMinutes = 15;

        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);
            LoadApplication(new App());

            CreateNotificationChannel();
            StartScheduledTask();
        }

        private void StartScheduledTask()
        {
            var intentAlarm = new Intent(this, typeof(BalanceBroadcastReceiver));

            var alarmManager = (AlarmManager)GetSystemService(Context.AlarmService);
            //var interval = 10000;

            alarmManager.SetRepeating(
                AlarmType.RtcWakeup, 
                SystemClock.ElapsedRealtime() + checkBalanceIntervalInMinutes * 60 * 1000, 
                checkBalanceIntervalInMinutes * 60 * 1000, 
                PendingIntent.GetBroadcast(this, 1, intentAlarm, PendingIntentFlags.CancelCurrent));
        }

        private void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                // Notification channels are new in API 26 (and not a part of the
                // support library). There is no need to create a notification
                // channel on older versions of Android.
                return;
            }

            var channelName = "Изменение баланса";
            var channelDescription = "M11TestChannelDescription";
            var channel = new NotificationChannel(ChannelId, channelName, NotificationImportance.Default)
            {
                Description = channelDescription
            };

            var notificationManager = (NotificationManager)GetSystemService(NotificationService);
            notificationManager.CreateNotificationChannel(channel);
        }
    }
}

