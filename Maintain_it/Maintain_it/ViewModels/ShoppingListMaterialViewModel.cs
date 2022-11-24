using System;
using System.Collections.Generic;
using System.Linq;
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

using Xamarin.Forms;

using Command = MvvmHelpers.Commands.Command;

namespace Maintain_it.ViewModels
{
    public class ShoppingListMaterialViewModel : BaseViewModel, IEquatable<ShoppingListMaterialViewModel>, IShoppingListItemViewModel
    {

        public ShoppingListMaterialViewModel( ShoppingListMaterial shoppingListMaterial )
        {
            ShoppingListMaterial = shoppingListMaterial;
            MaterialId = shoppingListMaterial.MaterialId;
            Material = shoppingListMaterial.Material;
            Name = shoppingListMaterial.Name;
            Quantity = shoppingListMaterial.Quantity;
            Purchased = shoppingListMaterial.Purchased;

            if( shoppingListMaterial.ShoppingList != null )
            {
                ShoppingList = shoppingListMaterial.ShoppingList;
            };

            if( Purchased )
            {
                TextDecoration = TextDecorations.Strikethrough;
                Color = (Color)App.Current.Resources["Secondary"];
            }
            else
            {
                TextDecoration = TextDecorations.None;
                Color = (Color)App.Current.Resources["Cue"];
            }
        }

        // Used when creating ShoppingListItemGroupViewModels on the fly.
        public ShoppingListMaterialViewModel( StepMaterial stepMaterial, ShoppingList shoppingList )
        {
            ShoppingListMaterial = new ShoppingListMaterial()
            {
                Material = stepMaterial.Material,
                MaterialId = stepMaterial.MaterialId,
                ShoppingList = shoppingList,
                ShoppingListId = shoppingList.Id,

            };
        }

        public ShoppingListMaterialViewModel( Material material, ShoppingList shoppingList )
        {
            _shoppingListMaterial = new ShoppingListMaterial()
            {
                Material = material,
                MaterialId = material.Id,
                ShoppingList = shoppingList,
                Name = material.Name,
                Quantity = 1,
                Purchased = false
            };

            Material = material;
            MaterialId = material.Id;
            ShoppingList = shoppingList;
            Name = material.Name;
            Quantity = 1;
            Purchased = false;

            Tags = new ObservableRangeCollection<Tag>( material.Tags );
        }

        //public ShoppingListMaterialViewModel( string name, Material mat, ShoppingList sList, int QuantityRequired )
        //{
        //    Name = name;
        //    Material = mat;
        //    MaterialId = mat.Id;
        //    ShoppingList = sList;
        //    ShoppingListId = sList.Id;
            
        //    ShoppingListMaterial
        //}

        #region _PROPERTIES_
        public event Action<int> OnPurchasedChanged;
        public event Action<int> OnItemPurchased;
        public event Action<int> OnItemDeleted;
        public event Action<int> OnItemSaved;

        public AsyncCommand<int> OnItemDeletedAsyncCommand;



        private Color _color;
        public Color Color
        {
            get => _color;
            set => SetProperty( ref _color, value );
        }

        private bool canEdit;
        public bool CanEdit
        {
            get => canEdit;
            set => SetProperty( ref canEdit, value );
        }

        private ShoppingListMaterial _shoppingListMaterial;
        public ShoppingListMaterial ShoppingListMaterial
        {
            get => _shoppingListMaterial;
            private set => SetProperty( ref _shoppingListMaterial, value );
        }

        private int materialId;
        public int MaterialId
        {
            get => materialId;
            set => SetProperty( ref materialId, value );
        }

        private Material material;
        public Material Material
        {
            get => material;
            set => SetProperty( ref material, value );
        }

        private ShoppingList shoppingList;
        public ShoppingList ShoppingList { get => shoppingList; set => SetProperty( ref shoppingList, value ); }

        private string name;
        public string Name { get => name; set => SetProperty( ref name, value ); }

        private int quantity;
        public int Quantity { get => quantity; set => SetProperty( ref quantity, value ); }

        private bool purchased;
        public bool Purchased
        {
            get => purchased;
            set => SetProperty( ref purchased, value );
        }

        private ObservableRangeCollection<Tag> tags;
        public ObservableRangeCollection<Tag> Tags { get => tags ??= new ObservableRangeCollection<Tag>(); set => SetProperty( ref tags, value ); }

        private TextDecorations textDecoration;
        public TextDecorations TextDecoration
        {
            get => textDecoration;
            set => SetProperty( ref textDecoration, value );
        }


        public int ShoppingListId { get; set; }
        public Config.EditState EditState { get; set; }
        #endregion

        #region _COMMANDS_

        // Cross Off
        private AsyncCommand crossOffCommand;
        private AsyncCommand togglePurchasedCommand;
        public ICommand TogglePurchasedCommand { get => togglePurchasedCommand ??= new AsyncCommand(TogglePurchased); }
        private async Task TogglePurchased()
        {
            Purchased = !Purchased;
            TextDecoration = Purchased ? TextDecorations.Strikethrough : TextDecorations.None;
            Color = Purchased ? (Color)App.Current.Resources["Secondary"] : (Color)App.Current.Resources["Cue"];

            await ShoppingListMaterialManager.PurchaseItemAsync( ShoppingListMaterial.Id, Purchased );

            OnItemPurchased?.Invoke( Purchased ? Quantity : -Quantity );
        }

        public ICommand CrossOffCommand { get => crossOffCommand ??= new AsyncCommand( CrossOff ); }
        private async Task CrossOff()
        {
            if( !CanEdit )
            {
                Purchased = !Purchased;
                ShoppingListMaterial.Purchased = Purchased;

                if( Material == null )
                {
                    Material = await DbServiceLocator.GetItemAsync<Material>( MaterialId );
                    ShoppingListMaterial.Material = Material;
                    await DbServiceLocator.UpdateItemAsync( ShoppingListMaterial );
                }

                await DbServiceLocator.UpdateItemAsync( ShoppingListMaterial );
                await UpdateMaterialQuantity();

                TextDecoration = TextDecoration == TextDecorations.Strikethrough ? TextDecorations.None : TextDecorations.Strikethrough;

                Color = Purchased ? Color.Green : Color.DarkRed;
            }
        }

        private async Task UpdateMaterialQuantity()
        {
            if( Purchased )
            {
                Material.QuantityOwned += Quantity;
            }
            else
            {
                Material.QuantityOwned -= Quantity;
            }

            await DbServiceLocator.UpdateItemAsync( Material );
        }

        // Toggle Edit State Command
        private AsyncCommand toggleEditStateCommand;
        public ICommand ToggleEditStateCommand { get => toggleEditStateCommand ??= new AsyncCommand(ToggleEditState); }
        private async Task ToggleEditState()
        {
            Console.WriteLine( $"{Name} Edit State Changed!!" );
        }

        private MvvmHelpers.Commands.Command<bool> toggleCanEditCommand;
        public ICommand ToggleCanEditCommand
        {
            get => toggleCanEditCommand ??= new MvvmHelpers.Commands.Command<bool>( x => ToggleCanEdit( x ) );
        }
        private void ToggleCanEdit( bool value )
        {
            CanEdit = value;
        }

        // Delete Command
        private AsyncCommand deleteItemCommand;
        private AsyncCommand deleteCommand;
        public ICommand DeleteCommand { get => deleteCommand ??= new AsyncCommand(Delete); }
        private async Task Delete()
        {
            Console.WriteLine( $"{Name} Deleted!!" );
        }

        public ICommand DeleteItemCommand
        {
            get => deleteItemCommand ??= new AsyncCommand( DeleteItem );
        }
        public async Task DeleteItem()
        {
            if( ShoppingListMaterial.Id > 0 )
            {
                await OnItemDeletedAsyncCommand?.ExecuteAsync( ShoppingListMaterial.Id );

                await DbServiceLocator.DeleteItemAsync<ShoppingListMaterial>( ShoppingListMaterial.Id );
            }
        }

        // Open Command
        private AsyncCommand openCommand;
        public ICommand OpenCommand { get => openCommand ??= new AsyncCommand( Open ); }
        private async Task Open()
        {
            Console.WriteLine( $"{Name} Opened!!" );
            //if( !CanEdit )
            //{
            //    string encodedId = HttpUtility.UrlEncode(ShoppingListMaterial.Id.ToString());

            //    await Shell.Current.GoToAsync( $"{nameof( ShoppingListMaterialDetailView )}?{QueryParameters.ShoppingListMaterialId}={encodedId}" );
            //}
        }

        // Save Command
        private AsyncCommand saveCommand;
        public ICommand SaveCommand { get => saveCommand ??= new AsyncCommand(Save); }
        private async Task Save() 
        {
            Console.WriteLine( $"{Name} Saved!!" );
        }

        // Refresh Command
        private AsyncCommand refreshCommand;
        public ICommand RefreshCommand { get => refreshCommand ??= new AsyncCommand(Refresh); }

        // TODO: Pick up here. Sort out the creation of SLMVMs and then flesh out this method.
        private async Task Refresh()
        {

            Console.WriteLine( $"{Name} Refreshed!!" );
            //if( ShoppingListMaterial != null )
            //{
            //    ShoppingListMaterial = await ShoppingListMaterialManager.GetItemRecursiveAsync( ShoppingListMaterial.Id );

            //    Name = ShoppingListMaterial.Name;
            //    Quantity = ShoppingListMaterial.Quantity;
            //    // Set the field directly to avoid triggering any logic in the Purchased setter
            //    purchased = ShoppingListMaterial.Purchased;
            //    Material = ShoppingListMaterial.Material;
            //    MaterialId = ShoppingListMaterial.MaterialId;
            //    ShoppingList = ShoppingListMaterial.ShoppingList;

            //}
        }


        // Back Command
        private AsyncCommand backCommand;
        public ICommand BackCommand { get => backCommand ??= new AsyncCommand(Back); }
        private async Task Back() 
        {
            Console.WriteLine( $"{Name} Sent Back!!" );
            //await Shell.Current.GoToAsync( $".." );
        }
        #endregion

        #region _METHODS_

        private int onPurchaseChanged( bool value )
        {
            return value ? 1 : -1;
        }

        internal async Task<int> AddOrUpdateAndReturnIdAsync()
        {
            Console.WriteLine( $"----- AddOrUpdateAndReturnIdAsync:" );

            Console.WriteLine( $"----------  AddOrUpdateAndReturnIdAsync: Old Name - {ShoppingListMaterial.Name}" );
            Console.WriteLine( $"----------  AddOrUpdateAndReturnIdAsync: New Name - {Name}" );
            ShoppingListMaterial.Name = Name;

            Console.WriteLine( $"----------  AddOrUpdateAndReturnIdAsync: Old Quanitiy - {ShoppingListMaterial.Quantity}" );
            Console.WriteLine( $"----------  AddOrUpdateAndReturnIdAsync: New Quanitiy - {Quantity}" );
            ShoppingListMaterial.Quantity = Quantity;

            Console.WriteLine( $"----------  AddOrUpdateAndReturnIdAsync: Old Material - {ShoppingListMaterial.Material}" );
            Console.WriteLine( $"----------  AddOrUpdateAndReturnIdAsync: New Material - {Material}" );
            ShoppingListMaterial.Material = Material;

            Console.WriteLine( $"----------  AddOrUpdateAndReturnIdAsync: Old Purchased State - {ShoppingListMaterial.Purchased}" );
            Console.WriteLine( $"----------  AddOrUpdateAndReturnIdAsync: New Purchased State - {Purchased}" );
            ShoppingListMaterial.Purchased = Purchased;

            Console.WriteLine( $"----------  AddOrUpdateAndReturnIdAsync: Old ShoppingListId - {ShoppingListMaterial.ShoppingList.Id}" );
            Console.WriteLine( $"----------  AddOrUpdateAndReturnIdAsync: New ShoppingListId - {ShoppingList.Id}" );
            ShoppingListMaterial.ShoppingList = ShoppingList;

            ShoppingListMaterial.CreatedOn = ShoppingListMaterial.CreatedOn != null ? ShoppingListMaterial.CreatedOn : DateTime.UtcNow;

            Console.WriteLine( $"----------  AddOrUpdateAndReturnIdAsync: Original ShoppingListMaterial Id - {ShoppingListMaterial.Id} " );
            
            int id = await DbServiceLocator.AddOrUpdateItemAndReturnIdAsync( ShoppingListMaterial );

            Console.WriteLine( $"----------  AddOrUpdateAndReturnIdAsync: New ShoppingListMaterial Id - {id}" );
            return id;
        }

        #endregion

        #region Query Handling
        private protected override async Task EvaluateQueryParams( KeyValuePair<string, string> kvp )
        {
            switch( kvp.Key )
            {
                case QueryParameters.Refresh:
                    await Refresh();
                    break;
            }
        }

        public bool Equals( ShoppingListMaterialViewModel other )
        {
            return other?.ShoppingListMaterial.Id == ShoppingListMaterial.Id;
        }

        #endregion Query Handling
    }
}
