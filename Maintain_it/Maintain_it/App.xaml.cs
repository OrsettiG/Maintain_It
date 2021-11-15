using System;

using Maintain_it.Models;
using Maintain_it.Services;
using Maintain_it.Views;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Maintain_it
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

            DependencyService.Register<MockDataStore>();

            MainPage = new AppShell();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
