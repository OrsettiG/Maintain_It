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
            maintenanceItems = LoadData();

            //_ = Task.Run( async () => await LoadData() );
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
        }
        #endregion

        #endregion

        private ObservableCollection<MaintenanceItem> LoadData()
        {
            ObservableCollection<MaintenanceItem> Items = new ObservableCollection<MaintenanceItem>();

            foreach( MaintenanceItem item in items )
            {
                Items.Add( item );
            }
            
            return Items;
        }

        private readonly IEnumerable<MaintenanceItem> items = new List<MaintenanceItem>()
        {
            new MaintenanceItem( "Item 1", DateTime.Now )
            {
                NextServiceDate = DateTime.Now.AddDays( 1 ),
                MaterialsAndEquipment = new List<Material>()
                {
                    new Material( "Mat1", "Store1", 10.00d, 1 ),
                    new Material( "Mat2", "Store2", 11.00d, 2 ),
                    new Material( "Mat3", "Store3", 12.00d, 3 ),
                    new Material( "Mat4", "Store4", 13.00d, 4 ),
                    new Material( "Mat2", "Store2", 11.00d, 2 ),
                    new Material( "Mat3", "Store3", 12.00d, 3 ),
                    new Material( "Mat4", "Store4", 13.00d, 4 ),
                    new Material( "Mat2", "Store2", 11.00d, 2 ),
                    new Material( "Mat3", "Store3", 12.00d, 3 ),
                    new Material( "Mat4", "Store4", 13.00d, 4 )
                }
            },
            new MaintenanceItem( "Item 2", DateTime.Now.AddDays(1) )
            {
                NextServiceDate = DateTime.Now.AddDays( 2 ),
                MaterialsAndEquipment = new List<Material>()
                {
                    new Material( "Mat1", "Store1", 10.00d, 1 ),
                    new Material( "Mat2", "Store2", 11.00d, 2 ),
                    new Material( "Mat3", "Store3", 12.00d, 3 ),
                    new Material( "Mat4", "Store4", 13.00d, 4 )
                }
            },
            new MaintenanceItem( "Item 3", DateTime.Now.AddDays(2) )
            {
                NextServiceDate = DateTime.Now.AddDays( 3 ),
                MaterialsAndEquipment = new List<Material>()
                {
                    new Material( "Mat1", "Store1", 10.00d, 1 ),
                    new Material( "Mat2", "Store2", 11.00d, 2 ),
                    new Material( "Mat3", "Store3", 12.00d, 3 ),
                    new Material( "Mat4", "Store4", 13.00d, 4 )
                }
            },
            new MaintenanceItem( "Item 4", DateTime.Now.AddDays(3) )
            {
                NextServiceDate = DateTime.Now.AddDays( 4 ),
                MaterialsAndEquipment = new List<Material>()
                {
                    new Material( "Mat1", "Store1", 10.00d, 1 ),
                    new Material( "Mat2", "Store2", 11.00d, 2 ),
                    new Material( "Mat3", "Store3", 12.00d, 3 ),
                    new Material( "Mat4", "Store4", 13.00d, 4 )
                }
            },
            new MaintenanceItem( "Item 5", DateTime.Now.AddDays(4) )
            {
                NextServiceDate = DateTime.Now.AddDays( 5 ),
                MaterialsAndEquipment = new List<Material>()
                {
                    new Material( "Mat1", "Store1", 10.00d, 1 ),
                    new Material( "Mat2", "Store2", 11.00d, 2 ),
                    new Material( "Mat3", "Store3", 12.00d, 3 ),
                    new Material( "Mat4", "Store4", 13.00d, 4 )
                }
            }

        };

    }
}
