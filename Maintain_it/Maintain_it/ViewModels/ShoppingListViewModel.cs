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
using Maintain_it.Models.Interfaces;
using Maintain_it.Services;
using Maintain_it.Views;

using MvvmHelpers;
using MvvmHelpers.Commands;

using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

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
            RemainingItemsCount = shoppingList.LooseMaterials.Count;
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
        public int RemainingItemsCount
        {
            get => count;
            set => SetProperty( ref count, value );
        }

        private int remainingProjectsCount;
        public int RemainingProjectsCount
        {
            get => remainingProjectsCount;
            set => SetProperty( ref remainingProjectsCount, value );
        }

        private int groupedItemsCount;
        public int GroupedItemsCount
        {
            get => groupedItemsCount;
            set => SetProperty( ref groupedItemsCount, value );
        }

        private int looseItemsCount;
        public int LooseItemsCount
        {
            get => looseItemsCount;
            set => SetProperty( ref looseItemsCount, value );
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
        private readonly Dictionary<int,int> _materialIdsAndQuantities = new Dictionary<int, int>();

        private ObservableRangeCollection<ShoppingListMaterialViewModel> looseMaterials;
        public ObservableRangeCollection<ShoppingListMaterialViewModel> LooseMaterials
        {
            get => looseMaterials ??= new ObservableRangeCollection<ShoppingListMaterialViewModel>();

            set => SetProperty( ref looseMaterials, (ObservableRangeCollection<ShoppingListMaterialViewModel>)value.OrderBy( x => x.Material.Id ) );
        }

        private ObservableRangeCollection<ShoppingListItemGroupViewModel> groupedMaterials;
        public ObservableRangeCollection<ShoppingListItemGroupViewModel> GroupedMaterials
        {
            get => groupedMaterials ??= new ObservableRangeCollection<ShoppingListItemGroupViewModel>();
            //( ObservableRangeCollection<ShoppingListItemGroupViewModel> )( groupedMaterials ??= new ObservableRangeCollection<ShoppingListItemGroupViewModel>() ).OrderBy( x => x.RemainingItemsCount );

            set => SetProperty( ref groupedMaterials, value );
        }
        #endregion

        #region Commands
        // Edit Shopping List LooseMaterials
        private AsyncCommand editShoppingListMaterialsCommand;
        public ICommand EditShoppingListMaterialsCommand
        {
            get => editShoppingListMaterialsCommand ??= new AsyncCommand( EditShoppingListMaterials );
        }

        //
        //
        //
        //
        private async Task EditShoppingListMaterials()
        {
            string notEditingColor = App.Current.UserAppTheme switch
            {
                OSAppTheme.Light => Config.AppResourceKeys.LightPrimary.ToString(),
                OSAppTheme.Dark => Config.AppResourceKeys.DarkPrimary.ToString(),
                _ => Config.AppResourceKeys.LightPrimary.ToString(),
            };

            string editingColor = Config.AppResourceKeys.Accent2.ToString();

            IsEditing = !IsEditing;
            if( IsEditing )
            {
                EditingStateColor = IsEditing ? (Color)App.Current.Resources[editingColor] : (Color)App.Current.Resources[notEditingColor];

                _ = Parallel.ForEach( GroupedMaterials, mat =>
                {
                    mat.ForEach( x => x.ToggleEditStateCommand?.Execute( IsEditing ) );
                } );
            }
        }

        // AddShallow Remove Items from Shopping List
        private AsyncCommand addRemoveItemsCommand;
        public ICommand AddRemoveItemsCommand
        {
            get => addRemoveItemsCommand ??= new AsyncCommand( AddRemoveItems );
        }
        private async Task AddRemoveItems()
        {
            await Save();

            StringBuilder builder = new StringBuilder();

            if( LooseMaterials.Count > 0 )
            {
                for( int i = 0; i < shoppingListMaterialIds.Count; i++ )
                {
                    _ = i < 1
                        ? builder.Append( $"{_shoppingList.LooseMaterials[i].MaterialId}" )
                        : builder.Append( $",{_shoppingList.LooseMaterials[i].MaterialId}" );
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
            //_shoppingList.LooseMaterials.Clear();
            //foreach( ShoppingListMaterialViewModel matVM in LooseMaterials )
            //{
            //    _shoppingList.LooseMaterials.Add( matVM.ShoppingListMaterial );
            //}
            //_shoppingList.Name = Name;
            //_shoppingList.Active = !( RemainingItemsCount == 0 );

            await DbServiceLocator.UpdateItemAsync( _shoppingList );
        }

        //TODO: Update this method to utilize the GroupedMaterials collection and refresh all the groups. It should put the LooseMaterials into their own GroupedShoppingListItemViewModel and add it to the GroupedMaterials collection under the name "Other Items"

        /* Problem:
         * This refresh method is called every time an item is deleted from the ShoppingList. That is because, as it stands, we do not do anything with the id passed back from the OnItemDeleted event, and instead just refresh the whole list.
         * What we need to do is use the id passed back from the OnItemDeleted event to remove the deleted item from the view without refreshing the whole list.
         * We also need a way to track items removed from the list that are not in the database. That way if the whole list needs to be refreshed the user does not find that items the thought were deleted are back.
         *  - This could be done by refreshing specific items as often as possible, instead of the whole list, and by comparing the data already in the list with the data in the database when refreshing.
         *  
         *  We may just need to create ShoppingListMaterials for all the items  
        */

        private async Task Refresh()
        {
            _shoppingList = await DbServiceLocator.GetItemRecursiveAsync<ShoppingList>( Id );
            Name = _shoppingList.Name;
            RemainingItemsCount = 0;


            LooseMaterials.Clear();
            shoppingListMaterialIds.Clear();

            // Will be processed into ShoppingListItemGroup
            ConcurrentDictionary<string, HashSet<IShoppingListItemViewModel>> materialNamesAndProjectVMs = new ConcurrentDictionary<string, HashSet<IShoppingListItemViewModel>>();

            ConcurrentDictionary<Material, ConcurrentDictionary<string, int>> materialNamesAndServiceItemNamesWithQuantities = new ConcurrentDictionary<Material, ConcurrentDictionary<string, int>>();

            // Used to calculate absolute shortfall
            ConcurrentDictionary<Material, int> materialsAndRequiredQuantities = new ConcurrentDictionary<Material, int>();

            ConcurrentBag<int> slmIds = new ConcurrentBag<int>();
            string otherProjects = "Other Projects";

            _ = Parallel.ForEach( _shoppingList.LooseMaterials, lMat =>
            {
                if( materialNamesAndServiceItemNamesWithQuantities.TryAdd( lMat.Material, new ConcurrentDictionary<string, int>() ) )
                {
                    if( !materialNamesAndServiceItemNamesWithQuantities[lMat.Material].TryAdd( otherProjects, lMat.Quantity ) )
                    {
                        materialNamesAndServiceItemNamesWithQuantities[lMat.Material][otherProjects] += lMat.Quantity;
                    }
                }
                else if( !materialNamesAndServiceItemNamesWithQuantities[lMat.Material].TryAdd( otherProjects, lMat.Quantity ) )
                {
                    materialNamesAndServiceItemNamesWithQuantities[lMat.Material][otherProjects] += lMat.Quantity;
                }
            } );

            /* Process:
             *  * Iterate through all ServiceItem StepMaterials and add the materials and absolute         quantity required to a dictionary
             *  * Create a new DynamicShoppingListItemViewModel for the ServiceItem of every material      with the Quantity of the material required for the Quantitiy.
             *      * This is so that the project can show up beneath the each material with the number        of that material which that project requires
             *  
             * 
             */



            _ = Parallel.ForEach( _shoppingList.ServiceItems, item =>
            {
                foreach( Step step in item.Steps )
                {
                    foreach( StepMaterial stepMaterial in step.StepMaterials )
                    {
                        if( !materialsAndRequiredQuantities.TryAdd( stepMaterial.Material, stepMaterial.Quantity ) )
                        {
                            materialsAndRequiredQuantities[stepMaterial.Material] += stepMaterial.Quantity;
                        }

                        if( materialNamesAndServiceItemNamesWithQuantities.TryAdd( stepMaterial.Material, new ConcurrentDictionary<string, int>() ) )
                        {
                            if( !materialNamesAndServiceItemNamesWithQuantities[stepMaterial.Material].TryAdd( item.Name, stepMaterial.Quantity ) )
                            {
                                materialNamesAndServiceItemNamesWithQuantities[stepMaterial.Material][item.Name] += stepMaterial.Quantity;
                            }
                        }
                        else if( !materialNamesAndServiceItemNamesWithQuantities[stepMaterial.Material].TryAdd( item.Name, stepMaterial.Quantity ) )
                        {
                            materialNamesAndServiceItemNamesWithQuantities[stepMaterial.Material][item.Name] += stepMaterial.Quantity;
                        }
                    }
                }
            } );

            ConcurrentDictionary<Material, int> materialsAndStockShortfallAmounts = new ConcurrentDictionary<Material, int>();

            // This needs to be updated. We can't check the shortfall of the materials until after we have added the loose materials to our item groups. Or at the very least, we need to go through our loose materials and add them back into the item groups eventually.
            foreach( KeyValuePair<Material, int> kvp in materialsAndRequiredQuantities )
            {
                if( IsShortfall( kvp.Value, kvp.Key.QuantityOwned, out int shortfall ) )
                {
                    if( !materialsAndStockShortfallAmounts.TryAdd( kvp.Key, shortfall ) )
                    {
                        Console.WriteLine( $"Something went wrong adding {kvp.Key.Name} to materialsAndStockShortfallAmounts with value {shortfall}" );
                    }
                }
            }

            ConcurrentBag<ShoppingListItemGroupViewModel> itemGroups = new ConcurrentBag<ShoppingListItemGroupViewModel>();

            _ = Parallel.ForEach( materialNamesAndServiceItemNamesWithQuantities.Keys, mat =>
            {
                List<IShoppingListItemViewModel> items = new List<IShoppingListItemViewModel>();

                int shortfallCount = 0;

                if( materialsAndStockShortfallAmounts.ContainsKey( mat ) )
                {
                    shortfallCount = materialsAndStockShortfallAmounts[mat];
                }

                foreach( KeyValuePair<string, int> item in materialNamesAndServiceItemNamesWithQuantities[mat] )
                {
                    if( item.Key == otherProjects )
                    {
                        ShoppingListMaterial sLM = _shoppingList.LooseMaterials.Where( x => x.MaterialId == mat.Id ).FirstOrDefault();

                        if( sLM != null )
                        {
                            ShoppingListMaterialViewModel sLMVM = new ShoppingListMaterialViewModel( sLM )
                            {
                                Name = item.Key
                            };
                            items.Add( sLMVM );

                            if( sLMVM.Purchased )
                                RemainingItemsCount -= item.Value;
                        }
                        else
                        {
                            items.Add( new DynamicShoppingListItemViewModel( item.Key, mat, _shoppingList, item.Value ) );
                        }

                        RemainingItemsCount += item.Value;
                        continue;
                    }
                    else
                    {
                        if( shortfallCount <= 0 )
                        {
                            break;
                        }

                        if( shortfallCount - item.Value > 0 )
                        {
                            items.Add( new DynamicShoppingListItemViewModel( item.Key, mat, _shoppingList, item.Value ) );
                            shortfallCount -= item.Value;
                        }
                        else if( shortfallCount - item.Value <= 0 )
                        {
                            items.Add( new DynamicShoppingListItemViewModel( item.Key, mat, _shoppingList, item.Value + ( shortfallCount - item.Value ) ) );
                            shortfallCount = 0;
                            break;
                        }
                    }
                }

                if( items.Count > 0 )
                {
                    ShoppingListItemGroupViewModel itemGroup = new ShoppingListItemGroupViewModel(mat.Name, items);
                    itemGroup.OnItemPurchased += UpdateCount;
                    itemGroup.OnAllItemsPurchased += AllItemsPurchasedInProject;

                    if( itemGroup.PurchasedCount == itemGroup.TotalCount )
                    {
                        AllItemsPurchasedInProject( 1 );
                    }

                    itemGroups.Add( itemGroup );
                }
            } );

            GroupedMaterials = new ObservableRangeCollection<ShoppingListItemGroupViewModel>( itemGroups );

            foreach( int id in slmIds )
            {
                _ = shoppingListMaterialIds.Add( id );
            }

            foreach( KeyValuePair<Material, int> kvp in materialsAndStockShortfallAmounts )
            {
                Console.WriteLine( $"Material Name: {kvp.Key.Name} Shortfall: {kvp.Value}" );

                RemainingItemsCount += kvp.Value;
            }

            if( isEditing )
            {
                await EditShoppingListMaterials();
            }
        }

        /// <summary>
        /// Takes the amount of a material required and the amount owned and returns true if the amount required is greater than the amount owned, while outing the difference. Otherwise returns false and outs zero. 
        /// </summary>
        private bool IsShortfall( int required, int inStock, out int shortfall )
        {
            if( required > inStock )
            {
                shortfall = required - inStock;
                return true;
            }

            shortfall = 0;
            return false;
        }

        private async Task<int> CalculateListItemCount()
        {
            throw new NotImplementedException();
        }

        private async Task CreateItemGroups()
        {
            ConcurrentDictionary<string, List<ShoppingListMaterialViewModel>> GroupDictionary = new ConcurrentDictionary<string, List<ShoppingListMaterialViewModel>>();

            _ = Parallel.ForEach( _shoppingList.ServiceItems, item =>
            {
                List<ShoppingListMaterialViewModel> slmVMs = new List<ShoppingListMaterialViewModel>();
                foreach( Step step in item.Steps )
                {
                    foreach( StepMaterial stepMaterial in step.StepMaterials )
                    {
                        if( stepMaterial.Quantity > stepMaterial.Material.QuantityOwned )
                        {
                            slmVMs.Add( new ShoppingListMaterialViewModel( stepMaterial, _shoppingList ) );
                        }
                    }
                }
            } );
        }

        private void UpdateCount( int x )
        {
            RemainingItemsCount -= x;
        }

        private void AllItemsPurchasedInProject( int change )
        {
            RemainingProjectsCount += change;
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


            LooseMaterials.Clear();
            LooseMaterials.AddRange( vms );
            await Save();
            await Refresh();
        }

        #endregion
        #endregion

    }
}