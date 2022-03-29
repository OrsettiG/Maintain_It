using System;
//using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Maintain_it.Models;
using Maintain_it.Services;
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
            itemNum = rand.Next( 100000 );
            Refresh();
        }

        private Random rand = new Random();
        private int itemNum;

        #region PROPERTIES
        #region READ-ONLY
        #endregion

        private ObservableRangeCollection<MaintenanceItem> _maintenanceItems = new ObservableRangeCollection<MaintenanceItem>();

        private ObservableRangeCollection<MaintenanceItemViewModel> displayedMaintenanceItems;
        public ObservableRangeCollection<MaintenanceItemViewModel> DisplayedMaintenanceItems
        {
            get => displayedMaintenanceItems ??= new ObservableRangeCollection<MaintenanceItemViewModel>();
            set => SetProperty( ref displayedMaintenanceItems, value );
        }

        private bool locked { get; set; } = false;
        private int _deletedRows;
        public int DeletedRows { get => _deletedRows; set => SetProperty( ref _deletedRows, value ); }

        private int _lastItemID;
        public int LastItemID { get => _lastItemID; set => SetProperty( ref _lastItemID, value ); }
        #endregion

        #region COMMANDS
        private AsyncCommand addCommand;
        public ICommand AddCommand => addCommand ??= new AsyncCommand( Add );
        private AsyncCommand deleteAllCommand;
        public ICommand DeleteAllCommand => deleteAllCommand ??= new AsyncCommand( DeleteAll );
        private AsyncCommand refreshCommand;
        public ICommand RefreshCommand => refreshCommand ??= new AsyncCommand( Refresh );
        private AsyncCommand updateCommand;
        public ICommand UpdateCommand => updateCommand ??= new AsyncCommand( Update );
        private AsyncCommand addAndReturnIdCommand;
        public ICommand AddAndReturnIdCommand => addAndReturnIdCommand ??= new AsyncCommand( AddAndReturnId );
        private AsyncCommand startMaintenanceCommand;
        public ICommand StartMaintenanceCommand => startMaintenanceCommand ??= new AsyncCommand( StartMaintenance );
        private AsyncCommand _deleteAllCommand;
        public ICommand DeleteAllDbsAsyncCommand => _deleteAllCommand ??= new AsyncCommand( DeleteAllAsync );
        #endregion

        #region Methods

        NodeList<NodeTest> Origin { get; set; }

        private AsyncCommand addTestCommand;
        public ICommand AddTestCommand => addTestCommand ??= new AsyncCommand( AddTest );

        int countAhead = 1;
        int countBehind = 1;
        Random random = new Random();

        //private async Task AddTest()
        //{
        //    if( Origin == null )
        //    {
        //        Origin = new NodeList<NodeTest>( new NodeTest( $"TestNode0" ) );
        //    }
        //    else
        //    {
        //        int place = random.Next(0,2);

        //        if(place == 0 )
        //        {
        //            _ = Origin.AddNewNodeAtEnd( new NodeTest( $"BehindNode{countBehind}" ) );
        //            countBehind++;
        //        }

        //        if(place == 1 )
        //        {

        //            _ = Origin.AddNewNodeAtStart( new NodeTest( $"AheadNode{countAhead}" ) );
        //            countAhead++;
        //        }
        //    }

        //    NodeList<NodeTest> item = Origin.GetFirstNode();
            
        //    while( item.HasNextNode() )
        //    {
        //        Console.WriteLine( item.GetValue().Name );
        //        item = item.GetNextNode();
        //    }

        //    Console.WriteLine( item.GetValue().Name );
        //}
        
        private async Task AddTest()
        {
            if( Origin == null )
            {
                Origin = new NodeList<NodeTest>( new NodeTest( $"TestNode0" ) );
            }
            else
            {
                int place = random.Next(0,2);

                if(place == 0 )
                {
                    _ = Origin.AddNewNodeAtEnd( new NodeTest( $"BehindNode{countBehind}" ) );
                    countBehind++;
                }

                if(place == 1 )
                {

                    _ = Origin.AddNewNodeAtStart( new NodeTest( $"AheadNode{countAhead}" ) );
                    countAhead++;
                }
            }
        }

        private async Task Add()
        {
            await Shell.Current.GoToAsync( nameof( MaintenanceItemDetailView ) );
        }
        
        

        private async Task AddAndReturnId()
        {
            MaintenanceItem item = new MaintenanceItem();
            item.Name += itemNum.ToString();

            LastItemID = await DbServiceLocator.AddItemAndReturnIdAsync( item );

            await Refresh();
        }

        private async Task Refresh()
        {
            if( !locked )
            {
                locked = true;

                _maintenanceItems.Clear();
                DisplayedMaintenanceItems.Clear();

                List<MaintenanceItem> items = await DbServiceLocator.GetAllItemsRecursiveAsync<MaintenanceItem>() as List<MaintenanceItem>;

                List<MaintenanceItemViewModel> vms = CreateRange( items );
                DisplayedMaintenanceItems.AddRange( vms );
                _maintenanceItems.AddRange( items );

            }

            locked = false;
        }

        private async Task StartMaintenance()
        {

        }

        private List<MaintenanceItemViewModel> CreateRange( List<MaintenanceItem> items )
        {
            List<MaintenanceItemViewModel> vms = new List<MaintenanceItemViewModel>();
            foreach( MaintenanceItem item in items )
            {
                MaintenanceItemViewModel i = new MaintenanceItemViewModel( item, this );
                vms.Add( i );
            }

            return vms.OrderBy(x => x.FirstServiceDate).ToList();
        }

        internal async Task ItemDeleted( int id )
        {
            // This should basically just call Refresh() on the view so that the item gets correctly removed from the UI.
            Console.WriteLine( $"Maintenance Item Delete Button pushed on item with id: {id}" );

            await Refresh();
        }

        private async Task DeleteAll()
        {
            foreach( MaintenanceItem item in _maintenanceItems )
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
            foreach( MaintenanceItem item in _maintenanceItems )
            {
                if( await DbServiceLocator.GetItemAsync<MaintenanceItem>( item.Id ) != item )
                {
                    await DbServiceLocator.UpdateItemAsync( item );
                }
            }

            await Refresh();
        }

        private async Task DeleteAllAsync()
        {
            int count = 0;
            count += await DbServiceLocator.DeleteAllAsync<MaintenanceItem>();
            count += await DbServiceLocator.DeleteAllAsync<Step>();
            count += await DbServiceLocator.DeleteAllAsync<Material>();
            count += await DbServiceLocator.DeleteAllAsync<Retailer>();
            count += await DbServiceLocator.DeleteAllAsync<ShoppingList>();
            count += await DbServiceLocator.DeleteAllAsync<ShoppingListMaterial>();
            count += await DbServiceLocator.DeleteAllAsync<Note>();
            count += await DbServiceLocator.DeleteAllAsync<StepMaterial>();

            DeletedRows = count;

            await Refresh();
        }

        private protected override async Task EvaluateQueryParams( KeyValuePair<string, string> kvp )
        {
            switch( kvp.Key )
            {
                case nameof( Refresh ):
                    await Refresh();
                    break;
            }
        }

        #endregion
    }
}
