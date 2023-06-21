using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Maintain_it.Droid.ServiceItemManagement
{
    [BroadcastReceiver(Enabled = true, Label = "Local Service Item Management Broadcast Receiver")]
    public class ServiceItemHandler : BroadcastReceiver
    {
        public override void OnReceive( Context context, Intent intent )
        {
            Toast.MakeText( context, "Received Item Management intent!", ToastLength.Short ).Show();
        }
    }
}