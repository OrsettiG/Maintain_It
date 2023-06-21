using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Maintain_it.Helpers;
using Maintain_it.Models;
using Maintain_it.Models.Interfaces;

using MvvmHelpers.Commands;

using Xamarin.Forms;

namespace Maintain_it.ViewModels
{
    public class DynamicShoppingListItemViewModel : BaseViewModel, IShoppingListItemViewModel, IEquatable<DynamicShoppingListItemViewModel>
    {
        #region Constructors
        public DynamicShoppingListItemViewModel( StepMaterial stepMat, ShoppingList sList )
        {
            // From StepMaterial
            Name = stepMat.Material.Name;
            MaterialId = stepMat.Material.Id;
            Material = stepMat.Material;
            Quantity = Material.QuantityOwned - stepMat.Quantity;

            // From ShoppingList
            ShoppingListId = sList.Id;
            ShoppingList = sList;

            // General
            Color = (Color)App.Current.Resources["Accent1"];
            TextDecoration = TextDecorations.None;
        }

        public DynamicShoppingListItemViewModel( StepMaterial stepMat, ShoppingList sList, int shortfall )
        {
            // From StepMaterial
            Name = stepMat.Material.Name;
            MaterialId = stepMat.Material.Id;
            Material = stepMat.Material;
            Quantity = shortfall;

            // From ShoppingList
            ShoppingListId = sList.Id;
            ShoppingList = sList;

            // General
            Color = (Color)App.Current.Resources["Accent1"];
            TextDecoration = TextDecorations.None;
        }
        
        public DynamicShoppingListItemViewModel( Material mat, ShoppingList sList, int shortfall )
        {
            // From StepMaterial
            Name = mat.Name;
            MaterialId = mat.Id;
            Material = mat;
            Quantity = shortfall;

            // From ShoppingList
            ShoppingListId = sList.Id;
            ShoppingList = sList;

            // General
            Color = (Color)App.Current.Resources["Accent1"];
            TextDecoration = TextDecorations.None;
        }
        
        public DynamicShoppingListItemViewModel( string name, Material mat, ShoppingList sList, int shortfall )
        {
            // From StepMaterial
            Name = name;
            MaterialId = mat.Id;
            Material = mat;
            Quantity = shortfall;

            // From ShoppingList
            ShoppingListId = sList.Id;
            ShoppingList = sList;

            // General
            Color = (Color)App.Current.Resources["Accent1"];
            TextDecoration = TextDecorations.None;
        }
        #endregion

        #region Events
        public event Action<int> OnItemPurchased;
        public event Action<int> OnItemDeleted;
        public event Action<int> OnItemSaved;
        #endregion Events

        #region Properties
        private string name;
        public string Name
        {
            get => name;
            set => SetProperty( ref name, value );
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

        private int shoppingListId;
        public int ShoppingListId
        {
            get => shoppingListId;
            set => SetProperty( ref shoppingListId, value );
        }

        private ShoppingList shoppingList;
        public ShoppingList ShoppingList
        {
            get => shoppingList;
            set => SetProperty( ref shoppingList, value );
        }

        private int quantity;
        public int Quantity
        {
            get => quantity;
            set => SetProperty( ref quantity, value );
        }

        private Color color;
        public Color Color
        {
            get => color;
            set => SetProperty( ref color, value );
        }

        private TextDecorations textDecoration;
        public TextDecorations TextDecoration
        {
            get => textDecoration;
            set => SetProperty( ref textDecoration, value );
        }
        #endregion Properties

        #region Commands
        // Toggle Purchased
        private bool purchased;
        public bool Purchased
        {
            get => purchased;
            set => SetProperty( ref purchased, value );
        }
        private AsyncCommand togglePurchasedCommand;
        public ICommand TogglePurchasedCommand => togglePurchasedCommand ??= new AsyncCommand( TogglePurchased );
        private async Task TogglePurchased()
        {
            Purchased = !Purchased;
            TextDecoration = Purchased ? TextDecorations.Strikethrough : TextDecorations.None;
            Color = Purchased ? (Color)App.Current.Resources["Secondary"] : (Color)App.Current.Resources["Accent1"];

            switch( Purchased )
            {
                case true:
                    await MaterialManager.IncreaseMaterialQuantity( MaterialId, Quantity );
                    break;
                case false:
                    await MaterialManager.DecreaseMaterialQuantity( MaterialId, Quantity );
                    break;
            }

            OnItemPurchased?.Invoke( Purchased ? Quantity : -Quantity );
        }

        // Toggle Edit State
        private Config.EditState editState;
        public Config.EditState EditState
        {
            get => editState;
            set => SetProperty( ref editState, value );
        }
        private AsyncCommand toggleEditStateCommand;
        public ICommand ToggleEditStateCommand { get => toggleEditStateCommand ??= new AsyncCommand( ToggleEditState ); }
        private async Task ToggleEditState()
        {
            Console.WriteLine( $"{Name} Edit State Toggled!!" );
        }

        // Open
        private AsyncCommand openCommand;
        public ICommand OpenCommand { get => openCommand ??= new AsyncCommand( Open ); }
        private async Task Open()
        {
            Console.WriteLine( $"{Name} Opened!!" );
        }

        // Save
        private AsyncCommand closeCommand;
        public ICommand SaveCommand { get => closeCommand ??= new AsyncCommand( Close ); }
        private async Task Close()
        {
            Console.WriteLine( $"{Name} Closed!!" );
        }

        // Refresh
        private AsyncCommand refreshCommand;
        public ICommand RefreshCommand { get => refreshCommand ??= new AsyncCommand( Refresh ); }
        private async Task Refresh()
        {
            Console.WriteLine( $"{Name} Refreshed!!" );
        }

        // Delete
        private AsyncCommand deleteCommand;
        public ICommand DeleteCommand { get => deleteCommand ??= new AsyncCommand( Delete ); }
        private async Task Delete()
        {
            Console.WriteLine( $"{Name} Deleted!!" );
        }

        // Back
        private AsyncCommand backCommand;
        public ICommand BackCommand { get => backCommand ??= new AsyncCommand( Back ); }
        private async Task Back()
        {
            Console.WriteLine( $"{Name} Sent Back!!" );
        }

        #endregion Commands

        #region IEquatable Implementation
        public bool Equals( DynamicShoppingListItemViewModel other )
        {
            return Name == other.Name;
        }
        #endregion IEquatable Implementation
    }
}
