using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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

        #region Events
        public AsyncCommand RefreshContainer;
        #endregion

        #region Properties
        private int Id;

        private ShoppingList _shoppingList;

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

        private bool isEditing = false;
        public bool IsEditing
        {
            get => isEditing;
            set => SetProperty( ref isEditing, value );
        }

        private Color editingStateColor = Color.White;
        public Color EditingStateColor
        {
            get => editingStateColor;
            set => SetProperty( ref editingStateColor, value );
        }

        private HashSet<int> shoppingListMaterialIds = new HashSet<int>();

        private ObservableRangeCollection<ShoppingListMaterialViewModel> materials;
        public ObservableRangeCollection<ShoppingListMaterialViewModel> Materials
        {
            get => materials ??= new ObservableRangeCollection<ShoppingListMaterialViewModel>();

            set => SetProperty( ref materials, (ObservableRangeCollection<ShoppingListMaterialViewModel>)value.OrderBy( x => x.Material.Id ) );
        }
        #endregion

        #region Commands
        // Edit Shopping List Materials
        private AsyncCommand editShoppingListMaterialsCommand;
        public ICommand EditShoppingListMaterialsCommand
        {
            get => editShoppingListMaterialsCommand ??= new AsyncCommand( EditShoppingListMaterials );
        }

        private async Task EditShoppingListMaterials()
        {
            IsEditing = !IsEditing;
            EditingStateColor = IsEditing ? Color.Red : Color.White;
            _ = Parallel.ForEach( Materials, mat =>
            {
                mat.ToggleCanEditCommand?.Execute( IsEditing );
            } );

        }

        // Add Remove Items from Shopping List
        private AsyncCommand addRemoveItemsCommand;
        public ICommand AddRemoveItemsCommand
        {
            get => addRemoveItemsCommand ??= new AsyncCommand( AddRemoveItems );
        }
        private async Task AddRemoveItems()
        {
            await Save();

            StringBuilder builder = new StringBuilder();

            if( Materials.Count > 0 )
            {
                for( int i = 0; i < shoppingListMaterialIds.Count; i++ )
                {
                    _ = i < 1
                        ? builder.Append( $"{_shoppingList.Materials[i].MaterialId}" )
                        : builder.Append( $",{_shoppingList.Materials[i].MaterialId}" );
                }
            }
            string encodedShoppingListId = HttpUtility.UrlEncode($"{ _shoppingList.Id }");

            await Shell.Current.GoToAsync( $"{nameof( AddMaterialsToShoppingListView )}?{QueryParameters.ShoppingListId}={encodedShoppingListId}" );
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

            await Shell.Current.GoToAsync( $"{nameof( ShoppingListDetailView )}?{QueryParameters.ShoppingListId}={encodedId}" );
        }

        // Delete Shopping List
        private AsyncCommand deleteShoppingListCommand;
        public ICommand DeleteShoppingListCommand
        {
            get => deleteShoppingListCommand ??= new AsyncCommand( DeleteShoppingList );
        }
        private async Task DeleteShoppingList()
        {
            if( await Shell.Current.DisplayAlert( $"Delete {_shoppingList.Name}", $"Are you sure you want to delete {_shoppingList.Name}?", "Yes", "No" ) )
            {
                await DbServiceLocator.DeleteItemAsync<ShoppingList>( _shoppingList.Id );
                await RefreshContainer.ExecuteAsync();

            }
        }

        private AsyncCommand saveAndGoBackCommand;
        public ICommand SaveAndGoBackCommand
        {
            get => saveAndGoBackCommand ??= new AsyncCommand( SaveAndGoBack );
        }
        private async Task SaveAndGoBack()
        {
            await Save();
            await Shell.Current.GoToAsync( $"..?{QueryParameters.Refresh}={QueryParameters.Refresh}" );
        }

        #endregion

        #region Methods

        private async Task Save()
        {
            _shoppingList.Materials.Clear();
            foreach( ShoppingListMaterialViewModel matVM in Materials )
            {
                _shoppingList.Materials.Add( matVM.ShoppingListMaterial );
            }
            _shoppingList.Name = Name;
            _shoppingList.Active = Count == 0;

            await DbServiceLocator.UpdateItemAsync( _shoppingList );
        }

        private async Task Refresh()
        {
            _shoppingList = await DbServiceLocator.GetItemRecursiveAsync<ShoppingList>( Id );
            Name = _shoppingList.Name;
            Count = _shoppingList.Materials.Count;

            if( isEditing )
            {
               await EditShoppingListMaterials();
            }
            Materials.Clear();
            shoppingListMaterialIds.Clear();

            ConcurrentBag<ShoppingListMaterialViewModel> vms = new ConcurrentBag<ShoppingListMaterialViewModel>();

            ConcurrentBag<int> slmIds = new ConcurrentBag<int>();

            _ = Parallel.ForEach( _shoppingList.Materials, material =>
            {
                slmIds.Add( material.Id );
                ShoppingListMaterialViewModel vm = new ShoppingListMaterialViewModel(material);
                vm.OnPurchasedChanged += UpdateCount;
                vm.RefreshParentPageOnItemDeleteAsyncCommand = new AsyncCommand( Refresh );
                vms.Add( vm );
            } );

            foreach( int id in slmIds )
            {
                _ = shoppingListMaterialIds.Add( id );
            }

            Materials.AddRange( vms );
            Count = Materials.Where( x => !x.Purchased ).Count();
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
                case QueryParameters.ShoppingListId:
                    if( int.TryParse( kvp.Value, out Id ) )
                    {
                        await Refresh();
                    }
                    break;

                case QueryParameters.ShoppingListMaterialIds:
                    string[] stringIds = HttpUtility.UrlDecode(kvp.Value).Split(',');
                    int[] ids = new int[stringIds.Length];

                    shoppingListMaterialIds.Clear();

                    for( int i = 0; i < stringIds.Length; i++ )
                    {
                        _ = int.TryParse( stringIds[i], out ids[i] );
                        _ = shoppingListMaterialIds.Add( ids[i] );
                    }
                    await AddShoppingListMaterialsToShoppingList();
                    break;

                case QueryParameters.Refresh:
                    await Refresh();
                    break;
            }
        }

        private async Task AddShoppingListMaterialsToShoppingList()
        {
            ConcurrentBag<ShoppingListMaterialViewModel> vms = new ConcurrentBag<ShoppingListMaterialViewModel>();

            List<ShoppingListMaterial> sLM = await DbServiceLocator.GetItemRangeRecursiveAsync<ShoppingListMaterial>(shoppingListMaterialIds) as List<ShoppingListMaterial>;

            _ = Parallel.ForEach( sLM, m =>
            {
                vms.Add( new ShoppingListMaterialViewModel( m ) );
            } );


            Materials.Clear();
            Materials.AddRange( vms );
            await Save();
            await Refresh();
        }

        #endregion
        #endregion

    }
}