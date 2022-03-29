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
            Routing.RegisterRoute( nameof( MaintenanceView ), typeof( MaintenanceView ) );
            Routing.RegisterRoute( nameof( TestView ), typeof( TestView ) );
            Routing.RegisterRoute( nameof( SettingsView ), typeof( SettingsView ) );
            Routing.RegisterRoute( nameof( AboutView ), typeof( AboutView ) );
            Routing.RegisterRoute( nameof( CreateNewMaterialView ), typeof( CreateNewMaterialView ) );
            Routing.RegisterRoute( $"{nameof( MaintenanceItemDetailView )}", typeof( MaintenanceItemDetailView ) );
            Routing.RegisterRoute( $"{nameof( MaintenanceItemDetailView )}/{nameof( AddNewStepView )}", typeof( AddNewStepView ) );
            Routing.RegisterRoute( $"{nameof( MaintenanceItemDetailView )}/{nameof( AddNewStepView )}/{nameof( AddStepMaterialsToStepView )}", typeof( AddStepMaterialsToStepView ) );
        }
    }
}
