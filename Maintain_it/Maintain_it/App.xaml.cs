using System;

using Maintain_it.Models;
using Maintain_it.Services;
using Maintain_it.Views;

using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.Xaml;

namespace Maintain_it
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();
            MainPage = new AppShell();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
            DependencyService.Get<INotificationManager>().Initialize();
        }

        protected override void OnResume()
        {
        }
    }
}
