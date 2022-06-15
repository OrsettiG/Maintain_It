using System;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;

using AndroidX.Core.App;

using Maintain_it.Models;
using Maintain_it.Services;

using Xamarin.Forms;

using AndroidApp = Android.App.Application;

[assembly: Dependency( typeof( Maintain_it.Droid.AndroidNotificationManager ) )]
namespace Maintain_it.Droid
{
    class AndroidNotificationManager : INotificationManager
    {
        #region Constants
        const string channelId = "default";
        const string channelName = "Default";
        const string channelDescription = "Default notification channel.";

        public const string TitleKey = "title";
        public const string MessageKey = "message";
        public const string IdKey = "id";
        #endregion

        #region Fields
        bool channelInitialized = false;
        int messageId = 0;
        int pendingIntentId = 0;

        NotificationManager manager;

        public event EventHandler NotificationRecieved;
        #endregion

        public static AndroidNotificationManager Instance { get; private set; }

        #region Initialization

        public AndroidNotificationManager() => Initialize();

        public void Initialize()
        {
            if( Instance == null )
            {
                CreateNotificationChannel();
                Instance = this;
            }
        }

        #endregion

        #region Notification Handling

        // Schedules/Sends the notification to the Android system.
        public void SendNotification( string title, string message, DateTime notifyTime = default )
        {
            if( !channelInitialized )
            {
                CreateNotificationChannel();
            }

            if( notifyTime != DateTime.MinValue )
            {
                Intent intent = new Intent(AndroidApp.Context, typeof(AlarmHandler));
                _ = intent.PutExtra( TitleKey, title ).PutExtra( MessageKey, message );

                PendingIntent pendingIntent = PendingIntent.GetBroadcast(AndroidApp.Context, pendingIntentId++, intent, PendingIntentFlags.CancelCurrent );

                long triggerTime = GetNotifyTime( notifyTime );
                AlarmManager alarmManager = AndroidApp.Context.GetSystemService(Context.AlarmService) as AlarmManager;
                alarmManager.SetAndAllowWhileIdle( AlarmType.RtcWakeup, triggerTime, pendingIntent );
                //alarmManager.Set(  );
            }
            else
            {
                Show( title, message );
            }
        }

        public void RecieveNotification( string title, string message )
        {
            NotificationEventArgs args = new NotificationEventArgs()
            {
                Name = title,
                Message = message
            };

            NotificationRecieved?.Invoke( null, args );
        }

        public void Show( string title, string message )
        {
            Intent intent = new Intent(AndroidApp.Context, typeof(MainActivity) );
            _ = intent.PutExtra( TitleKey, title ).PutExtra( MessageKey, message );

            PendingIntent pendingIntent = PendingIntent.GetActivity(AndroidApp.Context, pendingIntentId++, intent, PendingIntentFlags.UpdateCurrent);

            NotificationCompat.Builder builder = new NotificationCompat.Builder(AndroidApp.Context, channelId)
                .SetContentIntent(pendingIntent)
                .SetContentTitle(title)
                .SetContentText(message)
                .SetLargeIcon(BitmapFactory.DecodeResource(AndroidApp.Context.Resources, Resource.Drawable.xamarin_logo))
                .SetSmallIcon(Resource.Drawable.xamarin_logo)
                .SetDefaults((int)NotificationDefaults.Sound | (int)NotificationDefaults.Vibrate);

            Notification notification = builder.Build();
            manager.Notify( messageId++, notification );
        }

        #endregion

        private void CreateNotificationChannel()
        {
            manager = (NotificationManager)AndroidApp.Context.GetSystemService( AndroidApp.NotificationService );

            if( Build.VERSION.SdkInt >= BuildVersionCodes.O )
            {
                Java.Lang.String channelNameJava = new Java.Lang.String(channelName);
                NotificationChannel channel = new NotificationChannel( channelId, channelNameJava, NotificationImportance.Default)
                {
                    Description = channelDescription
                };

                manager.CreateNotificationChannel( channel );
            }

            channelInitialized = true;
        }

        private void ScheduleNotificationJob( int num )
        {
            PersistableBundle jobParams = new PersistableBundle();
            jobParams.PutInt( "num", num );
        }

        // Epoch time conversion to milliseconds
        private long GetNotifyTime( DateTime notifyTime )
        {
            DateTime utcTime = TimeZoneInfo.ConvertTimeToUtc(notifyTime);
            // Number of seconds in the Unix Epoch (subtract 0 to turn it into a timespan so we can get the total seconds)
            double epochDiff = (DateTime.UnixEpoch - DateTime.MinValue).TotalSeconds;
            // The number of Milliseconds after the Unix Epoch that we want to set the Alarm for
            // i.e.
            // Epoch = 500
            // Alarm = 800
            // utcAlarmTime = 800 - 500 = 300
            //
            long utcAlarmTime = utcTime.AddSeconds(-epochDiff).Ticks / TimeSpan.TicksPerMillisecond;
            return utcAlarmTime;
        }
    }
}