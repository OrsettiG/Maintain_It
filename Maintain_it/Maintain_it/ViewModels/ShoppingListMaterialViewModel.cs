using System;
using System.Collections.Generic;
using System.Linq;
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

using Command = MvvmHelpers.Commands.Command;

namespace Maintain_it.ViewModels
{
    public class ShoppingListMaterialViewModel : BaseViewModel
    {

        public ShoppingListMaterialViewModel( ShoppingListMaterial shoppingListMaterial )
        {
            _shoppingListMaterial = shoppingListMaterial;
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
                Color = Color.Green;
            }
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

            Tags = material.Tags.Where( x => x.TagType == TagType.ShoppingList || x.TagType == TagType.General ) as ObservableRangeCollection<Tag>;
        }

        #region _PROPERTIES_
        public event Action<int> OnPurchasedChanged;
        public AsyncCommand RefreshParentPageOnItemDeleteAsyncCommand;

        private Color _color = Color.DarkRed;
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
        public ShoppingListMaterial ShoppingListMaterial { get => _shoppingListMaterial; }

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
            set
            {
                if( !CanEdit && SetProperty( ref purchased, value ) )
                {
                    if( !value )
                    {
                        OnPurchasedChanged?.Invoke( 1 );
                    }
                    else
                    {
                        OnPurchasedChanged?.Invoke( -1 );
                    }
                }

            }
        }

        private ObservableRangeCollection<Tag> tags;
        public ObservableRangeCollection<Tag> Tags { get => tags ??= new ObservableRangeCollection<Tag>(); set => SetProperty( ref tags, value ); }

        private TextDecorations textDecoration;
        public TextDecorations TextDecoration 
        { 
            get => textDecoration; 
            set => SetProperty( ref textDecoration, value ); 
        }
        #endregion

        #region _COMMANDS_

        private AsyncCommand crossOffCommand;
        public ICommand CrossOffCommand { get => crossOffCommand ??= new AsyncCommand( CrossOff ); }
        private async Task CrossOff()
        {
            if( !CanEdit )
            {
                Purchased = !Purchased;
                ShoppingListMaterial.Purchased = Purchased;
                if(Material != null )
                {
                    await DbServiceLocator.UpdateItemAsync( ShoppingListMaterial );
                }
                else
                {
                    Material = await DbServiceLocator.GetItemAsync<Material>( MaterialId );
                    ShoppingListMaterial.Material = Material;
                    await DbServiceLocator.UpdateItemAsync( ShoppingListMaterial );
                }
                TextDecoration = TextDecoration == TextDecorations.Strikethrough ? TextDecorations.None : TextDecorations.Strikethrough;

                Color = Purchased ? Color.Green : Color.DarkRed;
            }
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

        private AsyncCommand deleteItemCommand;
        public ICommand DeleteItemCommand
        {
            get => deleteItemCommand ??= new AsyncCommand( DeleteItem );
        }
        public async Task DeleteItem()
        {
            await DbServiceLocator.DeleteItemAsync<ShoppingListMaterial>( ShoppingListMaterial.Id );

            await RefreshParentPageOnItemDeleteAsyncCommand?.ExecuteAsync();
        }

        private AsyncCommand openCommand;
        public ICommand OpenCommand { get => openCommand ??= new AsyncCommand( Open ); }
        private async Task Open()
        {
            if( !CanEdit )
            {
                string encodedId = HttpUtility.UrlEncode(ShoppingListMaterial.Id.ToString());

                await Shell.Current.GoToAsync( $"{nameof( ShoppingListMaterialDetailView )}?{RoutingPath.ShoppingListMaterialId}={encodedId}" );
            }
        }
        #endregion

        #region _METHODS_

        private int onPurchaseChanged( bool value )
        {
            return value ? 1 : -1;
        }

        internal async Task<int> AddOrUpdateAndReturnIdAsync()
        {
            ShoppingListMaterial.Name = Name;
            ShoppingListMaterial.Quantity = Quantity;
            ShoppingListMaterial.Material = Material;
            ShoppingListMaterial.Purchased = Purchased;
            ShoppingListMaterial.ShoppingList = ShoppingList;
            ShoppingListMaterial.CreatedOn = ShoppingListMaterial.CreatedOn != null ? ShoppingListMaterial.CreatedOn : DateTime.Now;

            return await DbServiceLocator.AddOrUpdateItemAndReturnIdAsync( ShoppingListMaterial );
        }
        #endregion
    }
}
