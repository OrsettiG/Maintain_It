using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Maintain_it.Views;

using MvvmHelpers.Commands;

using Xamarin.Forms;

namespace Maintain_it.ViewModels
{
    public class MaintenanceViewModel
    {
        public MaintenanceViewModel()
        {
            AddMaintenanceItem = new AsyncCommand( AddItem );
        }

        public AsyncCommand AddMaintenanceItem { get; }

        public async Task AddItem()
        {
            await Shell.Current.GoToAsync( $"//{nameof( AboutView )}");
        }
    }
}
