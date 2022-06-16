using System.Threading.Tasks;

using Android.App;
using Android.App.Job;
using Android.Content;
using Android.Runtime;

using Maintain_it.Helpers;
using Maintain_it.Services;

using Xamarin.Essentials;

namespace Maintain_it.Droid
{
    [Service( Exported = true, Permission = "android.permission.BIND_JOB_SERVICE" )]
    public class NotificationJobService : JobService
    {

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand( Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId )
        {
            if( intent.Action.Equals( Config.REMIND_LATER ) )
            {
                LocalNotificationManager.Log( "________________ CLOSE NOTIFICATION ________________" );
            }
            else if( intent.Action.Equals( Config.DO_NOT_REMIND ) )
            {
                LocalNotificationManager.Log( "################## MARK AS TRIGGERED ##################" );
            }

            return StartCommandResult.NotSticky;
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

        public void Schedule()
        {

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