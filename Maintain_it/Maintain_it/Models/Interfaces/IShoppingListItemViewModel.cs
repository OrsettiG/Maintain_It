using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using MvvmHelpers;

using Xamarin.Forms;

using static Maintain_it.Helpers.Config;

namespace Maintain_it.Models.Interfaces
{
    public interface IShoppingListItemViewModel : IViewModel
    {
        #region Events
        event Action<int> OnItemPurchased;
        event Action<int> OnItemDeleted;
        event Action<int> OnItemSaved;
        #endregion Events

        #region Properties
        // ShoppingListMaterial Properties
        public string Name { get; set; }
        public bool Purchased { get; set; }
        public int MaterialId { get; set; }
        public Material Material { get; set; }
        public int ShoppingListId { get; set; }
        public ShoppingList ShoppingList { get; set; }
        public int Quantity { get; set; }

        // ViewModel Properties
        public Color Color { get; set; }
        public EditState EditState { get; set; }
        public TextDecorations TextDecoration { get; set; }
        #endregion Properties

        #region Commands
        public ICommand TogglePurchasedCommand { get; }
        public ICommand ToggleEditStateCommand { get; }
        #endregion Commands
    }
}
