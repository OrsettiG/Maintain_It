using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Input;

using Maintain_it.Helpers;
using Maintain_it.Models;
using Maintain_it.Services;
using Maintain_it.Views;

using MvvmHelpers;
using MvvmHelpers.Commands;

using Xamarin.Forms;

namespace Maintain_it.ViewModels
{
    public class ShoppingListViewModel : BaseViewModel
    {
        public ShoppingListViewModel() { }

        public ShoppingListViewModel( ShoppingList shoppingList )
        {
            _shoppingList = shoppingList;
            Id = shoppingList.Id;
            Name = shoppingList.Name;
            Count = shoppingList.Materials.Count;
        }

        #region Properties
        private int Id;

        private ShoppingList _shoppingList;

        private string name;
        public string Name
        {
            get => name;
            set => SetProperty( ref name, value );
        }

        public event Action<int> OnPurchasedChanged;

        private int count;
        public int Count
        {
            get => count;
            set => SetProperty( ref count, value );
        }

        private ObservableRangeCollection<ShoppingListMaterialViewModel> materials;
        public ObservableRangeCollection<ShoppingListMaterialViewModel> Materials
        {
            get => materials ??= new ObservableRangeCollection<ShoppingListMaterialViewModel>();
            set => SetProperty( ref materials, value );
        }
        #endregion

        #region Commands
        // Add New Shopping List
        private AsyncCommand editNewShoppingListMaterialsCommand;
        public ICommand EditNewShoppingListMaterialsCommand
        {
            get => editNewShoppingListMaterialsCommand ??= new AsyncCommand( EditNewShoppingListMaterials );
        }

        private async Task EditNewShoppingListMaterials()
        {
            StringBuilder builder = new StringBuilder();

            if( Materials.Count > 0 )
            {
                for( int i = 0; i < Materials.Count; i++ )
                {
                    _ = i < 1
                        ? builder.Append( $"{_shoppingList.Materials[i].MaterialId}" )
                        : builder.Append( $",{_shoppingList.Materials[i].MaterialId}" );
                }
            }

            string encodedIds = HttpUtility.UrlEncode( builder.ToString() );

            await Shell.Current.GoToAsync( $"{nameof( AddMaterialsToShoppingListView )}?{RoutingPath.MaterialIds}={encodedIds}" );
        }

        // Open Shopping List
        private AsyncCommand openShoppingListCommand;
        public ICommand OpenShoppingListCommand
        {
            get => openShoppingListCommand ??= new AsyncCommand( OpenShoppingList );
        }
        private async Task OpenShoppingList()
        {
            Console.WriteLine( $"Open Shopping List with Id {Id}" );
            string encodedId = HttpUtility.UrlEncode($"{_shoppingList.Id}");

            await Shell.Current.GoToAsync( $"{nameof( ShoppingListDetailView )}?{RoutingPath.ShoppingListId}={encodedId}" );
        }
        #endregion

        #region Methods

        private async Task Refresh()
        {
            _shoppingList = await DbServiceLocator.GetItemRecursiveAsync<ShoppingList>( Id );
            Name = _shoppingList.Name;
            Count = _shoppingList.Materials.Count;

            Materials.Clear();

            ConcurrentBag<ShoppingListMaterialViewModel> vms = new ConcurrentBag<ShoppingListMaterialViewModel>();
            _ = Parallel.ForEach( _shoppingList.Materials, material =>
            {
                ShoppingListMaterialViewModel vm = new ShoppingListMaterialViewModel(material);
                vm.OnPurchasedChanged += UpdateCount;
                vms.Add( vm );
            } );

            Materials.AddRange( vms );
        }

        private void UpdateCount( int x )
        {
            Count += x;
        }

        #region Query Handling
        private protected override async Task EvaluateQueryParams( KeyValuePair<string, string> kvp )
        {
            switch( kvp.Key )
            {
                case RoutingPath.ShoppingListId:
                    if( int.TryParse( kvp.Value, out Id ) )
                    {
                        await Refresh();
                    }
                    break;
            }
        }
        #endregion
        #endregion

    }
}