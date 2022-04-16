using System;
using System.Collections.Generic;

using Maintain_it.ViewModels;
using Maintain_it.Views;

using Xamarin.Forms;

namespace Maintain_it
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            //Home Routes
            Routing.RegisterRoute( nameof( TestView ), typeof( TestView ) );
            Routing.RegisterRoute( nameof( CreateNewMaterialView ), typeof( CreateNewMaterialView ) );

            //MaintenanceItem Routes
            Routing.RegisterRoute( nameof( MaintenanceView ), typeof( MaintenanceView ) );
            Routing.RegisterRoute( $"{nameof( MaintenanceItemDetailView )}", typeof( MaintenanceItemDetailView ) );
            Routing.RegisterRoute( $"{nameof( MaintenanceItemDetailView )}/{nameof( AddNewStepView )}", typeof( AddNewStepView ) );
            Routing.RegisterRoute( $"{nameof( MaintenanceItemDetailView )}/{nameof( AddNewStepView )}/{nameof( AddStepMaterialsToStepView )}", typeof( AddStepMaterialsToStepView ) );

            //ShoppingList Views
            Routing.RegisterRoute( nameof( DisplayAllShoppingListsView ), typeof( DisplayAllShoppingListsView ) );
            Routing.RegisterRoute( nameof( CreateNewShoppingListView ), typeof( CreateNewShoppingListView ) );
            Routing.RegisterRoute( $"{nameof( CreateNewShoppingListView )}/{nameof( AddMaterialsToShoppingListView )}", typeof( AddMaterialsToShoppingListView ) );

            //Setting Routes
            Routing.RegisterRoute( nameof( SettingsView ), typeof( SettingsView ) );

            //About Routes
            Routing.RegisterRoute( nameof( AboutView ), typeof( AboutView ) );

            //Support Routes
        }
    }
}
