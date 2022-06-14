using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Maintain_it.Models;

using Xamarin.Forms;

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
            notificationManager.SendNotification( title, message );
            Console.WriteLine( $"Notification Recieved: \n{title}:\n{message}" );
        }

        public static void ScheduleNotification( object sender, EventArgs e )
        {
            NotificationEventArgs args = (NotificationEventArgs)e;

            string title = args.Name;
            string message = args.Message;

            notificationManager.SendNotification( title, message, args.NotifyTime );
        }

        // Called by AndroidNotificationManager on re-boot so that notifications are not lost on restart.
        public static async Task ReScheduleUnsentNotifications()
        {
            List<NotificationEventArgs> notifications = await DbServiceLocator.GetAllItemsAsync<NotificationEventArgs>() as List<NotificationEventArgs>;

            List<ShutdownDateTime> dTs = await DbServiceLocator.GetAllItemsAsync<ShutdownDateTime>() as List<ShutdownDateTime>;
            ShutdownDateTime dT = dTs.OrderBy(x => x.Id).Last();

            //We use the CreatedOn property to store the shutdown DateTime. There should only ever be one record in this table because when we set the values at shutdown we just update the record with Id == 1. However for safety sake we grab the last record.
            NotificationEventArgs[] unsentNotifications = notifications.Where( x => DateTime.Compare(x.NotifyTime, dT.CreatedOn) > 0).ToArray();

            foreach( NotificationEventArgs notification in unsentNotifications )
            {
                ScheduleNotification( null, notification );
            }
        }

        public static async Task<int> GetNewScheduledNotification( string name, DateTime serviceDate, int advanceNotice, int noticeTimeframe )
        {
            DateTime notifyDate = CalculateNotifyDate( serviceDate, advanceNotice, noticeTimeframe );

            string message = BuildServiceMessage(name, serviceDate, advanceNotice, (Timeframe)noticeTimeframe );


            NotificationEventArgs args = new NotificationEventArgs()
            {
                Name = name,
                Message = message,
                NotifyTime = notifyDate,
                CreatedOn = DateTime.UtcNow
            };

            int id = await DbServiceLocator.AddItemAndReturnIdAsync(args);

            ScheduleNotification( null, args );

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

        public static async Task UpdateScheduledNotification( int notificationId, string name, DateTime serviceDate, int advanceNotice, int noticeTimeframe )
        {
            NotificationEventArgs notification = await DbServiceLocator.GetItemAsync<NotificationEventArgs>(notificationId);

            string message = BuildServiceMessage(name, serviceDate, advanceNotice, (Timeframe)noticeTimeframe);

            DateTime notifyDate = CalculateNotifyDate( serviceDate, advanceNotice, noticeTimeframe );

            notification.Name = name;
            notification.Message = message;
            notification.NotifyTime = notifyDate;

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
