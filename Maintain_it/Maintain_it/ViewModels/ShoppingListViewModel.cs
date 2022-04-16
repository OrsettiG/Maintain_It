using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

using Maintain_it.Models;
using Maintain_it.Services;
using Maintain_it.Views;

using MvvmHelpers.Commands;

using Xamarin.Forms;

namespace Maintain_it.ViewModels
{
    public class ShoppingListViewModel : BaseViewModel
    {
        public ShoppingListViewModel(){}

        public ShoppingListViewModel( ShoppingList shoppingList )
        {
            _shoppingList = shoppingList;
            Id = shoppingList.Id;
            Name = shoppingList.Name;
            Count = shoppingList.Materials.Count;
        }

        #region Properties
        private ShoppingList _shoppingList;
        public ShoppingList ShoppingList 
        { 
            get => _shoppingList; 
            set => SetProperty( ref _shoppingList, value ); 
        }

        private int Id;

        private string name;
        public string Name
        {
            get => name;
            set => SetProperty( ref name, value );
        }

        private int count;
        public int Count
        {
            get => count;
            set => SetProperty( ref count, value );
        }
        #endregion

        #region Commands

        private AsyncCommand addNewShoppingListMaterialCommand;
        public ICommand AddNewShoppingListMaterialCommand
        {
            get => addNewShoppingListMaterialCommand ??= new AsyncCommand( AddNewShoppingListMaterial );
        }

        private async Task AddNewShoppingListMaterial()
        {
            await Shell.Current.GoToAsync( $"{nameof( AddMaterialsToShoppingListView )}" );
        }

        #endregion

        #region Methods

        private async Task Refresh()
        {
            ShoppingList = await DbServiceLocator.GetItemRecursiveAsync<ShoppingList>( Id );
        }
        
        private async Task Refresh( int id )
        {
            ShoppingList = await DbServiceLocator.GetItemRecursiveAsync<ShoppingList>( id );
        }

        #region Query Handling
        private protected override async Task EvaluateQueryParams( KeyValuePair<string, string> kvp )
        {
            switch( kvp.Key )
            {
                case nameof( Id ):
                    if( int.TryParse(kvp.Value, out Id ) )
                    {
                        await Refresh( Id );
                    }
                    break;
            }
        }
        #endregion
        #endregion

    }
}