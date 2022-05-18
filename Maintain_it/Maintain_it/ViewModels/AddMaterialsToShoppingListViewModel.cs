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

using MvvmHelpers;
using MvvmHelpers.Commands;

using Xamarin.Forms;

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
        private AsyncCommand saveCommand;
        public ICommand SaveCommand { get => saveCommand ??= new AsyncCommand( Save ); }
        private async Task Save()
        {
            if( shoppingList != null )
            {
                foreach( ShoppingListMaterialViewModel vm in DisplayedShoppingListMaterials )
                {
                    vm.ShoppingList = shoppingList;
                    int id = await vm.AddOrUpdateAndReturnIdAsync();

                    ShoppingListMaterial mat = await DbServiceLocator.GetItemRecursiveAsync<ShoppingListMaterial>(id);
                    if( !shoppingList.Materials.Contains( mat ) )
                    {
                        shoppingList.Materials.Add( mat );
                    }
                }

                int shoppingListId = await DbServiceLocator.AddOrUpdateItemAndReturnIdAsync( shoppingList );

                string encodedId = HttpUtility.UrlEncode( shoppingListId.ToString() );
                await Shell.Current.GoToAsync( $"..?{RoutingPath.ShoppingListId}={encodedId}" );
            }
            else
            {
                StringBuilder stringBuilder = new StringBuilder();

                for( int i = 0; i < DisplayedShoppingListMaterials.Count; i++ )
                {
                    DisplayedShoppingListMaterials[i].ShoppingList = shoppingList;

                    _ = i < 1 ? stringBuilder.Append( $"{await DisplayedShoppingListMaterials[i].AddOrUpdateAndReturnIdAsync()}" ) : stringBuilder.Append( $",{await DisplayedShoppingListMaterials[i].AddOrUpdateAndReturnIdAsync()}" );
                }

                string encoded = HttpUtility.UrlEncode( stringBuilder.ToString() );

                await Shell.Current.GoToAsync( $"..?{RoutingPath.ShoppingListMaterialIds}={encoded}" );

            }
        }

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
                    HashSet<int> temp = new HashSet<int>();

                    foreach( MaterialViewModel mVM in list )
                    {
                        _ = temp.Add( mVM.Material.Id );
                    }


                    displayedMaterialIds.Align( temp, out List<int> added, out List<int> removed );

                    UpdateCache( added );

                    RemoveItemsFromDisplayedList( removed );

                    await AddItemsToDisplayedList( added );
                }
            }
        }

        private async Task AddItemsToDisplayedList( IEnumerable<int> added )
        {
            if( DisplayedShoppingListMaterials.Count < displayedMaterialIds.Count )
            {
                foreach( int id in added )
                {
                    Material mat = await DbServiceLocator.GetItemAsync<Material>(id);
                    if( mat != null )
                    {
#nullable enable
                        ShoppingListMaterial? material = null;
#nullable disable
                        if( shoppingList != null )
                        {
                            foreach( ShoppingListMaterial sLMat in shoppingList.Materials )
                            {
                                if( sLMat.MaterialId == id )
                                {
                                    material = await DbServiceLocator.GetItemRecursiveAsync<ShoppingListMaterial>( sLMat.Id );
                                }
                            }
                        }

                        ShoppingListMaterialViewModel vm;

                        if( material != null )
                        {
                            vm = new ShoppingListMaterialViewModel( material );
                        }
                        else
                        {
                            vm = new ShoppingListMaterialViewModel( mat, shoppingList );
                        }

                        DisplayedShoppingListMaterials.Add( vm );
                    }

                }
            }
        }

        private void RemoveItemsFromDisplayedList( List<int> removed )
        {
            foreach( ShoppingListMaterialViewModel vm in DisplayedShoppingListMaterials.ToList() )
            {
                if( removed.Contains( vm.Material.Id ) )
                {
                    _ = cache.Add( vm );
                    _ = DisplayedShoppingListMaterials.Remove( vm );
                    _ = selectedMaterials.Remove( vm.ShoppingListMaterial.Id, out ShoppingListMaterial _ );
                }
            }
        }

        private void UpdateCache( List<int> added )
        {
            foreach( ShoppingListMaterialViewModel vm in cache.ToList() )
            {
                if( added.Contains( vm.Material.Id ) )
                {
                    DisplayedShoppingListMaterials.Add( vm );
                    _ = selectedMaterials.GetOrAdd( vm.ShoppingListMaterial.Id, vm.ShoppingListMaterial );
                    _ = cache.Remove( vm );
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



        #region Query Handling
        private protected override async Task EvaluateQueryParams( KeyValuePair<string, string> kvp )
        {
            await base.EvaluateQueryParams( kvp );

            switch( kvp.Key )
            {
                //TODO: Review
                case RoutingPath.ShoppingListId:
                    await Refresh();
                    if( int.TryParse( HttpUtility.HtmlDecode( kvp.Value ), out int shoppingListId ) )
                    {
                        shoppingList = await DbServiceLocator.GetItemRecursiveAsync<ShoppingList>( shoppingListId );
                        Dictionary<int, ShoppingListMaterial> preSelectedShoppingListMaterials = new Dictionary<int, ShoppingListMaterial>();
                        List<ShoppingListMaterial> slmMats = await DbServiceLocator.GetAllItemsRecursiveAsync<ShoppingListMaterial>() as List<ShoppingListMaterial>;

                        foreach( ShoppingListMaterial s in slmMats.Where( x => x.ShoppingListId == shoppingListId ) )
                        {
                            preSelectedShoppingListMaterials.Add( s.Id, s );
                            Console.WriteLine( $"ID:{s.Id} - Name: {s.Name} - Material ID: {s.MaterialId}" );
                        }

                        if( shoppingList.Materials.Count > 0 )
                        {
                            HashSet<int> items = new HashSet<int>();

                            foreach( ShoppingListMaterial material in shoppingList.Materials.ToList() )
                            {
                                _ = items.Add( material.MaterialId );
                            }

                            AddMaterialsToSelectedMaterials( items.ToList() );
                        }
                    }
                    break;
                //TODO: Review
                case RoutingPath.ShoppingListMaterialIds:
                    await Refresh();
                    string[] sLMIds = HttpUtility.HtmlDecode( kvp.Value ).Split(',');
                    List<int> shoppingListMaterialIds = new List<int>();
                    foreach( string id in sLMIds )
                    {
                        if( int.TryParse( id, out int shoppingListMaterialId ) )
                        {
                            shoppingListMaterialIds.Add( shoppingListMaterialId );
                        }
                    }

                    List<ShoppingListMaterial> mats = await DbServiceLocator.GetItemRangeAsync<ShoppingListMaterial>(shoppingListMaterialIds) as List<ShoppingListMaterial>;

                    _ = Parallel.ForEach( mats, mat =>
                     {
                         _ = selectedMaterials.GetOrAdd( mat.Id, mat );
                     } );
                    break;

                case RoutingPath.MaterialIds:
                    await Refresh();
                    string[] materialIds = HttpUtility.HtmlDecode(kvp.Value).Split(',');

                    foreach( string id in materialIds )
                    {
                        if( int.TryParse( id, out int materialId ) )
                        {
                            _ = displayedMaterialIds.Add( materialId );
                        }
                    }

                    AddMaterialsToSelectedMaterials( displayedMaterialIds.ToList() );
                    break;

            }

            await Refresh();
        }

        private void AddMaterialsToSelectedMaterials( List<int> items )
        {
            List<MaterialViewModel> mats = new List<MaterialViewModel>();

            foreach( int id in items )
            {
                if( materialVMs.ContainsKey( id ) )
                {
                    mats.Add( materialVMs[id] );
                }
            }
            SelectedMaterialViewModels.AddRange( mats );
        }
        #endregion
        #endregion



    }
}
