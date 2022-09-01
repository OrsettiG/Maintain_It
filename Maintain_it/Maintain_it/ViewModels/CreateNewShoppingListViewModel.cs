using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Input;

using Maintain_it.Helpers;
using Maintain_it.Models;
using Maintain_it.Services;
using Maintain_it.ViewModels;
using Maintain_it.Views;

using MvvmHelpers;
using MvvmHelpers.Commands;

using Xamarin.Forms;

using static Xamarin.Essentials.Permissions;

namespace Maintain_it.ViewModels
{
    public class CreateNewShoppingListViewModel : BaseViewModel
    {
        #region Constructors

        public CreateNewShoppingListViewModel()
        {
            shoppingList = new ShoppingList();
            _ = Task.Run( async () => await Init() );
        }

        #endregion


        #region Properties
        private readonly ShoppingList shoppingList;

        private string name;
        public string Name
        {
            get => name;
            set => SetProperty( ref name, value );
        }

        private ObservableRangeCollection<ShoppingListMaterialViewModel> shoppingListMaterials;
        public ObservableRangeCollection<ShoppingListMaterialViewModel> ShoppingListMaterials
        {
            get => shoppingListMaterials ??= new ObservableRangeCollection<ShoppingListMaterialViewModel>();
            set => SetProperty( ref shoppingListMaterials, value );
        }

        private Dictionary<int, int> preSelectedMaterialsAndQuantities = new Dictionary<int, int>();
        HashSet<int> shoppingListMaterialIds = new HashSet<int>();
        #endregion

        #region COMMANDS

        // --- AddShallow Material(s) ---
        private AsyncCommand addShoppingListMaterialsCommand;
        public ICommand AddShoppingListMaterialsCommand
        {
            get => addShoppingListMaterialsCommand ??= new AsyncCommand( AddShoppingListMaterials );
        }
        private async Task AddShoppingListMaterials()
        {
            string encodedIds = HttpUtility.HtmlEncode( shoppingListMaterialIds );

            await Shell.Current.GoToAsync( $"{nameof( AddMaterialsToShoppingListView )}?{QueryParameters.ShoppingListMaterialIds}={encodedIds}" );
        }

        // --- Save List ---
        private AsyncCommand saveCommand;
        public ICommand SaveCommand { get => saveCommand ??= new AsyncCommand( Save ); }
        private async Task Save()
        {
            shoppingList.Materials.Clear();

            foreach( ShoppingListMaterialViewModel vm in ShoppingListMaterials )
            {
                _ = await vm.AddOrUpdateAndReturnIdAsync();
                vm.ShoppingListMaterial.ShoppingList = shoppingList;
                shoppingList.Materials.Add( vm.ShoppingListMaterial );
            }

            shoppingList.Name = Name;
            shoppingList.CreatedOn = DateTime.UtcNow;
            shoppingList.Active = true;

            if( await ShoppingListManager.UpdateShoppingListAsync( shoppingList.Id, Name, materials: shoppingList.Materials ) )
            {
                await Shell.Current.GoToAsync( $"..?{QueryParameters.Refresh}=true" );
            }
            else
            {
                await Shell.Current.DisplayAlert( Alerts.Error, Alerts.DatabaseErrorMessage, Alerts.Confirmation );
            }
        }

        private AsyncCommand backWithRefreshCommand;
        public ICommand BackWithRefreshCommand
        {
            get => backWithRefreshCommand ??= new AsyncCommand( BackWithRefresh );
        }

        private async Task BackWithRefresh()
        {
            await Shell.Current.GoToAsync( $"..?{QueryParameters.Refresh}=true" );
        }
        #endregion

        #region Methods

        private async Task Init()
        {
            shoppingList.Materials = new List<ShoppingListMaterial>();
            shoppingList.CreatedOn = DateTime.UtcNow;


            await Refresh();
        }

        private async Task Refresh()
        {

        }

        /// <summary>
        /// Adds the ShoppingList to the Database if it does not already exist, updates the existing record if there is one.
        /// </summary>
        /// <returns><see cref="int"/> <see cref="ShoppingList.Id"/></returns>
        private async Task<int> AddOrUpdate()
        {
            ConcurrentBag<ShoppingListMaterial> mats = new ConcurrentBag<ShoppingListMaterial>();

            _ = Parallel.ForEach( ShoppingListMaterials, material =>
            {
                mats.Add( material.ShoppingListMaterial );
                material.ShoppingList = shoppingList;
            } );

            shoppingList.Name = Name;
            shoppingList.CreatedOn = DateTime.UtcNow;
            shoppingList.Materials = new List<ShoppingListMaterial>( mats );
            shoppingList.Active = true;

            return await DbServiceLocator.AddOrUpdateItemAndReturnIdAsync( shoppingList );
        }

        #region Query Handling
        private protected override async Task EvaluateQueryParams( KeyValuePair<string, string> kvp )
        {

            switch( kvp.Key )
            {
                case QueryParameters.NewItem:
                    shoppingList.Id = await ShoppingListManager.NewShoppingList();
                    break;

                case QueryParameters.PreSelectedMaterialIds:
                    await ProcessMaterialIds( kvp.Value );
                    break;

                case QueryParameters.ShoppingListMaterialIds:
                    string[] stringIds = HttpUtility.UrlDecode(kvp.Value).Split(',');
                    int[] ids = new int[stringIds.Length];
                    for( int i = 0; i < stringIds.Length; i++ )
                    {
                        _ = int.TryParse( stringIds[i], out ids[i] );
                        _ = shoppingListMaterialIds.Add( ids[i] );
                    }
                    await AddShoppingListMaterialsToShoppingList();
                    break;

                case QueryParameters.ItemName:
                    Name = HttpUtility.UrlDecode( kvp.Value );
                    break;

                case QueryParameters.Refresh:
                    await Refresh();
                    break;
            }

        }

        private async Task ProcessMaterialIds( string encodedKvps )
        {
            string[] kvps = HttpUtility.UrlDecode( encodedKvps ).Split(";");

            foreach( string kvp in kvps )
            {
                string[] KeyValuePair = kvp.Split("=");
                if( int.TryParse( KeyValuePair[0], out int k ) && int.TryParse( KeyValuePair[1], out int v ) )
                {
                    preSelectedMaterialsAndQuantities.Add( k, v );
                }
            }

            if( shoppingList.Id == 0 )
            {
                shoppingList.Id = await ShoppingListManager.NewShoppingList();
            }
            await AddPreselectedMaterialsToShoppingList();
        }


        private async Task AddPreselectedMaterialsToShoppingList()
        {
            List<ShoppingListMaterialViewModel> vms = new List<ShoppingListMaterialViewModel>();

            List<Material> materials = await DbServiceLocator.GetItemRangeRecursiveAsync<Material>(preSelectedMaterialsAndQuantities.Keys) as List<Material>;

            foreach( int key in preSelectedMaterialsAndQuantities.Keys )
            {
                Material m = materials.Where( x => x.Id == key ).First();

                int id = await ShoppingListMaterialManager.NewShoppingListMaterial( m.Id, shoppingList.Id, m.Name, preSelectedMaterialsAndQuantities[key] );

                ShoppingListMaterial sLM = await ShoppingListMaterialManager.GetItemAsync(id);

                vms.Add( new ShoppingListMaterialViewModel( sLM ) );
            }

            ShoppingListMaterials.AddRange( vms );
        }

        private async Task AddShoppingListMaterialsToShoppingList()
        {
            ConcurrentBag<ShoppingListMaterialViewModel> vms = new ConcurrentBag<ShoppingListMaterialViewModel>();

            List<ShoppingListMaterial> sLM = await DbServiceLocator.GetItemRangeRecursiveAsync<ShoppingListMaterial>(shoppingListMaterialIds) as List<ShoppingListMaterial>;

            _ = Parallel.ForEach( sLM, m =>
            {
                vms.Add( new ShoppingListMaterialViewModel( m ) );
            } );

            ShoppingListMaterials.Clear();
            ShoppingListMaterials.AddRange( vms );
        }


        #endregion
        #endregion

    }
}
