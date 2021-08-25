using System;
using System.Collections.Generic;

using Maintain_it.ViewModels;
using Maintain_it.Views;

using Xamarin.Forms;

namespace Maintain_it
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute( nameof( HomeView ), typeof( HomeView ) );
            Routing.RegisterRoute( nameof( MaintenanceView ), typeof( MaintenanceView ) );
            Routing.RegisterRoute( nameof( SettingsView ), typeof( SettingsView ) );
            Routing.RegisterRoute( nameof( AboutView ), typeof( AboutView ) );
        }

        private async void OnMenuItemClicked( object sender, EventArgs e )
        {
            await Shell.Current.GoToAsync( "//HomeView" );
        }
    }
}
