using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Maintain_it.Views
{
    [XamlCompilation( XamlCompilationOptions.Compile )]
    public partial class MaintenanceItemDetailView : ContentPage
    {
        public MaintenanceItemDetailView()
        {
            InitializeComponent();
        }

        private void Stepper_PropertyChanged( object sender, System.ComponentModel.PropertyChangedEventArgs e )
        {

        }
    }
}