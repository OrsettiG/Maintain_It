using System;
using System.Collections.Generic;
using System.Text;

namespace Maintain_it.Services
{
    // TODO: Set up Android and IOS local notification as described here: https://docs.microsoft.com/en-us/xamarin/xamarin-forms/app-fundamentals/local-notifications

    public interface INotificationManager
    {
        event EventHandler NotificationRecieved;
        void Initialize();
        void SendNotification( string title, string message, DateTime? notifyTime = null );
        void RecieveNotification( string title, string message );
    }

    public class NotificationEventArgs : EventArgs
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public DateTime? NotifyTime { get; set; }
    }
}
