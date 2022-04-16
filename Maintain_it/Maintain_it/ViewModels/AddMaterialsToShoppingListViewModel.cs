using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using Maintain_it.Helpers;
using Maintain_it.Models;
using Maintain_it.Services;

using MvvmHelpers;

namespace Maintain_it.ViewModels
{
    public class AddMaterialsToShoppingListViewModel : AddMaterialsViewModel
    {
        #region Properties
        private ShoppingList shoppingList;

        private List<Material> materials;

        private ConcurrentDictionary<int, MaterialViewModel> materialVMs = new ConcurrentDictionary<int, MaterialViewModel>();
        private ObservableRangeCollection<MaterialViewModel> materialViewModels;
        public ObservableRangeCollection<MaterialViewModel> MaterialViewModels
        {
            get => materialViewModels ??= new ObservableRangeCollection<MaterialViewModel>();
            set => SetProperty( ref materialViewModels, value );
        }

        private ConcurrentDictionary<int, ShoppingListMaterial> selectedMaterials = new ConcurrentDictionary<int, ShoppingListMaterial>();
        private ObservableRangeCollection<object> selectedMaterialViewModels;
        public ObservableRangeCollection<object> SelectedMaterialViewModels
        {
            get => selectedMaterialViewModels ??= new ObservableRangeCollection<object>();
            set => SetProperty( ref selectedMaterialViewModels, value );
        }

        private HashSet<ShoppingListMaterialViewModel> cache = new HashSet<ShoppingListMaterialViewModel>();

        private HashSet<int> displayedMaterialIds = new HashSet<int>();
        private ObservableRangeCollection<ShoppingListMaterialViewModel> displayedShoppingListMaterials;
        public ObservableRangeCollection<ShoppingListMaterialViewModel> DisplayedShoppingListMaterials
        {
            get => displayedShoppingListMaterials ??= new ObservableRangeCollection<ShoppingListMaterialViewModel>();
            set => SetProperty( ref displayedShoppingListMaterials, value );
        }
        #endregion

        #region Commands
        #endregion

        #region Methods

        /// <summary>
        /// Check the passed in obj against the current selections to see what has changed, and remove it from the selectedMaterials. 
        /// </summary>
        /// <param name="obj"></param>
        private protected override async void MaterialSelectionChanged( object obj )
        {
            if( locked )
                return;

            if( obj is IEnumerable<object> items )
            {

                if( items.ToListWithCast( out List<MaterialViewModel> list ) )
                {
                    List<int> temp = new List<int>();

                    foreach( MaterialViewModel mVM in list )
                    {
                        temp.Add( mVM.Material.Id );
                    }

                    displayedMaterialIds.Align( displayedMaterialIds.Except( temp ), out List<int> added, out List<int> removed );

                    foreach( ShoppingListMaterialViewModel vm in cache.ToList() )
                    {
                        if( added.Contains( vm.Material.Id ) )
                        {
                            DisplayedShoppingListMaterials.Add( vm );
                            _ = selectedMaterials.GetOrAdd( vm.ShoppingListMaterial.Id, vm.ShoppingListMaterial );
                            _ = cache.Remove( vm );
                        }
                    }

                    foreach( ShoppingListMaterialViewModel vm in DisplayedShoppingListMaterials.ToList() )
                    {
                        if( removed.Contains( vm.Material.Id ) )
                        {
                            _ = cache.Add( vm );
                            _ = DisplayedShoppingListMaterials.Remove( vm );
                            _ = selectedMaterials.Remove( vm.ShoppingListMaterial.Id, out ShoppingListMaterial _ );
                        }
                    }

                    if( DisplayedShoppingListMaterials.Count < displayedMaterialIds.Count )
                    {
                        foreach( int id in added )
                        {
                            ShoppingListMaterialViewModel vm = new ShoppingListMaterialViewModel( await DbServiceLocator.GetItemAsync<Material>(id), shoppingList);

                            DisplayedShoppingListMaterials.Add( vm );
                        }
                    }
                }
            }
        }

        private protected override async Task Refresh()
        {
            if( locked )
                return;


            locked = true;


            // All we really care about in this collection is the Id, Name, Description, Size, Units, and CreatedOn properties so we don't need to call recursive. If we want the Tags we will need to either call recursive on each individual material, or get things from the Tag side and filter by material id. Not sure which to do yet.
            materials = await DbServiceLocator.GetAllItemsAsync<Material>() as List<Material>;

            _ = Parallel.ForEach( materials, material =>
             {
                 if( !materialVMs.ContainsKey( material.Id ) )
                 {
                     // If we want to get the recursive data we can do that from within the MaterialViewModel instead of this one. We should try to keep the data loaded here to only what we need.
                     _ = materialVMs.GetOrAdd( material.Id, new MaterialViewModel( material ) );
                 }
             } );

            MaterialViewModels.Clear();

            MaterialViewModels.AddRange( materialVMs.Values );

            locked = false;
        }

        //TODO: Finish Save Method
        private async Task Save()
        {
            foreach( ShoppingListMaterialViewModel mat in DisplayedShoppingListMaterials )
            {
                await mat.UpdateAndReturnIdAsync();
            }
        }

        #region Query Handling
        private protected override async Task EvaluateQueryParams( KeyValuePair<string, string> kvp )
        {
            await base.EvaluateQueryParams( kvp );

            switch( kvp.Key )
            {
                case RoutingPath.ShoppingListId:
                    if( int.TryParse( HttpUtility.HtmlDecode( kvp.Value ), out int id ) )
                    {
                        shoppingList = await DbServiceLocator.GetItemRecursiveAsync<ShoppingList>( id );
                        materials = await DbServiceLocator.GetAllItemsAsync<Material>() as List<Material>;

                        if( shoppingList.Materials.Count > 0 )
                        {
                            _ = Parallel.ForEach( shoppingList.Materials, material =>
                            {
                                _ = selectedMaterials.GetOrAdd( material.Id, material );
                            } );
                        }

                        if( materials.Count > 0 )
                        {
                            _ = Parallel.ForEach( materials, material =>
                            {
                                _ = materialVMs.GetOrAdd( material.Id, new MaterialViewModel( material ) );
                            } );
                        }
                    }

                    await Refresh();
                    break;

            }
        }
        #endregion
        #endregion



    }
}
