using System;
using System.Collections.Generic;
using System.Text;

using Maintain_it.Services;

using Xamarin.Forms;

namespace Maintain_it.Helpers
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
                ShowNotification( eventArgs.Title, eventArgs.Message );
            };
        }

        public static void ShowNotification( string title, string message )
        {
            Console.WriteLine( $"Notification Recieved: \n{title}:\n{message}" );
        }

        public static void ScheduleNotification( object sender, EventArgs e )
        {
            NotificationEventArgs args = (NotificationEventArgs)e;

            string title = args.Title;
            string message = $"";
        }
    }
}
