using System;
using System.Linq;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Service.Notification;

using AndroidX.Core.App;

using Maintain_it.Helpers;
using Maintain_it.Models;
using Maintain_it.Services;

using Xamarin.Forms;

using static Maintain_it.Helpers.Config;

using AndroidApp = Android.App.Application;

[assembly: Dependency( typeof( Maintain_it.Droid.Notifications.AndroidNotificationManager) )]
namespace Maintain_it.Droid.Notifications
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
        int mainActivityPendingIntentId = 0;
        int notificationActionPendingIntentId = 0;

        NotificationManager manager;

        public event EventHandler NotificationReceived;
        #endregion

        // Wanted to make this a self initializing singleton, but Android throws an error when I do that, so it is just going to have to be null checked every time it is called.
        private static AndroidNotificationManager instance;
        public static AndroidNotificationManager Instance { get => instance ??= new AndroidNotificationManager(); }

        #region Initialization

        public AndroidNotificationManager() => Initialize();

        public void Initialize()
        {
            CreateNotificationChannel();
        }

        #endregion

        #region Notification Handling

        // Schedules/Sends the notification to the Android system.
        public void SendNotification( string title, string message, int notificationId, DateTime notifyTime = default )
        {
            if( !channelInitialized )
            {
                CreateNotificationChannel();
            }

            if( notifyTime != DateTime.MinValue )
            {
                Intent intent = new Intent(AndroidApp.Context, typeof(AlarmHandler));
                _ = intent.PutExtra( Config.TitleKey, title ).PutExtra( Config.MessageKey, message ).PutExtra(Config.NotificationIdKey, notificationId);

                PendingIntent pendingIntent = PendingIntent.GetBroadcast(AndroidApp.Context, mainActivityPendingIntentId++, intent, PendingIntentFlags.CancelCurrent );

                long triggerTime = GetNotifyTime( notifyTime );
                AlarmManager alarmManager = AndroidApp.Context.GetSystemService(Context.AlarmService) as AlarmManager;
                alarmManager.SetAndAllowWhileIdle( AlarmType.RtcWakeup, triggerTime, pendingIntent );
            }
            else
            {
                Show( title, message, notificationId );
            }
        }

        public void ReceiveNotification( string title, string message )
        {
            NotificationEventArgs args = new NotificationEventArgs()
            {
                Name = title,
                Message = message
            };

            NotificationReceived?.Invoke( null, args );
        }

        public void Show( string title, string message, int notificationId )
        {
            // Main Notification Intent
            Intent intent = new Intent(AndroidApp.Context, typeof(MainActivity) );
            _ = intent.PutExtra( Config.TitleKey, title ).PutExtra( Config.MessageKey, message ).PutExtra(IdKey, notificationId );

            PendingIntent pendingIntent = PendingIntent.GetActivity(AndroidApp.Context, mainActivityPendingIntentId++, intent, PendingIntentFlags.UpdateCurrent);

            NotificationCompat.Builder builder = new NotificationCompat.Builder(AndroidApp.Context, channelId)
                .SetContentIntent(pendingIntent)
                .SetContentTitle(title)
                .SetLargeIcon(BitmapFactory.DecodeResource(AndroidApp.Context.Resources, Resource.Drawable.xamarin_logo))
                .SetSmallIcon(Resource.Drawable.xamarin_logo)
                .SetDefaults((int)NotificationDefaults.Sound);

            // Big Text Style
            NotificationCompat.BigTextStyle bigText = new NotificationCompat.BigTextStyle();

            _ = bigText.BigText( message ).SetSummaryText( "Upcoming Maintenance." );
            _ = builder.SetStyle( bigText );

            // Remind Me Later Action
            _ = builder.AddAction( CreateNotificationAction( NotificationActions.REMIND_ME_LATER, notificationId ) );

            // Don't Remind Me Action
            _ = builder.AddAction( CreateNotificationAction( NotificationActions.DO_NOT_REMIND_ME, notificationId ) );


            Notification notification = builder.Build();
            manager.Notify( messageId++, notification );
        }

        public bool Cancel( int id )
        {

            StatusBarNotification[] notifications = manager.GetActiveNotifications();

            StatusBarNotification notification = notifications.Where(x => x.Id == id).SingleOrDefault();

            if( notification != default )
            {
                manager.Cancel( id );
                return true;
            }

            return false;
        }

        private NotificationCompat.Action CreateNotificationAction( NotificationActions action, int notificationId )
        {
            // Create our intent in this context and of the NotificationJobService Type so that it we can pick it up when the button is clicked.
            Intent actionIntent = new Intent(AndroidApp.Context, typeof(NotificationJobService));

            // Add the notificationId to intent so that if the user clicks this button we know which notification to cancel.
            _ = actionIntent.PutExtra( NotificationIdKey, notificationId ).PutExtra(MessageIdKey, messageId);

            // Add a const value to the button click so that when the user clicks it we know to just clear the notification.
            _ = actionIntent.SetAction( action.ToString() );

            // Create the pendingActionIntent. The pending intent id ensures that we always get a unique intent.
            PendingIntent pendingActionIntent = PendingIntent.GetService(AndroidApp.Context, notificationActionPendingIntentId++, actionIntent, PendingIntentFlags.UpdateCurrent);

            // Return the action
            string buttonText = action.ToString().ToLowerInvariant().Replace("_", " ").FirstLetterToUpper().Trim();

            return new NotificationCompat.Action.Builder( 1, buttonText, pendingActionIntent ).Build();
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
            // utcAlarmTime = 800 - 500 = 300 AFTER the epoch (basically treats the epoch as zero).
            long utcAlarmTime = utcTime.AddSeconds(-epochDiff).Ticks / TimeSpan.TicksPerMillisecond;
            return utcAlarmTime;
        }
    }
}