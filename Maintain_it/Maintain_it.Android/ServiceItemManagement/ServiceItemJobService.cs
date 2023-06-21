using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.App.Job;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using Maintain_it.Services;

namespace Maintain_it.Droid.ServiceItemManagement
{
    [Service( Name = "com.maintain_it.Droid.ServiceItemManagement.ServiceItemJobService", Exported = true, Permission = "android.permission.BIND_JOB_SERVICE", DirectBootAware = true )]
    public class ServiceItemJobService : JobService

    {
        public override bool OnStartJob( JobParameters jobParams )
        {
            // Testing that the job will start correctly
            LocalNotificationManager.Log( "Starting ServiceItem JobService" );
            JobFinished( jobParams, false );
            
            return false;
        }

        public override bool OnStopJob( JobParameters jobParams )
        {
            LocalNotificationManager.Log( "ServiceItem JobService Terminated Unexpectedly" );

            return false;
        }
    }
}