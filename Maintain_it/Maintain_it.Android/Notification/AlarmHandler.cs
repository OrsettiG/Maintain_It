using System.Threading.Tasks;

using Android.App;
using Android.Content;

using Maintain_it.Services;

namespace Maintain_it.Droid
{
    [BroadcastReceiver( Enabled = true, Label = "Local Notifications Broadcast Receiver" )]
    public class AlarmHandler : BroadcastReceiver
    {
        public override void OnReceive( Context context, Intent intent )
        {
            if( intent?.Extras != null )
            {
                string title = intent.GetStringExtra(AndroidNotificationManager.TitleKey);
                string message = intent.GetStringExtra(AndroidNotificationManager.MessageKey);

                AndroidNotificationManager manager = AndroidNotificationManager.Instance ?? new AndroidNotificationManager();

                manager.Show( title, message );
            }
        }
    }

    [BroadcastReceiver( Enabled = true, DirectBootAware = true, Exported = true, Label = "Boot Broadcast Receiver" )]
    [IntentFilter( new[] { Intent.ActionBootCompleted } )]
    public class BootReciever : BroadcastReceiver
    {
        public override void OnReceive( Context context, Intent intent )
        {
            //PICK UP HERE:
            //context.CreateJobBuilderUsingJobId<NotificationJobService>();

            //_ = Task.Run( async () => {
            //    await LocalNotificationManager.ReScheduleUnsentNotifications();
            //} );
        }
    }

    [BroadcastReceiver(Enabled = true, Label = "Shutdown Broadcast Reciever")]
    [IntentFilter( new[] { Intent.ActionShutdown })]
    public class ShutdownReciever : BroadcastReceiver
    {
        public override void OnReceive( Context context, Intent intent )
        {
            _ = Task.Run( async () =>
            {
                await LocalNotificationManager.SaveShutdownDateTimeUtc();
            } );
        }
    }
}