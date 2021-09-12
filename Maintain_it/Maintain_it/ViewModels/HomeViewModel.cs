﻿using System;
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
            db = new MaintenanceItemService();
            MaintenanceItems = new ObservableCollection<MaintenanceItem>();
            AddCommand = new AsyncCommand( Add );
            DeleteCommand = new AsyncCommand( Delete );
            RefreshCommand = new AsyncCommand( Refresh );

            Refresh();
        }

        private int itemNum = 1;

        #region PROPERTIES
        #region READ-ONLY
        private MaintenanceItemService db { get; }
        #endregion

        #region PRIVATE
        private ObservableCollection<MaintenanceItem> maintenanceItems;
        private bool IsBusy { get; set; }
        #endregion

        #region PUBLIC
        public ObservableCollection<MaintenanceItem> MaintenanceItems
        {
            get;
            set;
        }
        #endregion

        #endregion

        #region COMMANDS
        public AsyncCommand AddCommand { get; }
        public AsyncCommand DeleteCommand { get; }
        public AsyncCommand RefreshCommand { get; }
        #endregion

        private async Task Add()
        {
            MaintenanceItem item = new MaintenanceItem( $"Item {itemNum}" )
            {
                NextServiceDate = DateTime.Now.AddDays( 1 ),
                MaterialsAndEquipment = new List<Material>()
                {
                    new Material( "Default 1", "Store1", 10.00d, 1 ),
                    new Material( "Default 2", "Store2", 11.00d, 2 ),
                    new Material( "Default 3", "Store3", 12.00d, 3 ),
                    new Material( "Default 4", "Store4", 13.00d, 4 ),

                }
            };

            await db.AddItemAsync( item );
            itemNum++;

            await Refresh();
        }

        private async Task Refresh()
        {
            if( !IsBusy )
            {
                IsBusy = true;

                //await Task.Delay( 0 );

                MaintenanceItems.Clear();

                List<MaintenanceItem> items = await db.GetAllItemsAsync() as List<MaintenanceItem>;

                foreach( MaintenanceItem item in items )
                {
                    MaintenanceItems.Add( item );
                }

                IsBusy = false;
            }
        }

        private async Task Delete()
        {
            MaintenanceItem item = MaintenanceItems[0];

            if(item != null )
            {
                await db.DeleteItemAsync( item.Id );
            }

            await Refresh();
        }
    }
}
