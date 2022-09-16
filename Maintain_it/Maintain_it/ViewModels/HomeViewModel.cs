using System;
//using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Maintain_it.Helpers;
using Maintain_it.Models;
using Maintain_it.Views;

using MvvmHelpers;
using MvvmHelpers.Commands;
 
using Xamarin.Forms;

namespace Maintain_it.ViewModels
{
    public class HomeViewModel : BaseViewModel
    {
        public HomeViewModel()
        {
            _ = Task.Run( async () => await Refresh() );
        }

        #region PROPERTIES

        private ObservableRangeCollection<MaintenanceItem> maintenanceItems = new ObservableRangeCollection<MaintenanceItem>();

        private ObservableRangeCollection<MaintenanceItemViewModel> displayedMaintenanceItems;
        public ObservableRangeCollection<MaintenanceItemViewModel> DisplayedMaintenanceItems
        {
            get => displayedMaintenanceItems ??= new ObservableRangeCollection<MaintenanceItemViewModel>();
            set => SetProperty( ref displayedMaintenanceItems, value );
        }

        private bool Locked { get; set; } = false;
        public bool IsRefreshing => Locked;

        #endregion

        #region COMMANDS
        private AsyncCommand addCommand;
        public ICommand AddCommand => addCommand ??= new AsyncCommand( Add );
        private AsyncCommand refreshCommand;
        public ICommand RefreshCommand => refreshCommand ??= new AsyncCommand( Refresh );

        #endregion

        #region Methods

        private async Task Add()
        {
            await Shell.Current.GoToAsync( $"{nameof( MaintenanceItemDetailView )}?{QueryParameters.NewItem}=true" );
        }


        private async Task Refresh()
        {
            if( !Locked )
            {
                Locked = true;
                List<MaintenanceItem> items = await MaintenanceItemManager.GetAllItemsRecursiveAsync();

                //List<MaintenanceItemViewModel> vms = CreateRange( items );
                Task<List<MaintenanceItemViewModel>> vmsTask = MaintenanceItemManager.GetItemRangeAsViewModelAsync( items.GetIds() );

                List<MaintenanceItemViewModel> vms = await vmsTask;

                maintenanceItems.Clear();
                maintenanceItems.AddRange( items );

                vmsTask.Wait();
                DisplayedMaintenanceItems.Clear();
                DisplayedMaintenanceItems.AddRange( vms );
            }

            Locked = false;
        }

        private protected override async Task EvaluateQueryParams( KeyValuePair<string, string> kvp )
        {
            switch( kvp.Key )
            {
                case QueryParameters.Refresh:
                    await Refresh();
                    break;
            }
        }

        #endregion
    }
}
