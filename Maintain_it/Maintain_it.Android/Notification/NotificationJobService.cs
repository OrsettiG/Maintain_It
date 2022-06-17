using System.Threading.Tasks;

using Android.App;
using Android.App.Job;
using Android.Content;
using Android.Runtime;

using Maintain_it.Helpers;
using Maintain_it.Services;

using Xamarin.Essentials;

using static Maintain_it.Helpers.Config;

namespace Maintain_it.Droid
{
    [Service( Exported = true, Permission = "android.permission.BIND_JOB_SERVICE" )]
    public class NotificationJobService : JobService
    {

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand( Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId )
        {
            AndroidNotificationManager manager = AndroidNotificationManager.Instance;

            int messageId = intent.GetIntExtra(MessageIdKey, 0);
            int id = intent.GetIntExtra(NotificationIdKey, 0);

            if( intent.Action.Equals( NotificationActions.REMIND_ME_LATER.ToString() ) )
            {
                if( id > 0 )
                {
                    if( manager.Cancel( messageId ) )
                    {
                        _ = Task.Run( async () =>
                        {
                            await LocalNotificationManager.UpdateNotificationActiveStatus( id, false );
                        } );
                    }
                }
            }
            else if( intent.Action.Equals( NotificationActions.DO_NOT_REMIND_ME.ToString() ) )
            {
                manager.Cancel( messageId );

                _ = Task.Run( async () =>
                {
                    await LocalNotificationManager.UpdateNotificationActiveStatus( id, true );
                    LocalNotificationManager.Log( "Trigger Updated" );
                } );
            }

            return StartCommandResult.Sticky;
        }

        public override bool OnStartJob( JobParameters jobParams )
        {
            LocalNotificationManager.Log( "Running Notification Job Service" );

            _ = Task.Run( async () =>
            {
                await LocalNotificationManager.NotifyOfScheduledWork();

                JobFinished( jobParams, false );
            } );

            return true;
        }

        public override bool OnStopJob( JobParameters jobParams )
        {
            return true;
        }

    }

    public static class JobScheduleHelpers
    {
        public static JobInfo.Builder CreateJobBuilderUsingJobId<T>( this Context context, int jobId ) where T : JobService
        {
            Java.Lang.Class javaClass = Java.Lang.Class.FromType(typeof(T));
            ComponentName compName = new ComponentName( context, javaClass );
            return new JobInfo.Builder( jobId, compName );
        }
    }
}