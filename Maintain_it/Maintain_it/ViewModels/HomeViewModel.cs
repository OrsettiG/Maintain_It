using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Maintain_it.Models;
using Maintain_it.Services;

namespace Maintain_it.ViewModels
{
    internal class HomeViewModel : BaseViewModel
    {
        public HomeViewModel()
        {
            dBManager = new DBManager();
            _ = Task.Run( async () => await LoadData() );
        }

        #region PROPERTIES
        #region READ-ONLY
        private DBManager dBManager { get; }
        #endregion

        #region PRIVATE
        private ObservableCollection<MaintenanceItemViewModel> maintenanceItems;
        #endregion

        #region PUBLIC
        public ObservableCollection<MaintenanceItemViewModel> MaintenanceItems
        {
            get => maintenanceItems;
            set => maintenanceItems = value;
        }
        #endregion

        #endregion

        private async Task LoadData()
        {
            IEnumerable<MaintenanceItem> dbItems = await dBManager.GetItemsAsync();
            List<MaintenanceItemViewModel> maintenanceItemViewModels = (List<MaintenanceItemViewModel>)dbItems.Select( i => CreateMaintenanceItemViewModel( i ) );
            maintenanceItems = new ObservableCollection<MaintenanceItemViewModel>( maintenanceItemViewModels );
        }

        private MaintenanceItemViewModel CreateMaintenanceItemViewModel( MaintenanceItem i )
        {
            MaintenanceItemViewModel vm = new MaintenanceItemViewModel( i );
            vm.ItemStatusChanged += ItemStatusChanged;
            return vm;
        }

        private void ItemStatusChanged( object sender, EventArgs e ) { }
    }
}
