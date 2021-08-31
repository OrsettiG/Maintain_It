using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;

using Maintain_it.Models;
using Maintain_it.Services;

namespace Maintain_it.ViewModels
{
    internal class HomeViewModel
    {
        public HomeViewModel()
        {
            dBManager = new DBManager();
            Task.Run( async () => await LoadData() );
        }

        #region PROPERTIES
        #region READ-ONLY
        private DBManager dBManager { get; }
        #endregion
        
        #region PRIVATE
        private ObservableCollection<MaintenanceItem> maintenanceItems;
        #endregion
        
        #region PUBLIC // empty
        #endregion
        #endregion

        private async Task LoadData()
        {
            maintenanceItems = (ObservableCollection<MaintenanceItem>)await dBManager.GetItemsAsync();
        }

    }
}
