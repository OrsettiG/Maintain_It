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
            JobScheduler jobScheduler = (JobScheduler)GetSystemService(JobSchedulerService);

            if( jobScheduler.GetPendingJob( (int)Config.JobServiceIds.Notification ) == null )
            {
                JobInfo.Builder builder = this.CreateJobBuilderUsingJobId<NotificationJobService>((int)Config.JobServiceIds.Notification);

                _ = builder.SetPeriodic( (int)Config.MilliTimeIntervals.Hour * 12, (int)Config.MilliTimeIntervals.Hour ).SetPersisted( true );

                PersistableBundle extras = new PersistableBundle();
                extras.PutInt( "num", 100 );

                _ = builder.SetExtras( extras );

                JobInfo jobInfo = builder.Build();

                LocalNotificationManager.Log( $"NOTIFICATION JOBINFO FLEXMILI VALUE: {jobInfo.FlexMillis}" );
                LocalNotificationManager.Log( $"NOTIFICATION JOBINFO INTERVALMILI : {jobInfo.IntervalMillis}" );

                int result = jobScheduler.Schedule( jobInfo );

                if( result == JobScheduler.ResultSuccess )
                {
                    LocalNotificationManager.Log( "---------------------- JOB SUCCESSFULLY SCHEDULED ----------------------" );
                }
                else
                {
                    LocalNotificationManager.Log( "---------------------- JOB SCHEDULING FAILURE ----------------------" );
                }
            }
            else
            {
                LocalNotificationManager.Log( "!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! JOB ALREADY SCHEDULED  !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!" );
            }
        }
    }
}