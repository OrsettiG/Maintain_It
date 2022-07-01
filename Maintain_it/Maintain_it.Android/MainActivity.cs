using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using Android.App.Job;

using Maintain_it.Services;
using Maintain_it.Helpers;

namespace Maintain_it.Droid
{
    [Activity( Label = "Maintain_it", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize )]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate( Bundle savedInstanceState )
        {
            base.OnCreate( savedInstanceState );

            Xamarin.Essentials.Platform.Init( this, savedInstanceState );
            global::Xamarin.Forms.Forms.Init( this, savedInstanceState );
            ScheduleNotificationJobService();
            LoadApplication( new App() );
        }

        public override void OnRequestPermissionsResult( int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults )
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult( requestCode, permissions, grantResults );

            base.OnRequestPermissionsResult( requestCode, permissions, grantResults );
        }

        private void ScheduleNotificationJobService()
        {
            // Get the JobScheduler
            JobScheduler jobScheduler = (JobScheduler)GetSystemService(JobSchedulerService);

            // Make sure the service is not already running
            if( jobScheduler.GetPendingJob( (int)Config.JobServiceIds.Notification ) == null )
            {
                // Create our Notification Service with the Notification Id so that next time we start the app we can verify that the service is still running.
                JobInfo.Builder builder = this.CreateJobBuilderUsingJobId<NotificationJobService>((int)Config.JobServiceIds.Notification);

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