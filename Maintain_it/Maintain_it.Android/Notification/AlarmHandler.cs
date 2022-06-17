using System.Threading.Tasks;

using Android.App;
using Android.Content;

using Maintain_it.Helpers;
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
                int id = intent.GetIntExtra( Config.IdKey, 0);
                AndroidNotificationManager manager = AndroidNotificationManager.Instance;
                

                manager.Show( title, message, id );
            }
        }
    }
}