using System;
using System.Collections.Generic;

using Maintain_it.Helpers;
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

            Routing.RegisterRoute( nameof( EditNoteView ), typeof( EditNoteView ) );

            //Routing.RegisterRoute( nameof( HomeView ), typeof( HomeView ) );

            //ServiceItem Routes
            Routing.RegisterRoute( nameof( MaintenanceView ), typeof( MaintenanceView ) );

            Routing.RegisterRoute( $"{nameof( MaintenanceItemDetailView )}", typeof( MaintenanceItemDetailView ) );

            Routing.RegisterRoute( $"{nameof( MaintenanceItemDetailView )}/{nameof( AddNewStepView )}", typeof( AddNewStepView ) );

            Routing.RegisterRoute( $"{nameof( MaintenanceItemDetailView )}/{nameof( AddNewStepView )}/{nameof( AddStepMaterialsToStepView )}", typeof( AddStepMaterialsToStepView ) );
            
            Routing.RegisterRoute( $"{nameof(PerformMaintenanceView)}", typeof( PerformMaintenanceView ) );

            //ShoppingList Views
            Routing.RegisterRoute( $"{nameof(DisplayAllShoppingListsView)}/{nameof(ShoppingListDetailView)}", typeof( ShoppingListDetailView ) );

            Routing.RegisterRoute( $"{nameof( DisplayAllShoppingListsView )}/{nameof( CreateNewShoppingListView )}", typeof( CreateNewShoppingListView ) );

            Routing.RegisterRoute( $"{nameof( ShoppingListMaterialDetailView )}", typeof( ShoppingListMaterialDetailView ) );

            Routing.RegisterRoute( $"{nameof( DisplayAllShoppingListsView )}/{nameof( CreateNewShoppingListView )}/{nameof( AddMaterialsToShoppingListView )}", typeof( AddMaterialsToShoppingListView ) );

            //Setting Routes
            Routing.RegisterRoute( nameof( SettingsView ), typeof( SettingsView ) );

            //About Routes
            Routing.RegisterRoute( nameof( AboutView ), typeof( AboutView ) );

            //Support Routes

            //General Routes
            Routing.RegisterRoute($"{nameof(MaterialDetailView)}", typeof( MaterialDetailView ) );
        }
    }
}
