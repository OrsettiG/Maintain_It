using Maintain_it.Models;

namespace Maintain_it.ViewModels
{
    public class ShoppingListViewModel : BaseViewModel
    {

        public ShoppingListViewModel( ShoppingList shoppingList )
        {
            _shoppingList = shoppingList;
        }

        #region Properties
        private ShoppingList _shoppingList;
        public ShoppingList ShoppingList { get => _shoppingList; }
        #endregion

    }
}