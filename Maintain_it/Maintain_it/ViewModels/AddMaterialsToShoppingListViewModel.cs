using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Maintain_it.Helpers;
using Maintain_it.Models;

using Maintain_it.Services;
using Maintain_it.Views;

using MvvmHelpers.Commands;

using Xamarin.Forms;

namespace Maintain_it.ViewModels
{
    internal class AddMaterialsToShoppingListViewModel : BaseViewModel
    {
        public AddMaterialsToShoppingListViewModel()
        {
        }


        #region Properties

        private int maintenanceItemId;

        private MaintenanceItem selectedMaintenanceItem;
        public MaintenanceItem SelectedMaintenanceItem { get => selectedMaintenanceItem; set => SetProperty( ref selectedMaintenanceItem, value ); }

        private List<ShoppingList> shoppingLists;
        public List<ShoppingList> ShoppingLists { get => shoppingLists ??= new List<ShoppingList>(); set => SetProperty( ref shoppingLists, value ); }

        private List<ShoppingListViewModel> shoppingListViewModels;
        public List<ShoppingListViewModel> ShoppingListViewModels { get => shoppingListViewModels ??= new List<ShoppingListViewModel>(); set => SetProperty( ref shoppingListViewModels, value ); }

        private ShoppingListViewModel selectedShoppingList;
        public ShoppingListViewModel SelectedShoppingList { get => selectedShoppingList; set => SetProperty( ref selectedShoppingList, value ); }

        #endregion

        #region Commands

        private AsyncCommand createNewShoppingListCommand;
        public ICommand CreateNewShoppingListCommand => createNewShoppingListCommand ??= new AsyncCommand( CreateNewShoppingList );

        #endregion

        #region Methods

        public async Task Init()
        {
            ShoppingLists = await DbServiceLocator.GetAllItemsAsync<ShoppingList>() as List<ShoppingList>;

            foreach( ShoppingList sList in ShoppingLists )
            {
                ShoppingListViewModel model = new ShoppingListViewModel( sList );
                ShoppingListViewModels.Add( model );
            }
        }

        /// <summary>
        /// Converts all the StepMaterials from the selected MaintenanceItem into ShoppingListMaterials and adds them to the selected ShoppingList
        /// </summary>
        /// <returns></returns>
        private async Task AddMaterialsToShoppingList()
        {
            ConcurrentBag<Step> steps = new ConcurrentBag<Step>( selectedMaintenanceItem.Steps );
            ConcurrentBag<StepMaterial> stepMaterials = new ConcurrentBag<StepMaterial>();
            ConcurrentBag<ShoppingListMaterial> shoppingListMaterials = new ConcurrentBag<ShoppingListMaterial>();

            _ = Parallel.ForEach( steps, step =>
            {
                foreach( StepMaterial mat in step.StepMaterials )
                {
                    stepMaterials.Add( mat );
                }
            } );

            _ = Parallel.ForEach( stepMaterials, mat =>
            {
                //TODO: Re-work this into a proper MVVM format.
                SelectedShoppingList.ShoppingList.Materials.Add( MaterialConverter.ConvertToShoppingListMaterial( mat, SelectedShoppingList.ShoppingList ) );
            } );
        }

        private async Task CreateNewShoppingList()
        {
            await Shell.Current.GoToAsync( $"/{nameof( CreateNewShoppingListView )}" );
        }

        #endregion

        #region Handle Query Params
        private protected async override Task EvaluateQueryParams( KeyValuePair<string, string> kvp )
        {
            await Init();

            switch( kvp.Key )
            {
                case nameof( maintenanceItemId ):
                    if( int.TryParse( kvp.Key, out maintenanceItemId ) )
                    {
                        selectedMaintenanceItem = await DbServiceLocator.GetItemRecursiveAsync<MaintenanceItem>( maintenanceItemId );
                    }
                    break;
            }
        }
        #endregion
    }
}
