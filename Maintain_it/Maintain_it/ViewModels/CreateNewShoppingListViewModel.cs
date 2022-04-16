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

namespace Maintain_it.ViewModels
{
    public class CreateNewShoppingListViewModel : BaseViewModel
    {
        #region Constructors

        public CreateNewShoppingListViewModel()
        {
            shoppingList = new ShoppingList();
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

        private List<ShoppingListMaterialViewModel> shoppingListMaterials;
        public List<ShoppingListMaterialViewModel> ShoppingListMaterials 
        { 
            get => shoppingListMaterials ??= new List<ShoppingListMaterialViewModel>(); 
            set => SetProperty( ref shoppingListMaterials, value ); 
        }

        private List<int> preSelectecMaterialIds;
        #endregion

        #region Commands


        private AsyncCommand addShoppingListMaterialsCommand;
        public ICommand AddShoppingListMaterialsCommand { get => addShoppingListMaterialsCommand ??= new AsyncCommand( AddShoppingListMaterials ); }
        private async Task AddShoppingListMaterials()
        {
            int id = await AddOrUpdate();

            string encodedId = HttpUtility.HtmlEncode( id );

            await Shell.Current.GoToAsync( $"{nameof( AddMaterialsToShoppingListView )}?{RoutingPath.ShoppingListId}={encodedId}" );
        }

        #endregion

        #region Methods

        private async Task Init()
        {
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
            } );

            shoppingList.Name = Name;
            shoppingList.CreatedOn = DateTime.Now;
            shoppingList.Materials = new List<ShoppingListMaterial>( mats );
            shoppingList.Active = true;

            return await DbServiceLocator.AddItemAndReturnIdAsync( shoppingList );
        }

        #region Query Handling
        private protected override async Task EvaluateQueryParams( KeyValuePair<string, string> kvp )
        {
            await base.EvaluateQueryParams( kvp );

            switch( kvp.Key )
            {
                case nameof( preSelectecMaterialIds ):
                    await ProcessMaterialIds( kvp.Value );
                    break;
            }

        }

        private async Task ProcessMaterialIds( string encodedIds )
        {
            string[] ids = HttpUtility.HtmlDecode( encodedIds ).Split(",");

            foreach( string id in ids )
            {
                if( int.TryParse( id, out int n ) )
                {
                    preSelectecMaterialIds.Add( n );
                }
            }

            await AddPreselectedMaterialsToShoppingList();
        }

        private async Task AddPreselectedMaterialsToShoppingList()
        {
            ConcurrentBag<ShoppingListMaterialViewModel> vms = new ConcurrentBag<ShoppingListMaterialViewModel>();

            ConcurrentBag<Material> materials = await DbServiceLocator.GetItemRangeRecursiveAsync<Material>(preSelectecMaterialIds) as ConcurrentBag<Material>;

            _ = Parallel.ForEach( preSelectecMaterialIds, id =>
            {
                Material m = materials.Where( x => x.Id == id ).First();

                vms.Add( new ShoppingListMaterialViewModel( m, shoppingList ) );
            } );

            ShoppingListMaterials.AddRange( vms );
        }

        #endregion
        #endregion

    }
}
