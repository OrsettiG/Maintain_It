using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private ObservableCollection<MaintenanceItem> maintenanceItems;
        #endregion

        #region PUBLIC
        public ObservableCollection<MaintenanceItem> MaintenanceItems
        {
            get => maintenanceItems;
            set
            {
                if( maintenanceItems != value )
                {
                    maintenanceItems = value;
                    RaisePropertyChanged( nameof( MaintenanceItems ) );
                }
            }
        }
        #endregion

        #endregion

        private async Task LoadData()
        {
            IEnumerable<MaintenanceItem> items = await dBManager.GetItemsAsync();
            maintenanceItems = (ObservableCollection<MaintenanceItem>)items;
        }

    }
}
