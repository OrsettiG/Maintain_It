using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Maintain_it.Models;
using Maintain_it.Services;

using MvvmHelpers.Commands;

using Xamarin.Forms;

namespace Maintain_it.ViewModels
{
    internal class HomeViewModel : BaseViewModel
    {
        public HomeViewModel()
        {
            MaintenanceItems = new ObservableCollection<MaintenanceItem>();
            AddCommand = new AsyncCommand( Add );
            DeleteCommand = new AsyncCommand( Delete );
            UpdateCommand = new AsyncCommand( Update );
            RefreshCommand = new AsyncCommand( Refresh );
            AddAndReturnIDCommand = new AsyncCommand( AddAndReturnId );
            fakeData = new FakeData();
            itemNum = rand.Next( 100000 );
            Refresh();
        }

        private Random rand = new Random();
        private int itemNum;

        #region PROPERTIES
        #region READ-ONLY
        #endregion

        #region PRIVATE
        private ObservableCollection<MaintenanceItem> maintenanceItems;
        private bool IsBusy { get; set; }
        private FakeData fakeData { get; set; }
        #endregion

        #region PUBLIC
        public ObservableCollection<MaintenanceItem> MaintenanceItems
        {
            get;
            set;
        }

        private int _lastItemID;
        public int LastItemID { get => _lastItemID; set => SetProperty( ref _lastItemID, value); }
        #endregion

        #endregion

        #region COMMANDS
        public AsyncCommand AddCommand { get; }
        public AsyncCommand DeleteCommand { get; }
        public AsyncCommand RefreshCommand { get; }
        public AsyncCommand UpdateCommand { get; }
        public AsyncCommand AddAndReturnIDCommand { get; }
        #endregion

        private async Task Add()
        {
            MaintenanceItem item = fakeData.maintenanceItem;
            item.Name += itemNum.ToString();
            item.Steps.Add( fakeData.step );
            await DbServiceLocator.AddItemAsync( item );
            itemNum++;
            await Refresh();
        }

        private async Task AddAndReturnId()
        {
            MaintenanceItem item = fakeData.maintenanceItem;
            item.Name += itemNum.ToString();

            LastItemID = await DbServiceLocator.AddItemAndReturnIdAsync( item );

            await Refresh();
        }

        private async Task Refresh()
        {
            if( !IsBusy )
            {
                IsBusy = true;

                //await Task.Delay( 0 );

                MaintenanceItems.Clear();

                List<MaintenanceItem> items = await DbServiceLocator.GetAllItemsAsync<MaintenanceItem>() as List<MaintenanceItem>;

                foreach( MaintenanceItem item in items )
                {
                    MaintenanceItems.Add( item );
                }

                IsBusy = false;
            }
        }

        private async Task Delete()
        {
            foreach( MaintenanceItem item in MaintenanceItems )
            {
                //MaintenanceItem item = MaintenanceItems[0];

                if( item != null )
                {
                    await DbServiceLocator.DeleteItemAsync<MaintenanceItem>( item.Id );
                }
            }

            await Refresh();
        }

        private async Task Update()
        {
            foreach( MaintenanceItem item in MaintenanceItems )
            {
                if( await DbServiceLocator.GetItemAsync<MaintenanceItem>( item.Id ) != item )
                {
                    await DbServiceLocator.UpdateItemAsync( item );
                }
            }

            await Refresh();
        }
    }
}
