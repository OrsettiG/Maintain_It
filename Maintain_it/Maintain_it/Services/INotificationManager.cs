using System;

namespace Maintain_it.Services
{
    // TODO: Set up Android and IOS local notification as described here: https://docs.microsoft.com/en-us/xamarin/xamarin-forms/app-fundamentals/local-notifications

    public interface INotificationManager
    {
        event EventHandler NotificationReceived;
        void Initialize();
        void SendNotification( string title, string message, int notificationId, DateTime notifyTime = default );
        void ReceiveNotification( string title, string message );
    }
}
