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

        protected async override void OnStart()
        {
            await DbServiceLocator.Init<MaintenanceItem>();
            await DbServiceLocator.Init<Note>();
            await DbServiceLocator.Init<Step>();
            await DbServiceLocator.Init<Material>();
            await DbServiceLocator.Init<StepMaterial>();
            await DbServiceLocator.Init<Retailer>();
            await DbServiceLocator.Init<ShoppingList>();
            await DbServiceLocator.Init<ShoppingListItem>();
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
