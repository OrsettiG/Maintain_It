using System.Threading.Tasks;

using Android.App;
using Android.App.Job;
using Android.Content;
using Android.Runtime;

using Maintain_it.Helpers;
using Maintain_it.Services;

using Xamarin.Essentials;

using static Maintain_it.Helpers.Config;

namespace Maintain_it.Droid.Notifications
{
    [Service( Name = "com.maintain_it.Droid.NotificationJobService", Exported = true, Permission = "android.permission.BIND_JOB_SERVICE", DirectBootAware = true )]
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
                _ = manager.Cancel( messageId );

                _ = Task.Run( async () =>
                {
                    await LocalNotificationManager.UpdateNotificationActiveStatus( id, true );
                } );
            }
            else if( intent.Action.Equals( NotificationActions.DO_NOT_REMIND_ME.ToString() ) )
            {
                _ = manager.Cancel( messageId );

                _ = Task.Run( async () =>
                {
                    await LocalNotificationManager.UpdateNotificationActiveStatus( id, false );
                } );
            }

            return StartCommandResult.Sticky;
        }

        public override bool OnStartJob( JobParameters jobParams )
        {
            LocalNotificationManager.Log( " !!!!!!!!!!!!!!! Running Notification Job Service !!!!!!!!!!!!!!!" );

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

    [BroadcastReceiver( DirectBootAware = true, Exported = true, Enabled = true )]
    [IntentFilter( new[] { Intent.ActionBootCompleted, Intent.ActionLockedBootCompleted } )]
    public class BootReciever : BroadcastReceiver
    {
        public override void OnReceive( Context context, Intent intent )
        {
            // Get the JobScheduler
            JobScheduler jobScheduler = (JobScheduler)context.GetSystemService(Context.JobSchedulerService);

            // Make sure the service is not already running
            if( jobScheduler.GetPendingJob( (int)JobServiceIds.Notification ) == null )
            {
                // Create our Notification Service with the Notification Id so that next time we start the app we can verify that the service is still running.
                JobInfo.Builder builder = context.CreateJobBuilderUsingJobId<NotificationJobService>((int)JobServiceIds.Notification);

                // Set the service to run every 3-4 hours
                _ = builder.SetPeriodic( (int)Config.MilliTimeIntervals.Hour * 3, (int)Config.MilliTimeIntervals.Hour ).SetRequiresBatteryNotLow( true ).SetRequiresDeviceIdle( true );

                // Build the JobInfo Object that tells the service how and when to run.
                JobInfo jobInfo = builder.Build();

                // Start the service
                _ = jobScheduler.Schedule( jobInfo );
            }
        }
    }
}