using System.Threading.Tasks;

using Android.App;
using Android.App.Job;
using Android.Content;
using Android.Runtime;

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
            return base.OnStartCommand( intent, flags, startId );
        }

        public override bool OnStartJob( JobParameters jobParams )
        {
            _ = Task.Run( () =>
            {
                LocalNotificationManager.ShowNotification( "Test Notification", $"This is test number {jobParams.Extras.GetInt("num", 1)}" );

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