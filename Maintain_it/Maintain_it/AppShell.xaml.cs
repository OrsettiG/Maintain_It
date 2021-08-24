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
            Routing.RegisterRoute( nameof( ItemDetailPage ), typeof( ItemDetailPage ) );
            Routing.RegisterRoute( nameof( NewItemPage ), typeof( NewItemPage ) );
        }

        private async void OnMenuItemClicked( object sender, EventArgs e )
        {
            await Shell.Current.GoToAsync( "//LoginPage" );
        }
    }
}
