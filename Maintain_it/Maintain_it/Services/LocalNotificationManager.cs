using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Maintain_it.Helpers;
using Maintain_it.Models;

using Xamarin.Forms;

using static Maintain_it.Helpers.Config;

namespace Maintain_it.Services
{
    public static class LocalNotificationManager
    {
        private static INotificationManager notificationManager;

        static LocalNotificationManager()
        {
            notificationManager = DependencyService.Get<INotificationManager>();
            notificationManager.NotificationRecieved += ( sender, args ) =>
            {
                NotificationEventArgs eventArgs = (NotificationEventArgs)args;
                ShowNotification( eventArgs.Name, eventArgs.Message );
            };
        }

        public static void ShowNotification( string title, string message )
        {
            notificationManager.SendNotification( title, message, 0 );
            Console.WriteLine( $"Notification Recieved: \n{title}:\n{message}" );
        }

        public static async Task NotifyOfScheduledWork()
        {
            List<NotificationEventArgs> notifications = await DbServiceLocator.GetAllItemsAsync<NotificationEventArgs>() as List<NotificationEventArgs>;

            DateTime NotificationWindow = DateTime.UtcNow.AddHours(12);

            List<NotificationEventArgs> pendingNotifications = notifications.Where( x => DateTime.Compare(NotificationWindow, x.NotifyTime) >= 0 && x.Active ).ToList();

            foreach( NotificationEventArgs notification in pendingNotifications )
            {
                ScheduleNotification( null, notification );

                if( ++notification.TimesReminded >= notification.TimesToRemind )
                {
                    notification.Active = false;
                }
                
                await DbServiceLocator.UpdateItemAsync( notification );
            }
        }

        public static async Task UpdateNotificationActiveStatus( int notificationId, bool isActive )
        {
            NotificationEventArgs notification = await DbServiceLocator.GetItemAsync<NotificationEventArgs>( notificationId );

            if( notification.TimesReminded >= notification.TimesToRemind && isActive )
            {
                notification.TimesReminded = 0;
            }

            notification.Active = isActive;

            await DbServiceLocator.UpdateItemAsync( notification );
        }

        public static void ScheduleNotification( object sender, EventArgs e )
        {
            NotificationEventArgs args = (NotificationEventArgs)e;

            notificationManager.SendNotification( "Maintain It!", args.Message, args.Id, args.NotifyTime );
        }

        public static async Task<int> GetNewScheduledNotification( string name, DateTime serviceDate, int advanceNotice, int noticeTimeframe, int timesToRemind )
        {
            DateTime notifyDate = CalculateNotifyDate( serviceDate, advanceNotice, noticeTimeframe );

            string message = BuildServiceMessage(name, serviceDate, advanceNotice, (Timeframe)noticeTimeframe );


            NotificationEventArgs args = new NotificationEventArgs()
            {
                Name = name,
                Message = message,
                NotifyTime = notifyDate,
                Active = true,
                TimesReminded = 0,
                TimesToRemind = timesToRemind,
                CreatedOn = DateTime.UtcNow
            };

            int id = await DbServiceLocator.AddItemAndReturnIdAsync(args);
            
            if( notifyDate.Ticks < (long)TickTimeIntervals.Hour * 12 )
            {
                ScheduleNotification( null, args );
            }

            return id;
        }

        private static DateTime CalculateNotifyDate( DateTime serviceDate, int advanceNotice, int noticeTimeframe )
        {
            return (Timeframe)noticeTimeframe switch
            {
                Timeframe.Minutes => serviceDate.AddMinutes( -advanceNotice ),
                Timeframe.Hours => serviceDate.AddHours( -advanceNotice ),
                Timeframe.Days => serviceDate.AddDays( -advanceNotice ),
                Timeframe.Months => serviceDate.AddMonths( -advanceNotice ),
                Timeframe.Years => serviceDate.AddYears( -advanceNotice ),
                _ => serviceDate
            };
        }

        private static string BuildServiceMessage( string name, DateTime serviceDate, int advanceNotice, Timeframe noticeTimeframe )
        {
            return noticeTimeframe switch
            {
                Timeframe.Minutes => $"Upcoming service on {name} in {advanceNotice} minutes!",
                Timeframe.Hours => $"Upcoming service on {name} in {advanceNotice} hours!",
                Timeframe.Days => $"Service scheduled on {name} in {advanceNotice} days ({serviceDate.ToLongDateString()})",
                Timeframe.Months => $"Service scheduled on {name} in {advanceNotice} months ({serviceDate.ToLongDateString()})",
                Timeframe.Years => $"Service scheduled on {name} in {advanceNotice} days ({serviceDate.ToLongDateString()})",
                _ => $"Upcoming service required on {name}"
            };
        }

        public static async Task UpdateScheduledNotification( int notificationId, string name, DateTime serviceDate, int advanceNotice, int noticeTimeframe, bool isActive, int maxReminders )
        {
            NotificationEventArgs notification = await DbServiceLocator.GetItemAsync<NotificationEventArgs>(notificationId);

            string message = BuildServiceMessage(name, serviceDate, advanceNotice, (Timeframe)noticeTimeframe);

            DateTime notifyDate = CalculateNotifyDate( serviceDate, advanceNotice, noticeTimeframe );

            notification.Name = name;
            notification.Message = message;
            notification.NotifyTime = notifyDate;
            notification.TimesReminded = 0;
            notification.TimesToRemind = maxReminders == int.MinValue ? notification.TimesToRemind : maxReminders;
            notification.Active = isActive;

            await DbServiceLocator.UpdateItemAsync( notification );
        }

        public static async Task SaveShutdownDateTimeUtc()
        {
            ShutdownDateTime dT = new ShutdownDateTime()
            {
                Id = 1,
                Name = DateTime.UtcNow.ToString("dd.MM.yyyy"),
                CreatedOn = DateTime.UtcNow
            };

            int id = await DbServiceLocator.AddOrUpdateItemAndReturnIdAsync( dT );
            Console.WriteLine( id );

        }

        public static void Log( string data )
        {
            Console.WriteLine( data );
        }
    }
}
