using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using Android.App.Job;

using Maintain_it.Helpers;
using Google.Android.Material.Snackbar;
using Maintain_it.Services;

namespace Maintain_it.Droid
{
    [Activity( Label = "Maintain_it_NotificationScheduler", MainLauncher = false )]
    public class NotificationActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate( Bundle savedInstanceState )
        {


        }
    }
}